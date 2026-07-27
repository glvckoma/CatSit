using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CatSit.Audio;
using CatSit.Core.Audio;
using CatSit.Core.Detection;
using CatSit.Core.Learning;
using CatSit.Core.Settings;
using CatSit.Core.Sprites;
using CatSit.Overlay;
using CatSit.Settings;

namespace CatSit.UI;

public sealed class MainViewModel : INotifyPropertyChanged
{
    private readonly SettingsStore _store;
    private readonly SpriteBitmapCache _sprites;
    private readonly AudioService _audio;
    private readonly TriggerLog _log;
    private readonly ExclusionList _exclusions;
    private readonly CatClassifier _classifier;
    private bool _isLocked;

    public MainViewModel(
        SettingsStore store,
        AppSettings settings,
        SpriteBitmapCache sprites,
        AudioService audio,
        TriggerLog log,
        ExclusionList exclusions,
        CatClassifier classifier)
    {
        _store = store;
        _sprites = sprites;
        _audio = audio;
        _log = log;
        _exclusions = exclusions;
        _classifier = classifier;
        Settings = settings;
        ToggleLockCommand = new RelayCommand(() => ToggleRequested?.Invoke());
        Breeds = [.. Enum.GetValues<CatBreed>().Select(b => new NamedOption<CatBreed>(CatTheme.DisplayName(b), b))];
        Edges =
        [
            new NamedOption<EntryEdge>("Either side", EntryEdge.Random),
            new NamedOption<EntryEdge>("The left", EntryEdge.Left),
            new NamedOption<EntryEdge>("The right", EntryEdge.Right)
        ];
        LockSounds = [.. Enum.GetValues<LockSound>().Select(s => new NamedOption<LockSound>(SoundNames.For(s), s))];
        CatSounds = [.. Enum.GetValues<CatSound>().Select(s => new NamedOption<CatSound>(SoundNames.For(s), s))];
        DeterrentSounds = [.. Enum.GetValues<DeterrentSound>().Select(s => new NamedOption<DeterrentSound>(SoundNames.For(s), s))];
        RefreshActivity();
    }

    public AppSettings Settings { get; }

    public event PropertyChangedEventHandler? PropertyChanged;
    public event Action? ToggleRequested;
    public event Action? SettingsChanged;
    public event Action? AppearanceChanged;
    public event Action<string, TriggerVerdict>? VerdictReported;
    public event Action? ActivityCleared;

    public ICommand ToggleLockCommand { get; }

    public IReadOnlyList<NamedOption<CatBreed>> Breeds { get; }
    public IReadOnlyList<NamedOption<EntryEdge>> Edges { get; }
    public IReadOnlyList<NamedOption<LockSound>> LockSounds { get; }
    public IReadOnlyList<NamedOption<CatSound>> CatSounds { get; }
    public IReadOnlyList<NamedOption<DeterrentSound>> DeterrentSounds { get; }

    public ObservableCollection<TriggerViewModel> Triggers { get; } = [];
    public ObservableCollection<ExclusionViewModel> Exclusions { get; } = [];

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

    public string StatusText => IsLocked ? "Locked" : "Unlocked";

    public string ToggleButtonText => IsLocked ? "Unlock now" : "Lock input";

    public string HotkeyText => Settings.ToggleHotkey.ToString();

    public string ModeSummary => Settings.WarnOnly
        ? "Warn only, never locks"
        : Settings switch
        {
            { LockKeyboard: true, LockMouse: true } => "Locks keyboard and mouse",
            { LockMouse: true } => "Locks the mouse",
            _ => "Locks the keyboard"
        };

    public string DetectionSummary => Settings.DetectionEnabled
        ? $"Watching for cats · {SensitivityWord}"
        : "Cat detection is off";

    public string ActivitySummary
    {
        get
        {
            var count = _log.Entries.Count;
            var pending = _log.Entries.Count(r => r.Verdict == TriggerVerdict.Unreviewed);
            if (count == 0) return "No triggers recorded yet";
            return pending > 0 ? $"{count} recorded · {pending} awaiting your verdict" : $"{count} recorded";
        }
    }

    public string SensitivityWord => Settings.Sensitivity switch
    {
        <= 2 => "relaxed",
        <= 4 => "forgiving",
        <= 6 => "balanced",
        <= 8 => "eager",
        _ => "twitchy"
    };

    public string SensitivityExplanation
    {
        get
        {
            var profile = SensitivityProfile.FromLevel(Settings.Sensitivity);
            var holdSeconds = profile.HoldMsForTwoKeys / 1000.0;
            return $"Right now: {profile.ConcurrentKeys} keys pressed together trip it instantly, " +
                   $"and two keys held for about {holdSeconds:0.#} seconds counts as a cat sitting on the keyboard " +
                   $"(sooner if the keys are neighbors or more keys are down).";
        }
    }

    public string LearningStatus => Settings.LearningEnabled
        ? _classifier.IsActive
            ? $"Learning from {_classifier.SampleCount} labeled trigger(s)."
            : $"Needs {CatClassifier.MinSamplesBeforeVeto} labeled triggers before it starts filtering. Label them under Activity."
        : "Learning is off, only the sensitivity rules apply.";

