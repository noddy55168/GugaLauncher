using System.Text.Json;
namespace GugaLauncher;

/// <summary>只保存協同所需的安裝資料夾，不包含 mod、注入器或 XXMI 設定副本。</summary>
internal sealed record Settings(string XxmiFolder, string GameFolder, string OfficialFolder)
{
    public static string FilePath => Path.Combine(AppContext.BaseDirectory,"settings.json");
    public string XxmiExe => ExistingFile(Path.Combine(XxmiFolder,"Resources","Bin","XXMI Launcher.exe"));
    public string GameExe => ExistingFile(Path.Combine(GameFolder,"Endfield.exe"));
    public string LauncherExe => ExistingFile(Path.Combine(OfficialFolder,"Launcher.exe"));
    public string OfficialExe => Directory.EnumerateDirectories(OfficialFolder)
        .Select(p => (Path:p, Version:Version.TryParse(Path.GetFileName(p),out var v)?v:null))
        .Where(p=>p.Version is not null && File.Exists(Path.Combine(p.Path,"Games.exe")))
        .OrderByDescending(p=>p.Version).Select(p=>ExistingFile(Path.Combine(p.Path,"Games.exe"))).FirstOrDefault()
        ?? throw new InvalidDataException("找不到官方版本資料夾中的 Games.exe。");
    public static Settings Load() => JsonSerializer.Deserialize<Settings>(File.ReadAllText(ExistingFile(FilePath)))
        ?? throw new InvalidDataException("請先執行 Setup.cmd。");
    public void ValidateFiles(){ _=XxmiExe;_=GameExe;_=LauncherExe;_=OfficialExe; }
    /// <summary>拒絕重新解析點，不將使用者路徑拼入 shell 命令。</summary>
    public static string ExistingFile(string path)
    {
        path=Path.GetFullPath(path);
        if(!File.Exists(path)) throw new FileNotFoundException("找不到必要檔案："+path,path);
        for(string? p=path;p is not null;p=Path.GetDirectoryName(p))
            if((File.GetAttributes(p)&FileAttributes.ReparsePoint)!=0) throw new InvalidDataException("不接受連結路徑："+p);
        return path;
    }
    /// <summary>CUSTOM 經 cmd 解釋，拒絕有環境展開或特殊 shell 字元的程式位置。</summary>
    public static string BridgeCommand(string exe)
    {
        if(exe.IndexOfAny(['%','!','^','&','|','<','>','\r','\n','"'])>=0)
            throw new InvalidDataException("協同工具路徑含特殊 shell 字元，請改放一般資料夾。");
        return '"'+exe+"\" bridge";
    }
    public string Command => BridgeCommand(Environment.ProcessPath!);
    /// <summary>唯讀確認原版 UI 已完成設定；不代寫 XXMI 的簽章或設定。</summary>
    public void ValidateXxmi()
    {
        using var doc=JsonDocument.Parse(File.ReadAllText(ExistingFile(Path.Combine(XxmiFolder,"XXMI Launcher Config.json"))));
        var root=doc.RootElement;var imp=root.GetProperty("Importers").GetProperty("EFMI").GetProperty("Importer");
        string Read(string key)=>imp.GetProperty(key).GetString()??"";
        if(root.GetProperty("Launcher").GetProperty("auto_update").GetBoolean())
            throw new InvalidDataException("原型需要先在 XXMI 關閉自動更新，避免受測版本在協同途中改變。");
        if(Read("game_launch")!="CUSTOM" || Read("start_method")!="NATIVE" || Read("xxmi_dll_inject_mode")!="DIRECT")
            throw new InvalidDataException("請依 Instructions.cmd 設定：「通用」的「游戏启动」=自定义、「启动方式」=Native；「EFMI」的「XXMI DLL 注入模式」=注入。启动方式僅在暫選「直接」時顯示。");
        if(Read("custom_launch")!=Command || string.IsNullOrWhiteSpace(Read("custom_launch_signature")))
            throw new InvalidDataException("請將 Instructions.cmd 顯示的完整命令貼到「通用」→「启动命令」，包含雙引號與 bridge；完成後正常退出 XXMI 再檢查。");
        if(!Path.GetFullPath(Read("game_folder")).Equals(Path.GetFullPath(GameFolder),StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("XXMI 遊戲資料夾與協同設定不符。");
        if(imp.GetProperty("process_timeout").GetInt32()<60) throw new InvalidDataException("請將「通用」→「游戏进程启动等待超时」設為 60 或更大。");
        foreach(string key in new[]{"run_pre_launch_enabled","run_post_load_enabled","extra_libraries_enabled"})
            if(imp.TryGetProperty(key,out var value)&&value.ValueKind==JsonValueKind.True)
                throw new InvalidDataException("原型請先在 XXMI 關閉額外命令／DLL："+key);
        string version=System.Diagnostics.FileVersionInfo.GetVersionInfo(XxmiExe).FileVersion??"";
        if(version!="2.4.9.0") throw new InvalidDataException("目前原型僅核對 XXMI 2.4.9.0，其他版本需重新評估。");
    }
}

