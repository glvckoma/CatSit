using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CatSit.UI;

public partial class WindowHeader : UserControl
{
    public static readonly DependencyProperty HeaderTitleProperty = DependencyProperty.Register(
        nameof(HeaderTitle), typeof(string), typeof(WindowHeader),
        new PropertyMetadata(string.Empty, (d, e) => ((WindowHeader)d).TitleText.Text = (string?)e.NewValue ?? string.Empty));

    public static readonly DependencyProperty AllowMaximizeProperty = DependencyProperty.Register(
        nameof(AllowMaximize), typeof(bool), typeof(WindowHeader),
        new PropertyMetadata(false, (d, e) => ((WindowHeader)d).MaximizeButton.Visibility =
            (bool)e.NewValue ? Visibility.Visible : Visibility.Collapsed));

    private Window? _window;

    public WindowHeader()
    {
        InitializeComponent();
        MaximizeButton.Visibility = Visibility.Collapsed;
        Loaded += OnLoaded;
    }

    public string HeaderTitle
    {
        get => (string)GetValue(HeaderTitleProperty);
        set => SetValue(HeaderTitleProperty, value);
    }

    public bool AllowMaximize
    {
        get => (bool)GetValue(AllowMaximizeProperty);
        set => SetValue(AllowMaximizeProperty, value);
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _window = Window.GetWindow(this);
        if (_window is null) return;
        _window.StateChanged += (_, _) => ApplyState();
        ApplyState();
    }

    private void ApplyState()
    {
        if (_window is null) return;
        var maximized = _window.WindowState == WindowState.Maximized;
        MaximizeIcon.Geometry = (Geometry)FindResource(maximized ? "IconRestore" : "IconMaximize");
        var inset = maximized ? SystemParameters.WindowResizeBorderThickness.Left + 4 : 0;
        _window.Padding = new Thickness(inset);
    }

    private void OnMinimize(object sender, RoutedEventArgs e)
    {
        if (_window is not null) _window.WindowState = WindowState.Minimized;
    }

    private void OnMaximize(object sender, RoutedEventArgs e)
    {
        if (_window is null) return;
        _window.WindowState = _window.WindowState == WindowState.Maximized
            ? WindowState.Normal
            : WindowState.Maximized;
    }

    private void OnClose(object sender, RoutedEventArgs e) => _window?.Close();
}
