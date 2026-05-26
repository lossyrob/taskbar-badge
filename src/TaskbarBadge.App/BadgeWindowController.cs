using System.Windows;
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

    public BadgeWindowController(
        BadgeWindow window,
        ConfigStore configStore,
        TaskbarInfoProvider taskbarInfoProvider)
    {
        _window = window;
        _configStore = configStore;
        _taskbarInfoProvider = taskbarInfoProvider;
        _config = configStore.LoadOrDefault();
        _positionTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(2)
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
        if (!_config.ShowBadge || !_window.IsVisible)
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

        var badge = PlacementCalculator.Calculate(_config, dipTaskbar);
        _window.Left = badge.Left;
        _window.Top = badge.Top;
        _window.Width = badge.Width;
        _window.Height = badge.Height;
        _window.Topmost = true;
    }

    public void Dispose()
    {
        _positionTimer.Stop();
        _window.Close();
    }
}
