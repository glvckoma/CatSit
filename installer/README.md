# Building the installer locally

Install the .NET 10 SDK and [Inno Setup 6.6 or later](https://jrsoftware.org/isinfo.php), then run:

```powershell
.\installer\Build-Installer.ps1
```

To override the version from `CatSit.csproj`:

```powershell
.\installer\Build-Installer.ps1 -Version 0.2.0
```

The script publishes a self-contained Windows x64 build and creates `installer\Output\CatSit-Setup-<version>.exe`.

The `AppId` GUID in `CatSit.iss` is permanent. It lets future versions upgrade in place and reuse the existing Add or Remove Programs entry.
