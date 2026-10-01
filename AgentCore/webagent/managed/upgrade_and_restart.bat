@echo on
rem One-shot webagent upgrade, the head of the supervision chain:
rem     this script -> monitor (BatchCmdDslHost.exe, bare) -> webagent -> agent
rem Launched detached by the monitor when managed/upgrade_state.json asks for
rem an update. Nothing dsl hosted may run this flow: the agent process and the
rem monitor both load managed\BatchCmdDsl.dll -> BatchScriptApi.dll, so while
rem any of them is alive those dlls are locked and cannot be replaced.
rem
rem Flow: wait for webagent.exe and BatchCmdDslHost.exe to exit (the browser
rem exits through exit_browser, the agent through its parent watchdog, the
rem monitor right after launching this script) - force kill after a timeout -
rem build the four projects - deploy them (dist_to_cefclient.bat runs the four
rem copy.dsl) - restart the MONITOR (which then starts webagent again, closing
rem the chain).

setlocal EnableExtensions
set BATDIR=%~dp0
rem webagent root (this bat lives in <root>\managed)
for %%i in ("%BATDIR%..") do set ROOT=%%~fi
cd /d "%ROOT%"

set BUILDROOT=d:\GitHub\BatchCommand
set LOG=%ROOT%\managed\upgrade_build.log

echo [%date% %time%] upgrade start > "%LOG%"

rem ---- 1) wait for the processes to release the dlls ----
set /a TRIES=0
:wait_loop
tasklist /FI "IMAGENAME eq webagent.exe" 2>nul | find /I "webagent.exe" >nul && goto still_running
tasklist /FI "IMAGENAME eq BatchCmdDslHost.exe" 2>nul | find /I "BatchCmdDslHost.exe" >nul && goto still_running
goto build
:still_running
set /a TRIES+=1
if %TRIES% LSS 30 (
    timeout /t 2 /nobreak >nul
    goto wait_loop
)
echo [%date% %time%] processes still alive after 60s, force kill >> "%LOG%"
taskkill /F /IM webagent.exe >> "%LOG%" 2>&1
taskkill /F /IM BatchCmdDslHost.exe >> "%LOG%" 2>&1
timeout /t 3 /nobreak >nul

rem ---- 2) build ----
:build
echo [%date% %time%] building BatchScriptApi >> "%LOG%"
dotnet build "%BUILDROOT%\BatchScriptApi\BatchScriptApi.csproj" -v q --nologo >> "%LOG%" 2>&1
if errorlevel 1 goto build_failed
echo [%date% %time%] building BatchCmdDsl >> "%LOG%"
dotnet build "%BUILDROOT%\BatchCmdDslHost\BatchCmdDsl\BatchCmdDsl.csproj" -v q --nologo >> "%LOG%" 2>&1
if errorlevel 1 goto build_failed
echo [%date% %time%] building AgentCore >> "%LOG%"
dotnet build "%BUILDROOT%\AgentCore\AgentCore.csproj" -v q --nologo >> "%LOG%" 2>&1
if errorlevel 1 goto build_failed
echo [%date% %time%] building CefDotnetApp >> "%LOG%"
dotnet build "%BUILDROOT%\CefDotnetApp\CefDotnetApp.csproj" -v q --nologo >> "%LOG%" 2>&1
if errorlevel 1 goto build_failed

rem ---- 3) deploy (runs all four copy.dsl; CefDotnetApp has no PostBuild) ----
echo [%date% %time%] deploying >> "%LOG%"
call "%BUILDROOT%\CefDotnetApp\dist_to_cefclient.bat" >> "%LOG%" 2>&1
if errorlevel 1 goto build_failed

rem ---- 3b) deploy js/dsl/docs/skills from the source tree ----
rem The copy.dsl scripts above deploy dlls ONLY. js/dsl/docs/skills have
rem their source of truth in AgentCore\webagent and are deployed ONLY here,
rem so a plain build can never overwrite the live files with a stale copy.
rem robocopy exit codes 0-7 are success, >= 8 is failure. The upgrade
rem scripts themselves are excluded: cmd reads this bat line by line, so
rem overwriting the running script mid-flight is not safe (a newer source
rem version needs a one-time manual copy into managed\, or let a deploy
rem that runs it from elsewhere pick it up).
robocopy "%BUILDROOT%\AgentCore\webagent\managed" "%ROOT%\managed" /E /XF upgrade_and_restart.bat upgrade_and_restart.sh >> "%LOG%"
if errorlevel 8 goto build_failed
robocopy "%BUILDROOT%\AgentCore\webagent\docs" "%ROOT%\docs" /E >> "%LOG%"
if errorlevel 8 goto build_failed
robocopy "%BUILDROOT%\AgentCore\webagent\skills" "%ROOT%\skills" /E >> "%LOG%"
if errorlevel 8 goto build_failed

rem ---- 4) restart the monitor (it starts webagent again) ----
echo [%date% %time%] starting monitor >> "%LOG%"
start "" "%ROOT%\BatchCmdDslHost.exe"
echo [%date% %time%] upgrade done >> "%LOG%"
endlocal
exit /b 0

:build_failed
echo [%date% %time%] BUILD/DEPLOY FAILED, starting the monitor with the current managed >> "%LOG%"
start "" "%ROOT%\BatchCmdDslHost.exe"
endlocal
exit /b 1
