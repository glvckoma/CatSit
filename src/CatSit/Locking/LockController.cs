using System.Diagnostics;
using System.Windows.Threading;
using CatSit.Audio;
using CatSit.Core.Settings;
using CatSit.Interop;

namespace CatSit.Locking;

public sealed class LockController
{
    private readonly InputHookManager _hooks;
    private readonly AppSettings _settings;
    private readonly AudioService _audio;
    private readonly DispatcherTimer? _debugUnlockTimer;

    public LockController(InputHookManager hooks, AppSettings settings, AudioService audio)
    {
        _hooks = hooks;
        _settings = settings;
        _audio = audio;
        _hooks.HotkeyToggled += OnHotkeyToggled;
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

    private void OnHotkeyToggled(bool nowLocked)
    {
        if (nowLocked == IsLocked) return;
        SetLocked(nowLocked, LockReason.Hotkey);
    }

    private void SetLocked(bool locked, LockReason reason)
    {
        IsLocked = locked;
        if (locked)
        {
            _audio.PlayLock();
            _debugUnlockTimer?.Start();
        }
        else
        {
            _debugUnlockTimer?.Stop();
        }
        StateChanged?.Invoke(locked, reason);
    }
}
