#!/bin/bash
# One-shot webagent upgrade (macOS), the head of the supervision chain:
#     this script -> monitor (BatchCmdDslHost, bare) -> webagent -> agent
# Counterpart of upgrade_and_restart.bat, same reasoning: the agent process and
# the monitor both load managed/BatchCmdDl.dll -> BatchScriptApi.dll, so while
# any of them is alive those dlls are locked; the upgrader therefore has to be
# a process that loads none of them - a shell script.
#
# Usage: launched detached by the monitor (needs chmod +x once). Waits for
# webagent and BatchCmdDslHost to exit, builds the four projects, deploys them
# through the copy.dsl scripts, then restarts the MONITOR (which starts
# webagent again, closing the chain).
#
# The source repo location comes from (in order): the WEBAGENT_BUILD_ROOT
# environment variable, managed/upgrade_build_root.txt, or the default below.
# On a mac bundle layout the executables live in <Contents>/MacOS while this
# script lives in <Contents>/managed - handled below.

set -u

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
cd "$ROOT"

BUILD_ROOT="${WEBAGENT_BUILD_ROOT:-}"
if [ -z "$BUILD_ROOT" ] && [ -f "$SCRIPT_DIR/upgrade_build_root.txt" ]; then
    BUILD_ROOT="$(head -n 1 "$SCRIPT_DIR/upgrade_build_root.txt" | tr -d '\r\n')"
fi
if [ -z "$BUILD_ROOT" ]; then
    BUILD_ROOT="$HOME/GitHub/BatchCommand"
fi

# Windows-like layout: exes next to managed/. Mac bundle: exes in MacOS/.
EXE_DIR="$ROOT"
if [ -d "$ROOT/MacOS" ]; then
    EXE_DIR="$ROOT/MacOS"
fi

LOG="$SCRIPT_DIR/upgrade_build.log"
echo "[$(date '+%F %T')] upgrade start" > "$LOG"

# ---- 1) wait for the processes to release the dlls ----
# The agent exits on its own through its parent watchdog, the browser through
# exit_browser, the monitor right after launching this script. Force kill
# after ~60s as the fallback.
TRIES=0
while :; do
    if ! pgrep -x webagent > /dev/null && ! pgrep -x BatchCmdDslHost > /dev/null; then
        break
    fi
    TRIES=$((TRIES + 1))
    if [ "$TRIES" -ge 30 ]; then
        echo "[$(date '+%F %T')] processes still alive after 60s, force kill" >> "$LOG"
        pkill -x webagent >> "$LOG" 2>&1
        pkill -x BatchCmdDslHost >> "$LOG" 2>&1
        sleep 3
        break
    fi
    sleep 2
done

# ---- 2) build ----
FAILED=0
for proj in BatchScriptApi/BatchScriptApi.csproj \
            BatchCmdDslHost/BatchCmdDsl/BatchCmdDsl.csproj \
            AgentCore/AgentCore.csproj \
            CefDotnetApp/CefDotnetApp.csproj; do
    echo "[$(date '+%F %T')] building $proj" >> "$LOG"
    if ! dotnet build "$BUILD_ROOT/$proj" -v q --nologo >> "$LOG" 2>&1; then
        echo "[$(date '+%F %T')] BUILD FAILED: $proj" >> "$LOG"
        FAILED=1
        break
    fi
done

# ---- 3) deploy through the four copy.dsl scripts ----
# (CefDotnetApp has no PostBuild; run all four explicitly. The runner is the
# repo's own BatchCommand build, whose dlls are not held by anything running.)
if [ "$FAILED" -eq 0 ]; then
    echo "[$(date '+%F %T')] deploying" >> "$LOG"
    RUNNER="$BUILD_ROOT/bin/Debug/net9.0/BatchCommand.dll"
    for dsl in CefDotnetApp/copy.dsl \
               BatchCmdDslHost/BatchCmdDsl/copy.dsl \
               BatchScriptApi/copy.dsl \
               AgentCore/copy.dsl; do
        if ! dotnet "$RUNNER" "$BUILD_ROOT/$dsl" bat >> "$LOG" 2>&1; then
            echo "[$(date '+%F %T')] DEPLOY FAILED: $dsl" >> "$LOG"
            FAILED=1
            break
        fi
    done
fi

# ---- 4) restart the monitor (also when the build failed: keep the chain up) ----
echo "[$(date '+%F %T')] starting monitor" >> "$LOG"
nohup "$EXE_DIR/BatchCmdDslHost" > /dev/null 2>&1 &

if [ "$FAILED" -eq 0 ]; then
    echo "[$(date '+%F %T')] upgrade done" >> "$LOG"
    exit 0
else
    echo "[$(date '+%F %T')] upgrade failed, monitor restarted with the current managed" >> "$LOG"
    exit 1
fi
