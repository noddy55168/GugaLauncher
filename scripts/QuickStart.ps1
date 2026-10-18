# Quick Start 始終先進入協同前置檢查，不直接啟動 XXMI 或獨立 bridge。
$ErrorActionPreference='Stop'
# 腳本位於 scripts；應用程式與玩家設定仍位於專案根目錄的 app。
$Root=Split-Path -Parent $PSScriptRoot
[Console]::OutputEncoding=[Text.UTF8Encoding]::new($false)
try {
 # 先檢查交付檔案，再進行設定或提權，避免原始碼下載被誤當作可執行包。
 $appPath=Join-Path $Root 'app\GugaLauncher.exe'
 if(!(Test-Path -LiteralPath $appPath -PathType Leaf)) {
   throw '缺少 app\GugaLauncher.exe。若下載的是 GitHub 原始碼，請先建置，或改從 Releases 下載完整程式 ZIP 並全部解壓。建置方式請見 docs/repository-layout.md。'
 }
 $identity=[Security.Principal.WindowsIdentity]::GetCurrent()
 $admin=([Security.Principal.WindowsPrincipal]::new($identity)).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
 if(!$admin) {
   $arguments='-NoProfile -ExecutionPolicy Bypass -File "'+$PSCommandPath+'"'
   Write-Host '正在要求管理員權限（UAC）；核准後會交接到啟動視窗。' -ForegroundColor Yellow
   # 交接後結束此啟動視窗，不讓未提權視窗持續等待。
   Start-Process -FilePath "$PSHOME\powershell.exe" -Verb RunAs -ArgumentList $arguments
   exit
 }
 Write-Host 'GugaEFMI | 正在檢查並啟動遊戲…' -ForegroundColor Cyan
 Write-Host '遊戲程序建立成功後，本視窗會自動關閉；mod 由原版 EFMI 載入。' -ForegroundColor DarkGray
 & $appPath start --live
 # 只在成功結束時關閉；非零結束碼留住畫面，避免錯誤一閃而過。
 if($LASTEXITCODE -eq 0) { exit 0 }
 Write-Host '啟動未完成，請依上方訊息處理後再試。' -ForegroundColor Yellow
} catch { Write-Host ('無法啟動：'+$_.Exception.Message) -ForegroundColor Red }
Write-Host '按任意鍵結束視窗。'
# 重導向輸入（例如離線驗證）時不等待按鍵；一般玩家視窗仍保留提示。
if(![Console]::IsInputRedirected) { [void][Console]::ReadKey($true) }