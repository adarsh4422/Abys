# Abys — Offline AI Chat

A fully offline AI chatbot packaged as a single `.exe`. Just double-click and chat — no setup wizards, no accounts, no internet.

## Download & Run

1. **Download** `Abys.exe` from this repo
2. **Install Ollama** from [ollama.com/download](https://ollama.com/download) (one-time setup)
3. **Open a terminal** and run `ollama pull tinyllama` to download the AI model (637MB, one-time)
4. **Double-click `Abys.exe`** — the chat opens in your browser. Done!

> After step 2–3, everything works **100% offline**. No internet needed ever again.

## How It Works

- `Abys.exe` starts the Ollama AI server in the background
- Extracts a beautiful chat UI and opens it in your default browser
- Sits quietly in your **system tray** — right-click the "A" icon to reopen or exit
- All HTML/CSS/JS is embedded inside the `.exe` — no loose files

## Screenshot

Dark-mode chat interface with streaming AI responses, model selector, and system tray integration.

## Models

| Model | Size | Speed | Quality |
|---|---|---|---|
| **TinyLlama** (default) | 637 MB | ⚡ Fast | Good for quick chats |
| Llama 3 | 4.7 GB | 🐢 Slower | Better quality |

Switch models from the dropdown in the chat UI.

## Build from Source

```powershell
& "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe" /target:winexe /r:System.Windows.Forms.dll /r:System.Drawing.dll /out:Abys.exe Abys.cs
```

## Requirements

- Windows 10/11
- [Ollama](https://ollama.com/download) installed
- ~2 GB RAM

## License

MIT
