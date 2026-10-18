# GugaLauncher — GugaEFMI 使用說明

[English](README.md) | 繁體中文 | [简体中文](README.zh-CN.md)

GugaLauncher 的設計目的，是將原本需要手動操作 **XXMI／EFMI 與官方 GRYPHLINK 啟動器**的步驟簡化，整合成一鍵啟動流程。

完成首次設定後，只要雙擊 **GugaEFMI**，並在 Windows 要求時核准管理員權限，工具就會自動協調官方啟動器與原版 XXMI／EFMI，啟動終末地並交由 EFMI 載入 mod，減少每次遊玩前的重複操作。

GugaLauncher 使用 C# 開發，負責啟動前檢查與程序協調；mod 的準備與注入仍由原版 XXMI／EFMI 處理。本工具不包含遊戲、XXMI、EFMI 或 mod，亦非遊戲官方啟動器。

> [!CAUTION]
> **GugaLauncher 是非官方的個人研究專案，由作者免費提供**，旨在簡化 XXMI／EFMI 與官方啟動器的手動操作，實現一鍵協同啟動。
>
> 本專案與《明日方舟：終末地》的開發商、發行商，以及 GRYPHLINK、XXMI／EFMI 專案均無隸屬關係，亦未獲其官方背書。
>
> 本工具按現況提供，**不保證與目前或未來版本的遊戲、官方啟動器、XXMI／EFMI 或 mod 相容**，也不承諾持續維護或技術支援。既有測試成功不代表所有環境均可正常運作，也不代表獲得官方允許。
>
> 請使用合法取得的遊戲客戶端。遊戲內容、商標、圖示、第三方工具及 mod 的權利均屬各自權利人；本專案不授予這些內容的使用或再散布權利，使用時仍須遵守其各自的授權與適用條款。

## 授權與第三方權利

本專案維持現有授權，詳見隨附的 `LICENSE`；第三方程式與 .NET Runtime 保留各自的授權及著作權聲明，詳見 `runtime-licenses/` 與各專案文件。

「個人研究」描述本專案的開發目的，「免費提供」描述作者的發布方式，**不新增禁止商用、販售或再散布的限制**，亦不縮減既有授權授予的權利。遊戲素材、商標、圖示與 mod 不因本工具的授權而取得再授權。

> [!NOTE]
> 此倉庫提供原始碼與腳本，不包含已編譯程式或 .NET Runtime。一般玩家請下載 GitHub Releases 中的完整 ZIP，再依下列步驟設定；不要直接執行原始碼倉庫中的啟動腳本。

## 使用前準備

- Windows 64 位元環境。
- 已安裝終末地及官方 **GRYPHLINK** 啟動器。
- 已安裝 **XXMI 2.4.9**，並完成 EFMI 與所需 mod 的準備。本版會檢查 XXMI 執行檔版本 `2.4.9.0`，其他版本目前不接受。
- 啟動時可以核准 Windows 管理員權限提示。

交付包已包含 .NET Runtime，**不需要另外安裝 Python 或 .NET**。

先將整個 ZIP 解壓到固定位置，再開始設定。不要直接在壓縮檔內執行，也不要只取出其中的 `.exe`。

## 首次設定

### 1. 輸入三個安裝資料夾

雙擊主資料夾中的 **`Setup.cmd`**，在開啟的 PowerShell 視窗依序輸入：

| 項目 | 絕對路徑範例 | 如何確認位置 |
| --- | --- | --- |
| XXMI | `C:\XXMI Launcher` | 包含 `Resources` 資料夾與 `XXMI Launcher Config.json`；執行檔位於 `Resources\Bin` |
| 終末地 | `C:\Arknights Endfield` | 直接包含 `Endfield.exe` 的那一層 |
| GRYPHLINK | `C:\GRYPHLINK` | 包含 `Launcher.exe` 與官方版本子資料夾 |

**以上只是範例，不是預設安裝位置。** 請填入自己的實際資料夾，不要選 `.exe`，也不要選 `Endfield_Data` 或 `Mods`。可從檔案總管複製完整路徑；包住路徑的雙引號可以保留。

