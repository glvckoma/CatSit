using System.Diagnostics;
using System.Windows.Threading;
using CatSit.Audio;
using CatSit.Core.Detection;
using CatSit.Core.Settings;
using CatSit.Interop;

namespace CatSit.Locking;

public sealed class LockController
{
    private const int UnlockGraceMs = 3000;

    private readonly InputHookManager _hooks;
    private readonly AppSettings _settings;
    private readonly AudioService _audio;
    private readonly DispatcherTimer? _debugUnlockTimer;
    private KeyboardCatDetector _keyboardDetector;
    private MouseCatDetector _mouseDetector;
    private long _lastUnlockMs = long.MinValue;

    public LockController(InputHookManager hooks, AppSettings settings, AudioService audio)
    {
        _hooks = hooks;
        _settings = settings;
        _audio = audio;
        var profile = SensitivityProfile.FromLevel(settings.Sensitivity);
        _keyboardDetector = new KeyboardCatDetector(profile);
        _mouseDetector = new MouseCatDetector(profile);
        _hooks.HotkeyToggled += OnHotkeyToggled;
        _hooks.KeyEvent += OnKeyEvent;
        _hooks.MouseEvent += OnMouseEvent;
        if (Debugger.IsAttached)
        {
            _debugUnlockTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(30) };
            _debugUnlockTimer.Tick += (_, _) => Unlock(LockReason.Manual);
        }
        ApplySettings();
    }

    public bool IsLocked { get; private set; }

    public event Action<bool, LockReason>? StateChanged;

    public void ApplySettings()
    {
        _hooks.ToggleHotkey = _settings.ToggleHotkey;
        _hooks.HotkeyLocksKeyboard = _settings.LockKeyboard;
        _hooks.HotkeyLocksMouse = _settings.LockMouse && !Debugger.IsAttached;
        var profile = SensitivityProfile.FromLevel(_settings.Sensitivity);
        _keyboardDetector = new KeyboardCatDetector(profile);
        _mouseDetector = new MouseCatDetector(profile);
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

    private void OnKeyEvent(KeyEventInfo e)
    {
        if (!DetectionActive) return;
        if (_keyboardDetector.Observe(e)) Lock(LockReason.CatDetected);
    }

    private void OnMouseEvent(MouseEventInfo e)
    {
        if (!DetectionActive) return;
        if (_mouseDetector.Observe(e)) Lock(LockReason.CatDetected);
    }

    private bool DetectionActive =>
        !IsLocked
        && _settings.DetectionEnabled
        && Environment.TickCount64 - _lastUnlockMs > UnlockGraceMs;

    private void OnHotkeyToggled(bool nowLocked)
    {
        if (nowLocked == IsLocked) return;
        SetLocked(nowLocked, LockReason.Hotkey);
    }

    private void SetLocked(bool locked, LockReason reason)
    {
        IsLocked = locked;
        _keyboardDetector.Reset();
        _mouseDetector.Reset();
        if (locked)
        {
            _audio.PlayLock();
            _debugUnlockTimer?.Start();
        }
        else
        {
            _lastUnlockMs = Environment.TickCount64;
            _debugUnlockTimer?.Stop();
        }
        StateChanged?.Invoke(locked, reason);
    }
}
