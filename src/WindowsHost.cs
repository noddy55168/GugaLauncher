using System.Diagnostics;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;

namespace GugaLauncher;
/// <summary>唯一實際副作用宿主。只建立官方/XXMI/遊戲，不提供終止、注入或 mod 操作。</summary>
internal sealed class WindowsHost(Settings settings) : ICoordinatorHost, IDisposable
{
    private readonly string nonce=Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
    private readonly string pipeName="GugaLauncher-"+Guid.NewGuid().ToString("N");
    private NamedPipeServerStream? pipe;
    private Process? xxmi;
    private ParentLease? parent;
    private Stopwatch? deadline;
    private StreamWriter? reply;
    public static void RejectNamed(string name)
    {
        var processes=Process.GetProcessesByName(name);
        try {if(processes.Length!=0) throw new InvalidOperationException($"{name} 已在執行，已停止；不會重啟或終止它。");}
        finally{foreach(var process in processes)process.Dispose();}
    }
    public void RejectExistingGame()=>RejectNamed("Endfield");
    public void RejectExistingXxmi()=>RejectNamed("XXMI Launcher");
    public void Validate()
    {
        settings.ValidateFiles();settings.ValidateXxmi();
        using var me=WindowsIdentity.GetCurrent();
        if(!new WindowsPrincipal(me).IsInRole(WindowsBuiltInRole.Administrator))
            throw new InvalidOperationException("本機已知原生權限需求：請由使用者以管理員身分開啟協同入口；不自動提權。");
    }
    public IDisposable PrepareOfficial()
    {
        var existing=Process.GetProcessesByName("Games");
        bool found=existing.Length!=0;foreach(var p in existing)p.Dispose();
        if(!found)
        {
            string launcher=Settings.ExistingFile(settings.LauncherExe);
            using var image=new FileStream(launcher,FileMode.Open,FileAccess.Read,FileShare.Read);
            _=ParentLease.VerifySignature(launcher);
            Process.Start(new ProcessStartInfo(launcher){UseShellExecute=false,WorkingDirectory=Path.GetDirectoryName(launcher)!})?.Dispose();
            var watch=Stopwatch.StartNew();
            while(watch.Elapsed<TimeSpan.FromSeconds(45))
            {
                RejectExistingGame();
                var candidates=Process.GetProcessesByName("Games");
                bool appeared=candidates.Length!=0;foreach(var p in candidates)p.Dispose();
                if(appeared)break;
                Thread.Sleep(250);
            }
        }
        parent=ParentLease.Acquire(settings.OfficialExe);
        return parent;
    }
    public void DispatchXxmi()
    {
        // 冷啟動與原生預檢先完成，才讓 XXMI 開始計算其等待期限。
        pipe=new NamedPipeServerStream(pipeName,PipeDirection.InOut,1,PipeTransmissionMode.Byte,
            PipeOptions.Asynchronous|PipeOptions.CurrentUserOnly);
        var start=new ProcessStartInfo(settings.XxmiExe){UseShellExecute=false,WorkingDirectory=settings.XxmiFolder};
        start.ArgumentList.Add("--nogui");start.ArgumentList.Add("--xxmi");start.ArgumentList.Add("EFMI");
        start.Environment["GUGA_COORD_PIPE"]=pipeName;start.Environment["GUGA_COORD_NONCE"]=nonce;
        deadline=Stopwatch.StartNew();
        xxmi=Process.Start(start)??throw new InvalidOperationException("XXMI 未建立。");
    }
    public void ReceiveCallback()
    {
        // 不以 helper 結束碼代表 XXMI 成功；需本次環境 nonce 及同使用者 pipe 回呼。
        using var timeout=new CancellationTokenSource(TimeSpan.FromSeconds(20));
        pipe!.WaitForConnectionAsync(timeout.Token).GetAwaiter().GetResult();
        Native.Require(GetNamedPipeClientProcessId(pipe.SafePipeHandle.DangerousGetHandle(),out uint clientPid),"Query callback PID");
        using(var client=Native.OpenProcess(0x1000,false,clientPid))
        {
            Native.Require(!client.IsInvalid,"Read callback process");
            var path=new StringBuilder(32768);uint size=32768;
            Native.Require(Native.QueryFullProcessImageNameW(client,0,path,ref size),"Read callback image");
            if(!path.ToString().Equals(Environment.ProcessPath,StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("回呼不是本版協同工具。");
        }
        var reader=new StreamReader(pipe,Encoding.UTF8,false,1024,true);
        string request=reader.ReadLineAsync(timeout.Token).AsTask().GetAwaiter().GetResult()??"";
        if(request.Length!=nonce.Length || !CryptographicOperations.FixedTimeEquals(Encoding.ASCII.GetBytes(request),Encoding.ASCII.GetBytes(nonce)))
            throw new InvalidOperationException("回呼不屬於本次啟動。");
        if(deadline!.Elapsed>TimeSpan.FromSeconds(20)||xxmi!.HasExited)
            throw new InvalidOperationException("XXMI 已退出或回呼逾時；不建立遊戲。");
        reply=new StreamWriter(pipe,new UTF8Encoding(false),1024,true){AutoFlush=true};
    }
    public uint CreateGame()
    {
        parent!.CheckAlive();
        if(deadline!.Elapsed>TimeSpan.FromSeconds(20)||xxmi!.HasExited)
            throw new InvalidOperationException("派送已失效，拒絕延遲建立遊戲。");
        RejectExistingGame();
        uint pid=GameCreation.Create(settings.GameExe,parent);
        // 遊戲已建立後即使回覆失敗也保留程序，不回滾或重試。
        reply!.WriteLine("CREATED:"+pid);
        Thread.Sleep(3200);
        return pid;
    }
    public void Dispose(){reply?.Dispose();pipe?.Dispose();xxmi?.Dispose();}
    [DllImport("kernel32.dll",SetLastError=true)]
    [return:MarshalAs(UnmanagedType.Bool)] private static extern bool GetNamedPipeClientProcessId(nint pipe,out uint pid);

    /// <summary>只能回應由前置入口繼承的單次握手，不支援單獨建立遊戲。</summary>
    public static void Bridge()
    {
        string name=Environment.GetEnvironmentVariable("GUGA_COORD_PIPE")??"";
        string token=Environment.GetEnvironmentVariable("GUGA_COORD_NONCE")??"";
        if(!name.StartsWith("GugaLauncher-",StringComparison.Ordinal)||name.Length>80||token.Length!=64)
            throw new InvalidOperationException("缺少前置檢查會話。請從本版 GugaEFMI 捷徑啟動，不要直接按原版啟動鈕。");
        using var pipe=new NamedPipeClientStream(".",name,PipeDirection.InOut,PipeOptions.Asynchronous|PipeOptions.CurrentUserOnly);
        using var timeout=new CancellationTokenSource(TimeSpan.FromSeconds(10));
        pipe.ConnectAsync(timeout.Token).GetAwaiter().GetResult();
        using var writer=new StreamWriter(pipe,new UTF8Encoding(false),1024,true){AutoFlush=true};
        using var reader=new StreamReader(pipe,Encoding.UTF8,false,1024,true);
        writer.WriteLine(token);
        string result=reader.ReadLineAsync(timeout.Token).AsTask().GetAwaiter().GetResult()??"";
        if(!result.StartsWith("CREATED:",StringComparison.Ordinal))throw new InvalidOperationException("協同未完成，請查看本版日誌。");
    }
}
/// <summary>只建立遊戲，注入與 mod 完全交回原版 EFMI；只釋放自己取得的 handle。</summary>
internal static class GameCreation
{
    public static string Quote(string value)
    {
        var text=new StringBuilder("\"");int slashes=0;
        foreach(char c in value){if(c=='\\'){slashes++;continue;}text.Append('\\',c=='"'?slashes*2+1:slashes);text.Append(c);slashes=0;}
        return text.Append('\\',slashes*2).Append('"').ToString();
    }
    public static uint Create(string exe,ParentLease parent)
    {
        nuint size=0;Native.InitializeProcThreadAttributeList(0,1,0,ref size);
        if(size==0)throw new InvalidOperationException("無法配置啟動屬性。");
        nint list=Marshal.AllocHGlobal(checked((int)size)),value=Marshal.AllocHGlobal(nint.Size);bool initialized=false;
        try
        {
            Native.Require(Native.InitializeProcThreadAttributeList(list,1,0,ref size),"Initialize attributes");initialized=true;
            Marshal.WriteIntPtr(value,parent.Handle.DangerousGetHandle());
            Native.Require(Native.UpdateProcThreadAttribute(list,0,0x20000,value,(nuint)nint.Size,0,0),"Set parent attribute");
            var startup=new Native.StartupInfoEx{Attributes=list};startup.Startup.Size=(uint)Marshal.SizeOf<Native.StartupInfoEx>();
            // 直接建立可執行狀態，不遺留等待 helper 恢復的暫停遊戲。
            Native.Require(Native.CreateProcessW(exe,new StringBuilder(Quote(exe)+" -force-d3d11"),0,0,false,0x80000|0x20,0,Path.GetDirectoryName(exe)!,ref startup,out var child),"Create game");
            using var process=new OwnedHandle(child.Process);using var thread=new OwnedHandle(child.Thread);return child.Pid;
        }
        finally{GC.KeepAlive(parent);if(initialized)Native.DeleteProcThreadAttributeList(list);Marshal.FreeHGlobal(value);Marshal.FreeHGlobal(list);}
    }
}
