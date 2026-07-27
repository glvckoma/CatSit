namespace CatSit.Core.Detection;

public readonly record struct KeyEventInfo(int Vk, bool IsDown, long TimestampMs, bool Injected, bool CtrlOrAltHeld);
