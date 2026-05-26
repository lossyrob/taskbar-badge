using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Runtime.InteropServices;
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
            Visible = true
        };

        _notifyIcon.DoubleClick += (_, _) => _openSettings();
        Refresh();
    }

    public void Refresh()
    {
        var config = _configStore.LoadOrDefault();
        _notifyIcon.Icon?.Dispose();
        _notifyIcon.Icon = CreateBadgeIcon(config);
        _notifyIcon.Text = config.Label.Length > 63 ? config.Label[..63] : config.Label;

        var menu = new ContextMenuStrip();

        menu.Items.Add("Settings...", null, (_, _) => _openSettings());
        menu.Items.Add(CreateColorMenu(config));

        var showBadgeItem = new ToolStripMenuItem("Show overlay badge")
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
        menu.Items.Add("Reset overlay position", null, (_, _) => _badgeController.ResetOverlayPosition());

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

        menu.Items.Add("Open Windows taskbar settings", null, (_, _) =>
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "ms-settings:taskbar",
                UseShellExecute = true
            });
        });

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
        _notifyIcon.Icon?.Dispose();
        _notifyIcon.Dispose();
    }

    private static Icon CreateBadgeIcon(BadgeConfig config)
    {
        using var bitmap = new Bitmap(64, 64);
        using var graphics = Graphics.FromImage(bitmap);
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        graphics.Clear(Color.Transparent);

        using var background = new SolidBrush(ParseColor(config.BackgroundColor, Color.DodgerBlue));
        using var border = new Pen(Color.FromArgb(235, Color.White), 4)
        {
            LineJoin = LineJoin.Round
        };
        using var hole = new SolidBrush(Color.FromArgb(245, Color.White));
        using var shadow = new SolidBrush(Color.FromArgb(55, Color.Black));

        using var shadowPath = CreateTagPath(offsetX: 2, offsetY: 3);
        graphics.FillPath(shadow, shadowPath);

        using var tagPath = CreateTagPath(offsetX: 0, offsetY: 0);
        graphics.FillPath(background, tagPath);
        graphics.DrawPath(border, tagPath);
        graphics.FillEllipse(hole, 24, 11, 11, 11);

        var handle = bitmap.GetHicon();
        try
        {
            using var icon = Icon.FromHandle(handle);
            return (Icon)icon.Clone();
        }
        finally
        {
            _ = DestroyIcon(handle);
        }
    }

    private static GraphicsPath CreateTagPath(float offsetX, float offsetY)
    {
        var path = new GraphicsPath();
        path.AddLines(
        [
            new PointF(14 + offsetX, 6 + offsetY),
            new PointF(50 + offsetX, 6 + offsetY),
            new PointF(58 + offsetX, 14 + offsetY),
            new PointF(58 + offsetX, 58 + offsetY),
            new PointF(6 + offsetX, 58 + offsetY),
            new PointF(6 + offsetX, 14 + offsetY)
        ]);
        path.CloseFigure();
        return path;
    }

    private static Color ParseColor(string value, Color fallback)
    {
        try
        {
            return ColorTranslator.FromHtml(value);
        }
        catch (Exception)
        {
            return fallback;
        }
    }

    private ToolStripMenuItem CreateColorMenu(BadgeConfig config)
    {
        var menu = new ToolStripMenuItem("Badge color");
        var colors = new (string Name, string Hex)[]
        {
            ("Blue", "#0078D4"),
            ("Green", "#107C10"),
            ("Purple", "#5C2D91"),
            ("Orange", "#CA5010"),
            ("Red", "#D13438"),
            ("Teal", "#008575"),
            ("Gray", "#5E5E5E")
        };

        foreach (var color in colors)
        {
            var item = new ToolStripMenuItem(color.Name)
            {
                Checked = string.Equals(config.BackgroundColor, color.Hex, StringComparison.OrdinalIgnoreCase)
            };
            item.Click += (_, _) => SetBadgeColor(color.Hex);
            menu.DropDownItems.Add(item);
        }

        menu.DropDownItems.Add("-");
        menu.DropDownItems.Add("Custom...", null, (_, _) => _openSettings());
        return menu;
    }

    private void SetBadgeColor(string hexColor)
    {
        var config = _configStore.LoadOrDefault() with
        {
            BackgroundColor = hexColor
        };
        _configStore.Save(config);
        _badgeController.Apply(config);
        Refresh();
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool DestroyIcon(IntPtr hIcon);
}
