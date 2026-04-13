@echo off
xcopy /S /Y "dependencies\*" "servers\oxide-production\oxide\"
xcopy /S /Y "dependencies\*" "servers\oxide-staging\oxide\"
xcopy /S /Y "dependencies\*" "servers\carbon-production\carbon\"
xcopy /S /Y "dependencies\*" "servers\carbon-staging\carbon\"
