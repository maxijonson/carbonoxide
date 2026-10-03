@echo off
if exist "servers\oxide-production\oxide\plugins" copy /Y "lite\MyCarbonoxide.cs" "servers\oxide-production\oxide\plugins\MyCarbonoxide.cs"
if exist "servers\oxide-staging\oxide\plugins" copy /Y "lite\MyCarbonoxide.cs" "servers\oxide-staging\oxide\plugins\MyCarbonoxide.cs"
if exist "servers\carbon-production\carbon\plugins" copy /Y "lite\MyCarbonoxide.cs" "servers\carbon-production\carbon\plugins\MyCarbonoxide.cs"
if exist "servers\carbon-staging\carbon\plugins" copy /Y "lite\MyCarbonoxide.cs" "servers\carbon-staging\carbon\plugins\MyCarbonoxide.cs"
