using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CatSit.Core.Settings;

namespace CatSit.UI;

public sealed class HotkeyCaptureBox : TextBox
{
    public static readonly DependencyProperty ComboProperty = DependencyProperty.Register(
        nameof(Combo), typeof(HotkeyCombo), typeof(HotkeyCaptureBox),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnComboChanged));

    public HotkeyCaptureBox()
    {
        IsReadOnly = true;
        IsReadOnlyCaretVisible = false;
        IsUndoEnabled = false;
        Cursor = Cursors.Hand;
    }

    public HotkeyCombo? Combo
    {
        get => (HotkeyCombo?)GetValue(ComboProperty);
        set => SetValue(ComboProperty, value);
    }

    private static void OnComboChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var box = (HotkeyCaptureBox)d;
        box.Text = e.NewValue?.ToString() ?? string.Empty;
    }

    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        e.Handled = true;
        var key = e.Key == Key.System ? e.SystemKey : e.Key;
        if (key == Key.Escape)
        {
            Text = Combo?.ToString() ?? string.Empty;
            Keyboard.ClearFocus();
            return;
        }
        if (IsModifierKey(key)) return;
        var mods = HotkeyModifiers.None;
        if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control)) mods |= HotkeyModifiers.Ctrl;
        if (Keyboard.Modifiers.HasFlag(ModifierKeys.Alt)) mods |= HotkeyModifiers.Alt;
        if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift)) mods |= HotkeyModifiers.Shift;
        if (Keyboard.Modifiers.HasFlag(ModifierKeys.Windows)) mods |= HotkeyModifiers.Win;
        if (mods == HotkeyModifiers.None) return;
        Combo = new HotkeyCombo { Modifiers = mods, Vk = KeyInterop.VirtualKeyFromKey(key) };
    }

    private static bool IsModifierKey(Key key) => key is Key.LeftCtrl or Key.RightCtrl
        or Key.LeftAlt or Key.RightAlt
        or Key.LeftShift or Key.RightShift
        or Key.LWin or Key.RWin;
}
