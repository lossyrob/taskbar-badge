using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Threading;
using TaskbarBadge.Core;

namespace TaskbarBadge.App;

public sealed class BadgeWindowController : IDisposable
{
    private readonly BadgeWindow _window;
    private readonly ConfigStore _configStore;
    private readonly TaskbarInfoProvider _taskbarInfoProvider;
    private readonly DispatcherTimer _positionTimer;
    private BadgeConfig _config;
    private DesktopRect? _lastLoggedBadge;
    private bool _hiddenForFullscreen;
    private int _fullscreenTicks;
    private int _nonFullscreenTicks;

    public BadgeWindowController(
        BadgeWindow window,
        ConfigStore configStore,
        TaskbarInfoProvider taskbarInfoProvider)
    {
        _window = window;
        _configStore = configStore;
        _taskbarInfoProvider = taskbarInfoProvider;
        _config = configStore.LoadOrDefault();
        _window.PositionChangedByUser += (_, _) => SaveOverlayPosition();
        _positionTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _positionTimer.Tick += (_, _) => PositionWindow();
        _positionTimer.Start();
    }

    public BadgeConfig CurrentConfig => _config;

    public string ConfigDirectory => _configStore.ConfigDirectory;

    public void Apply(BadgeConfig config)
    {
        _config = ConfigValidation.Normalize(config);
        _window.Apply(_config);

        if (_config.ShowBadge)
        {
            if (!_window.IsVisible)
            {
                _window.Show();
            }

            PositionWindow();
        }
        else
        {
            _window.Hide();
        }
    }

    public void ToggleVisibility()
    {
        Apply(_config with { ShowBadge = !_config.ShowBadge });
        _configStore.Save(_config);
    }

    private void PositionWindow()
    {
        if (!_config.ShowBadge)
        {
            return;
        }

        var taskbar = _taskbarInfoProvider.GetPrimaryTaskbarSnapshot();
        var dpi = VisualTreeHelper.GetDpi(_window);
        var dipTaskbar = new TaskbarSnapshot(
            new DesktopRect(
                taskbar.Bounds.Left / dpi.DpiScaleX,
                taskbar.Bounds.Top / dpi.DpiScaleY,
                taskbar.Bounds.Width / dpi.DpiScaleX,
                taskbar.Bounds.Height / dpi.DpiScaleY),
            taskbar.Edge);

        var badge = _config.OverlayLeft is not null && _config.OverlayTop is not null
            ? new DesktopRect(_config.OverlayLeft.Value, _config.OverlayTop.Value, _config.Width, _config.Height)
            : PlacementCalculator.Calculate(_config, dipTaskbar);

        if (UpdateFullscreenState(badge))
        {
            return;
        }

        if (!_window.IsVisible)
        {
            _window.Show();
        }

        _window.Place(badge.Left, badge.Top, badge.Width, badge.Height);

        if (_lastLoggedBadge is null || HasMoved(_lastLoggedBadge.Value, badge))
        {
            _lastLoggedBadge = badge;
            File.AppendAllText(
                Path.Combine(_configStore.ConfigDirectory, "TaskbarBadge.log"),
                $"{DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss} Badge {badge.Left:0.##},{badge.Top:0.##} {badge.Width:0.##}x{badge.Height:0.##}; Taskbar {dipTaskbar.Bounds.Left:0.##},{dipTaskbar.Bounds.Top:0.##} {dipTaskbar.Bounds.Width:0.##}x{dipTaskbar.Bounds.Height:0.##} {dipTaskbar.Edge}{Environment.NewLine}");
        }
    }

    private bool UpdateFullscreenState(DesktopRect badge)
    {
        if (IsForegroundWindowFullscreenOnBadgeScreen(badge))
        {
            _fullscreenTicks++;
            _nonFullscreenTicks = 0;

            if (_fullscreenTicks >= 1)
            {
                if (!_hiddenForFullscreen)
                {
                    _hiddenForFullscreen = true;
                    File.AppendAllText(
                        Path.Combine(_configStore.ConfigDirectory, "TaskbarBadge.log"),
                        $"{DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss} Overlay hidden for full-screen foreground window{Environment.NewLine}");
                }

                if (_window.IsVisible)
                {
                    _window.Hide();
                }
            }

            return true;
        }

        _nonFullscreenTicks++;
        _fullscreenTicks = 0;

        if (_hiddenForFullscreen && _nonFullscreenTicks < 3)
        {
            return true;
        }

        if (_hiddenForFullscreen)
        {
            _hiddenForFullscreen = false;
            File.AppendAllText(
                Path.Combine(_configStore.ConfigDirectory, "TaskbarBadge.log"),
                $"{DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss} Overlay restored after full-screen foreground window{Environment.NewLine}");
        }

        return false;
    }

    public void ResetOverlayPosition()
    {
        _config = _config with
        {
            OverlayLeft = null,
            OverlayTop = null
        };
        _configStore.Save(_config);
        Apply(_config);
    }

    private void SaveOverlayPosition()
    {
        _config = _config with
        {
            OverlayLeft = _window.Left,
            OverlayTop = _window.Top
        };
        _configStore.Save(_config);
    }

    private static bool HasMoved(DesktopRect previous, DesktopRect current)
    {
        return Math.Abs(previous.Left - current.Left) > 0.5
            || Math.Abs(previous.Top - current.Top) > 0.5
            || Math.Abs(previous.Width - current.Width) > 0.5
            || Math.Abs(previous.Height - current.Height) > 0.5;
    }

    private static bool IsForegroundWindowFullscreenOnBadgeScreen(DesktopRect badge)
    {
        var foreground = GetForegroundWindow();
        if (foreground == IntPtr.Zero)
        {
            return false;
        }

        _ = GetWindowThreadProcessId(foreground, out var processId);
        using var currentProcess = System.Diagnostics.Process.GetCurrentProcess();
        if (processId == currentProcess.Id)
        {
            return false;
        }

        if (!GetWindowRect(foreground, out var rect))
        {
            return false;
        }

        var badgeCenter = new System.Drawing.Point(
            (int)Math.Round(badge.Left + (badge.Width / 2)),
            (int)Math.Round(badge.Top + (badge.Height / 2)));
        var screenBounds = Screen.FromPoint(badgeCenter).Bounds;

        const int tolerance = 4;
        return rect.Left <= screenBounds.Left + tolerance
            && rect.Top <= screenBounds.Top + tolerance
            && rect.Right >= screenBounds.Right - tolerance
            && rect.Bottom >= screenBounds.Bottom - tolerance;
    }

    public void Dispose()
    {
        _positionTimer.Stop();
        _window.Close();
    }

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);

    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(IntPtr hWnd, out NativeRect lpRect);

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeRect
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }
}
