using System.Runtime.InteropServices;
using System.Windows.Threading;
using CatSit.Core.Detection;
using CatSit.Core.Hotkeys;

namespace CatSit.Interop;

public sealed class InputHookManager : IDisposable
{
    private readonly HookThread _hookThread = new();
    private readonly Dispatcher _appDispatcher;
    private NativeMethods.HookProc? _keyboardProc;
    private NativeMethods.HookProc? _mouseProc;
    private IntPtr _keyboardHook;
    private IntPtr _mouseHook;
    private volatile bool _blockKeyboard;
    private volatile bool _blockMouse;
    private bool _started;
    private int _modifierState;

    public InputHookManager(Dispatcher appDispatcher)
    {
        _appDispatcher = appDispatcher;
    }

    public event Action<KeyEventInfo>? KeyEvent;
    public event Action<MouseEventInfo>? MouseEvent;

    public bool BlockKeyboard
    {
        get => _blockKeyboard;
        set => _blockKeyboard = value;
    }

    public bool BlockMouse
    {
        get => _blockMouse;
        set => _blockMouse = value;
    }

    public void Start()
    {
        _hookThread.Start();
        _hookThread.Dispatcher.Invoke(() =>
        {
            _keyboardProc = KeyboardCallback;
            _mouseProc = MouseCallback;
            var module = NativeMethods.GetModuleHandleW(null);
            _keyboardHook = NativeMethods.SetWindowsHookExW(NativeMethods.WH_KEYBOARD_LL, _keyboardProc, module, 0);
            _mouseHook = NativeMethods.SetWindowsHookExW(NativeMethods.WH_MOUSE_LL, _mouseProc, module, 0);
        });
        if (_keyboardHook == IntPtr.Zero || _mouseHook == IntPtr.Zero)
        {
            throw new InvalidOperationException("Failed to install input hooks");
        }
        _started = true;
    }

    public void Dispose()
    {
        if (!_started) return;
        _started = false;
        _hookThread.Dispatcher.Invoke(() =>
        {
            if (_keyboardHook != IntPtr.Zero) NativeMethods.UnhookWindowsHookEx(_keyboardHook);
            if (_mouseHook != IntPtr.Zero) NativeMethods.UnhookWindowsHookEx(_mouseHook);
            _keyboardHook = IntPtr.Zero;
            _mouseHook = IntPtr.Zero;
        });
        _hookThread.Stop();
    }

    private IntPtr KeyboardCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode < 0) return NativeMethods.CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
        var msg = wParam.ToInt32();
        var isDown = msg is NativeMethods.WM_KEYDOWN or NativeMethods.WM_SYSKEYDOWN;
        var isUp = msg is NativeMethods.WM_KEYUP or NativeMethods.WM_SYSKEYUP;
        if (!isDown && !isUp) return NativeMethods.CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
        var data = Marshal.PtrToStructure<NativeMethods.KBDLLHOOKSTRUCT>(lParam);
        var vk = (int)data.vkCode;
        var injected = (data.flags & NativeMethods.LLKHF_INJECTED) != 0;
        if (!injected) _modifierState = ModifierTracker.Update(_modifierState, vk, isDown);
        var handler = KeyEvent;
        if (handler is not null)
        {
            var info = new KeyEventInfo(vk, isDown, Environment.TickCount64, injected, ModifierTracker.CtrlOrAltHeld(_modifierState));
            _appDispatcher.BeginInvoke(handler, info);
        }
        if (_blockKeyboard && !injected) return (IntPtr)1;
        return NativeMethods.CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
    }

    private IntPtr MouseCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode < 0) return NativeMethods.CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
        var data = Marshal.PtrToStructure<NativeMethods.MSLLHOOKSTRUCT>(lParam);
        var injected = (data.flags & NativeMethods.LLMHF_INJECTED) != 0;
        var msg = wParam.ToInt32();
        MouseButton? button = msg switch
        {
            NativeMethods.WM_LBUTTONDOWN or NativeMethods.WM_LBUTTONUP => MouseButton.Left,
            NativeMethods.WM_RBUTTONDOWN or NativeMethods.WM_RBUTTONUP => MouseButton.Right,
            NativeMethods.WM_MBUTTONDOWN or NativeMethods.WM_MBUTTONUP => MouseButton.Middle,
            NativeMethods.WM_XBUTTONDOWN or NativeMethods.WM_XBUTTONUP => MouseButton.X,
            _ => null
        };
        if (button is not null)
        {
            var isDown = msg is NativeMethods.WM_LBUTTONDOWN or NativeMethods.WM_RBUTTONDOWN
                or NativeMethods.WM_MBUTTONDOWN or NativeMethods.WM_XBUTTONDOWN;
            var handler = MouseEvent;
            if (handler is not null)
            {
                var info = new MouseEventInfo(button.Value, isDown, Environment.TickCount64, injected);
                _appDispatcher.BeginInvoke(handler, info);
            }
        }
        if (_blockMouse && !injected) return (IntPtr)1;
        return NativeMethods.CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
    }
}
