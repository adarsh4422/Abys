@echo off
title Local AI Chat Launcher
color 0B

echo ============================================
echo        Local AI Chat - Setup ^& Launch
echo ============================================
echo.

:: Check if Ollama is already installed
set "OLLAMA_PATH=%LOCALAPPDATA%\Programs\Ollama\ollama.exe"

if exist "%OLLAMA_PATH%" (
    echo [OK] Ollama is already installed.
    goto :start_server
)

echo [!] Ollama is not installed.
echo.
echo This will download and install Ollama for you.
echo Press any key to continue, or close this window to cancel.
pause >nul

:: Download installer
echo.
echo [*] Downloading Ollama installer...
set "INSTALLER=%~dp0OllamaSetup.exe"
powershell -Command "& { [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12; (New-Object Net.WebClient).DownloadFile('https://ollama.com/download/OllamaSetup.exe', '%INSTALLER%') }"

if not exist "%INSTALLER%" (
    echo [ERROR] Download failed. Please check your internet connection.
    pause
    exit /b 1
)

echo [OK] Download complete.
echo.
echo [*] Running installer... Please follow any prompts.
start /wait "" "%INSTALLER%" /S
echo [OK] Installation complete.

:start_server
echo.
echo [*] Starting Ollama server...

:: Kill any existing instances
taskkill /F /IM ollama.exe >nul 2>&1
timeout /t 2 >nul

:: Set environment variable for CORS
set OLLAMA_ORIGINS=*

:: Start Ollama serve in background
start "" /B "%OLLAMA_PATH%" serve
timeout /t 4 >nul

echo [OK] Server started.
echo.

:: Pull model
echo [*] Downloading AI model (llama3)...
echo     This may take several minutes on first run.
"%OLLAMA_PATH%" pull llama3

echo.
echo [OK] Model ready!
echo.

:: Open UI
echo [*] Opening Chat UI in your browser...
start "" "%~dp0index.html"

echo.
echo ============================================
echo   Local AI Chat is running!
echo   Keep this window open while chatting.
echo   Close this window to stop the server.
echo ============================================
echo.
echo Press any key to stop the server and exit...
pause >nul

taskkill /F /IM ollama.exe >nul 2>&1
echo Server stopped. Goodbye!