輸入路徑時留空按 Enter 會取消。三個路徑通過檔案檢查後會自動儲存。

### 2. 依提示設定 XXMI

先保留設定視窗，開啟 XXMI、選擇 **EFMI**，再進入齒輪 **「设置」**。以下名稱對應 XXMI 中文介面。

**「通用」→「启动设置」**

1. 將「游戏启动」暫時選為「直接」。
2. 把出現的「启动方式」設為 **Native**。
3. 再將「游戏启动」改回 **「自定义」**。此時「启动方式」會隱藏，屬正常顯示行為。
4. 將 GugaLauncher 設定視窗顯示的**整行命令**貼入「启动命令」。保留雙引號與最後的 `bridge`；不要改填 `Start.cmd` 或 `XXMI Launcher.exe`。
5. 「游戏进程启动等待超时」填 **60** 或更大，單位為秒。
6. 「自定义游戏进程可执行文件名称」維持**不勾選**，保留預設 `Endfield.exe`。

其他頁籤請依下表設定：

| 頁籤 | 欄位 | 設定 |
| --- | --- | --- |
| EFMI | XXMI DLL 注入模式 | 選「注入」（英文 Direct） |
| 启动器 | 自动更新 | 取消勾選 |
| 高级 | 启动前命令、加载后命令、注入库 | 取消啟用；不必刪除原有文字 |

XXMI 的遊戲資料夾須與第一步輸入的位置相同。以上是目前版本的相容性要求；GugaLauncher 不會代你改寫 XXMI 設定。

### 3. 按 Enter 檢查並完成

1. 關閉 XXMI 設定視窗並**正常退出 XXMI**。若仍在系統匣，請從系統匣退出。
2. 回到原本的 PowerShell 視窗，按 **Enter** 檢查。
3. 若顯示問題，依提示修正後再次按 Enter；不必重輸三個路徑。輸入 `Q` 可取消檢查，已儲存路徑會保留。
4. 顯示 **OK** 後，程式會在主資料夾自動建立 **`GugaEFMI.lnk`**，並套用終末地圖示。
5. 按任意鍵結束設定視窗。

捷徑圖示取自你安裝的 XXMI；找不到圖示時會提示缺少檔案，設定仍會保留。此流程**不修改原版 `EFMI Quick Start` 捷徑**。

## 日常啟動

**確認遊戲與 XXMI 已正常退出 → 雙擊 `GugaEFMI` → 核准管理員提示 → 等待遊戲。**

- 設定會保留，下次不必重跑 Setup。
- 官方 GRYPHLINK 可以保持開啟；未開啟時工具會嘗試啟動它。若需要登入或更新，請先完成官方流程。
- 要使用本版協同流程，請從 **GugaEFMI** 進入。原版 XXMI 可用來管理設定，但其「启动」按鈕與原版 EFMI Quick Start 不會建立本版所需的前置會話。
- 若偵測到既有遊戲或 XXMI，本工具會停止並提示，不替你強制結束它們。

### 為什麼會看到 PowerShell 與管理員提示？

GugaEFMI 透過 PowerShell 啟動協同程式；目前協同流程需要管理員權限存取官方程序，因此會出現 Windows **UAC** 提示。

核准後，一般視窗會交接到管理員視窗並退出，因此可能短暫看到兩個視窗。**遊戲程序建立成功後，啟動視窗會自動關閉**；若失敗或取消提權，會保留訊息供閱讀。

視窗關閉代表程序建立流程完成，並不單獨證明 mod 已正常顯示；請以遊戲畫面確認。

### 防毒出現警示時怎麼辦？

PowerShell、管理員權限、未簽章檔案，以及原版 EFMI 用來載入 mod 的 DLL 注入行為，都可能引起安全軟體注意。但**不能僅因工具用途就認定所有警示都是誤報**。

