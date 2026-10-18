# Repository layout and build / 倉庫結構與建置

This document describes the reorganized source repository. The published v0.1.0 ZIP keeps its original layout; do not copy individual files from this checkout over an installed release.

本文件描述整理後的原始碼倉庫。已發布的 v0.1.0 ZIP 保持原結構；不要把此倉庫的單一腳本覆蓋到已安裝的 Release。

## Layout / 目錄

```text
README.md                  English homepage
README.zh-TW.md             繁體中文
README.zh-CN.md             简体中文
LICENSE                    Existing GPLv3 license (unchanged)
Setup.cmd                  Initial setup entry
Check.cmd                  Read-only configuration/process check
Instructions.cmd           Display configuration instructions
app/
  settings.template.json   Blank template; generated application files are ignored
src/                       C# application sources and project
scripts/
  Start.cmd                Backup launch entry; normally use GugaEFMI
  Setup.ps1                Setup orchestration
  QuickStart.ps1           Elevation and coordinated launch
  New-GugaShortcut.ps1     Generate the local GugaEFMI shortcut
  Offline-Tests.cmd        Run the compiled application's offline tests
runtime-licenses/          .NET and third-party notices
docs/
  repository-layout.md     This document
  technical-notes.md       Technical boundaries, provenance and validation scope
```

The duplicate English README is consolidated into README.md. The duplicate EFMI Quick Start.cmd wrapper is consolidated into scripts/Start.cmd. The original XXMI shortcut is not part of this repository and is not modified.

英文文件統一使用 README.md；重複的 EFMI Quick Start.cmd 合併為 scripts/Start.cmd。這不是修改玩家原版 XXMI 的 EFMI Quick Start 捷徑。

## Build / 建置

Developers need the .NET 10 SDK on Windows x64. From the repository root:

開發者需使用 Windows x64 與 .NET 10 SDK，在倉庫根目錄執行：

```powershell
dotnet publish .\src\GugaLauncher.csproj -c Release -r win-x64 --self-contained true -o .\app
```

Build only in a clean source checkout, not over an existing player installation. Generated executable/runtime files and local settings under app/ are ignored by Git; the blank template remains tracked. Players should use the complete ZIP from Releases instead of building.

請在乾淨原始碼目錄建置，不要覆蓋使用中的玩家安裝。app/ 的生成程式、Runtime 與本機設定由 .gitignore 排除，僅保留空白範本。一般玩家請下載 Releases 的完整 ZIP。

After building, the offline tests can be run without starting the game:

建置後，可執行不啟動遊戲的離線測試：

```powershell
.\scripts\Offline-Tests.cmd
```

Setup.cmd and scripts/Start.cmd are live user workflows, not offline tests. They are not run as part of this structural cleanup.

Setup.cmd 與 scripts/Start.cmd 是實際使用入口，不是離線測試；本次結構整理沒有執行它們。

## Path contract / 路徑規則

- Root CMD entry points locate scripts relative to their own file location. The backup scripts/Start.cmd calls QuickStart.ps1 in the same folder.
- PowerShell helpers derive the repository root from the parent of scripts/; the current working directory does not determine the app location.
- Setup passes that root to the shortcut generator. The shortcut and icon remain at the root, and the shortcut targets scripts/QuickStart.ps1.
- app/settings.json and logs remain beside the application executable; the C# implementation is unchanged.
- scripts/Offline-Tests.cmd resolves the application through ../app/.

根目錄 CMD 依自身位置尋找腳本；PowerShell 由 scripts/ 的上一層定位根目錄。設定、捷徑與圖示的存放規則維持不變，C# 程式不變。日後若重新製作發行包，必須完整保留 scripts/ 結構並重新產生捷徑。

## Technical reference / 技術參考

See [technical notes](technical-notes.md) for the coordination flow, protection boundaries, license provenance and historical validation scope. User setup instructions are maintained only in the three language READMEs.

協同流程、保護範圍、授權來源與歷史驗證摘要統一收納於[技術說明](technical-notes.md)。玩家操作步驟只維護於三語 README，避免重複或過時指引。