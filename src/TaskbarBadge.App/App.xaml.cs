using System.Threading;
using System.Windows;
using TaskbarBadge.Core;

namespace TaskbarBadge.App;

public partial class App : System.Windows.Application
{
    private Mutex? _mutex;
    private BadgeWindow? _badgeWindow;
    private BadgeWindowController? _badgeController;
    private TrayIconService? _trayIconService;
    private ConfigStore? _configStore;
    private StartupService? _startupService;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        _mutex = new Mutex(initiallyOwned: true, "TaskbarBadge.SingleInstance", out var createdNew);
        if (!createdNew)
        {
            _mutex.Dispose();
            _mutex = null;
            Shutdown();
            return;
        }

        ShutdownMode = ShutdownMode.OnExplicitShutdown;

        _configStore = ConfigStore.CreateDefault();
        var config = _configStore.LoadOrDefault();
        _startupService = new StartupService(Environment.ProcessPath ?? AppContext.BaseDirectory);
        if (config.RunAtStartup && !_startupService.IsEnabled())
        {
            _startupService.SetEnabled(true);
        }

        _badgeWindow = new BadgeWindow();
        _badgeController = new BadgeWindowController(_badgeWindow, _configStore, new TaskbarInfoProvider());
        _badgeController.Apply(config);

        _trayIconService = new TrayIconService(
            _configStore,
            _badgeController,
            _startupService,
            OpenSettings,
            ExitApplication);
    }

    private void OpenSettings()
    {
        if (_configStore is null || _badgeController is null || _startupService is null)
        {
            return;
        }

        var window = new SettingsWindow(
            _configStore.LoadOrDefault(),
            _startupService.IsEnabled())
        {
            Owner = _badgeWindow,
            WindowStartupLocation = WindowStartupLocation.CenterScreen
        };

        if (window.ShowDialog() == true && window.Config is not null)
        {
            var config = ConfigValidation.Normalize(window.Config with
            {
                RunAtStartup = window.RunAtStartup
            });

            _configStore.Save(config);
            _startupService.SetEnabled(config.RunAtStartup);
            _badgeController.Apply(config);
            _trayIconService?.Refresh();
        }
    }

    private void ExitApplication()
    {
        Shutdown();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _trayIconService?.Dispose();
        _badgeController?.Dispose();
        _mutex?.Dispose();
        base.OnExit(e);
    }
}
