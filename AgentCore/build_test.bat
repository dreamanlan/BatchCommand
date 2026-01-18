@echo on
rem working directory

set driver=%~d0
set workdir=%~dp0
cd /d "%workdir%"

echo Building AgentCore...
dotnet build AgentCore.csproj > build_output.txt 2>&1
type build_output.txt
echo.
echo Build completed. Check build_output.txt for details.
