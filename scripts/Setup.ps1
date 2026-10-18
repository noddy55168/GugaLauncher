# 單一 PowerShell 視窗：三個路徑、設定指引與重試檢查均由 C# 處理。
$ErrorActionPreference='Stop'
# 腳本位於 scripts；應用程式與玩家設定仍位於專案根目錄的 app。
$Root=Split-Path -Parent $PSScriptRoot
[Console]::InputEncoding=[Text.UTF8Encoding]::new($false)
[Console]::OutputEncoding=[Text.UTF8Encoding]::new($false)
try {
 # 先檢查交付檔案，再進行設定或提權，避免原始碼下載被誤當作可執行包。
 $appPath=Join-Path $Root 'app\GugaLauncher.exe'
 if(!(Test-Path -LiteralPath $appPath -PathType Leaf)) {
   throw '缺少 app\GugaLauncher.exe。若下載的是 GitHub 原始碼，請先建置，或改從 Releases 下載完整程式 ZIP 並全部解壓。建置方式請見 docs/repository-layout.md。'
 }
    & $appPath setup
    # 外部程式失敗不會自動成為 PowerShell 例外，必須檢查結束碼。
    & (Join-Path $PSScriptRoot 'New-GugaShortcut.ps1') -Root $Root -SetupExitCode $LASTEXITCODE
}
catch { Write-Host ('設定未完成：'+$_.Exception.Message) -ForegroundColor Red }
Write-Host '按任意鍵結束視窗。' -ForegroundColor Yellow
# 重導向輸入（例如離線驗證）時不等待按鍵；一般玩家視窗仍保留提示。
if(![Console]::IsInputRedirected) { [void][Console]::ReadKey($true) }