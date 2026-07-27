namespace CatSit.Interop;

public static class ForegroundWindowProbe
{
    public static bool IsFullscreenAppActive()
    {
        var foreground = NativeMethods.GetForegroundWindow();
        if (foreground == IntPtr.Zero) return false;
        if (foreground == NativeMethods.GetShellWindow() || foreground == NativeMethods.GetDesktopWindow()) return false;
        var style = NativeMethods.GetWindowLongW(foreground, NativeMethods.GWL_STYLE);
        if ((style & NativeMethods.WS_CAPTION) == NativeMethods.WS_CAPTION) return false;
        if (!NativeMethods.GetWindowRect(foreground, out var rect)) return false;
        var screenWidth = NativeMethods.GetSystemMetrics(NativeMethods.SM_CXSCREEN);
        var screenHeight = NativeMethods.GetSystemMetrics(NativeMethods.SM_CYSCREEN);
        return rect.Right - rect.Left >= screenWidth && rect.Bottom - rect.Top >= screenHeight;
    }
}
