<p align="center">
  <img src="docs/hero.png" width="700" alt="CatSit">
</p>

<h1 align="center">CatSit</h1>

<p align="center">
  Locks your keyboard and mouse when your cat walks across them, and shows an animated<br>
  pixel-art cat until you tap a hotkey or click it away.
</p>

<p align="center">
  <img src="https://img.shields.io/github/license/glvckoma/CatSit" alt="License">
  <img src="https://img.shields.io/badge/.NET-8.0-512BD4" alt=".NET 8">
  <img src="https://img.shields.io/badge/platform-Windows-0078D6" alt="Windows">
  <img src="https://img.shields.io/github/actions/workflow/status/glvckoma/CatSit/ci.yml?branch=main" alt="Build">
</p>

![CatSit demo](docs/demo.gif)

## What it is

CatSit watches your keyboard and mouse for the kind of input a cat produces (paws mashing keys, sitting on the keyboard, batting the mouse) and locks input the moment it's confident, showing an animated cat on top of everything until you either press a hotkey or click the cat to send it away. It's a Windows desktop app, not a browser extension or background service.

## Quick start

**Download** (no signup, no installer prompts for a runtime — the exe is self-contained):

1. Grab the latest installer or portable exe from the [Releases page](https://github.com/glvckoma/CatSit/releases).
2. Run it. CatSit starts in the tray.
3. Tell your cat to have at it.

**Build from source:**

```bash
git clone https://github.com/glvckoma/CatSit.git
cd CatSit
dotnet build CatSit.sln -c Release
dotnet test tests/CatSit.Core.Tests
```

Publish a self-contained single-file build yourself:

```bash
dotnet publish src/CatSit/CatSit.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
```

Or build the installer (see [installer/README.md](installer/README.md)).

## How detection works

Five rules, all scaled by a single 1-10 sensitivity slider:

- **Paw cluster** — several adjacent keys pressed at once
- **Mash burst** — a rapid burst of distinct keys in a short window with high adjacency
- **Sustained hold** — 2+ keys held down past a duration (shorter if more keys, or if they're neighbors)
- **Click burst** — rapid mouse clicks per second
- **Both buttons held** — left and right mouse held together (higher sensitivity only)

## Adaptive learning

CatSit keeps a rolling log of the last 50 triggers. Mark any of them "Not a cat" in the Activity tab, and after 3+ corrections a small built-in classifier starts suppressing repeats of that same pattern. It's a from-scratch logistic-regression model, no ML framework involved.

## Customize your cat

Seven breed presets — Orange Tabby, Gray Tabby, Tuxedo, Siamese, Black, White, Calico — plus a custom fur-tint mode where picking one color derives the stripes and belly automatically.

## Sound

All audio is synthesized at runtime; no `.wav` files are shipped. Lock sound (Chime / Beep / Thud / Alarm, or silent), cat sound (Meow / Chirp / Trill, or silent), and deterrent sound (Hiss / Shake, or silent) each also support pointing at your own custom `.wav` file, plus a volume slider and an in-app preview button.

## Settings tour

| General | Detection |
|---|---|
| ![General settings](docs/screenshots/settings-general.png) | ![Detection settings](docs/screenshots/settings-detection.png) |
| Lock keyboard/mouse, hotkey, Start with Windows, close-to-tray, update checks, unlock hint, auto-unlock timer. | The five detection rules, warn-only mode, pause in fullscreen, the learning toggle, and the sensitivity slider. |

| Cat | Sounds |
|---|---|
| ![Cat settings](docs/screenshots/settings-cat.png) | ![Sounds settings](docs/screenshots/settings-sounds.png) |
| Breed picker, custom fur color, size, entry edge, sleep timer. | Volume, and a picker/preview/file-chooser for each of the three sound categories. |

**Activity** — the trigger log with "Was a cat" / "Not a cat" feedback, which drives the learning classifier:

![Activity settings](docs/screenshots/settings-activity.png)

## Updates

CatSit checks GitHub Releases once per launch (toggle it off in Settings > General) and always asks before doing anything — Download now, Remind me later, or Skip this version. Nothing installs itself without you saying so.

## Why no external assets and no telemetry

Zero third-party NuGet packages in the shipped app or its core library. Every pixel of the cat and the app icon is hand-authored as C# string-grid data, rasterized at runtime, not loaded from image files. Every sound is synthesized at runtime, not a bundled `.wav`. The app makes no network calls except the optional update check above, and reports nothing anywhere. The only registry write is the explicitly-toggled Start-with-Windows entry.

## Contributing

Contributions welcome — see [CONTRIBUTING.md](CONTRIBUTING.md).

## License

MIT — see [LICENSE](LICENSE).

Made by [glvckoma](https://github.com/glvckoma).
