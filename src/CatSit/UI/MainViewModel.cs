using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using CatSit.Core.Settings;
using CatSit.Settings;

namespace CatSit.UI;

public sealed class MainViewModel : INotifyPropertyChanged
{
    private readonly SettingsStore _store;
    private bool _isLocked;
    private string _lastInputDebug = string.Empty;

    public MainViewModel(SettingsStore store, AppSettings settings)
    {
        _store = store;
        Settings = settings;
        ToggleLockCommand = new RelayCommand(() => ToggleRequested?.Invoke());
    }

    public AppSettings Settings { get; }

    public event PropertyChangedEventHandler? PropertyChanged;
    public event Action? ToggleRequested;
    public event Action? SettingsChanged;

    public ICommand ToggleLockCommand { get; }

    public bool IsLocked
    {
        get => _isLocked;
        set
        {
            if (_isLocked == value) return;
            _isLocked = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(StatusText));
            OnPropertyChanged(nameof(ToggleButtonText));
        }
    }

    public string LastInputDebug
    {
        get => _lastInputDebug;
        set
        {
            _lastInputDebug = value;
            OnPropertyChanged();
        }
    }

    public string StatusText => IsLocked ? "Locked" : "Unlocked";
    public string ToggleButtonText => IsLocked ? "Disable Cat Lock" : "Enable Cat Lock";
    public string HotkeyText => Settings.ToggleHotkey.ToString();

    public bool LockKeyboard
    {
        get => Settings.LockKeyboard;
        set { Settings.LockKeyboard = value; SaveAndRaise(); }
    }

    public bool LockMouse
    {
        get => Settings.LockMouse;
        set { Settings.LockMouse = value; SaveAndRaise(); }
    }

    public bool DetectionEnabled
    {
        get => Settings.DetectionEnabled;
        set { Settings.DetectionEnabled = value; SaveAndRaise(); }
    }

    public int Sensitivity
    {
        get => Settings.Sensitivity;
        set { Settings.Sensitivity = value; SaveAndRaise(); }
    }

    public int SecondsUntilSleep
    {
        get => Settings.SecondsUntilSleep;
        set { Settings.SecondsUntilSleep = value; SaveAndRaise(); }
    }

    public bool SoundsEnabled
    {
        get => Settings.SoundsEnabled;
        set { Settings.SoundsEnabled = value; SaveAndRaise(); }
    }

    public HotkeyCombo ToggleHotkey
    {
        get => Settings.ToggleHotkey;
        set
        {
            Settings.ToggleHotkey = value;
            SaveAndRaise();
            OnPropertyChanged(nameof(HotkeyText));
        }
    }

    private void SaveAndRaise([CallerMemberName] string? name = null)
    {
        _store.Save(Settings);
        OnPropertyChanged(name);
        SettingsChanged?.Invoke();
    }

    private void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
