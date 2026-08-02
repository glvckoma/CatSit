using System.Diagnostics;
using System.Net.Http;
using System.Windows;
using CatSit.Audio;
using CatSit.Core.Detection;
using CatSit.Core.Learning;
using CatSit.Core.Settings;
using CatSit.Core.Updates;
using CatSit.Interop;
using CatSit.Locking;
using CatSit.Overlay;
using CatSit.Settings;
using CatSit.UI;
using CatSit.Updates;

namespace CatSit;

public partial class App : Application
{
    private const string MutexName = "CatSit.SingleInstance";
    private const string ShowEventName = "CatSit.ShowWindow";

    private static readonly HttpClient UpdateHttpClient = new() { Timeout = TimeSpan.FromSeconds(6) };

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
    private bool _updateCheckInProgress;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        var background = AppLaunchOptions.Parse(e.Args).Background;
        _instanceMutex = new Mutex(true, MutexName, out var isFirstInstance);
        if (!isFirstInstance)
        {
            if (!background) SignalExistingInstance();
            Shutdown();
            return;
        }
        _showEvent = new EventWaitHandle(false, EventResetMode.AutoReset, ShowEventName);

        var store = new SettingsStore();
        var settingsLoad = store.Load();
        var settings = settingsLoad.Value;
        _activityStore = new ActivityStore();
        var activityLoad = _activityStore.Load();
        var activity = activityLoad.Value;
        _log = new TriggerLog();
        _log.Load(activity.Triggers);
        _classifier = new CatClassifier();
        _classifier.LoadSamples(activity.Samples);
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
            viewModel.ReportPersistence(PersistFeedback(), PersistFeedback);
        };
        viewModel.ActivityCleared += () => viewModel.ReportPersistence(PersistFeedback(), PersistFeedback);

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
            viewModel.ReportPersistence(activityStore.Save(log, classifier), () => activityStore.Save(log, classifier));
            viewModel.RefreshActivity();
            if (!record.Locked && !record.SuppressedByLearning && settings.ShowTrayAlerts)
            {
                tray.Notify("Cat detected", "Warn-only mode, input is still active.");
            }
        };

        overlay.CatClicked += () =>
        {
            audio.PlayCat();
            controller.Unlock(LockReason.CatClicked);
        };
        _hooks.BlockedLeftClick += (x, y) =>
        {
            if (!settings.ClickToDismiss || !controller.IsLocked || !overlay.ContainsCatScreenPoint(x, y)) return;
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
        viewModel.ManualUpdateRequested += () => CheckForUpdatesAsync(
            viewModel,
            (Window?)_settingsWindow ?? window,
            store,
            settings,
            manual: true,
            showDialog: true);

        if (!settingsLoad.Status.Success) viewModel.ReportPersistence(settingsLoad.Status);
        if (!activityLoad.Status.Success) viewModel.ReportPersistence(activityLoad.Status);

        StartupRegistration.Sync(settings.StartWithWindows);
        window.Icon = AppIcon.Image;
        MainWindow = window;
        StartShowEventListener();
        if (!background) window.Show();

        _ = CheckForUpdatesAsync(viewModel, window, store, settings, manual: false, showDialog: !background);

        PersistenceResult PersistFeedback()
        {
            var settingsResult = store.Save(settings);
            return settingsResult.Success ? activityStore.Save(log, classifier) : settingsResult;
        }
    }

    private async Task CheckForUpdatesAsync(
        MainViewModel viewModel,
        Window owner,
        SettingsStore store,
        AppSettings settings,
        bool manual,
        bool showDialog)
    {
        if ((!manual && !settings.CheckForUpdatesAutomatically) || _updateCheckInProgress) return;
        _updateCheckInProgress = true;
        viewModel.IsCheckingForUpdates = true;
        if (manual) viewModel.UpdateCheckStatus = "Checking for updates...";
        try
        {
            if (!manual) await Task.Delay(TimeSpan.FromSeconds(3));
            var current = typeof(App).Assembly.GetName().Version ?? new Version(0, 1, 0);
            var skippedTag = manual ? null : settings.LastSkippedUpdateTag;
            var outcome = await UpdateChecker.CheckLatestAsync(UpdateHttpClient, current, skippedTag);
            if (outcome is null)
            {
                if (manual) viewModel.UpdateCheckStatus = "Could not check for updates. Try again later.";
                return;
            }
            if (outcome.Availability != UpdateAvailability.NewVersionAvailable)
            {
                if (manual) viewModel.UpdateCheckStatus = "CatSit is up to date.";
                return;
            }

            viewModel.SetUpdateAvailable(outcome.LatestTag, outcome.ReleaseHtmlUrl);
            viewModel.UpdateCheckStatus = $"Update {outcome.LatestTag} is available.";
            if (!showDialog)
            {
                _tray?.Notify("CatSit update available", $"Version {outcome.LatestTag} is ready to download.");
                return;
            }

            var dialog = new UpdateWindow(viewModel.VersionText, outcome.LatestTag) { Owner = owner };
            dialog.ShowDialog();
            switch (dialog.Choice)
            {
                case UpdateChoice.DownloadNow:
                    Process.Start(new ProcessStartInfo(outcome.ReleaseHtmlUrl) { UseShellExecute = true });
                    break;
                case UpdateChoice.SkipVersion:
                    settings.LastSkippedUpdateTag = outcome.LatestTag;
                    viewModel.ReportPersistence(store.Save(settings), () => store.Save(settings));
                    break;
            }
        }
        catch (Exception e) when (e is HttpRequestException or TaskCanceledException or InvalidOperationException)
        {
            if (manual) viewModel.UpdateCheckStatus = "Could not check for updates. Try again later.";
        }
        finally
        {
            _updateCheckInProgress = false;
            viewModel.IsCheckingForUpdates = false;
        }
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
        var showEvent = _showEvent ??= new EventWaitHandle(false, EventResetMode.AutoReset, ShowEventName);
        var listener = new Thread(() =>
        {
            while (showEvent.WaitOne())
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
        for (var attempt = 0; attempt < 20; attempt++)
        {
            try
            {
                using var showEvent = EventWaitHandle.OpenExisting(ShowEventName);
                showEvent.Set();
                return;
            }
            catch (WaitHandleCannotBeOpenedException)
            {
                Thread.Sleep(100);
            }
        }
    }
}
