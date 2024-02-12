; Script generated by the Inno Setup Script Wizard.
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!

#define MyAppName "VedAstro"
#define MyAppVersion "1.9"
#define MyAppPublisher "VedAstro"
#define MyAppURL "https://vedastro.org/"
#define MyAppExeName "Desktop.exe"
#define Net7Installer "windowsdesktop-runtime-7.0.15-win-x64.exe"

[Setup]
; NOTE: The value of AppId uniquely identifies this application. Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{36D1EEDA-139C-4001-AB14-E15B9ACE4AD9}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
;AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={autopf}\{#MyAppName}
DisableDirPage=yes
DisableProgramGroupPage=yes
; Remove the following line to run in administrative install mode (install for all users.)
PrivilegesRequired=lowest
OutputDir=C:\Users\ASUS\Desktop\Projects\VedAstro\Desktop\InnoSetupOutput
OutputBaseFilename=VedAstroSetup
SetupIconFile=C:\Users\ASUS\Desktop\Projects\VedAstro\Website\wwwroot\images\favicon.ico
Compression=lzma
SolidCompression=yes
WizardStyle=modern

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "C:\Users\ASUS\Desktop\Projects\VedAstro\Desktop\bin\Release\net8.0-windows10.0.19041.0\win10-x64\{#MyAppExeName}"; DestDir: "{app}"; Flags: onlyifdoesntexist
Source: "C:\Users\ASUS\Desktop\Projects\VedAstro\Desktop\bin\Release\net8.0-windows10.0.19041.0\win10-x64\*"; DestDir: "{app}"; Flags: onlyifdoesntexist recursesubdirs createallsubdirs
; NOTE: Don't use "Flags: ignoreversion" on any shared system files
Source: "{#SourcePath}\{#Net7Installer}"; DestDir: "{tmp}"; Flags: deleteafterinstall

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{src}\favicon.ico"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[Code]
function InitializeSetup(): Boolean;
var
  ErrorCode: Integer;
begin
  Result := True;
  if not RegKeyExists(HKLM, 'SOFTWARE\Microsoft\dotnet\CoreRuntime\7.0') then begin
    if not Exec('msiexec.exe', '/i netfx_setup.msi /qn', '', SW_SHOWNORMAL, ewWaitUntilTerminated, ErrorCode) then begin
      MsgBox('Error installing .NET 7.', mbError, MB_OK);
      Result := False;
    end;
  end;
end;