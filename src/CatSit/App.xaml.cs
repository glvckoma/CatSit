using System.Windows;
using CatSit.Settings;
using CatSit.UI;

namespace CatSit;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        var store = new SettingsStore();
        var settings = store.Load();
        var viewModel = new MainViewModel(store, settings);
        viewModel.ToggleRequested += () => viewModel.IsLocked = !viewModel.IsLocked;
        var window = new MainWindow { DataContext = viewModel };
        MainWindow = window;
        window.Show();
    }
}
