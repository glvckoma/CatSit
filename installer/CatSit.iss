#ifndef MyAppVersion
  #define MyAppVersion "0.1.0"
#endif

#ifndef MyPublishDir
  #define MyPublishDir "..\publish"
#endif

#define MyAppName "CatSit"
#define MyAppExeName "CatSit.exe"
#define MyAppUrl "https://github.com/glvckoma/CatSit"

[Setup]
AppId={{DA24D769-D062-44A3-950E-ABDFDB777155}
AppName={#MyAppName}
AppVerName={#MyAppName} {#MyAppVersion}
AppVersion={#MyAppVersion}
AppPublisher=glvckoma
AppPublisherURL={#MyAppUrl}
AppSupportURL={#MyAppUrl}/issues
AppUpdatesURL={#MyAppUrl}/releases
AppComments=Protects your keyboard from curious paws.
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
PrivilegesRequired=lowest
ArchitecturesAllowed=x64compatible
DisableProgramGroupPage=yes
DisableWelcomePage=no
OutputDir=Output
OutputBaseFilename=CatSit-Setup-{#MyAppVersion}
Compression=lzma2/ultra64
SolidCompression=yes
WizardStyle=modern dynamic polar includetitlebar
WizardSizePercent=115
SetupIconFile=..\src\CatSit\app.ico
AppMutex=CatSit.SingleInstance
CloseApplications=yes
CloseApplicationsFilter={#MyAppExeName}
RestartApplications=no
UninstallDisplayIcon={app}\{#MyAppExeName}
UninstallDisplayName={#MyAppName}
VersionInfoCompany=glvckoma
VersionInfoDescription={#MyAppName} Setup
VersionInfoProductName={#MyAppName}
VersionInfoProductVersion={#MyAppVersion}

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Messages]
SetupAppRunningError=CatSit is still running in the notification area.%n%nRight-click the CatSit tray icon, choose Exit, then click OK to continue.
UninstallAppRunningError=CatSit is still running in the notification area.%n%nRight-click the CatSit tray icon, choose Exit, then click OK to continue.
WelcomeLabel1=Set up [name]
WelcomeLabel2=CatSit protects your keyboard from curious paws.%n%nSetup will add [name/ver] for your Windows account. Your existing preferences are kept when upgrading.
SelectTasksDesc=Choose your CatSit shortcuts.
SelectTasksLabel2=Setup always adds CatSit to the Start Menu. You can also create a desktop shortcut.
ReadyLabel1=CatSit is ready to install.
FinishedHeadingLabel=CatSit is ready
FinishedLabelNoIcons=Setup has finished installing [name] on your computer.
FinishedLabel=Setup has finished installing [name]. You can open it from the installed shortcuts.
ConfirmUninstall=Remove %1 from this computer?
UninstallStatusLabel=Please wait while %1 is removed from your computer.
UninstalledAll=%1 has been removed from your computer.

[Tasks]
Name: "desktopicon"; Description: "Create a &desktop shortcut"; GroupDescription: "Shortcuts:"; Flags: unchecked

[Files]
Source: "{#MyPublishDir}\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; WorkingDir: "{app}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; WorkingDir: "{app}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Launch CatSit"; WorkingDir: "{app}"; Flags: nowait postinstall skipifsilent

[Code]
var
  RemoveUserData: Boolean;

procedure RemoveCatSitStartupEntry;
var
  InstalledExe: String;
  StartupCommand: String;
begin
  InstalledExe := ExpandConstant('{app}\{#MyAppExeName}');
  if RegQueryStringValue(
       HKCU,
       'Software\Microsoft\Windows\CurrentVersion\Run',
       '{#MyAppName}',
     StartupCommand) and
     ((CompareText(StartupCommand, InstalledExe) = 0) or
      (CompareText(StartupCommand, '"' + InstalledExe + '"') = 0) or
      (CompareText(StartupCommand, InstalledExe + ' --background') = 0) or
      (CompareText(StartupCommand, '"' + InstalledExe + '" --background') = 0)) then
  begin
    RegDeleteValue(
      HKCU,
      'Software\Microsoft\Windows\CurrentVersion\Run',
      '{#MyAppName}');
  end;
end;

procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
var
  ButtonLabels: TArrayOfString;
  Choice: Integer;
  UserDataPath: String;
begin
  if CurUninstallStep = usUninstall then
  begin
    RemoveCatSitStartupEntry;
    UserDataPath := ExpandConstant('{userappdata}\{#MyAppName}');
    if (not UninstallSilent) and DirExists(UserDataPath) then
    begin
      SetArrayLength(ButtonLabels, 2);
      ButtonLabels[0] := 'Keep my data';
      ButtonLabels[1] := 'Remove my data';
      Choice := SuppressibleTaskDialogMsgBox(
        'Keep your CatSit data?',
        'Your preferences and learned activity can be kept for a future installation. Choose Remove my data for a clean uninstall.',
        mbConfirmation,
        MB_YESNO,
        ButtonLabels,
        0,
        IDYES);
      RemoveUserData := Choice = IDNO;
    end;
  end;

  if (CurUninstallStep = usPostUninstall) and RemoveUserData then
  begin
    DelTree(ExpandConstant('{userappdata}\{#MyAppName}'), True, True, True);
  end;
end;
