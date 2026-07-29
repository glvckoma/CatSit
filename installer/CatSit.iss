#ifndef MyAppVersion
  #define MyAppVersion "0.1.0"
#endif

[Setup]
AppId={{DA24D769-D062-44A3-950E-ABDFDB777155}
AppName=CatSit
AppVersion={#MyAppVersion}
AppPublisher=glvckoma
AppPublisherURL=https://github.com/glvckoma
AppUpdatesURL=https://github.com/glvckoma/CatSit/releases
DefaultDirName={autopf}\CatSit
PrivilegesRequired=lowest
DisableProgramGroupPage=yes
OutputDir=Output
OutputBaseFilename=CatSit-Setup-{#MyAppVersion}
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
AppMutex=CatSit.SingleInstance
CloseApplications=yes
RestartApplications=yes
UninstallDisplayIcon={app}\CatSit.exe

[Files]
Source: "..\src\CatSit\bin\publish\CatSit.exe"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{autoprograms}\CatSit"; Filename: "{app}\CatSit.exe"
Name: "{autodesktop}\CatSit"; Filename: "{app}\CatSit.exe"; Tasks: desktopicon

[Tasks]
Name: "desktopicon"; Description: "Create a &desktop icon"; Flags: unchecked

[Run]
Filename: "{app}\CatSit.exe"; Description: "Launch CatSit"; Flags: nowait postinstall skipifsilent
