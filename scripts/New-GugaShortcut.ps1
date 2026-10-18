param(
    [Parameter(Mandatory=$true)][string]$Root,
    [Parameter(Mandatory=$true)][int]$SetupExitCode
)
# 只有 C# 設定與驗證成功後才產生本程式的捷徑；完全不搜尋或修改原版捷徑。
$ErrorActionPreference='Stop'
if($SetupExitCode -ne 0) { return }
$Root=[IO.Path]::GetFullPath($Root)
$config=Get-Content -LiteralPath (Join-Path $Root 'app\settings.json') -Raw -Encoding UTF8 | ConvertFrom-Json
$sourceIcon=Join-Path $config.XxmiFolder 'Themes\Default\Shortcuts\EFMI.ico'
$launcher=Join-Path $Root 'scripts\QuickStart.ps1'
$icon=Join-Path $Root 'GugaEFMI.ico'
$link=Join-Path $Root 'GugaEFMI.lnk'
foreach($file in @($sourceIcon,$launcher)) {
    if(!(Test-Path -LiteralPath $file -PathType Leaf)) { throw "無法建立 GugaEFMI，缺少檔案：$file。設定已保留。" }
}
# 引號不允許進入 -File 的雙引號包圍參數；一般 Windows 檔名本身不允許雙引號。
if($launcher.Contains('"')) { throw '程式位置含不支援的雙引號。' }
Copy-Item -LiteralPath $sourceIcon -Destination $icon -Force
$shell=New-Object -ComObject WScript.Shell
$shortcut=$shell.CreateShortcut($link)
$shortcut.TargetPath=Join-Path $env:SystemRoot 'System32\WindowsPowerShell\v1.0\powershell.exe'
$shortcut.Arguments='-NoProfile -ExecutionPolicy Bypass -File "'+$launcher+'"'
$shortcut.WorkingDirectory=$Root
$shortcut.IconLocation=$icon+',0'
$shortcut.Description='GugaEFMI：透過 C# 協同入口啟動原版 EFMI'
$shortcut.WindowStyle=1
$shortcut.Save()
$check=$shell.CreateShortcut($link)
if($check.TargetPath -ne $shortcut.TargetPath -or $check.Arguments -ne $shortcut.Arguments -or $check.IconLocation -ne $shortcut.IconLocation) {
    throw 'GugaEFMI 捷徑讀回檢查失敗，設定已保留。'
}
Write-Host '已建立 GugaEFMI 捷徑（終末地圖示）。日常遊玩請雙擊主資料夾內的 GugaEFMI。' -ForegroundColor Green