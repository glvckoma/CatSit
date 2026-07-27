namespace CatSit.Core.Detection;

public enum MouseButton
{
    Left,
    Right,
    Middle,
    X
}

public readonly record struct MouseEventInfo(MouseButton Button, bool IsDown, long TimestampMs, bool Injected);
