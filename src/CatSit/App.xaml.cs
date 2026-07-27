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
    private const string MutexName = "CatSit.SingleInstance";
    private const string ShowEventName = "CatSit.ShowWindow";

    private Mutex? _instanceMutex;
    private EventWaitHandle? _showEvent;
    private InputHookManager? _hooks;
    private AudioService? _audio;
    private LockController? _lockController;
    private OverlayWindow? _overlay;
    private TrayIconService? _tray;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        _instanceMutex = new Mutex(true, MutexName, out var isFirstInstance);
        if (!isFirstInstance)
        {
            SignalExistingInstance();
            Shutdown();
            return;
        }
        var store = new SettingsStore();
        var settings = store.Load();
        var viewModel = new MainViewModel(store, settings);
        _audio = new AudioService(settings);
        _hooks = new InputHookManager(Dispatcher);
        _hooks.Start();
        _lockController = new LockController(_hooks, settings, _audio);
        var cache = new SpriteBitmapCache();
        _overlay = new OverlayWindow(cache);
        _tray = new TrayIconService();
        var window = new MainWindow { DataContext = viewModel };
        var audio = _audio;
        var controller = _lockController;
        var overlay = _overlay;
        var tray = _tray;
        viewModel.ToggleRequested += controller.Toggle;
        viewModel.SettingsChanged += controller.ApplySettings;
        controller.StateChanged += (locked, _) =>
        {
            viewModel.IsLocked = locked;
            tray.SetLocked(locked);
            if (locked) overlay.ShowCat(settings.SecondsUntilSleep, settings.ToggleHotkey.ToString());
            else overlay.WakeAndDismiss();
        };
        overlay.CatClicked += () =>
        {
            audio.PlayMeow();
            controller.Unlock(LockReason.CatClicked);
        };
        tray.ToggleRequested += controller.Toggle;
        tray.OpenRequested += window.ShowFromTray;
        tray.ExitRequested += Shutdown;
#if DEBUG
        _hooks.KeyEvent += k =>
            viewModel.LastInputDebug = $"key {(k.IsDown ? "down" : "up  ")} vk=0x{k.Vk:X2}{(k.Injected ? " inj" : "")}{(k.CtrlOrAltHeld ? " mod" : "")}";
        _hooks.MouseEvent += m =>
            viewModel.LastInputDebug = $"mouse {m.Button} {(m.IsDown ? "down" : "up")}{(m.Injected ? " inj" : "")}";
#endif
        window.Icon = cache.Get(Core.Animation.CatState.Sitting, 0, false);
        MainWindow = window;
        StartShowEventListener();
        window.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _lockController?.Unlock(LockReason.Manual);
        _tray?.Dispose();
        _hooks?.Dispose();
        _audio?.Dispose();
        _instanceMutex?.Dispose();
        _showEvent?.Dispose();
        base.OnExit(e);
    }

    private void StartShowEventListener()
    {
        _showEvent = new EventWaitHandle(false, EventResetMode.AutoReset, ShowEventName);
        var listener = new Thread(() =>
        {
            while (_showEvent.WaitOne())
            {
                Dispatcher.BeginInvoke(() => (MainWindow as MainWindow)?.ShowFromTray());
            }
        })
        {
            IsBackground = true,
            Name = "CatSit.ShowListener"
        };
        listener.Start();
    }

    private static void SignalExistingInstance()
    {
        try
        {
            using var showEvent = EventWaitHandle.OpenExisting(ShowEventName);
            showEvent.Set();
        }
        catch (WaitHandleCannotBeOpenedException)
        {
        }
    }
}
