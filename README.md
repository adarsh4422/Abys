# Local AI Chat

A fully offline, elegant AI chatbot running locally on your Windows machine using Ollama and Llama 3. No intermediate backend server required—everything is served directly from a standalone `.exe` launcher.

## Features
- **One-Click Setup:** Run `LocalAIChat.exe` and it automatically downloads the official Ollama installer, installs it, pulls the `llama3` model, and opens the UI.
- **Offline Capable:** Once installed, everything runs locally on your hardware.
- **Beautiful UI:** A dark-mode, responsive HTML/JS web application that features smooth micro-animations, code formatting, and model switching.

## Files
- `LocalAIChat.exe`: The main launcher. Double-click this to start everything.
- `index.html`, `style.css`, `script.js`: The frontend interface.
- `LocalAIChat.cs`: The source code for the launcher.

## How to Compile (Optional)
If you want to recompile the launcher from source, open PowerShell in this directory and run:
```powershell
& "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe" /target:winexe /r:System.Windows.Forms.dll /r:System.Drawing.dll /out:LocalAIChat.exe LocalAIChat.cs
```
