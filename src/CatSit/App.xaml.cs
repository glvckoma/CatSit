using System.Windows;
using CatSit.Audio;
using CatSit.Core.Detection;
using CatSit.Core.Learning;
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
    private SettingsWindow? _settingsWindow;
    private ActivityStore? _activityStore;
    private TriggerLog? _log;
    private CatClassifier? _classifier;
    private bool _trayHintShown;

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
        _activityStore = new ActivityStore();
        var activity = _activityStore.Load();
        _log = new TriggerLog();
        _log.Load(activity.Triggers);
        _classifier = new CatClassifier();
        _classifier.Load(activity.Weights, activity.Bias, activity.Samples);
        var exclusions = new ExclusionList();
        exclusions.Load(settings.ExcludedSignatures);

        _audio = new AudioService(settings);
        var sprites = new SpriteBitmapCache();
        sprites.Rebuild(settings.Theme());
        _hooks = new InputHookManager(Dispatcher);
        _hooks.Start();
        _lockController = new LockController(_hooks, settings, _audio, _log, _classifier, exclusions);
        _overlay = new OverlayWindow(sprites, settings);
        _tray = new TrayIconService();

        var viewModel = new MainViewModel(store, settings, sprites, _audio, _log, exclusions, _classifier);
        var window = new MainWindow { DataContext = viewModel };
        var controller = _lockController;
        var overlay = _overlay;
        var tray = _tray;
        var audio = _audio;
        var activityStore = _activityStore;
        var log = _log;
        var classifier = _classifier;

        viewModel.ToggleRequested += controller.Toggle;
        viewModel.SettingsChanged += () =>
        {
            controller.ApplySettings();
            overlay.RefreshAppearance();
        };
        viewModel.AppearanceChanged += overlay.RefreshAppearance;
        viewModel.VerdictReported += (id, verdict) =>
        {
            controller.ReportVerdict(id, verdict);
            store.Save(settings);
            activityStore.Save(log, classifier);
        };
        viewModel.ActivityCleared += () => activityStore.Save(log, classifier);

        controller.StateChanged += (locked, reason) =>
        {
            viewModel.IsLocked = locked;
            tray.SetLocked(locked);
            if (locked) overlay.ShowCat(settings.ToggleHotkey.ToString());
            else overlay.WakeAndDismiss();
            if (locked && reason == LockReason.CatDetected && settings.ShowTrayAlerts)
            {
                tray.Notify("Cat detected", "Input is locked until you unlock it.");
            }
        };
        controller.TriggerRecorded += record =>
        {
            activityStore.Save(log, classifier);
            viewModel.RefreshActivity();
            if (!record.Locked && settings.ShowTrayAlerts)
            {
                tray.Notify("Cat detected", "Warn-only mode, input is still active.");
            }
        };

        overlay.CatClicked += () =>
        {
            audio.PlayCat();
            controller.Unlock(LockReason.CatClicked);
        };

        window.SettingsRequested += section => ShowSettings(viewModel, window, section);
        window.HiddenToTray += () =>
        {
            if (_trayHintShown) return;
            _trayHintShown = true;
            tray.Notify("CatSit is still running", "Find it in the tray. Right-click the icon to exit.");
        };
        window.Closed += (_, _) => Shutdown();
        tray.ToggleRequested += controller.Toggle;
        tray.OpenRequested += window.ShowFromTray;
        tray.ExitRequested += Shutdown;

        StartupRegistration.Sync(settings.StartWithWindows);
        window.Icon = AppIcon.Image;
        MainWindow = window;
        StartShowEventListener();
        window.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _lockController?.Unlock(LockReason.Manual);
        if (_activityStore is not null && _log is not null && _classifier is not null)
        {
            _activityStore.Save(_log, _classifier);
        }
        _tray?.Dispose();
        _hooks?.Dispose();
        _audio?.Dispose();
        _instanceMutex?.Dispose();
        _showEvent?.Dispose();
        base.OnExit(e);
    }

    private void ShowSettings(MainViewModel viewModel, Window owner, string? section)
    {
        if (_settingsWindow is null)
        {
            _settingsWindow = new SettingsWindow(viewModel) { Owner = owner };
            _settingsWindow.Closed += (_, _) => _settingsWindow = null;
            _settingsWindow.Show();
        }
        else
        {
            _settingsWindow.Activate();
        }
        viewModel.RefreshActivity();
        _settingsWindow.ShowSection(section);
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