    public string AutoUnlockText => Settings.AutoUnlockMinutes == 0 ? "Never" : $"{Settings.AutoUnlockMinutes} min";

    public string SleepDelayText => $"{Settings.SecondsUntilSleep}s";

    public string VolumeText => $"{Settings.Volume}%";

    public string LockSoundPathText => PathText(Settings.LockSound == LockSound.Custom, Settings.CustomLockSoundPath);

    public string CatSoundPathText => PathText(Settings.CatSound == CatSound.Custom, Settings.CustomCatSoundPath);

    public string DeterrentSoundPathText => PathText(Settings.DeterrentSound == DeterrentSound.Custom, Settings.CustomDeterrentSoundPath);

    public ImageSource CatPreview => _sprites.Preview(Settings.Theme());

    public Brush CustomFurBrush => new SolidColorBrush(ToColor(Settings.CustomFurColor));

    public Visibility EmptyActivityVisibility => Triggers.Count == 0 ? Visibility.Visible : Visibility.Collapsed;

    public Visibility EmptyExclusionsVisibility => Exclusions.Count == 0 ? Visibility.Visible : Visibility.Collapsed;

    public bool LockKeyboard
    {
        get => Settings.LockKeyboard;
        set
        {
            if (!value && !Settings.LockMouse) { OnPropertyChanged(); return; }
            Settings.LockKeyboard = value;
            SaveAndRaise();
            OnPropertyChanged(nameof(ModeSummary));
        }
    }

    public bool LockMouse
    {
        get => Settings.LockMouse;
        set
        {
            if (!value && !Settings.LockKeyboard) { OnPropertyChanged(); return; }
            Settings.LockMouse = value;
            SaveAndRaise();
            OnPropertyChanged(nameof(ModeSummary));
        }
    }

    public bool DetectionEnabled
    {
        get => Settings.DetectionEnabled;
        set { Settings.DetectionEnabled = value; SaveAndRaise(); OnPropertyChanged(nameof(DetectionSummary)); }
    }

    public bool WarnOnly
    {
        get => Settings.WarnOnly;
        set { Settings.WarnOnly = value; SaveAndRaise(); OnPropertyChanged(nameof(ModeSummary)); }
    }

    public bool PauseInFullscreen
    {
        get => Settings.PauseInFullscreen;
        set { Settings.PauseInFullscreen = value; SaveAndRaise(); }
    }

    public bool ShowTrayAlerts
    {
        get => Settings.ShowTrayAlerts;
        set { Settings.ShowTrayAlerts = value; SaveAndRaise(); }
    }

    public bool ShowUnlockHint
    {
        get => Settings.ShowUnlockHint;
        set { Settings.ShowUnlockHint = value; SaveAndRaise(); }
    }

    public bool ClickToDismiss
    {
        get => Settings.ClickToDismiss;
        set { Settings.ClickToDismiss = value; SaveAndRaise(); }
    }

    public bool LearningEnabled
    {
        get => Settings.LearningEnabled;
        set { Settings.LearningEnabled = value; SaveAndRaise(); OnPropertyChanged(nameof(LearningStatus)); }
    }

    public int AutoUnlockMinutes
    {
        get => Settings.AutoUnlockMinutes;
        set { Settings.AutoUnlockMinutes = value; SaveAndRaise(); OnPropertyChanged(nameof(AutoUnlockText)); }
    }

    public int Sensitivity
    {
        get => Settings.Sensitivity;
        set
        {
            Settings.Sensitivity = value;
            SaveAndRaise();
            OnPropertyChanged(nameof(SensitivityExplanation));
            OnPropertyChanged(nameof(DetectionSummary));
        }
    }

    public int SecondsUntilSleep
    {
        get => Settings.SecondsUntilSleep;
        set { Settings.SecondsUntilSleep = value; SaveAndRaise(); OnPropertyChanged(nameof(SleepDelayText)); }
    }

    public bool SoundsEnabled
    {
        get => Settings.SoundsEnabled;
        set { Settings.SoundsEnabled = value; SaveAndRaise(); }
    }

    public int Volume
    {
        get => Settings.Volume;
        set
        {
            Settings.Volume = value;
            SaveAndRaise();
            OnPropertyChanged(nameof(VolumeText));
            _audio.Rebuild();
        }
    }

    public NamedOption<LockSound> SelectedLockSound
    {
        get => LockSounds.First(option => option.Value == Settings.LockSound);
        set
        {
            Settings.LockSound = value.Value;
            SaveAndRaise();
            OnPropertyChanged(nameof(LockSoundPathText));
            _audio.Rebuild();
        }
    }

    public NamedOption<CatSound> SelectedCatSound
    {
        get => CatSounds.First(option => option.Value == Settings.CatSound);
        set
        {
            Settings.CatSound = value.Value;
            SaveAndRaise();
            OnPropertyChanged(nameof(CatSoundPathText));
            _audio.Rebuild();
        }
    }

