using System.Windows;
using CatSit.Interop;
using CatSit.Settings;
using CatSit.UI;

namespace CatSit;

public partial class App : Application
{
    private InputHookManager? _hooks;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        var store = new SettingsStore();
        var settings = store.Load();
        var viewModel = new MainViewModel(store, settings);
        viewModel.ToggleRequested += () => viewModel.IsLocked = !viewModel.IsLocked;
        _hooks = new InputHookManager(Dispatcher);
        _hooks.Start();
#if DEBUG
        _hooks.KeyEvent += k =>
            viewModel.LastInputDebug = $"key {(k.IsDown ? "down" : "up  ")} vk=0x{k.Vk:X2}{(k.Injected ? " inj" : "")}{(k.CtrlOrAltHeld ? " mod" : "")}";
        _hooks.MouseEvent += m =>
            viewModel.LastInputDebug = $"mouse {m.Button} {(m.IsDown ? "down" : "up")}{(m.Injected ? " inj" : "")}";
#endif
        var window = new MainWindow { DataContext = viewModel };
        MainWindow = window;
        window.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _hooks?.Dispose();
        base.OnExit(e);
    }
}
