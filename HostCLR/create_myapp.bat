dotnet new console -n MyApp
cd MyApp
dotnet publish -c Release -r win-x64 --self-contained

rem dotnet publish -c Release -r linux-x64 --self-contained

rem dotnet publish -c Release -r osx-x64 --self-contained

set COREHOST_TRACE=1