    public NamedOption<DeterrentSound> SelectedDeterrentSound
    {
        get => DeterrentSounds.First(option => option.Value == Settings.DeterrentSound);
        set
        {
            Settings.DeterrentSound = value.Value;
            SaveAndRaise();
            OnPropertyChanged(nameof(DeterrentSoundPathText));
            _audio.Rebuild();
        }
    }

    public string? CustomLockSoundPath
    {
        get => Settings.CustomLockSoundPath;
        set { Settings.CustomLockSoundPath = value; SaveAndRaise(); OnPropertyChanged(nameof(LockSoundPathText)); _audio.Rebuild(); }
    }

    public string? CustomCatSoundPath
    {
        get => Settings.CustomCatSoundPath;
        set { Settings.CustomCatSoundPath = value; SaveAndRaise(); OnPropertyChanged(nameof(CatSoundPathText)); _audio.Rebuild(); }
    }

    public string? CustomDeterrentSoundPath
    {
        get => Settings.CustomDeterrentSoundPath;
        set { Settings.CustomDeterrentSoundPath = value; SaveAndRaise(); OnPropertyChanged(nameof(DeterrentSoundPathText)); _audio.Rebuild(); }
    }

    public NamedOption<CatBreed> SelectedBreed
    {
        get => Breeds.First(option => option.Value == Settings.Breed);
        set { Settings.Breed = value.Value; ApplyAppearance(); }
    }

    public NamedOption<EntryEdge> SelectedEdge
    {
        get => Edges.First(option => option.Value == Settings.EntryEdge);
        set { Settings.EntryEdge = value.Value; SaveAndRaise(); }
    }

    public bool UseCustomFur
    {
        get => Settings.UseCustomFur;
        set { Settings.UseCustomFur = value; ApplyAppearance(); }
    }

    public uint CustomFurColor
    {
        get => Settings.CustomFurColor;
        set { Settings.CustomFurColor = value; ApplyAppearance(); OnPropertyChanged(nameof(CustomFurBrush)); }
    }

    public int CatScale
    {
        get => Settings.CatScale;
        set { Settings.CatScale = value; SaveAndRaise(); AppearanceChanged?.Invoke(); }
    }

    public bool ShowZzz
    {
        get => Settings.ShowZzz;
        set { Settings.ShowZzz = value; SaveAndRaise(); }
    }

    public HotkeyCombo ToggleHotkey
    {
        get => Settings.ToggleHotkey;
        set { Settings.ToggleHotkey = value; SaveAndRaise(); OnPropertyChanged(nameof(HotkeyText)); }
    }

    public void PreviewLockSound()
    {
        if (Settings.LockSound == LockSound.Custom) _audio.PreviewFile(Settings.CustomLockSoundPath ?? string.Empty);
        else _audio.Preview(WavSynthesizer.For(Settings.LockSound));
    }

    public void PreviewCatSound()
    {
        if (Settings.CatSound == CatSound.Custom) _audio.PreviewFile(Settings.CustomCatSoundPath ?? string.Empty);
        else _audio.Preview(WavSynthesizer.For(Settings.CatSound));
    }

    public void PreviewDeterrentSound()
    {
        if (Settings.DeterrentSound == DeterrentSound.Custom) _audio.PreviewFile(Settings.CustomDeterrentSoundPath ?? string.Empty);
        else _audio.Preview(WavSynthesizer.For(Settings.DeterrentSound));
    }

    public void ClearActivity()
    {
        _log.Clear();
        ActivityCleared?.Invoke();
        RefreshActivity();
    }

    public void RefreshActivity()
    {
        Triggers.Clear();
        foreach (var record in _log.Entries)
        {
            Triggers.Add(new TriggerViewModel(record, (id, verdict) =>
            {
                VerdictReported?.Invoke(id, verdict);
                RefreshActivity();
            }));
        }
        Exclusions.Clear();
        foreach (var signature in _exclusions.Signatures.OrderBy(s => s))
        {
            Exclusions.Add(new ExclusionViewModel(signature, RemoveExclusion));
        }
        OnPropertyChanged(nameof(EmptyActivityVisibility));
        OnPropertyChanged(nameof(EmptyExclusionsVisibility));
        OnPropertyChanged(nameof(ActivitySummary));
        OnPropertyChanged(nameof(LearningStatus));
    }

    private void RemoveExclusion(string signature)
    {
        _exclusions.Remove(signature);
        Settings.ExcludedSignatures = [.. _exclusions.Signatures];
        _store.Save(Settings);
        SettingsChanged?.Invoke();
        RefreshActivity();
    }

    private void ApplyAppearance()
    {
        _sprites.Rebuild(Settings.Theme());
        SaveAndRaise();
        OnPropertyChanged(nameof(CatPreview));
        OnPropertyChanged(nameof(UseCustomFur));
        AppearanceChanged?.Invoke();
    }

    private static Color ToColor(uint argb) =>
        Color.FromArgb((byte)(argb >> 24), (byte)(argb >> 16), (byte)(argb >> 8), (byte)argb);

    private static string PathText(bool isCustom, string? path)
    {
        if (!isCustom) return "Using the built-in sound.";
        return string.IsNullOrWhiteSpace(path) ? "No file chosen yet." : path;
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
