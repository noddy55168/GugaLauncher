# GugaLauncher — GugaEFMI User Guide

[繁體中文](README.md) | [简体中文](README.zh-CN.md) | English

GugaLauncher simplifies the manual steps needed to operate **XXMI/EFMI and the official GRYPHLINK launcher**, combining them into a one-click launch workflow.

After initial setup, double-click **GugaEFMI** and approve the Windows administrator prompt when requested. The tool coordinates the official launcher and the original XXMI/EFMI to start Endfield, leaving mod loading to EFMI.

GugaLauncher is written in C# and handles pre-launch checks and process coordination. The original XXMI/EFMI still prepares and injects mods. This tool does not include the game, XXMI, EFMI, or mods, and is not an official game launcher.

> [!CAUTION]
> **GugaLauncher is an unofficial personal research project provided free of charge by its author.** It simplifies the manual steps involved in using XXMI/EFMI and the official launcher into a one-click coordinated workflow.
>
> This project is not affiliated with or officially endorsed by the developers or publishers of Arknights: Endfield, or by the GRYPHLINK or XXMI/EFMI projects.
>
> The tool is provided as is. **Compatibility with current or future versions of the game, official launcher, XXMI/EFMI, or mods is not guaranteed.** There is no commitment to continued maintenance or technical support. Successful testing does not guarantee operation in every environment or imply official approval.
>
> Use a legally obtained game client. Game content, trademarks, icons, third-party tools, and mods belong to their respective rights holders. This project grants no rights to use or redistribute those materials; their respective licenses and applicable terms still apply.

## Licensing and third-party rights

The project's existing license remains unchanged; see the included `LICENSE`. Third-party software and the .NET Runtime retain their own licenses and copyright notices; see `runtime-licenses/` and the respective project documentation.

“Personal research” describes the project's development purpose, and “provided free of charge” describes the author's distribution policy. **These statements add no restrictions on commercial use, sale, or redistribution**, and do not reduce rights granted by the existing license. The tool's license does not relicense game assets, trademarks, icons, or mods.

> [!NOTE]
> This repository contains source code and scripts, not compiled executables or the .NET Runtime. Players should download the complete ZIP from GitHub Releases before following the setup instructions below. Do not run launch scripts directly from the source checkout.

## Before you start

- 64-bit Windows.
- Endfield and the official **GRYPHLINK** launcher installed.
- **XXMI 2.4.9** installed, with EFMI and your desired mods already prepared. This release checks for executable version `2.4.9.0`; other versions are currently rejected.
- Ability to approve Windows administrator prompts.

The package includes the .NET Runtime. **You do not need to install Python or .NET separately.**

Extract the entire ZIP to a permanent location before setup. Do not run files inside the ZIP or extract only the executable.

## Initial setup

### 1. Enter three installation folders

Double-click **`Setup.cmd`** in the main folder. Enter the following in the PowerShell window:

| Item | Example absolute path | How to identify the folder |
| --- | --- | --- |
| XXMI | `C:\XXMI Launcher` | Contains `Resources` and `XXMI Launcher Config.json`; the executable is under `Resources\Bin` |
| Endfield | `C:\Arknights Endfield` | Directly contains `Endfield.exe` |
| GRYPHLINK | `C:\GRYPHLINK` | Contains `Launcher.exe` and the official version subfolders |

**These are examples, not default installation locations.** Enter your actual folders. Do not select an executable, `Endfield_Data`, or `Mods`. You can copy the full path from File Explorer; enclosing quotation marks may remain.

Pressing Enter with an empty path cancels setup. Once all three paths pass the file checks, they are saved automatically.

### 2. Configure XXMI as instructed

Keep the setup window open. Open XXMI, select **EFMI**, and open the gear icon **Settings (“设置”)**. The Chinese labels below match the XXMI interface used for this release; English descriptions explain their meaning.

Under **General (“通用”) → Launch settings (“启动设置”)**:

1. Temporarily set game launch (“游戏启动”) to Direct (“直接”).
2. Set the launch method (“启动方式”) that appears to **Native**.
3. Change game launch back to **Custom (“自定义”)**. The launch-method field will disappear; this is expected.
4. Paste the **entire command shown by the GugaLauncher setup window** into the launch-command field (“启动命令”). Keep the quotation marks and the final `bridge` argument. Do not substitute `Start.cmd` or `XXMI Launcher.exe`.
5. Set the game-process launch timeout (“游戏进程启动等待超时”) to **60 seconds or more**.
6. Leave the custom game-process executable name (“自定义游戏进程可执行文件名称”) **unchecked**, retaining `Endfield.exe`.

Configure the other tabs as follows:

| Tab | Field | Required setting |
| --- | --- | --- |
| EFMI | XXMI DLL injection mode (“XXMI DLL 注入模式”) | Injection (“注入”; Direct in English) |
| Launcher (“启动器”) | Automatic updates (“自动更新”) | Uncheck |
| Advanced (“高级”) | Pre-launch command, post-load command, injection libraries (“启动前命令”, “加载后命令”, “注入库”) | Disable; existing text does not need to be deleted |

