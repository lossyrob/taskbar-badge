# TaskbarBadge

TaskbarBadge is a small Windows utility that shows a configurable identity badge from the notification area, with an optional draggable overlay badge. It is useful when you work across multiple machines, remote desktops, Microsoft Dev Boxes, Cloud PCs, or any environment where you want a persistent visual label.

The tray icon is the stable default because Windows owns the taskbar surface. The overlay is optional and movable for users who want a larger visual marker.

## Features

- Custom tray icon generated from the configured color, with the full label available as the tooltip.
- Optional overlay badge with custom label, colors, opacity, size, anchor, offsets, manual drag position, and lock state.
- Tray menu with settings, overlay hide/show, reset overlay position, startup toggle, config folder, and exit commands.
- Per-user config stored at `%APPDATA%\TaskbarBadge\config.json`.
- Per-user startup registration.
- MSI and portable artifacts from GitHub Releases.

## Install

Download the latest MSI from GitHub Releases and run it.

The installer shows a standard setup wizard and launches TaskbarBadge when installation completes.

If Windows SmartScreen warns about the installer, it is because early releases are unsigned. Review the source and release checksums if needed.

## Usage

After launch, use the tray icon to open **Settings** and customize:

- **Label**: text shown in the overlay and used to generate the tray icon.
- **Badge color/Text color**: hex colors such as `#0078D4`.
- **Width/Height/Font size/Opacity**: badge appearance.
- **Anchor**: start, center, or end of the taskbar.
- **Along-taskbar offset**: distance from the selected anchor.
- **Cross-taskbar offset**: vertical/horizontal nudge across the taskbar.
- **Show overlay badge**: show or hide the optional overlay.
- **Lock overlay position**: prevent accidental dragging after placing the overlay.
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

- Windows does not expose a supported API for custom weather-style taskbar items.
- The tray icon is Windows-managed; the optional overlay is not a native Explorer taskbar extension.
- Windows controls whether tray icons are pinned or hidden. Use **Open Windows taskbar settings** from the tray menu, then enable TaskbarBadge under taskbar corner/other system tray icons.
- Full-screen and taskbar z-order behavior can vary, so the overlay is optional and movable.
- Multi-monitor behavior starts with the primary taskbar. More placement modes can be added later.
- The MSI is unsigned unless a release maintainer adds signing in the release workflow.

## Dev Box / Cloud PC tip

Install TaskbarBadge inside each Dev Box or Cloud PC, then set a different label/color in each environment. Keep the tray icon visible in Windows taskbar corner settings, and optionally show/position the overlay if you want a larger marker.
