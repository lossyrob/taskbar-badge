using System.Globalization;
using System.Windows;
using System.Windows.Media;
using TaskbarBadge.Core;
using WpfButton = System.Windows.Controls.Button;
using WpfPanel = System.Windows.Controls.Panel;
using WpfBrush = System.Windows.Media.Brush;
using WpfBrushes = System.Windows.Media.Brushes;
using DrawingColor = System.Drawing.Color;
using DrawingColorTranslator = System.Drawing.ColorTranslator;
using FormsColorDialog = System.Windows.Forms.ColorDialog;
using FormsDialogResult = System.Windows.Forms.DialogResult;

namespace TaskbarBadge.App;

public partial class SettingsWindow : Window
{
    private static readonly (string Name, string Hex)[] BadgeColorPresets =
    [
        ("Blue", "#0078D4"),
        ("Green", "#107C10"),
        ("Purple", "#5C2D91"),
        ("Orange", "#CA5010"),
        ("Red", "#D13438"),
        ("Teal", "#008575"),
        ("Gray", "#5E5E5E")
    ];

    private static readonly (string Name, string Hex)[] TextColorPresets =
    [
        ("White", "#FFFFFF"),
        ("Black", "#000000"),
        ("Cream", "#FFF4CE"),
        ("Light blue", "#DFF6FF")
    ];

    private readonly BadgeConfig _initialConfig;
    private string _backgroundColor;
    private string _textColor;

    public SettingsWindow(BadgeConfig config, bool runAtStartup)
    {
        InitializeComponent();
        _initialConfig = ConfigValidation.Normalize(config);
        _backgroundColor = _initialConfig.BackgroundColor;
        _textColor = _initialConfig.TextColor;

        AnchorBox.ItemsSource = Enum.GetValues<BadgeAnchor>();
        LabelBox.Text = _initialConfig.Label;
        WidthBox.Text = _initialConfig.Width.ToString(CultureInfo.InvariantCulture);
        HeightBox.Text = _initialConfig.Height.ToString(CultureInfo.InvariantCulture);
        FontSizeBox.Text = _initialConfig.FontSize.ToString(CultureInfo.InvariantCulture);
        OpacityBox.Text = _initialConfig.Opacity.ToString(CultureInfo.InvariantCulture);
        AnchorBox.SelectedItem = _initialConfig.Anchor;
        AlongOffsetBox.Text = _initialConfig.AlongTaskbarOffset.ToString(CultureInfo.InvariantCulture);
        CrossOffsetBox.Text = _initialConfig.CrossTaskbarOffset.ToString(CultureInfo.InvariantCulture);
        ShowBadgeBox.IsChecked = _initialConfig.ShowBadge;
        LockOverlayBox.IsChecked = _initialConfig.LockOverlayPosition;
        RunAtStartupBox.IsChecked = runAtStartup;

        CreatePresetButtons(BackgroundPresetPanel, BadgeColorPresets, SetBackgroundColor);
        CreatePresetButtons(TextPresetPanel, TextColorPresets, SetTextColor);
        UpdateColorButton(BackgroundColorButton, "Badge color", _backgroundColor);
        UpdateColorButton(TextColorButton, "Text color", _textColor);
    }

    public BadgeConfig? Config { get; private set; }

