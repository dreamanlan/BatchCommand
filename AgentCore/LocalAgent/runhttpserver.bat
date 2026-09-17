@echo on
rem working directory

set driver=%~d0
set workdir=%~dp0
cd /d "%workdir%"

python -m http.server 8080
