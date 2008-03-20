;Requires Inno Setup - http://www.jrsoftware.org/isinfo.php

[Setup]
AppName=Witty 0.1.7 Beta 1
AppVerName=Witty 0.1.7 Beta 1
AppVersion=0.1.7 Beta 1
VersionInfoVersion=0.1.7.1
OutputBaseFilename=Setup-Witty-0.1.7-Beta1
AppPublisherURL=http://code.google.com/p/wittytwitter/
AppSupportURL=http://code.google.com/p/wittytwitter/
AppUpdatesURL=http://code.google.com/p/wittytwitter/
DefaultDirName={pf}\Witty
DefaultGroupName=Witty
AllowNoIcons=true
OutputDir=..\..\..\Installer
SourceDir=..\Witty\bin\Release\
AppID={{BA21EA94-D0A0-11DC-AE7E-C71856D89593}

SetupIconFile=C:\Projects\Jon\WittyTwitter\Witty\Witty\Resources\AppIcon.ico
Compression=lzma
SolidCompression=true

[Languages]
Name: english; MessagesFile: compiler:Default.isl

[Tasks]
Name: desktopicon; Description: {cm:CreateDesktopIcon}; GroupDescription: {cm:AdditionalIcons}; Flags: unchecked
Name: quicklaunchicon; Description: {cm:CreateQuickLaunchIcon}; GroupDescription: {cm:AdditionalIcons}; Flags: unchecked

[Files]
Source: Witty.exe; DestDir: {app}; Flags: ignoreversion
Source: *; DestDir: {app}; Flags: ignoreversion recursesubdirs createallsubdirs
; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[Icons]
Name: {group}\Witty; Filename: {app}\Witty.exe
Name: {group}\{cm:UninstallProgram,Witty}; Filename: {uninstallexe}
Name: {userdesktop}\Witty; Filename: {app}\Witty.exe; Tasks: desktopicon
Name: {userappdata}\Microsoft\Internet Explorer\Quick Launch\Witty; Filename: {app}\Witty.exe; Tasks: quicklaunchicon

[Run]
Filename: {app}\Witty.exe; Description: {cm:LaunchProgram,Witty}; Flags: nowait postinstall skipifsilent

[Code]
const
//Use these values for .NET 1.1
//dotnetRedistURL = 'http://download.microsoft.com/download/a/a/c/aac39226-8825-44ce-90e3-bf8203e74006/dotnetfx.exe';
//dotnetRegKey = 'SOFTWARE\Microsoft\.NETFramework\policy\v1.1';
//version = '1.1';

//Use these values for .NET 2.0
//dotnetRedistURL = 'http://download.microsoft.com/download/5/6/7/567758a3-759e-473e-bf8f-52154438565a/dotnetfx.exe';
//dotnetRegKey = 'SOFTWARE\Microsoft\.NETFramework\policy\v2.0';
//version = '2.0';

//Use these values for .NET 3.0
dotnetRedistURL = 'http://download.microsoft.com/download/4/d/a/4da3a5fa-ee6a-42b8-8bfa-ea5c4a458a7d/dotnetfx3setup.exe';
dotnetRegKey = 'SOFTWARE\Microsoft\Net Framework Setup\NDP\v3.0';
version = '3.0';

//Use these values to test for install prompt
//dotnetRedistURL = 'http://pretend.microsoft.com/dotnetfx4setup.exe';
//dotnetRegKey = 'SOFTWARE\Microsoft\Net Framework Setup\NDP\v4.0';
//version = '4.0';

function InitializeSetup(): Boolean;
var
    ErrorCode: Integer;
    NetFrameWorkInstalled : Boolean;
    InstallDotNetResponse : Boolean;
begin
	NetFrameWorkInstalled := RegKeyExists(HKLM,dotnetRegKey);
	if NetFrameWorkInstalled =true then
	   begin
		  Result := true;
	   end
	else
	   begin
		  InstallDotNetResponse := MsgBox('This setup requires version ' + version + ' of the .NET Framework. Please download and install the .NET Framework and run this setup again. Do you want to download the framework now?',mbConfirmation,MB_YESNO)= idYes;
		  if InstallDotNetResponse =false then
			begin
			  Result:=false;
			end
		  else
			begin
			  Result:=false;
			  ShellExec('open',dotnetRedistURL,'','',SW_SHOWNORMAL,ewNoWait,ErrorCode);
			end;
	   end;
	end;
