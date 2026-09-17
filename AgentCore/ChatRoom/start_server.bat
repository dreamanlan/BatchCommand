@echo off
cd /d D:\AiClaw
echo Starting local server at http://localhost:8082
echo Press Ctrl+C to stop
python -m http.server 8082
