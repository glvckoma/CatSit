using CatSit.Core.Detection;

namespace CatSit.Interop;

public sealed class BlockedMouseButtonState
{
    private int _buttons;

    public bool ShouldSwallow(MouseButton button, bool isDown, bool blocking)
    {
        var mask = 1 << (int)button;
        if (isDown)
        {
            if (!blocking) return false;
            _buttons |= mask;
            return true;
        }

        var wasBlocked = (_buttons & mask) != 0;
        if (wasBlocked) _buttons &= ~mask;
        return blocking || wasBlocked;
    }
}
