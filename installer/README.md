# Building the installer locally

Requires [Inno Setup 6](https://jrsoftware.org/isinfo.php) (`ISCC.exe` on your PATH or use the full path below).

```powershell
dotnet publish ..\src\CatSit\CatSit.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o ..\src\CatSit\bin\publish

& "C:\Program Files (x86)\Inno Setup 6\ISCC.exe" CatSit.iss /DMyAppVersion=0.1.0
```

Output lands in `installer\Output\CatSit-Setup-0.1.0.exe`.

The `AppId` GUID in `CatSit.iss` is permanent and must never change — it's what lets future versions upgrade in place instead of creating a duplicate Add/Remove Programs entry.