XXMI's game folder must match the folder entered in step 1. These are the current compatibility requirements. GugaLauncher does not modify XXMI's settings for you.

### 3. Press Enter to check and finish

1. Close XXMI's settings window and **exit XXMI normally**. If it remains in the system tray, exit it there.
2. Return to the original PowerShell window and press **Enter**.
3. If problems are reported, correct them and press Enter again. You do not need to re-enter the three paths. Enter `Q` to cancel the check; saved paths are retained.
4. After **OK**, setup automatically creates **`GugaEFMI.lnk`** in the main folder and applies the Endfield icon.
5. Press any key to close the setup window.

The shortcut icon comes from your installed XXMI. If it cannot be found, setup reports the missing file and retains your settings. This process **does not modify the original `EFMI Quick Start` shortcut**.

## Everyday use

**Make sure the game and XXMI have exited normally → double-click `GugaEFMI` → approve the administrator prompt → wait for the game.**

- Settings are retained; you do not need to run Setup each time.
- Official GRYPHLINK may remain open. If it is not running, the tool attempts to start it. Complete any official login or update requirements first.
- Use **GugaEFMI** to enter this coordinated workflow. The original XXMI can manage settings, but its launch button and the original EFMI Quick Start do not establish the prerequisite session required by this release.
- If an existing game or XXMI process is detected, the tool stops and reports it instead of forcibly ending it.

### Why do PowerShell windows and an administrator prompt appear?

GugaEFMI uses PowerShell to start the coordinator. The current workflow needs administrator privileges to access the official process, so Windows displays a **UAC** prompt.

After approval, the regular window hands off to an administrator window and exits. You may briefly see two windows. **The launch window closes automatically after the game process has been created.** On failure or canceled elevation, messages remain available to read.

A closed window indicates that process creation has completed; it does not independently confirm that mods display correctly. Check the game itself.

### What if antivirus software displays a warning?

PowerShell, administrator privileges, unsigned files, and the DLL injection used by the original EFMI to load mods may attract security-software attention. **The tool's purpose alone does not establish that a warning is a false positive.**

Verify the file source and note the detection name and blocked file path. If uncertain, stop and retain the message. This tool does not require disabling antivirus protection or excluding an entire drive.

Official references: [Windows UAC](https://learn.microsoft.com/en-us/windows/security/application-security/application-control/user-account-control/settings-and-configuration) and [Microsoft file analysis and suspected false-positive submissions](https://www.microsoft.com/en-us/wdsi/filesubmission/?persona=HomeUser). This tool does not submit files automatically.

## Troubleshooting

| Situation | What to do |
| --- | --- |
| Required files not found | Check the folder characteristics in step 1; do not select an executable or the wrong folder level |
| XXMI or the game is already running | Exit normally before retrying; avoid repeated clicks |
| Prerequisite check session is missing | Use this release's GugaEFMI, not `bridge` directly, the original launch button, or the original Quick Start |
| Settings validation fails | Correct the reported fields, save and exit XXMI, then press Enter in the original setup window |
| Official launcher is not ready | Complete official login or updates, then retry; do not alter process permissions or disable protection |
| Need the setup command again | Run `Instructions.cmd`; it generates the command for the current program location |
| GugaLauncher folder was moved | Run Setup again, update the XXMI command, and regenerate the shortcut |
| An error appears | Retain the complete message; redact personal paths and account information before sharing logs |

## Files and privacy

| File or folder | Purpose |
| --- | --- |
| `Setup.cmd` | Initial setup, Enter-to-recheck workflow, and shortcut generation |
| `GugaEFMI.lnk` | Everyday shortcut generated after successful setup |
| `Start.cmd` | Alternative entry point for the same coordinated workflow |
| `Instructions.cmd` | Displays configuration instructions and the command again |
| `Check.cmd` | Checks only; does not start the game |
| `Offline-Tests.cmd` | Offline development/troubleshooting tests; not a launch entry point |
| `app/` | Program and bundled .NET Runtime; retain the entire folder |
| `src/` | C# source code |
| `PACKAGE-MANIFEST.json` | SHA256 inventory of initial package files; not a digital signature |
| `LICENSE`, `runtime-licenses/` | License and third-party notices |

The clean package contains no player paths, accounts, mods, logs, or configured shortcuts. Files created after setup, including `app/settings.json`, configuration backups, `app/local-errors.log`, and shortcuts, may contain local paths. **Share the original distribution ZIP, not a newly zipped copy of your used folder.**

GugaLauncher does not move the original Mods folder. The original XXMI/EFMI still performs its own necessary configuration saving and mod processing.

## Scope of validation

The coordinated launch and injection workflow was tested locally, and the user confirmed that the game and mods worked. Subsequent window-exit behavior, setup prompts, and shortcut generation received offline or isolated checks. This does not mean every computer, game update, or XXMI version has been validated.

Duplicate-launch checks cover this tool's own coordinated entry point. The original XXMI behavior has not been modified. Other entry points, or launching the game from another program during startup, may bypass these checks. Use only one launch entry point at a time.