# TaskbarBadge

TaskbarBadge is a small Windows utility that shows a configurable identity badge near the taskbar. It is useful when you work across multiple machines, remote desktops, Microsoft Dev Boxes, Cloud PCs, or any environment where you want a persistent visual label.

The badge is an always-on-top overlay positioned over the taskbar area. It does not modify Explorer or inject into the Windows taskbar.

## Features

- Custom badge label, colors, opacity, size, anchor, and offsets.
- Tray icon with settings, hide/show, startup toggle, config folder, and exit commands.
- Per-user config stored at `%APPDATA%\TaskbarBadge\config.json`.
- Per-user startup registration.
- MSI and portable artifacts from GitHub Releases.

## Install

Download the latest MSI from GitHub Releases and run it.

If Windows SmartScreen warns about the installer, it is because early releases are unsigned. Review the source and release checksums if needed.

## Usage

After launch, use the tray icon to open **Settings** and customize:

- **Label**: text shown in the badge.
- **Background/Text color**: hex colors such as `#0078D4`.
- **Width/Height/Font size/Opacity**: badge appearance.
- **Anchor**: start, center, or end of the taskbar.
- **Along-taskbar offset**: distance from the selected anchor.
- **Cross-taskbar offset**: vertical/horizontal nudge across the taskbar.
- **Start with Windows**: per-user auto-start.

## Build locally

Requirements:

- Windows
- .NET 10 SDK

```powershell
dotnet restore .\TaskbarBadge.slnx
dotnet build .\TaskbarBadge.slnx --configuration Release
dotnet test .\TaskbarBadge.slnx --configuration Release
.\scripts\build.ps1 -Configuration Release -Version 0.1.0
```

Use `-SkipInstaller` if you only want the portable zip and do not need the WiX-built MSI.

## Limitations

- This is a taskbar-area overlay, not a native Explorer taskbar extension.
- Very unusual taskbar customizations may require offset adjustment.
- Multi-monitor behavior starts with the primary taskbar. More placement modes can be added later.
- The MSI is unsigned unless a release maintainer adds signing in the release workflow.

## Dev Box / Cloud PC tip

Install TaskbarBadge inside each Dev Box or Cloud PC, then set a different label/color in each environment. The badge remains visible in the remote Windows session and makes it easier to orient yourself after switching.
