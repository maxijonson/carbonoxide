@echo off

if "%~3" EQU "" (
	echo Usage: _update_oxide.bat ^<tag^> ^<branch^> ^<server-dir^>
	exit /b 1
)

set TAG=%~1
set BRANCH=%~2

SET root=%cd%
SET server=%root%\servers\%~3
SET steam=%root%\steam
SET steamCmd=https://steamcdn-a.akamaihd.net/client/installer/steamcmd.zip

echo Server directory: %server%
echo Steam directory: %steam%
echo Root directory: %root%
echo Branch: %BRANCH%

@REM Ensure folders are created
if not exist "%server%" mkdir "%server%"

@REM Download & extract Steam it in the steam folder
if not exist "%steam%" (
	mkdir "%steam%"
	cd "%steam%"

	echo Downloading Steam
	powershell -Command "(New-Object Net.WebClient).DownloadFile('%steamCmd%', '%root%\steam.zip')"
	echo Extracting Steam
	powershell -Command "Expand-Archive '%root%\steam.zip' -DestinationPath '%steam%'" -Force

	del "%root%\steam.zip"
)

@REM Download the server
cd "%steam%"
echo Downloading Rust server on %BRANCH% branch...
@REM Forget the installed build so steam checks every file against the new one
if exist "%server%\steamapps\appmanifest_258550.acf" del "%server%\steamapps\appmanifest_258550.acf"
steamcmd.exe +force_install_dir "%server%" ^
			 +login anonymous ^
             +app_update 258550 ^
			 -beta %BRANCH% ^
             validate ^
             +quit
if errorlevel 1 (
	echo Steam update failed, Oxide was not installed
	exit /b 1
)

@REM Download latest Oxide build for the tag (release or staging)
echo Downloading Oxide
powershell -Command "(New-Object Net.WebClient).DownloadFile('https://downloads.oxidemod.com/artifacts/Oxide.Rust/%TAG%/Oxide.Rust.zip', '%root%\oxide.zip')"

@REM Extract it in the server folder
echo Extracting Oxide
powershell -Command "Expand-Archive '%root%\oxide.zip' -DestinationPath '%root%\Oxide.Rust'" -Force

@REM Copy the files to the server folder, only overwriting files in recursive folders, not the folders themselves
echo Copying Oxide files
xcopy %root%\Oxide.Rust\* "%server%" /y /s /i

@REM Cleanup
echo Cleaning up
del %root%\oxide.zip
rmdir /s /q %root%\Oxide.Rust
