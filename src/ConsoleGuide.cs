namespace GugaLauncher;

/// <summary>以主控台色彩凸顯操作；重導向與測試輸出維持純文字，不加入 ANSI 控制字元。</summary>
internal static class ConsoleGuide
{
    public static void Write(string text, ConsoleColor color, bool newLine=true, TextWriter? output=null)
    {
        output ??= Console.Out;
        bool useColor=ReferenceEquals(output,Console.Out) && !Console.IsOutputRedirected;
        ConsoleColor previous=default;
        if(useColor){previous=Console.ForegroundColor;Console.ForegroundColor=color;}
        try {if(newLine)output.WriteLine(text);else output.Write(text);}
        finally {if(useColor)Console.ForegroundColor=previous;}
    }

    /// <summary>依步驟分組，讓設定名稱與可複製命令各自獨立，不混入內部列舉名稱。</summary>
    public static void Show(string command)
    {
        Write("\n[2/3] 在 XXMI 完成設定",ConsoleColor.Cyan);
        Console.WriteLine("選擇 EFMI → 齒輪「设置」。請依下列頁籤調整：\n");
        Write("通用 → 启动设置",ConsoleColor.Cyan);
        Console.WriteLine("  先將「游戏启动」暫選「直接」，把「启动方式」設為 Native。");
        Write("  再將「游戏启动」改回「自定义」。Native 欄位隨後隱藏，屬正常。",ConsoleColor.Yellow);
        Console.WriteLine("  「启动命令」貼上下面整行（保留雙引號與最後的 bridge）：\n");
        Write(command,ConsoleColor.Yellow);
        Console.WriteLine("\n  「游戏进程启动等待超时」：60 或更大（秒）。");
        Console.WriteLine("  「自定义游戏进程可执行文件名称」：不勾選，保留 Endfield.exe。\n");
        Write("EFMI",ConsoleColor.Cyan);
        Console.WriteLine("  「XXMI DLL 注入模式」：選「注入」（英文 Direct）。\n");
        Write("启动器",ConsoleColor.Cyan);
        Console.WriteLine("  取消勾選「自动更新」。\n");
        Write("高级",ConsoleColor.Cyan);
        Console.WriteLine("  取消啟用「启动前命令」「加载后命令」「注入库」；不用刪除原有文字。\n");
        Write("啟動時會看到什麼？",ConsoleColor.Cyan);
        Console.WriteLine("  UAC 管理員提示：本機協同需要權限存取官方程序，這是預期的提權流程。");
        Console.WriteLine("  可能短暫看到兩個視窗：一般視窗交接給管理員視窗，交接後即關閉。");
        Console.WriteLine("  遊戲程序建立成功後，啟動視窗自動關閉；不代表 mod 畫面已驗收。");
        Console.WriteLine("  啟動失敗或取消 UAC 時保留訊息，按任意鍵關閉。\n");
        Write("防毒／安全提示",ConsoleColor.Yellow);
        Console.WriteLine("  本工具使用 PowerShell 與管理員權限；原版 EFMI 使用 DLL 注入載入 mod。");
        Console.WriteLine("  這些行為或未簽章檔案可能觸發安全警示，但警示本身不等於已確認誤報。");
        Console.WriteLine("  請核對檔案來源、警示名稱與路徑；不確定時先停止並保留訊息。");
        Write("  不需要停用防毒或加入整個磁碟排除。",ConsoleColor.Yellow);
        Write("\n[3/3] 回到這裡檢查",ConsoleColor.Cyan);
        Console.WriteLine("關閉設定視窗並正常退出 XXMI（包含系統匣），再回此視窗按 Enter。");
        Console.WriteLine("通過後自動建立 GugaEFMI 捷徑。日常使用 GugaEFMI，不直接按 XXMI 的「启动」。");
    }
}
