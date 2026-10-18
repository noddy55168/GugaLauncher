using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;

namespace GugaLauncher;

/// <summary>只擁有 Win32 handle；釋放 handle 不會結束其指向的程序。</summary>
internal sealed class OwnedHandle : SafeHandleZeroOrMinusOneIsInvalid
{
    public OwnedHandle() : base(true) { }
    public OwnedHandle(nint value) : base(true) => SetHandle(value);
    protected override bool ReleaseHandle() => Native.CloseHandle(handle);
}

/// <summary>Windows x64 API 邊界；沒有終止程序、修改 ACL 或調整 privilege 的介面。</summary>
internal static class Native
{
    public const uint ParentAccess = 0x101080;
    public static void Require(bool success, string operation)
    {
        if (!success) throw new Win32Exception(Marshal.GetLastWin32Error(), operation);
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern OwnedHandle OpenProcess(uint access, bool inherit, uint pid);
    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)] public static extern bool CloseHandle(nint handle);
    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern uint WaitForSingleObject(OwnedHandle handle, uint milliseconds);
    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    [return: MarshalAs(UnmanagedType.Bool)] public static extern bool QueryFullProcessImageNameW(OwnedHandle handle, uint flags, StringBuilder name, ref uint size);
    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)] public static extern bool GetProcessTimes(OwnedHandle handle, out long created, out long exited, out long kernel, out long user);
    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)] public static extern bool ProcessIdToSessionId(uint pid, out uint session);
    [DllImport("advapi32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)] public static extern bool OpenProcessToken(OwnedHandle process, uint access, out OwnedHandle token);
    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)] public static extern bool InitializeProcThreadAttributeList(nint list, int count, uint flags, ref nuint size);
    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)] public static extern bool UpdateProcThreadAttribute(nint list, uint flags, nuint attribute, nint value, nuint size, nint previous, nint returnedSize);
    [DllImport("kernel32.dll")]
    public static extern void DeleteProcThreadAttributeList(nint list);
    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    [return: MarshalAs(UnmanagedType.Bool)] public static extern bool CreateProcessW(string app, StringBuilder command, nint processAttributes, nint threadAttributes, bool inherit, uint flags, nint environment, string directory, ref StartupInfoEx startup, out ProcessInfo result);
    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern uint ResumeThread(OwnedHandle thread);
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)] public static extern bool EnumWindows(WindowCallback callback, nint parameter);
    public delegate bool WindowCallback(nint window, nint parameter);
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)] public static extern bool IsWindowVisible(nint window);
    [DllImport("user32.dll")]
    public static extern uint GetWindowThreadProcessId(nint window, out uint pid);

    [StructLayout(LayoutKind.Sequential)]
    internal struct StartupInfo
    {
        public uint Size;
        public nint Reserved, Desktop, Title;
        public uint X, Y, XSize, YSize, XChars, YChars, Fill, Flags;
        public ushort Show, ReservedSize;
        public nint ReservedBytes, Input, Output, Error;
    }
    [StructLayout(LayoutKind.Sequential)]
    internal struct StartupInfoEx { public StartupInfo Startup; public nint Attributes; }
    [StructLayout(LayoutKind.Sequential)]
    internal struct ProcessInfo { public nint Process, Thread; public uint Pid, Tid; }
}
