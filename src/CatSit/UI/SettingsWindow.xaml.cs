using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CatSit.Core.Audio;
using WinForms = System.Windows.Forms;

namespace CatSit.UI;

public partial class SettingsWindow : Window
{
    private readonly MainViewModel _viewModel;

    public SettingsWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;
    }

    public void ShowSection(string? tag)
    {
        if (tag is null) return;
        foreach (var item in SectionList.Items.OfType<ListBoxItem>())
        {
            if ((string?)item.Tag == tag)
            {
                SectionList.SelectedItem = item;
                return;
            }
        }
    }

    private void OnSectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (GeneralPanel is null) return;
        var tag = (SectionList.SelectedItem as ListBoxItem)?.Tag as string ?? "General";
        GeneralPanel.Visibility = Visible(tag, "General");
        DetectionPanel.Visibility = Visible(tag, "Detection");
        CatPanel.Visibility = Visible(tag, "Cat");
        SoundsPanel.Visibility = Visible(tag, "Sounds");
        ActivityPanel.Visibility = Visible(tag, "Activity");
    }

    private static Visibility Visible(string tag, string section) =>
        tag == section ? Visibility.Visible : Visibility.Collapsed;

    private void OnPickFurColor(object sender, RoutedEventArgs e)
    {
        var current = _viewModel.CustomFurColor;
        using var dialog = new WinForms.ColorDialog
        {
            FullOpen = true,
            Color = System.Drawing.Color.FromArgb(unchecked((int)current))
        };
        if (dialog.ShowDialog() != WinForms.DialogResult.OK) return;
        _viewModel.CustomFurColor = (uint)dialog.Color.ToArgb();
        _viewModel.UseCustomFur = true;
    }

    private void OnPreviewLock(object sender, RoutedEventArgs e) => _viewModel.PreviewLockSound();

    private void OnPreviewCat(object sender, RoutedEventArgs e) => _viewModel.PreviewCatSound();

    private void OnPreviewDeterrent(object sender, RoutedEventArgs e) => _viewModel.PreviewDeterrentSound();

    private void OnPickLockFile(object sender, RoutedEventArgs e)
    {
        var path = PickWav();
        if (path is null) return;
        _viewModel.CustomLockSoundPath = path;
        _viewModel.SelectedLockSound = _viewModel.LockSounds.First(option => option.Value == LockSound.Custom);
    }

    private void OnPickCatFile(object sender, RoutedEventArgs e)
    {
        var path = PickWav();
        if (path is null) return;
        _viewModel.CustomCatSoundPath = path;
        _viewModel.SelectedCatSound = _viewModel.CatSounds.First(option => option.Value == CatSound.Custom);
    }

    private void OnPickDeterrentFile(object sender, RoutedEventArgs e)
    {
        var path = PickWav();
        if (path is null) return;
        _viewModel.CustomDeterrentSoundPath = path;
        _viewModel.SelectedDeterrentSound = _viewModel.DeterrentSounds.First(option => option.Value == DeterrentSound.Custom);
    }

    private void OnClearActivity(object sender, RoutedEventArgs e) => _viewModel.ClearActivity();

    private void OnCreditClicked(object sender, RoutedEventArgs e) =>
        Process.Start(new ProcessStartInfo("https://github.com/glvckoma") { UseShellExecute = true });

    private void OnViewUpdateClicked(object sender, MouseButtonEventArgs e)
    {
        if (_viewModel.UpdateReleaseUrl is { } url)
        {
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }
    }

    private static string? PickWav()
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "Wave audio (*.wav)|*.wav",
            Title = "Choose a sound"
        };
        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }
}
