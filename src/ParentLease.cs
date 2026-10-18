using System.Diagnostics;
using System.Security.Principal;
using System.Text;
using System.Text.Json;

namespace GugaLauncher;

/// <summary>固定父程序身分；建立時間來自 handle，不將 PID 當永久識別碼。</summary>
internal sealed record ParentIdentity(uint Pid, long Created, string Path, uint Session, string Sid);

/// <summary>借用既有官方程序，並持有其映像檔唯讀鎖；不擁有程序生命週期。</summary>
internal sealed class ParentLease : IDisposable
{
    private readonly FileStream image;
    public OwnedHandle Handle { get; }
    public ParentIdentity Identity { get; }
    public string Thumbprint { get; }
    private ParentLease(OwnedHandle handle, ParentIdentity identity, FileStream image, string thumbprint)
        => (Handle, Identity, this.image, Thumbprint) = (handle, identity, image, thumbprint);

    public static ParentIdentity ReadIdentity(OwnedHandle handle, uint pid)
    {
        var text = new StringBuilder(32768);
        uint size = 32768;
        Native.Require(Native.QueryFullProcessImageNameW(handle, 0, text, ref size), "Query parent image");
        Native.Require(Native.GetProcessTimes(handle, out long created, out _, out _, out _), "Query parent creation time");
        Native.Require(Native.ProcessIdToSessionId(pid, out uint session), "Query parent session");
        Native.Require(Native.OpenProcessToken(handle, 8, out var token), "Query parent token");
        using (token)
        using (var identity = new WindowsIdentity(token.DangerousGetHandle()))
            return new(pid, created, System.IO.Path.GetFullPath(text.ToString()), session,
                identity.User?.Value ?? throw new InvalidOperationException("Missing user SID"));
    }

    public static ParentLease Acquire(string expectedPath)
    {
        expectedPath = Settings.ExistingFile(expectedPath);
        if (!System.IO.Path.GetFileName(expectedPath).Equals("Games.exe", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Official executable must be Games.exe");
        Native.Require(Native.ProcessIdToSessionId((uint)Environment.ProcessId, out uint session), "Current session");
        using var me = WindowsIdentity.GetCurrent();
        var accepted = new List<ParentLease>();
        try
        {
            foreach (var process in Process.GetProcessesByName("Games"))
            using (process)
            {
                OwnedHandle? handle = Native.OpenProcess(Native.ParentAccess, false, (uint)process.Id);
                FileStream? file = null;
                try
                {
                    Native.Require(!handle.IsInvalid, "Open official parent");
                    var id = ReadIdentity(handle, (uint)process.Id);
                    if (!id.Path.Equals(expectedPath, StringComparison.OrdinalIgnoreCase)) continue;
                    if (id.Session != session || id.Sid != me.User?.Value)
                        throw new InvalidOperationException("Parent user/session differs");
                    file = new FileStream(expectedPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                    string thumb = VerifySignature(expectedPath);
                    var lease = new ParentLease(handle, id, file, thumb);
                    lease.CheckAlive();
                    accepted.Add(lease);
                    handle = null; file = null; // 資源所有權移交 lease。
                }
                finally { file?.Dispose(); handle?.Dispose(); }
            }
            if (accepted.Count != 1) throw new InvalidOperationException($"Expected one official parent, found {accepted.Count}");
            var result = accepted[0]; accepted.Clear(); return result;
        }
        finally { foreach (var lease in accepted) lease.Dispose(); }
    }

    public void CheckAlive()
    {
        uint wait = Native.WaitForSingleObject(Handle, 0);
        Native.Require(wait != uint.MaxValue, "Check parent lifetime");
        if (wait != 258 || ReadIdentity(Handle, Identity.Pid) != Identity)
            throw new InvalidOperationException("Borrowed parent exited or changed");
    }

    /// <summary>沿用已實測的系統 Authenticode 驗證；憑證存在本身不代表簽章有效。</summary>
    internal static string VerifySignature(string path)
    {
        string encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(path));
        string script = "$ErrorActionPreference='Stop';" +
            $"$p=[Text.Encoding]::UTF8.GetString([Convert]::FromBase64String('{encoded}'));" +
            "$s=Get-AuthenticodeSignature -LiteralPath $p;" +
            "[pscustomobject]@{Status=$s.Status.ToString();Thumbprint=$s.SignerCertificate.Thumbprint}|ConvertTo-Json -Compress";
        var start = new ProcessStartInfo(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows),
            "System32", "WindowsPowerShell", "v1.0", "powershell.exe"))
        { UseShellExecute = false, CreateNoWindow = true, RedirectStandardOutput = true, RedirectStandardError = true };
        foreach (string arg in new[] { "-NoLogo", "-NoProfile", "-NonInteractive", "-EncodedCommand", Convert.ToBase64String(Encoding.Unicode.GetBytes(script)) })
            start.ArgumentList.Add(arg);
        foreach (string key in start.Environment.Keys.Where(k => k.Equals("PSMODULEPATH", StringComparison.OrdinalIgnoreCase)).ToArray())
            start.Environment.Remove(key);
        using var process = Process.Start(start) ?? throw new InvalidOperationException("Signature helper failed to start");
        // 同時排空兩個 pipe，避免緩衝區互鎖；無強制 timeout/kill。
        var output = process.StandardOutput.ReadToEndAsync();
        var error = process.StandardError.ReadToEndAsync();
        process.WaitForExit();
        Task.WaitAll(output, error);
        if (process.ExitCode != 0) throw new InvalidOperationException($"Authenticode helper exit {process.ExitCode}: {error.Result}");
        using var json = JsonDocument.Parse(output.Result);
        string thumb = json.RootElement.GetProperty("Thumbprint").GetString()?.ToUpperInvariant() ?? "";
        if (json.RootElement.GetProperty("Status").GetString() != "Valid" || !AllowedThumbprints.Contains(thumb))
            throw new InvalidOperationException("Official signature/fingerprint mismatch");
        return thumb;
    }

    internal static readonly HashSet<string> AllowedThumbprints = [
        "2B366F397E64A9494A553B70004E8CA1AAE02B9D", "FF4AF264C36A27F0C805936708EF8EC67B396225"];
    public void Dispose() { image.Dispose(); Handle.Dispose(); }
}
