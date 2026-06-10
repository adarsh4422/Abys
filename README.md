# Abys — Offline AI Chat

A fully offline AI chatbot packaged as a single `.exe`. Just double-click and chat.

## How to Use

1. **Double-click `Abys.exe`**
2. The AI server starts automatically and the chat opens in your browser.
3. **Abys** sits in your system tray — right-click the tray icon to reopen or exit.

## What's Inside

- The entire chat UI (HTML/CSS/JS) is embedded inside the `.exe`
- Ollama AI engine runs locally from the `ollama/` folder
- Default model: **TinyLlama** (fast, lightweight)
- Zero internet required after initial setup

## Requirements

- Windows 10/11
- `ollama/` folder with `ollama.exe` next to `Abys.exe`
- ~2 GB RAM

## Build from Source

```powershell
& "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe" /target:winexe /r:System.Windows.Forms.dll /r:System.Drawing.dll /out:Abys.exe Abys.cs
```
