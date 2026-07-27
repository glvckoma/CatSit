using System.ComponentModel;
using System.Windows;

namespace CatSit.UI;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    public event Action<string?>? SettingsRequested;
    public event Action? HiddenToTray;

    protected override void OnStateChanged(EventArgs e)
    {
        base.OnStateChanged(e);
        if (WindowState == WindowState.Minimized) Hide();
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        if (DataContext is MainViewModel { CloseToTray: true })
        {
            e.Cancel = true;
            Hide();
            HiddenToTray?.Invoke();
            return;
        }
        base.OnClosing(e);
    }

    public void ShowFromTray()
    {
        Show();
        WindowState = WindowState.Normal;
        Activate();
    }

    private void OnSettingsClicked(object sender, RoutedEventArgs e) => SettingsRequested?.Invoke(null);

    private void OnActivityClicked(object sender, RoutedEventArgs e) => SettingsRequested?.Invoke("Activity");
}
