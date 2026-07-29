using System.Windows;

namespace CatSit.UI;

public enum UpdateChoice
{
    DownloadNow,
    RemindLater,
    SkipVersion
}

public partial class UpdateWindow : Window
{
    public UpdateWindow(string currentVersionText, string latestVersionText)
    {
        CurrentVersionText = currentVersionText;
        LatestVersionText = latestVersionText;
        InitializeComponent();
    }

    public string CurrentVersionText { get; }
    public string LatestVersionText { get; }
    public UpdateChoice Choice { get; private set; } = UpdateChoice.RemindLater;

    private void OnDownloadNow(object sender, RoutedEventArgs e)
    {
        Choice = UpdateChoice.DownloadNow;
        Close();
    }

    private void OnRemindLater(object sender, RoutedEventArgs e)
    {
        Choice = UpdateChoice.RemindLater;
        Close();
    }

    private void OnSkipVersion(object sender, RoutedEventArgs e)
    {
        Choice = UpdateChoice.SkipVersion;
        Close();
    }
}
