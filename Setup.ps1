# 單一 PowerShell 視窗：三個路徑、設定指引與重試檢查均由 C# 處理。
$ErrorActionPreference='Stop'
[Console]::InputEncoding=[Text.UTF8Encoding]::new($false)
[Console]::OutputEncoding=[Text.UTF8Encoding]::new($false)
try {
    & (Join-Path $PSScriptRoot 'app\GugaLauncher.exe') setup
    # 外部程式失敗不會自動成為 PowerShell 例外，必須檢查結束碼。
    & (Join-Path $PSScriptRoot 'New-GugaShortcut.ps1') -Root $PSScriptRoot -SetupExitCode $LASTEXITCODE
}
catch { Write-Host ('設定未完成：'+$_.Exception.Message) -ForegroundColor Red }
Write-Host '按任意鍵結束視窗。' -ForegroundColor Yellow
[void][Console]::ReadKey($true)