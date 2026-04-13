@echo off
cls

if "%~1" EQU "" (
	echo Usage: _run.bat ^<server-dir^> ^<hostname^> ^<identity^> ^<port^>
	exit /b 1
)

SET root=%cd%
SET server=%root%\servers\%~1
SET /a rcon_port=%4 + 100

echo Starting server in %server%...
cd "%server%"
RustDedicated.exe -batchmode -nographics ^
+rcon.ip 0.0.0.0 ^
+rcon.port %rcon_port% ^
+rcon.password "dk2lksk3" ^
+server.ip 0.0.0.0 ^
+server.port %4 ^
+server.maxplayers 3 ^
+server.hostname %2 ^
+server.identity %3 ^
+server.level "Procedural Map" ^
+server.seed 131302294 ^
+server.worldsize 1000 ^
+server.saveinterval 300 ^
+server.globalchat true ^
+server.description "Powered by Oxide" ^
+server.headerimage "http://i.imgur.com/xNyLhMt.jpg" ^
+server.url "https://oxidemod.org" ^
-logfile "%server%\logs\server_log.txt"
