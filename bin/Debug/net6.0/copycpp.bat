@echo on
rem working directory

set driver=%~d0
set workdir=%~dp0
cd /d "%workdir%"

copy /y ..\..\..\x64\Debug\DebugScriptVM.dll .