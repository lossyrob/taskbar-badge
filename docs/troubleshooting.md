# Troubleshooting

## Badge is not visible

1. Check the tray icon and ensure **Show badge** is enabled.
2. Open Settings and reduce `AlongTaskbarOffset` or reset placement values.
3. Confirm the app is running in Task Manager as `TaskbarBadge`.

## Badge is in the wrong place

TaskbarBadge positions an overlay near the taskbar. Open Settings and adjust:

- `Anchor`
- `AlongTaskbarOffset`
- `CrossTaskbarOffset`
- `Width`
- `Height`

Primary-taskbar placement is the initial supported mode.

## Badge disappeared after Explorer restarted

The app repositions on a timer. Wait a few seconds or use the tray menu to hide/show the badge.

## Startup does not work

Open Settings and toggle **Start with Windows** off and back on. The app uses a per-user `HKCU\Software\Microsoft\Windows\CurrentVersion\Run` entry.

## SmartScreen warns about the installer

Early releases may be unsigned. Download only from the official GitHub Releases page and verify `checksums.txt` if needed.
