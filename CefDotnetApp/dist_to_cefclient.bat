@echo on
rem working directory

set driver=%~d0
set workdir=%~dp0
cd /d "%workdir%"

..\bin\Debug\net9.0\BatchCommand.exe copy.dsl bat
..\bin\Debug\net9.0\BatchCommand.exe ..\BatchCmdDslHost\BatchCmdDsl\copy.dsl bat
..\bin\Debug\net9.0\BatchCommand.exe ..\BatchScriptApi\copy.dsl bat
..\bin\Debug\net9.0\BatchCommand.exe ..\AgentCore\copy.dsl bat

