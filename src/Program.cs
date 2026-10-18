using System.Security.Principal;
using System.Text;
using System.Text.Json;
namespace GugaLauncher;
/// <summary>預設僅說明；只有明確 start --live 會走原版 XXMI，bridge 必須持有本次會話。</summary>
internal static class Program
{
    public static int Main(string[] args)
    {
        Console.OutputEncoding=Encoding.UTF8; Console.InputEncoding=Encoding.UTF8;
        try
        {
            if(args.SequenceEqual(new[]{"--self-test"})){OfflineTests.Run();return 0;}
            if(args.Length==0||args.SequenceEqual(new[]{"--help"})){Help();return 0;}
            if(args.SequenceEqual(new[]{"setup"})){Setup();return 0;}
            if(args.SequenceEqual(new[]{"guide"})){Guide();return 0;}
            if(args.SequenceEqual(new[]{"bridge"})){WindowsHost.Bridge();return 0;}
            if(args.SequenceEqual(new[]{"command"})){Console.WriteLine(Settings.BridgeCommand(Environment.ProcessPath!));return 0;}
            if(args.SequenceEqual(new[]{"check"}))
            {
                WindowsHost.RejectNamed("Endfield");WindowsHost.RejectNamed("XXMI Launcher");
                var settings=Settings.Load();settings.ValidateFiles();settings.ValidateXxmi();
                Console.WriteLine("設定與前置檢查通過；未開啟 XXMI／官方程序／遊戲，也未測試原生權限。");return 0;
            }
            if(args.SequenceEqual(new[]{"start","--live"}))
            {
                // 同一使用者／session 跨本版所有路徑共用鎖；不從另一實例接管。
                using var identity=WindowsIdentity.GetCurrent();
                using var gate=new Mutex(false,"Local\\GugaLauncher-Coordinator-"+identity.User!.Value);
                bool owned;
                try{owned=gate.WaitOne(0);}catch(AbandonedMutexException){owned=true;}
                if(!owned)throw new InvalidOperationException("已有協同流程執行中，不重複啟動。");
                try
                {
                    // 在讀設定或任何其他副作用前就檢查，避免先行打開原版 XXMI。
                    WindowsHost.RejectNamed("Endfield");WindowsHost.RejectNamed("XXMI Launcher");
                    using var host=new WindowsHost(Settings.Load());
                    uint pid=Coordinator.Run(host);
                    Console.WriteLine($"已建立遊戲 PID {pid}；注入由原版 EFMI 處理，請確認遊戲畫面。");
                }
                finally{gate.ReleaseMutex();}
                return 0;
            }
            throw new ArgumentException("不支援的參數；請使用 --help。未啟動任何程序。");
        }
        catch(Exception error)
        {
            string message=error is OperationCanceledException && args.FirstOrDefault() is "start" or "bridge"?"協同等待逾時，已停止；不會延遲建立遊戲。":error.Message;
            Console.Error.WriteLine("已停止："+message);
            // XXMI 的 CUSTOM 會隱藏 stdout/stderr；錯誤仍留在本版本機日誌。
            if(args.Length>0&&args[0] is "start" or "bridge")
            {
                try{File.AppendAllText(Path.Combine(AppContext.BaseDirectory,"local-errors.log"),DateTimeOffset.Now.ToString("o")+" "+message+Environment.NewLine);}
                catch(IOException){}catch(UnauthorizedAccessException){}
            }
            return 1;
        }
    }
    private static void Help()=>Console.WriteLine("GugaLauncher C# 協同原型\nsetup：只保存本版位置並顯示 XXMI 設定說明\nguide：顯示逐步設定說明\ncommand：顯示要貼入「启动命令」的整行文字\ncheck：唯讀檢查，不啟動程序\n--self-test：離線假宿主測試\nstart --live：前置防重複檢查後呼叫原版 XXMI\nbridge：僅供本次 XXMI 回呼，不可直接使用\n請勿直接按原版 XXMI 啟動鈕繞過前置入口。原版自身的關閉遊戲行為未修改。");
    private static void Setup()
    {
        ConsoleGuide.Write("GugaEFMI | 首次設定",ConsoleColor.Cyan);
        ConsoleGuide.Write("[1/3] 指定三個安裝資料夾",ConsoleColor.Cyan);
        Console.WriteLine("請輸入資料夾，不是 .exe；可貼上含雙引號的路徑。Enter 留空取消。\n");
        string Folder(string label,string example)
        {
            ConsoleGuide.Write("\n"+label,ConsoleColor.Yellow);
            Console.WriteLine("範例："+example+"（僅示意，請輸入自己的實際安裝位置）");
            ConsoleGuide.Write("實際資料夾：",ConsoleColor.Yellow,false);string text=Console.ReadLine()?.Trim().Trim('"')??"";
            if(text.Length==0)throw new InvalidOperationException("已取消設定。");
            return Path.GetFullPath(text);
        }
        var settings=new Settings(Folder("① XXMI 根目錄（內含 Resources 資料夾）",@"C:\XXMI Launcher"),Folder("② Endfield 目錄（內含 Endfield.exe）",@"C:\Arknights Endfield"),Folder("③ GRYPHLINK 目錄（內含 Launcher.exe）",@"C:\GRYPHLINK"));
        settings.ValidateFiles();_=settings.Command;
        // 不修改 XXMI；重設時只備份自己的設定，避免覆蓋使用者原有資料。
        if(File.Exists(Settings.FilePath))
            File.Copy(Settings.ExistingFile(Settings.FilePath),Settings.FilePath+".backup-"+Guid.NewGuid().ToString("N"));
        File.WriteAllText(Settings.FilePath,JsonSerializer.Serialize(settings,new JsonSerializerOptions{WriteIndented=true,IgnoreReadOnlyProperties=true}),new UTF8Encoding(false));
        ConsoleGuide.Write("\n三個路徑已儲存。接著請在 XXMI 介面設定。",ConsoleColor.Green);
        Guide();
        ReviewSetup(Console.In,Console.Out,()=>
        {
            // XXMI 的設定需先儲存並正常退出；不替玩家終止它。
            WindowsHost.RejectNamed("Endfield");WindowsHost.RejectNamed("XXMI Launcher");
            settings.ValidateFiles();settings.ValidateXxmi();
        });
    }

    /// <summary>同一視窗按 Enter 檢查，失敗保留流程供修正後重試；輸入結束即取消。</summary>
    internal static void ReviewSetup(TextReader input,TextWriter output,Action validate)
    {
        while(true)
        {
            ConsoleGuide.Write("\n按 Enter 檢查設定；輸入 Q 取消：",ConsoleColor.Yellow,output:output);
            string? answer=input.ReadLine();
            if(answer is null || answer.Trim().Equals("Q",StringComparison.OrdinalIgnoreCase))
                throw new OperationCanceledException("已取消檢查，已儲存的路徑會保留。");
            try{validate();}
            catch(Exception error) when(error is InvalidOperationException or InvalidDataException or IOException or UnauthorizedAccessException or JsonException or KeyNotFoundException)
            {ConsoleGuide.Write("尚未完成："+error.Message,ConsoleColor.Red,output:output);continue;}
            ConsoleGuide.Write("OK，基本設定檢查通過。接著會建立 GugaEFMI 捷徑。",ConsoleColor.Green,output:output);
            return;
        }
    }

    /// <summary>顯示與目前執行檔位置一致的命令及彩色首次設定指引。</summary>
    private static void Guide()=>ConsoleGuide.Show(Settings.BridgeCommand(Environment.ProcessPath!));
}


