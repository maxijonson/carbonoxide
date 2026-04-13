@echo off

if "%~4" EQU "" (
	echo Usage: _update_carbon.bat ^<tag^> ^<branch^> ^<build^> ^<server-dir^>
	exit /b 1
)

set TAG=%~1
set BRANCH=%~2
set BUILD=%~3

SET root=%cd%
SET server=%root%\servers\%~4
SET steam=%root%\steam
SET steamCmd=https://steamcdn-a.akamaihd.net/client/installer/steamcmd.zip
SET carbonUrl=https://github.com/CarbonCommunity/Carbon.Core/releases/download/%TAG%_build/Carbon.Windows.%BUILD%.zip

echo Server directory: %server%
echo Steam directory: %steam%
echo Root directory: %root%
echo Branch: %BRANCH%
echo Carbon URL: %carbonUrl%

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
steamcmd.exe +force_install_dir "%server%" ^
			 +login anonymous ^
             +app_update 258550 ^
			 -beta %BRANCH% ^
             validate ^
             +quit ^

@REM Download latest Carbon build
echo Downloading Carbon
powershell -Command "(New-Object Net.WebClient).DownloadFile('%carbonUrl%', '%root%\carbon.zip')"

@REM Extract Carbon
echo Extracting Carbon
powershell -Command "Expand-Archive '%root%\carbon.zip' -DestinationPath '%root%\Carbon'" -Force

@REM Copy the files to the server folder
echo Copying Carbon files
xcopy %root%\Carbon\* "%server%" /y /s /i

@REM Cleanup
echo Cleaning up
del %root%\carbon.zip
rmdir /s /q %root%\Carbon
