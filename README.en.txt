GugaLauncher C# — Technical and Historical Supplement (English)

For current player instructions, read README.en.md (English) or README.zh-TW.md (Traditional Chinese). README.txt preserves earlier Chinese notes. Older references there to manual administrator launch or EFMI Quick Start.cmd are historical; the current recommended entry point is the generated GugaEFMI shortcut.

This English companion summarizes the technical and historical material in README.txt. Current setup steps and all user-facing notices are translated in full in README.en.md.

PURPOSE AND COMPONENTS
The original XXMI/EFMI owns Mods, configuration preparation, and injection. GugaLauncher performs preflight checks, official-process preparation, and coordinated game creation. It contains no Python environment, EFMI DLL, mod copies, or injector.
app/ contains a self-contained win-x64 .NET 10.0.12 application.
src/ contains the C# sources and project, built with the .NET 10 SDK.

ENTRY POINTS AND CONFIGURATION
Setup.cmd saves the three folders to app/settings.json, displays the CUSTOM command for XXMI, validates settings, and generates the GugaEFMI shortcut and icon after success. It does not edit XXMI configuration.
Check.cmd inspects installation paths, XXMI settings, and existing processes without starting XXMI, official processes, or the game.
Offline-Tests.cmd runs fake-host tests without starting or ending processes.
Start.cmd is the alternative coordinated launch entry point.
Instructions.cmd displays the current instructions without changing settings.
The complete setup workflow, exact Chinese UI labels, and example paths are in README.en.md. Examples are not default values.
The game-launch setting must end at Custom; its Native method is selected before switching back to Custom. EFMI injection mode Direct is a separate setting.
XXMI executable version 2.4.9.0 is required. Automatic updates and advanced pre-launch/post-load/injection-library options must be disabled for this prototype's compatibility requirements.

DUPLICATE-LAUNCH PROTECTION
The coordinator checks for existing Endfield and XXMI processes before dispatch and again after official preflight. A per-user/session lock blocks duplicate coordinator runs. The callback requires a one-time credential inherited by this invocation of XXMI, uses a same-user named pipe, and verifies the callback process path. Further game checks surround the callback.
The coordinator has no API to terminate processes, inject DLLs, change ACLs, or disable protection.
Original XXMI behavior, including its own game-closing behavior, is unchanged. Other launch entry points and external launches racing with checks may bypass protection. This prototype does not claim to eliminate every race across an unmodified external application.

SEQUENCE
Official cold start and native permission preflight complete before original XXMI is invoked with --nogui --xxmi EFMI.
CUSTOM calls back into this tool and cannot independently create the game. The callback expires after 20 seconds and is rejected if XXMI has exited. The new game uses DX11. Original EFMI performs injection.
Successful process creation alone is not proof of successful injection or correct in-game display.
Official login/update requirements and XXMI's own behavior remain outside this coordinator's control. On errors, processes are retained and owned handles are released; there is no forced rollback or retry.

SHORTCUTS AND WINDOWS
Setup.ps1 calls New-GugaShortcut.ps1 only after successful C# setup validation. It creates or updates GugaEFMI.lnk in the main folder and copies the icon from Themes/Default/Shortcuts/EFMI.ico under the selected XXMI installation.
Cancellation or failed checks do not generate the shortcut. Missing icons produce an error while retaining settings. The original EFMI Quick Start is untouched.
The clean package includes no machine-specific shortcut or extracted icon; setup generates these locally. Repeat setup after moving the folder.
The regular PowerShell window may appear briefly before handing off to an administrator window. On successful game creation, the administrator window exits. Failure or canceled elevation retains diagnostic output.
Setup groups its prompts into paths, XXMI settings, and validation. Cyan indicates headings, yellow required actions, green success, and red errors. Redirected output stays plain text.
Security warnings cannot automatically be treated as false positives. Refer to README.en.md for the UAC and antivirus explanation.

LICENSE AND PROVENANCE
LICENSE retains the existing research code's GPLv3 terms. Native.cs and ParentLease.cs were separated from the earlier C# research implementation; the provenance statement is retained.
runtime-licenses/ contains the .NET license and third-party notices. These apply to separate components and do not replace the project's GPLv3 license.
No license change or new commercial-use restriction is introduced by this documentation update.

PACKAGING AND PRIVACY
PACKAGE-MANIFEST.json lists initial file hashes for detecting changes; it is not a tamper-proof signature. The manifest itself is not self-listed.
settings.json, backups, local-errors.log, generated shortcuts, and icons are local-use files excluded from the clean package.
app/settings.template.json is an empty template; Setup creates settings.json without requiring a manual copy.
Share the clean ZIP instead of repacking a used folder. Existing working settings remain in the user's current folder.

HISTORICAL VALIDATION
An earlier local launch test confirmed original XXMI/EFMI injection and working in-game mods.
Historical checks include Release publication, 44 offline assertions, PowerShell syntax checks, isolated shortcut-generation cases, and substitute-based window-handoff checks without real elevation or game launch.
These are historical records, not new runtime tests performed during this documentation-only packaging update. See VALIDATION.txt for the retained Chinese chronology and README.en.md for current validation limits.