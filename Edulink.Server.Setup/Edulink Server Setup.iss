#define MyAppName "Edulink Server"
#define MyAppVersion "2.0.0.0"
#define MyAppPublisher "Edulink Labs"
#define MyAppURL "https://github.com/lxvdev/Edulink"
#define MyAppExeName "Edulink Server.exe"

[Setup]
; NOTE: The value of AppId uniquely identifies this application. Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
SourceDir=C:\Users\lxvdev\source\repos\Edulink\Edulink.Server.Setup
CloseApplications=yes
AppId={{5124AD52-0279-40CC-8C99-8D01FFD83FB9}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
;AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}/releases
DefaultDirName={autopf}\{#MyAppName}
UninstallDisplayIcon={app}\{#MyAppExeName}
DisableProgramGroupPage=yes
LicenseFile=..\LICENSE.txt
; Uncomment the following line to run in non administrative install mode (install for current user only).
;PrivilegesRequired=lowest
OutputDir=.\Output
OutputBaseFilename=Edulink Server Setup
SetupIconFile=..\Edulink.Server\Resources\Edulink.Server.ico
SolidCompression=yes
WizardStyle=modern

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"

[Files]
Source: "..\Edulink.Server\bin\Release\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\Edulink.Server\bin\Release\*"; Excludes: "*.pdb, *.xml, *.config"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall

[UninstallDelete]
Type: filesandordirs; Name: "{app}"

[Code]
// Process exists
function ProcessExists(ProcessName: string): Boolean;
var
  ResultCode: Integer;
begin
  Exec(ExpandConstant('{cmd}'), '/C tasklist | findstr /C:"' + ProcessName + '"', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
  Result := (ResultCode = 0);
end;

// Kill process
function KillProcess(ProcessName: string): Boolean;
var
  ResultCode: Integer;
begin
  if not ProcessExists(ProcessName) then
  begin
    Result := True;
    Exit;
  end;
  
  Exec(ExpandConstant('{sys}\taskkill.exe'), '/f /im ' + '"' + ProcessName + '"', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
  
  Result := (ResultCode = 0);
  
  if not Result then
  begin
    // Show a message box and ask if the user wants to retry
    if MsgBox('Failed to kill the process "' + ProcessName + '". Do you want to retry?', mbError, MB_RETRYCANCEL) = mrRetry then
    begin
      // Retry the operation if the user clicks Retry
      Result := KillProcess(ProcessName);
    end;
  end;
end;

// Kill process before starting
function InitializeSetup: Boolean;
begin
  Result := KillProcess('{#MyAppExeName}');
end;

function InitializeUninstall: Boolean;
begin
  Result := KillProcess('{#MyAppExeName}');
end;

// Ask to keep settings after uninstall
procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
begin
  if CurUninstallStep = usPostUninstall then
  begin
    if MsgBox('Would you like to keep the settings?', mbConfirmation, MB_YESNO or MB_DEFBUTTON2) = IDNO then
    begin
        DelTree(ExpandConstant('{commonappdata}\Edulink Client'), True, True, True);
    end;
  end;
end;