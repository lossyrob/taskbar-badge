using System.Globalization;
using System.Windows;
using TaskbarBadge.Core;

namespace TaskbarBadge.App;

public partial class SettingsWindow : Window
{
    public SettingsWindow(BadgeConfig config, bool runAtStartup)
    {
        InitializeComponent();

        AnchorBox.ItemsSource = Enum.GetValues<BadgeAnchor>();
        LabelBox.Text = config.Label;
        BackgroundColorBox.Text = config.BackgroundColor;
        TextColorBox.Text = config.TextColor;
        WidthBox.Text = config.Width.ToString(CultureInfo.InvariantCulture);
        HeightBox.Text = config.Height.ToString(CultureInfo.InvariantCulture);
        FontSizeBox.Text = config.FontSize.ToString(CultureInfo.InvariantCulture);
        OpacityBox.Text = config.Opacity.ToString(CultureInfo.InvariantCulture);
        AnchorBox.SelectedItem = config.Anchor;
        AlongOffsetBox.Text = config.AlongTaskbarOffset.ToString(CultureInfo.InvariantCulture);
        CrossOffsetBox.Text = config.CrossTaskbarOffset.ToString(CultureInfo.InvariantCulture);
        ShowBadgeBox.IsChecked = config.ShowBadge;
        RunAtStartupBox.IsChecked = runAtStartup;
    }

    public BadgeConfig? Config { get; private set; }

    public bool RunAtStartup { get; private set; }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        Config = ConfigValidation.Normalize(new BadgeConfig
        {
            Label = LabelBox.Text,
            BackgroundColor = BackgroundColorBox.Text,
            TextColor = TextColorBox.Text,
            Width = ParseDouble(WidthBox.Text, BadgeConfig.Default.Width),
            Height = ParseDouble(HeightBox.Text, BadgeConfig.Default.Height),
            FontSize = ParseDouble(FontSizeBox.Text, BadgeConfig.Default.FontSize),
            Opacity = ParseDouble(OpacityBox.Text, BadgeConfig.Default.Opacity),
            Anchor = AnchorBox.SelectedItem is BadgeAnchor anchor ? anchor : BadgeAnchor.End,
            AlongTaskbarOffset = ParseDouble(AlongOffsetBox.Text, BadgeConfig.Default.AlongTaskbarOffset),
            CrossTaskbarOffset = ParseDouble(CrossOffsetBox.Text, BadgeConfig.Default.CrossTaskbarOffset),
            ShowBadge = ShowBadgeBox.IsChecked == true,
            RunAtStartup = RunAtStartupBox.IsChecked == true
        });

        RunAtStartup = RunAtStartupBox.IsChecked == true;
        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }

    private static double ParseDouble(string value, double fallback)
    {
        return double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : fallback;
    }
}
