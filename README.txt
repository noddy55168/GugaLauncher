目前玩家操作請以 README.md 為準；英文完整指南為 README.en.md，英文技術摘要為 README.en.txt。
以下保留歷史補充；其中手動以管理員啟動及 EFMI Quick Start.cmd 的舊說明不作為目前建議入口，請使用 GugaEFMI。主程式 GPLv3 與 runtime-licenses 的 .NET 第三方授權分別適用，未改用參考專案的 MIT 授權。

玩家使用說明請先閱讀同資料夾的 README.md（已核准之發布說明）。本檔保留補充與歷程內容。

簡易使用：Setup.cmd 輸入三個資料夾 → 依畫面設定 XXMI → 回原視窗按 Enter，修正直到 OK → 自動建立 GugaEFMI 捷徑 → 按任意鍵結束。
日常遊玩：雙擊主資料夾的 GugaEFMI 捷徑（Start.cmd 為備用入口）。原版 XXMI 可管理設定，其啟動按鈕不支援本版前置保護。

GugaLauncher C# 協同版（2026-10-17）
本版啟動流程已完成一次本機實測：原版 XXMI/EFMI 注入成功，使用者確認遊戲啟動且 mod 正常。此次交付僅修訂說明文字，未改變啟動邏輯；不同環境仍需自行驗證。

這不是另一套 EFMI。原 XXMI/EFMI 負責自己的 Mods、設定準備及注入；本工具只作前置檢查、官方程序準備與遊戲建立的協同。沒有 Python、EFMI DLL、mod 副本或注入器。

入口
Setup.cmd：輸入 XXMI、遊戲、官方安裝資料夾，保存 app/settings.json，檢查通過後產生 GugaEFMI 捷徑與圖示。顯示需貼入原 XXMI 的 CUSTOM 命令，不代改 XXMI。沒有帶入開發者路徑。
Check.cmd：唯讀檢查安裝位置、XXMI 設定及重複程序，不會開啟 XXMI、官方程序或遊戲。
Offline-Tests.cmd：執行假宿主測試，不啟動或終止程序。
Start.cmd：實際協同入口！會啟動原版 XXMI，符合所有條件後可建立遊戲。日常使用此入口，以滑鼠右鍵選「以系統管理員身分執行」。

設定步驟（對照 XXMI 2.4.9 中文介面；僅說明，不代改原設定）
1. 首次執行 Setup.cmd，在同一 PowerShell 視窗依序指定 XXMI、終末地遊戲、官方 GRYPHLINK 安裝資料夾。
   已完成 Setup 者不必重設。執行 Instructions.cmd 可重新顯示本版完整命令與說明，不會修改設定。
2. 在 XXMI 選擇 EFMI，開啟齒輪「设置」，左側選「通用」。
   在「启动设置」先將「游戏启动」暫選「直接」，把出現的「启动方式」選為 Native。
   再把「游戏启动」改回「自定义」。此時「启动方式」會隱藏，屬正常。
3. 「启动命令」貼上 Instructions.cmd 顯示的整行，格式為：
   "本版實際位置\app\GugaLauncher.exe" bridge
   上面是格式示意，不要照貼；請使用 Instructions.cmd 產生的實際路徑。
   雙引號與最後的 bridge 都要保留。不要填 Start.cmd 或 XXMI Launcher.exe。
   「游戏进程启动等待超时」填 60 或更大，單位為秒。
   「自定义游戏进程可执行文件名称」不勾選，保留預設 Endfield.exe。
4. 左側「EFMI」→「XXMI DLL 注入模式」選「注入」（英文 Direct）。
   注意：這與「通用」的「游戏启动」是兩個不同欄位；遊戲啟動最後必須是「自定义」。
5. 左側「启动器」→取消勾選「自动更新」。
   左側「高级」→取消「启动前命令」「加载后命令」「注入库」的啟用勾選。
   不必刪除原有命令文字；這是本原型的相容性限制。
   XXMI 遊戲資料夾須與 Setup 的遊戲資料夾相同；原型目前只接受 XXMI 2.4.9.0。
6. 關閉設定視窗，正常退出 XXMI，回到原設定視窗按 Enter 核對。若失敗，依提示修正後再次按 Enter；通過後顯示 OK，按任意鍵結束。
   若 XXMI 仍在系統匣，請從系統匣正常退出；Check 不會替你關閉任何程序。
   遊戲仍在執行時 Check 也會停止，不會強制關閉遊戲。
7. Check 通過後完成首次設定，下次不必重跑 Setup。
   日常啟動：雙擊本版 EFMI Quick Start.cmd 並核准 UAC，讓它先檢查再呼叫原版 XXMI/EFMI。
   不要直接按 XXMI 主畫面的「启动」，那會跳過本版前置防重複檢查。
   本版尚不能以原版「启动」按鈕達到同等防重複保護。

防重複啟動的範圍
前置入口在任何 XXMI 派送前檢查既有 Endfield／XXMI；官方預檢後再次檢查。單一使用者/session 共用鎖阻擋本工具的重複執行。回呼只允許本次 XXMI 繼承的單次憑證，使用同使用者 named pipe，並檢查回呼程序路徑；回呼前後再檢查遊戲。協同工具沒有終止程序、注入、改 ACL 或關閉保護的 API。
原版 XXMI 自身的先關閉遊戲行為未修改。直接按原版啟動鈕、從其他捷徑啟動、或外部程序恰於前置檢查與 XXMI 內部檢查間建立遊戲，仍可能繞過此保護。本原型不宣稱可跨原版未修改程式消除全部競態。若需要全系統絕對保證，必須修改原版 XXMI 的最早檢查位置，另行核准；不能只靠 CUSTOM 命令達成。

