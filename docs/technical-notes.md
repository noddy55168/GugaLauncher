# Technical notes / 技術說明

User instructions: [English](../README.md) · [繁體中文](../README.zh-TW.md) · [简体中文](../README.zh-CN.md).
For current script locations and build commands, see [repository layout](repository-layout.md).

## Responsibilities / 職責

GugaLauncher coordinates preflight checks, official-process readiness and game creation. Original XXMI/EFMI remains responsible for mod preparation and injection. This source repository contains neither an injector nor copies of EFMI, Python, the game or mods. The published v0.1.0 package bundles the win-x64 .NET 10.0.12 runtime; building the source requires the .NET 10 SDK.

GugaLauncher 負責前置檢查、官方程序就緒與遊戲建立；mod 準備和注入由原版 XXMI／EFMI 處理。倉庫不包含注入器、EFMI、Python、遊戲或 mod 副本。v0.1.0 發行包隨附 win-x64 .NET 10.0.12，原始碼建置需要 .NET 10 SDK。

## Coordinated flow / 協同流程

1. Check for existing Endfield and XXMI processes before dispatch. A per-user/session lock prevents duplicate coordinator runs.
2. Complete official-launcher startup and native permission preflight, then check again before invoking original XXMI with `--nogui --xxmi EFMI`.
3. XXMI's CUSTOM command calls back into the coordinator using a one-time credential inherited by that invocation. A same-user named pipe and callback process-path check scope the request to the current session.
4. The callback is limited to 20 seconds and rejects requests if XXMI has exited. Game checks also surround the callback. Game creation uses DX11; original EFMI then handles injection.

派送前先檢查既有 Endfield 與 XXMI，並以同使用者／session 的鎖限制重複協同執行。官方啟動與原生權限預檢完成後再次檢查，再呼叫原版 XXMI。CUSTOM 命令須帶有本次繼承的單次憑證，透過同使用者 named pipe 及回呼程序路徑檢查完成協同；回呼限時 20 秒，XXMI 已退出即拒絕。遊戲使用 DX11，由 EFMI 接手注入。

## Protection boundaries / 保護範圍

The coordinator does not terminate processes, inject DLLs, modify ACLs or disable protection. On error, it retains processes and releases handles it owns; it does not force rollback or retries. Official login/update requirements and XXMI's own behavior remain outside its control.

Original XXMI behavior is unchanged, including its own game-closing behavior. Other entry points or an external launch racing with the checks can bypass this protection. The coordinator does not guarantee elimination of all races across an unmodified external program. Successful process creation is not proof of successful mod injection or correct in-game display.

協同程式不終止程序、不注入 DLL、不修改 ACL 或停用保護；錯誤時保留程序並釋放自身 handle，不強制回滾或重試。官方登入／更新與原版 XXMI 自身行為不受接管。其他入口或同時發生的外部啟動仍可能繞過檢查；程序建立成功也不等同注入與畫面驗收成功。

## Setup and shortcuts / 設定與捷徑

Setup saves validated folder paths locally and displays the required XXMI configuration without changing XXMI settings. After successful validation, `scripts/Setup.ps1` calls `scripts/New-GugaShortcut.ps1`, creating the root-level GugaEFMI shortcut and copying its icon from the selected XXMI installation. Failure or cancellation skips shortcut generation; a missing icon is reported while saved settings remain. The original EFMI Quick Start shortcut is untouched.

The shortcut targets `scripts/QuickStart.ps1`. Repeat setup after moving the installation. Administrator elevation hands off between PowerShell windows; successful game-process creation closes the launch window. Errors remain available to read. Refer to the user guide for UAC and antivirus details.

設定會保存本機路徑並顯示 XXMI 指引，不代改 XXMI。檢查成功才生成主資料夾的 GugaEFMI 捷徑與圖示；失敗或取消不生成，圖示缺漏會報錯並保留設定。原版快捷方式不變。搬移位置後需重跑設定。提權會交接 PowerShell 視窗；成功建立遊戲程序後關閉啟動視窗，錯誤則保留訊息。

## License and provenance / 授權與來源

The existing [GPLv3 license](../LICENSE) remains unchanged. `Native.cs` and `ParentLease.cs` were separated from the earlier C# research implementation; this source attribution is retained. The .NET runtime and its dependencies retain the notices in [runtime-licenses](../runtime-licenses). Their licenses apply to those components and do not replace the project's GPLv3 license. No new commercial-use restriction is introduced.

主程式沿用既有 GPLv3。Native.cs 與 ParentLease.cs 由先前 C# 研究程式分離，保留來源說明；.NET 與依賴保留各自授權，不取代主程式 GPLv3，也沒有新增禁止商用條款。

## Validation and privacy / 驗證與隱私

Historical testing includes a successful local coordinated launch with working mods, 44 offline assertions, PowerShell syntax checks and isolated shortcut/window-handoff checks. These are historical results, not tests rerun by this documentation cleanup, and do not establish compatibility on every machine or future version.

Only the blank settings template is tracked. Player settings, logs, shortcuts, icons, build outputs and original local documentation records are excluded from Git. Distribute the clean release package, not a used installation folder. Package hashes detect changes but are not digital signatures.

歷史紀錄包含成功啟動及 mod 顯示、44 項離線斷言、腳本語法及隔離捷徑／視窗交接檢查；不表示本次文件整理重跑了這些測試，也不保證其他環境或未來版本相容。Git 僅保留空白設定範本，本機設定、日誌、捷徑、圖示、建置產物與原始紀錄不提交。分享請使用乾淨 Release，雜湊不是數位簽章。