    public bool RunAtStartup { get; private set; }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        Config = ConfigValidation.Normalize(new BadgeConfig
        {
            Label = LabelBox.Text,
            BackgroundColor = _backgroundColor,
            TextColor = _textColor,
            Width = ParseDouble(WidthBox.Text, BadgeConfig.Default.Width),
            Height = ParseDouble(HeightBox.Text, BadgeConfig.Default.Height),
            FontSize = ParseDouble(FontSizeBox.Text, BadgeConfig.Default.FontSize),
            Opacity = ParseDouble(OpacityBox.Text, BadgeConfig.Default.Opacity),
            Anchor = AnchorBox.SelectedItem is BadgeAnchor anchor ? anchor : BadgeAnchor.End,
            AlongTaskbarOffset = ParseDouble(AlongOffsetBox.Text, BadgeConfig.Default.AlongTaskbarOffset),
            CrossTaskbarOffset = ParseDouble(CrossOffsetBox.Text, BadgeConfig.Default.CrossTaskbarOffset),
            OverlayLeft = _initialConfig.OverlayLeft,
            OverlayTop = _initialConfig.OverlayTop,
            LockOverlayPosition = LockOverlayBox.IsChecked == true,
            ShowBadge = ShowBadgeBox.IsChecked == true,
            RunAtStartup = RunAtStartupBox.IsChecked == true
        });

        RunAtStartup = RunAtStartupBox.IsChecked == true;
        DialogResult = true;
    }

    private void BackgroundColorButton_Click(object sender, RoutedEventArgs e)
    {
        ChooseColor(_backgroundColor, SetBackgroundColor);
    }

    private void TextColorButton_Click(object sender, RoutedEventArgs e)
    {
        ChooseColor(_textColor, SetTextColor);
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }

    private void SetBackgroundColor(string hexColor)
    {
        _backgroundColor = ConfigValidation.Normalize(new BadgeConfig { BackgroundColor = hexColor }).BackgroundColor;
        UpdateColorButton(BackgroundColorButton, "Badge color", _backgroundColor);
    }

    private void SetTextColor(string hexColor)
    {
        _textColor = ConfigValidation.Normalize(new BadgeConfig { TextColor = hexColor }).TextColor;
        UpdateColorButton(TextColorButton, "Text color", _textColor);
    }

    private static void CreatePresetButtons(
        WpfPanel panel,
        IEnumerable<(string Name, string Hex)> presets,
        Action<string> setColor)
    {
        foreach (var preset in presets)
        {
            var button = new WpfButton
            {
                Width = 28,
                Height = 22,
                Margin = new Thickness(0, 0, 6, 4),
                Background = ToBrush(preset.Hex),
                BorderBrush = WpfBrushes.White,
                ToolTip = preset.Name,
                Content = ""
            };
            button.Click += (_, _) => setColor(preset.Hex);
            panel.Children.Add(button);
        }
    }

    private static void ChooseColor(string currentHex, Action<string> setColor)
    {
        using var dialog = new FormsColorDialog
        {
            AllowFullOpen = true,
            FullOpen = true,
            Color = ParseDrawingColor(currentHex, DrawingColor.DodgerBlue)
        };

        if (dialog.ShowDialog() == FormsDialogResult.OK)
        {
            setColor(ToHex(dialog.Color));
        }
    }

    private static void UpdateColorButton(WpfButton button, string label, string hexColor)
    {
        button.Content = $"{label}: {hexColor}";
        button.Background = ToBrush(hexColor);
        button.Foreground = GetReadableTextBrush(hexColor);
    }

    private static double ParseDouble(string value, double fallback)
    {
        return double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : fallback;
    }

    private static WpfBrush ToBrush(string hexColor)
    {
        try
        {
            return (WpfBrush)new BrushConverter().ConvertFromString(hexColor)!;
        }
        catch (FormatException)
        {
            return WpfBrushes.DodgerBlue;
        }
    }

    private static WpfBrush GetReadableTextBrush(string hexColor)
    {
        var color = ParseDrawingColor(hexColor, DrawingColor.DodgerBlue);
        var luminance = (0.299 * color.R) + (0.587 * color.G) + (0.114 * color.B);
        return luminance > 140 ? WpfBrushes.Black : WpfBrushes.White;
    }

    private static DrawingColor ParseDrawingColor(string hexColor, DrawingColor fallback)
    {
        try
        {
            return DrawingColorTranslator.FromHtml(hexColor);
        }
        catch (Exception)
        {
            return fallback;
        }
    }

    private static string ToHex(DrawingColor color)
    {
        return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
    }
}