時序
官方冷啟動及原生權限預檢先完成，才用 --nogui --xxmi EFMI 進入原版流程。CUSTOM 只回呼本工具，不能獨立建立遊戲。回呼限制20秒，超時或 XXMI 已退出便拒絕建立；新遊戲使用 DX11。注入由原 EFMI 接手，工具只報建立成功，不把它當作注入／畫面驗收成功。
官方與 XXMI 的自然登入／更新或原版自身行為不由本工具接管。出錯保留程序，自己取得的 handle 會釋放；不強制回滾或重試。

內容與限制
app/：win-x64 自包含 .NET 10.0.12，玩家不需另裝 .NET。
src/：完整 C# 原始碼及專案，使用 .NET 10 SDK 建置。沒有引用原安裝的內部 Python 程式碼。
LICENSE：沿用既有研究程式 GPLv3 授權；Native.cs／ParentLease.cs 由既有 C# 研究程式分離，保留來源說明。runtime-licenses/ 為 .NET 聲明。
PACKAGE-MANIFEST.json：交付檔案 SHA256，用於核對意外變動，不是防篡改簽章。settings.json、備份與 local-errors.log 是日後本機內容，不在初始清單內。
此次封裝不啟動遊戲；先前本機實測已確認原版 XXMI/EFMI 接力、注入及 mod 顯示正常，其他電腦相容性未驗收。

解壓與首次使用
請將整個資料夾解壓到固定位置後再設定，不要在 ZIP 裡直接執行。
環境：Windows x64；已安裝官方 GRYPHLINK、遊戲及 XXMI 2.4.9。已附 .NET runtime，不需安裝 Python 或 .NET。
Setup 的三個輸入都是資料夾，不是 .exe。以下統一使用 C 槽絕對路徑作為範例，不是預設值，請填自己的實際安裝位置：
- XXMI 根目錄（範例 C:\XXMI Launcher）：其下應有 Resources/Bin/XXMI Launcher.exe 與 XXMI Launcher Config.json。
- 遊戲目錄（範例 C:\Arknights Endfield）：直接包含 Endfield.exe 的那一層。
- 官方啟動器目錄（範例 C:\GRYPHLINK）：包含 Launcher.exe 及版本子資料夾的那一層。
本包不附 mod；原版 EFMI 繼續使用自己的 Mods 與注入設定。
日常順序：確保遊戲及 XXMI 已正常退出 → 雙擊 EFMI Quick Start.cmd → 核准 UAC → 等待遊戲。
官方啟動器可保持開啟；未開啟時工具會嘗試開啟它。登入或更新尚未完成時，先在官方介面完成後再試。
錯誤處理：提示既有程序時請自行正常退出；設定錯誤時對照 Instructions.cmd；不要反覆點擊或強制結束遊戲。
搬動本資料夾後，需重新取得 Instructions.cmd 的命令並更新 XXMI「启动命令」。

隱私與設定
交付包不含 app/settings.json、設定備份、本機日誌、帳號資料、mod 或原 XXMI 設定。
app/settings.template.json 為空白範本；Setup 會產生自己的 settings.json，無須手動複製範本。
使用後產生的設定、備份和 local-errors.log 可能含個人路徑；分享時請使用原始乾淨 ZIP，不要直接壓縮使用中的資料夾。
已有可用版本的使用者可繼續使用原資料夾；本乾淨交付包不會覆蓋或繼承原設定。


捷徑生成：Setup 檢查成功後才建立或更新主資料夾的 GugaEFMI.lnk，並從玩家指定 XXMI 的 Themes/Default/Shortcuts/EFMI.ico 複製終末地圖示到本資料夾。取消或檢查失敗不生成；缺少圖示則明確報錯並保留設定。原版 EFMI Quick Start 不受影響。乾淨 ZIP 不含機器專屬 .lnk；首次設定時才依實際位置產生。搬移資料夾後需重新設定，以更新命令與捷徑。



視窗與提示顏色
首次設定依 1/3 路徑、2/3 XXMI 設定、3/3 檢查分組；青色為標題、黃色為必要操作、綠色為成功、紅色為錯誤。重導向輸出維持純文字。
點擊 GugaEFMI 後，一般 PowerShell 可能短暫出現；核准 UAC 後交接到管理員視窗，一般視窗立即退出，不再等待第二個視窗。
遊戲程序建立成功後管理員視窗自動關閉。這只表示啟動流程已交接，mod 與遊戲畫面仍由原版 EFMI／遊戲處理；失敗或取消提權則保留訊息供閱讀。
UAC 是 Windows 要求管理員權限的確認流程，本機協同需要存取官方程序，因此出現此提示是預期行為。
原版 EFMI 使用 DLL 注入載入 mod；PowerShell、提權或未簽章檔案可能觸發安全提示。沒有取得實際警示內容前，不能判定它只是誤報。請核對來源、偵測名稱與路徑；不需要停用防毒或排除整個磁碟。
參考：Microsoft UAC 說明 https://learn.microsoft.com/en-us/windows/security/application-security/application-control/user-account-control/settings-and-configuration
疑似誤報可依 Microsoft 官方說明送交分析（本工具不會自動提交檔案）：https://www.microsoft.com/en-us/wdsi/filesubmission/?persona=HomeUser



