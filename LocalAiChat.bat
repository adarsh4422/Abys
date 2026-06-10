@echo off
title Abys - Offline AI Chat
color 0B

echo ============================================
echo           Abys - Offline AI Chat
echo ============================================
echo.

:: Find Ollama
set "OLLAMA_PATH=%~dp0ollama\ollama.exe"
if not exist "%OLLAMA_PATH%" set "OLLAMA_PATH=%LOCALAPPDATA%\Programs\Ollama\ollama.exe"
if not exist "%OLLAMA_PATH%" (
    echo [ERROR] Ollama not found. Install from https://ollama.com
    pause
    exit /b 1
)

echo [OK] Engine found.
echo.
echo [*] Starting Abys...

:: Kill old instances
taskkill /F /IM ollama.exe >nul 2>&1
timeout /t 1 /nobreak >nul

:: Fully offline + allow browser CORS
set OLLAMA_ORIGINS=*
set OLLAMA_NO_CLOUD=true
set OLLAMA_NOPRUNE=true

:: Start server
start "AbysServer" /MIN "%OLLAMA_PATH%" serve
timeout /t 3 /nobreak >nul

echo [OK] Server running (fully offline).
echo.
echo [*] Opening Abys in your browser...
start "" "%~dp0index.html"

echo.
echo ============================================
echo   Abys is running! Chat away.
echo   Keep this window open.
echo   Press any key to shut down.
echo ============================================
echo.
pause >nul

taskkill /F /IM ollama.exe >nul 2>&1
echo Abys stopped. Goodbye!
