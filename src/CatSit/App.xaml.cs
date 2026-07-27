using System.Windows;
using CatSit.Audio;
using CatSit.Interop;
using CatSit.Locking;
using CatSit.Overlay;
using CatSit.Settings;
using CatSit.UI;

namespace CatSit;

public partial class App : Application
{
    private InputHookManager? _hooks;
    private AudioService? _audio;
    private LockController? _lockController;
    private OverlayWindow? _overlay;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        var store = new SettingsStore();
        var settings = store.Load();
        var viewModel = new MainViewModel(store, settings);
        _audio = new AudioService(settings);
        _hooks = new InputHookManager(Dispatcher);
        _hooks.Start();
        _lockController = new LockController(_hooks, settings, _audio);
        _overlay = new OverlayWindow(new SpriteBitmapCache());
        viewModel.ToggleRequested += _lockController.Toggle;
        viewModel.SettingsChanged += _lockController.ApplySettings;
        var audio = _audio;
        var controller = _lockController;
        var overlay = _overlay;
        controller.StateChanged += (locked, _) =>
        {
            viewModel.IsLocked = locked;
            if (locked) overlay.ShowCat(settings.SecondsUntilSleep, settings.ToggleHotkey.ToString());
            else overlay.WakeAndDismiss();
        };
        overlay.CatClicked += () =>
        {
            audio.PlayMeow();
            controller.Unlock(LockReason.CatClicked);
        };
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
        _lockController?.Unlock(LockReason.Manual);
        _hooks?.Dispose();
        _audio?.Dispose();
        base.OnExit(e);
    }
}