請確認檔案來源，記下偵測名稱與被攔截的檔案路徑；不確定時先停止並保留訊息。使用本工具不需要停用防毒，也不應將整個磁碟加入排除。

相關官方說明：[Windows UAC](https://learn.microsoft.com/en-us/windows/security/application-security/application-control/user-account-control/settings-and-configuration)、[Microsoft 檔案分析與疑似誤報提交](https://www.microsoft.com/en-us/wdsi/filesubmission/?persona=HomeUser)。本工具不會自動提交檔案。

## 常見問題

| 情況 | 處理方式 |
| --- | --- |
| 找不到必要檔案 | 依第一步的資料夾特徵重新確認，不要選到執行檔或下一層資料夾 |
| 提示 XXMI 或遊戲已在執行 | 正常退出後再試；不要連續重複點擊 |
| 提示缺少前置檢查會話 | 改用本版 GugaEFMI，不直接執行 `bridge`、原版啟動按鈕或原版 Quick Start |
| 設定檢查不通過 | 依錯誤提示核對欄位，儲存並退出 XXMI 後，在原設定視窗按 Enter 重查 |
| 官方啟動器未就緒 | 先完成官方登入或更新，再重試；不要自行修改程序權限或停用保護 |
| 想再看一次設定命令 | 執行 `Instructions.cmd`；命令會依目前程式位置產生 |
| 搬動了 GugaLauncher 資料夾 | 重新執行 Setup，更新 XXMI 命令並重新生成捷徑 |
| 視窗顯示錯誤 | 保留完整訊息供排查；提供日誌前先遮蔽個人路徑與帳號資訊 |

## 原始碼倉庫結構

請參閱[目錄與建置說明](docs/repository-layout.md)。輔助腳本收納於 scripts/，技術與來源說明統一收納於 [docs/technical-notes.md](docs/technical-notes.md)。下方檔案表描述已發布的 v0.1.0 ZIP，其結構維持不變；本倉庫的離線測試入口則為 scripts/Offline-Tests.cmd。 本倉庫的備用啟動入口已收納至 scripts/Start.cmd；首次使用請執行 Setup.cmd，日常使用生成的 GugaEFMI 捷徑。

## 檔案用途與隱私

| 檔案／資料夾 | 用途 |
| --- | --- |
| `Setup.cmd` | 首次設定、按 Enter 重查、生成捷徑 |
| `GugaEFMI.lnk` | 設定成功後產生的日常啟動捷徑 |
| `Start.cmd` | 相同協同流程的備用啟動入口 |
| `Instructions.cmd` | 重新顯示設定指引與命令 |
| `Check.cmd` | 只檢查，不啟動遊戲 |
| `Offline-Tests.cmd` | 開發／排查使用的離線測試，不是啟動入口 |
| `app/` | 程式及隨附 .NET Runtime；請整個保留 |
| `src/` | C# 原始碼 |
| `PACKAGE-MANIFEST.json` | 初始交付檔案的 SHA256 清單；不是數位簽章 |
| `LICENSE`、`runtime-licenses/` | 授權與第三方聲明 |

乾淨交付包不包含玩家路徑、帳號、mod、日誌或已設定的捷徑。首次設定後產生的 `app/settings.json`、設定備份、`app/local-errors.log` 與捷徑可能包含本機路徑，**分享時請使用原始交付 ZIP，不要直接壓縮已使用的資料夾**。

GugaLauncher 不搬移原版 Mods；原版 XXMI／EFMI 仍會依自身設定進行必要的設定保存與 mod 處理。

## 適用範圍

協同啟動與注入流程已完成本機實測，使用者確認遊戲與 mod 正常。後續的視窗退出、設定提示與捷徑生成已做離線或隔離驗證；這不代表所有電腦、遊戲更新或 XXMI 版本均已驗收。

本工具的防重複檢查只涵蓋自己的協同入口。原版 XXMI 的行為沒有被修改；直接使用其他入口，或在啟動途中同時從其他程式啟動遊戲，仍可能繞過檢查。啟動時請只使用一個入口。

