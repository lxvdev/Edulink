#define MyAppName "Edulink Client"
#define MyAppVersion "2.0.0.0"
#define MyAppPublisher "Edulink Labs"
#define MyAppURL "https://github.com/lxvdev/Edulink"
#define MyAppExeName "Edulink Client.exe"

[Setup]
SourceDir=C:\Users\lxvdev\source\repos\Edulink\Edulink.Client.Setup
CloseApplications=yes
Uninstallable=yes
AppId={{4D5D80A0-D70E-4BCD-B181-EE1F68EDAB32}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
;AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}/releases
DefaultDirName={autopf}\{#MyAppName}
DisableDirPage=no
UninstallDisplayIcon={app}\{#MyAppExeName}
DisableProgramGroupPage=yes
LicenseFile=..\LICENSE.txt
; Uncomment the following line to run in non administrative install mode (install for current user only).
;PrivilegesRequired=lowest
OutputDir=\Output
OutputBaseFilename=Edulink Client Setup
SetupIconFile=..\Edulink.Client\Resources\Edulink.Client.ico
SolidCompression=yes
WizardStyle=modern

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Files]
Source: "..\Edulink.Client\bin\Release\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\Edulink.Client\bin\Release\*"; Excludes: "*.pdb, *.xml, *.config"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

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
    if MsgBox('Would you like to keep the settings?', mbConfirmation, MB_YESNO or MB_DEFBUTTON1) = IDNO then
    begin
        DelTree(ExpandConstant('{commonappdata}\Edulink Client'), True, True, True);
    end;
  end;
end;