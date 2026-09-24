; PhysSim Studio — установщик Windows (Inno Setup 6)
; Перед сборкой: соберите EXE через Unity (PhysSim -> Собрать EXE),
; затем в Inno Setup Compiler: Build -> Compile.
; Результат: installer/Output/PhysSimStudio-Setup.exe

#define AppName "PhysSim Studio"
#define AppVersion "1.0.0"
#define AppPublisher "PhysSim Studio"
#define AppExeName "PhysSimStudio.exe"

[Setup]
AppId={{8C1A6E5C-2C34-4B7D-9A61-1F2E3D4C5B60}
AppName={#AppName}
AppVersion={#AppVersion}
AppPublisher={#AppPublisher}
DefaultDirName={autopf}\PhysSimStudio
DefaultGroupName={#AppName}
DisableProgramGroupPage=yes
OutputDir=Output
OutputBaseFilename=PhysSimStudio-Setup
Compression=lzma2/max
SolidCompression=yes
WizardStyle=modern
ArchitecturesInstallIn64BitMode=x64compatible
PrivilegesRequiredOverridesAllowed=dialog

[Languages]
Name="russian"; MessagesFile="compiler:Languages\Russian.isl"

[Tasks]
Name="desktopicon"; Description="{cm:CreateDesktopIcon}"; GroupDescription="{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "..\Build\Windows\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name="{group}\{#AppName}"; Filename="{app}\{#AppExeName}"
Name="{autodesktop}\{#AppName}"; Filename="{app}\{#AppExeName}"; Tasks: desktopicon

[Run]
Filename="{app}\{#AppExeName}"; Description="{cm:LaunchProgram,{#AppName}}"; Flags: nowait postinstall skipifsilent
