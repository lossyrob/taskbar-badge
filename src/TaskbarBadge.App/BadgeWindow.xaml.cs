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
    private static readonly IntPtr HwndTopmost = new(-1);
    private const uint SwpNoMove = 0x0002;
    private const uint SwpNoSize = 0x0001;
    private const uint SwpNoActivate = 0x0010;
    private const uint SwpShowWindow = 0x0040;
    private const uint SwpNoOwnerZOrder = 0x0200;
    private bool _isPositionLocked;

    public event EventHandler? PositionChangedByUser;

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
        _isPositionLocked = config.LockOverlayPosition;
    }

    public void Place(double left, double top, double width, double height)
    {
        Left = left;
        Top = top;
        Width = width;
        Height = height;
        Topmost = true;

        var handle = new WindowInteropHelper(this).Handle;
        if (handle == IntPtr.Zero)
        {
            return;
        }

        _ = SetWindowPos(
            handle,
            HwndTopmost,
            0,
            0,
            0,
            0,
            SwpNoMove | SwpNoSize | SwpNoActivate | SwpShowWindow | SwpNoOwnerZOrder);
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

    private void Window_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (_isPositionLocked)
        {
            return;
        }

        DragMove();
    }

    private void Window_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (!_isPositionLocked)
        {
            PositionChangedByUser?.Invoke(this, EventArgs.Empty);
        }
    }

    [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr")]
    private static extern IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
    private static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SetWindowPos(
        IntPtr hWnd,
        IntPtr hWndInsertAfter,
        int x,
        int y,
        int cx,
        int cy,
        uint uFlags);
}
