namespace CatSit.Core.Audio;

public enum LockSound
{
    None,
    Chime,
    Beep,
    Thud,
    Alarm,
    Custom
}

public enum CatSound
{
    None,
    Meow,
    Chirp,
    Trill,
    Custom
}

public enum DeterrentSound
{
    None,
    Hiss,
    Shake,
    Custom
}

public static class SoundNames
{
    public static string For(LockSound sound) => sound switch
    {
        LockSound.None => "Silent",
        LockSound.Chime => "Chime",
        LockSound.Beep => "Beep",
        LockSound.Thud => "Thud",
        LockSound.Alarm => "Alarm",
        LockSound.Custom => "Custom file",
        _ => sound.ToString()
    };

    public static string For(CatSound sound) => sound switch
    {
        CatSound.None => "Silent",
        CatSound.Meow => "Meow",
        CatSound.Chirp => "Chirp",
        CatSound.Trill => "Trill",
        CatSound.Custom => "Custom file",
        _ => sound.ToString()
    };

    public static string For(DeterrentSound sound) => sound switch
    {
        DeterrentSound.None => "Silent",
        DeterrentSound.Hiss => "Hiss",
        DeterrentSound.Shake => "Shake can",
        DeterrentSound.Custom => "Custom file",
        _ => sound.ToString()
    };
}
