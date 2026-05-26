using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using TaskbarBadge.Core;

namespace TaskbarBadge.App;

public sealed class TrayIconService : IDisposable
{
    private readonly ConfigStore _configStore;
    private readonly BadgeWindowController _badgeController;
    private readonly StartupService _startupService;
    private readonly Action _openSettings;
    private readonly Action _exitApplication;
    private readonly NotifyIcon _notifyIcon;

    public TrayIconService(
        ConfigStore configStore,
        BadgeWindowController badgeController,
        StartupService startupService,
        Action openSettings,
        Action exitApplication)
    {
        _configStore = configStore;
        _badgeController = badgeController;
        _startupService = startupService;
        _openSettings = openSettings;
        _exitApplication = exitApplication;

        _notifyIcon = new NotifyIcon
        {
            Icon = SystemIcons.Application,
            Text = "TaskbarBadge",
            Visible = true
        };

        _notifyIcon.DoubleClick += (_, _) => _openSettings();
        Refresh();
    }

    public void Refresh()
    {
        var config = _configStore.LoadOrDefault();
        var menu = new ContextMenuStrip();

        menu.Items.Add("Settings...", null, (_, _) => _openSettings());
        var showBadgeItem = new ToolStripMenuItem("Show badge")
        {
            Checked = config.ShowBadge,
            CheckOnClick = false
        };
        showBadgeItem.Click += (_, _) =>
        {
            _badgeController.ToggleVisibility();
            Refresh();
        };
        menu.Items.Add(showBadgeItem);

        var startupItem = new ToolStripMenuItem("Start with Windows")
        {
            Checked = _startupService.IsEnabled(),
            CheckOnClick = false
        };
        startupItem.Click += (_, _) =>
        {
            var enabled = !_startupService.IsEnabled();
            _startupService.SetEnabled(enabled);
            _configStore.Save(_configStore.LoadOrDefault() with { RunAtStartup = enabled });
            Refresh();
        };
        menu.Items.Add(startupItem);

        menu.Items.Add("Open config folder", null, (_, _) =>
        {
            Directory.CreateDirectory(_badgeController.ConfigDirectory);
            Process.Start(new ProcessStartInfo
            {
                FileName = _badgeController.ConfigDirectory,
                UseShellExecute = true
            });
        });

        menu.Items.Add("-");
        menu.Items.Add("Exit", null, (_, _) => _exitApplication());

        var oldMenu = _notifyIcon.ContextMenuStrip;
        _notifyIcon.ContextMenuStrip = menu;
        oldMenu?.Dispose();
    }

    public void Dispose()
    {
        _notifyIcon.Visible = false;
        _notifyIcon.ContextMenuStrip?.Dispose();
        _notifyIcon.Dispose();
    }
}
