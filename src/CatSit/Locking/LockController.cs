using System.Diagnostics;
using System.Windows.Threading;
using CatSit.Audio;
using CatSit.Core.Detection;
using CatSit.Core.Learning;
using CatSit.Core.Settings;
using CatSit.Interop;

namespace CatSit.Locking;

public sealed class LockController
{
    private const int HoldPollMs = 200;

    private readonly InputHookManager _hooks;
    private readonly AppSettings _settings;
    private readonly AudioService _audio;
    private readonly TriggerLog _log;
    private readonly CatClassifier _classifier;
    private readonly ExclusionList _exclusions;
    private readonly TriggerFilter _filter;
    private readonly DispatcherTimer _holdTimer;
    private readonly DispatcherTimer _autoUnlockTimer;
    private readonly DispatcherTimer? _debugUnlockTimer;
    private KeyboardCatDetector _keyboardDetector;
    private MouseCatDetector _mouseDetector;

    public LockController(
        InputHookManager hooks,
        AppSettings settings,
        AudioService audio,
        TriggerLog log,
        CatClassifier classifier,
        ExclusionList exclusions)
    {
        _hooks = hooks;
        _settings = settings;
        _audio = audio;
        _log = log;
        _classifier = classifier;
        _exclusions = exclusions;
        _filter = new TriggerFilter(exclusions, classifier);
        var profile = SensitivityProfile.FromLevel(settings.Sensitivity);
        _keyboardDetector = new KeyboardCatDetector(profile);
        _mouseDetector = new MouseCatDetector(profile);
        _hooks.HotkeyToggled += OnHotkeyToggled;
        _hooks.KeyEvent += OnKeyEvent;
        _hooks.MouseEvent += OnMouseEvent;
        _holdTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(HoldPollMs) };
        _holdTimer.Tick += (_, _) => PollHold();
        _holdTimer.Start();
        _autoUnlockTimer = new DispatcherTimer();
        _autoUnlockTimer.Tick += (_, _) => Unlock(LockReason.AutoTimeout);
        if (Debugger.IsAttached)
        {
            _debugUnlockTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(30) };
            _debugUnlockTimer.Tick += (_, _) => Unlock(LockReason.Manual);
        }
        ApplySettings();
    }

    public bool IsLocked { get; private set; }

    public event Action<bool, LockReason>? StateChanged;
    public event Action<TriggerRecord>? TriggerRecorded;

    public void ApplySettings()
    {
        _hooks.ToggleHotkey = _settings.ToggleHotkey;
        _hooks.HotkeyLocksKeyboard = _settings.LockKeyboard && !_settings.WarnOnly;
        _hooks.HotkeyLocksMouse = _settings.LockMouse && !_settings.WarnOnly && !Debugger.IsAttached;
        _hooks.AllowMouseMovementWhileBlocked = _settings.ClickToDismiss;
        if (IsLocked)
        {
            _hooks.BlockKeyboard = _settings.LockKeyboard && !_settings.WarnOnly;
            _hooks.BlockMouse = _settings.LockMouse && !_settings.WarnOnly && !Debugger.IsAttached;
        }
        var profile = SensitivityProfile.FromLevel(_settings.Sensitivity);
        _keyboardDetector = new KeyboardCatDetector(profile);
        _mouseDetector = new MouseCatDetector(profile);
        _exclusions.Load(_settings.ExcludedSignatures);
        _filter.DetectionEnabled = _settings.DetectionEnabled;
        _filter.LearningEnabled = _settings.LearningEnabled;
    }

    public void Toggle()
    {
        if (IsLocked) Unlock(LockReason.Manual);
        else Lock(LockReason.Manual);
    }

    public void Lock(LockReason reason)
    {
        if (IsLocked) return;
        _hooks.BlockKeyboard = _settings.LockKeyboard;
        _hooks.BlockMouse = _settings.LockMouse && !Debugger.IsAttached;
        if (!_hooks.BlockKeyboard && !_hooks.BlockMouse) return;
        SetLocked(true, reason);
    }

    public void Unlock(LockReason reason)
    {
        if (!IsLocked) return;
        _hooks.BlockKeyboard = false;
        _hooks.BlockMouse = false;
        SetLocked(false, reason);
    }

    public void ReportVerdict(string triggerId, TriggerVerdict verdict)
    {
        var record = _log.SetVerdict(triggerId, verdict);
        if (record is null) return;
        if (verdict == TriggerVerdict.FalseAlarm && !string.IsNullOrEmpty(record.Signature))
        {
            _exclusions.Add(record.Signature);
            _settings.ExcludedSignatures = [.. _exclusions.Signatures];
        }
        else if (verdict == TriggerVerdict.ConfirmedCat && !string.IsNullOrEmpty(record.Signature)
            && !_log.Entries.Any(entry => entry.Id != record.Id
                && entry.Signature == record.Signature
                && entry.Verdict == TriggerVerdict.FalseAlarm))
        {
            _exclusions.Remove(record.Signature);
            _settings.ExcludedSignatures = [.. _exclusions.Signatures];
        }
        _classifier.SetLabel(record.Id, record.Features, verdict == TriggerVerdict.ConfirmedCat);
    }

    private void OnKeyEvent(KeyEventInfo e)
    {
        if (!_settings.DetectKeyboardInput || !Accepting()) return;
        Handle(_keyboardDetector.Observe(e));
    }

    private void OnMouseEvent(MouseEventInfo e)
    {
        if (!_settings.DetectMouseInput || !Accepting()) return;
        Handle(_mouseDetector.Observe(e));
    }

    private void PollHold()
    {
        if (!_settings.DetectKeyboardInput || !Accepting()) return;
        Handle(_keyboardDetector.Poll(Environment.TickCount64));
    }

    private bool Accepting()
    {
        _filter.Suspended = _settings.PauseInFullscreen && ForegroundWindowProbe.IsFullscreenAppActive();
        return _filter.AcceptsInput(Environment.TickCount64);
    }

    private void Handle(CatSignal? signal)
    {
        if (signal is null) return;
        var decision = _filter.Evaluate(signal);
        if (decision == TriggerDecision.ExactExclusion) return;
        if (decision == TriggerDecision.LearnedSuppression)
        {
            var suppressed = _log.Add(
                signal,
                locked: false,
                DateTimeOffset.UtcNow,
                Guid.NewGuid().ToString("N"),
                suppressedByLearning: true);
            _keyboardDetector.Reset();
            _mouseDetector.Reset();
            _filter.NotifySuppressed(Environment.TickCount64);
            TriggerRecorded?.Invoke(suppressed);
            return;
        }
        var shouldLock = !_settings.WarnOnly;
        var record = _log.Add(signal, shouldLock, DateTimeOffset.UtcNow, Guid.NewGuid().ToString("N"));
        _audio.PlayDeterrent();
        TriggerRecorded?.Invoke(record);
        if (shouldLock) Lock(LockReason.CatDetected);
    }

    private void OnHotkeyToggled(bool nowLocked)
    {
        if (nowLocked == IsLocked) return;
        SetLocked(nowLocked, LockReason.Hotkey);
    }

    private void SetLocked(bool locked, LockReason reason)
    {
        IsLocked = locked;
        _filter.Locked = locked;
        _keyboardDetector.Reset();
        _mouseDetector.Reset();
        if (locked)
        {
            _audio.PlayLock();
            _debugUnlockTimer?.Start();
            if (_settings.AutoUnlockMinutes > 0)
            {
                _autoUnlockTimer.Interval = TimeSpan.FromMinutes(_settings.AutoUnlockMinutes);
                _autoUnlockTimer.Start();
            }
        }
        else
        {
            _filter.NotifyUnlocked(Environment.TickCount64);
            _debugUnlockTimer?.Stop();
            _autoUnlockTimer.Stop();
        }
        StateChanged?.Invoke(locked, reason);
    }
}
