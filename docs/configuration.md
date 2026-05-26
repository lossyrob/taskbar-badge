# Configuration

TaskbarBadge stores per-user settings in:

```text
%APPDATA%\TaskbarBadge\config.json
```

Most users should edit settings through the tray icon. The JSON file is useful for scripting or copying a profile between machines.

## Settings

| Setting | Description |
| --- | --- |
| `Label` | Text displayed in the badge. Defaults to the machine name. |
| `BackgroundColor` | Badge background color as `#RRGGBB`. |
| `TextColor` | Badge text color as `#RRGGBB`. |
| `Width` / `Height` | Badge size in WPF device-independent pixels. |
| `FontSize` | Badge font size. |
| `Opacity` | Badge opacity from `0.2` to `1.0`. |
| `Anchor` | `Start`, `Center`, or `End` along the taskbar. |
| `AlongTaskbarOffset` | Distance from the anchor along the taskbar. |
| `CrossTaskbarOffset` | Nudge across the taskbar. |
| `ShowBadge` | Whether the badge is visible. |
| `RunAtStartup` | Whether TaskbarBadge registers per-user startup. |

Invalid colors or out-of-range values are normalized by the app.
