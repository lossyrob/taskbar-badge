using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using TaskbarBadge.Core;
using MediaBrush = System.Windows.Media.Brush;
using MediaBrushes = System.Windows.Media.Brushes;
using MediaBrushConverter = System.Windows.Media.BrushConverter;

namespace TaskbarBadge.App;

public partial class BadgeWindow : Window
{
    private const int GwlExStyle = -20;
    private const int WsExToolWindow = 0x00000080;
    private const int WsExNoActivate = 0x08000000;

    public BadgeWindow()
    {
        InitializeComponent();
        SourceInitialized += (_, _) => ConfigureExtendedWindowStyles();
    }

    public void Apply(BadgeConfig config)
    {
        BadgeText.Text = config.Label;
        BadgeText.FontSize = config.FontSize;
        BadgeText.Foreground = ToBrush(config.TextColor, MediaBrushes.White);
        BadgeBorder.Background = ToBrush(config.BackgroundColor, MediaBrushes.DodgerBlue);
        Opacity = config.Opacity;
        Width = config.Width;
        Height = config.Height;
    }

    private static MediaBrush ToBrush(string color, MediaBrush fallback)
    {
        try
        {
            return (MediaBrush)new MediaBrushConverter().ConvertFromString(color)!;
        }
        catch (FormatException)
        {
            return fallback;
        }
    }

    private void ConfigureExtendedWindowStyles()
    {
        var handle = new WindowInteropHelper(this).Handle;
        if (handle == IntPtr.Zero)
        {
            return;
        }

        var styles = GetWindowLongPtr(handle, GwlExStyle);
        var updatedStyles = new IntPtr(styles.ToInt64() | WsExToolWindow | WsExNoActivate);
        _ = SetWindowLongPtr(handle, GwlExStyle, updatedStyles);
    }

    [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr")]
    private static extern IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
    private static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);
}
