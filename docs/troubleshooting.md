# Troubleshooting

## Badge is not visible

1. Check whether the tray icon is hidden behind the taskbar corner overflow (`^`) menu.
2. Use the tray menu item **Open Windows taskbar settings**, then enable TaskbarBadge under taskbar corner/other system tray icons if you want persistent visibility.
3. If you mean the overlay badge, check the tray menu and ensure **Show overlay badge** is enabled.
4. Open Settings or use **Reset overlay position** from the tray menu.
5. Confirm the app is running in Task Manager as `TaskbarBadge`.

## Badge is in the wrong place

TaskbarBadge has a Windows-managed tray icon and an optional overlay. If the overlay is in the wrong place:

- Unlock overlay position.
- Drag the overlay to the desired place.
- Lock overlay position again.
- Use **Reset overlay position** to return to calculated placement.

The calculated placement uses the primary taskbar as a starting point.

## Badge disappeared after Explorer restarted

The app repositions on a timer. Wait a few seconds or use the tray menu to hide/show the badge.

## Startup does not work

Open Settings and toggle **Start with Windows** off and back on. The app uses a per-user `HKCU\Software\Microsoft\Windows\CurrentVersion\Run` entry.

## SmartScreen warns about the installer

Early releases may be unsigned. Download only from the official GitHub Releases page and verify `checksums.txt` if needed.
