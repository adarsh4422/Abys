# Local AI Chat

A fully offline AI chatbot that runs locally on your Windows machine. Beautiful dark-mode UI with streaming responses.

## Quick Start

**Option 1 — Double-click the `.exe`:**
> Run `LocalAIChat.exe`. It opens a terminal that guides you through everything.

**Option 2 — Double-click the `.bat`:**
> Run `LocalAIChat.bat` directly if you prefer.

Both options will:
1. Check if Ollama is installed. If not, download and install it automatically.
2. Start the local AI server.
3. Download the `llama3` model (first time only, ~4.7 GB).
4. Open the chat UI in your browser.

## Files
| File | Purpose |
|---|---|
| `LocalAIChat.exe` | Launcher executable (runs the .bat) |
| `LocalAIChat.bat` | Setup & launch script |
| `index.html` | Chat UI |
| `style.css` | UI styling |
| `script.js` | Chat logic |
| `LocalAIChat.cs` | Source code for the .exe |

## How to Compile the .exe (Optional)
```powershell
& "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe" /target:exe /out:LocalAIChat.exe LocalAIChat.cs
```

## Requirements
- Windows 10/11
- ~8 GB RAM (for llama3)
- Internet connection (first-time setup only)
