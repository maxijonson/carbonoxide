@echo off
cls

if "%~1" EQU "" (
	echo Usage: _wipe.bat ^<server-dir^> ^<identity^>
	exit /b 1
)

SET root=%cd%

SET identity=%~2
SET server=%root%\servers\%~1\server\%identity%

echo Wiping server in %server%...

del "%server%\player.*" /q
del "%server%\proceduralmap.*" /q
del "%server%\relationship.*" /q
del "%server%\sv.files.*" /q