using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using System.Drawing;

namespace Abys
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new AbysForm());
        }
    }

    public class AbysForm : Form
    {
        private NotifyIcon trayIcon;
        private string tempDir;
        private string ollamaExe;

        public AbysForm()
        {
            // Hidden main form
            this.ShowInTaskbar = false;
            this.WindowState = FormWindowState.Minimized;
            this.Opacity = 0;
            this.FormBorderStyle = FormBorderStyle.None;
            this.Size = new Size(1, 1);

            // System tray icon
            trayIcon = new NotifyIcon();
            trayIcon.Text = "Abys - Offline AI Chat";
            trayIcon.Visible = true;

            // Create a simple icon programmatically
            Bitmap bmp = new Bitmap(16, 16);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.FromArgb(100, 108, 255));
                using (Font f = new Font("Arial", 9, FontStyle.Bold))
                {
                    g.DrawString("A", f, Brushes.White, 1, 0);
                }
            }
            trayIcon.Icon = Icon.FromHandle(bmp.GetHicon());

            // Tray menu
            ContextMenuStrip menu = new ContextMenuStrip();
            menu.Items.Add("Open Abys", null, OnOpen);
            menu.Items.Add("-");
            menu.Items.Add("Exit", null, OnExit);
            trayIcon.ContextMenuStrip = menu;
            trayIcon.DoubleClick += OnOpen;
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            this.Hide();
            StartAbys();
        }

        private void StartAbys()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;

            // Find Ollama
            ollamaExe = Path.Combine(baseDir, "ollama", "ollama.exe");
            if (!File.Exists(ollamaExe))
            {
                ollamaExe = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "Programs", "Ollama", "ollama.exe");
            }

            if (!File.Exists(ollamaExe))
            {
                MessageBox.Show(
                    "Ollama engine not found.\n\nPlease place the 'ollama' folder next to Abys.exe, or install Ollama from https://ollama.com",
                    "Abys", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
                return;
            }

            // Write embedded UI to temp folder
            tempDir = Path.Combine(Path.GetTempPath(), "Abys");
            if (!Directory.Exists(tempDir))
                Directory.CreateDirectory(tempDir);

            File.WriteAllText(Path.Combine(tempDir, "index.html"), GetHtml());
            File.WriteAllText(Path.Combine(tempDir, "style.css"), GetCss());
            File.WriteAllText(Path.Combine(tempDir, "script.js"), GetJs());

            // Kill old Ollama
            foreach (Process p in Process.GetProcessesByName("ollama"))
            {
                try { p.Kill(); } catch { }
            }
            Thread.Sleep(1000);

            // Start Ollama server
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = ollamaExe;
            psi.Arguments = "serve";
            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;
            psi.EnvironmentVariables["OLLAMA_ORIGINS"] = "*";
            psi.EnvironmentVariables["OLLAMA_NO_CLOUD"] = "true";
            psi.EnvironmentVariables["OLLAMA_NOPRUNE"] = "true";
            Process.Start(psi);

            Thread.Sleep(3000);

            // Open browser
            OpenBrowser();

            trayIcon.ShowBalloonTip(3000, "Abys", "Running in the tray. Right-click to exit.", ToolTipIcon.Info);
        }

        private void OpenBrowser()
        {
            string htmlPath = Path.Combine(tempDir, "index.html");
            Process.Start(new ProcessStartInfo()
            {
                FileName = htmlPath,
                UseShellExecute = true
            });
        }

        private void OnOpen(object sender, EventArgs e)
        {
            OpenBrowser();
        }

        private void OnExit(object sender, EventArgs e)
        {
            foreach (Process p in Process.GetProcessesByName("ollama"))
            {
                try { p.Kill(); } catch { }
            }
            trayIcon.Visible = false;
            trayIcon.Dispose();
            Application.Exit();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            OnExit(null, null);
            base.OnFormClosing(e);
        }

        // =============================================
        //  EMBEDDED UI FILES
        // =============================================

        private static string GetHtml()
        {
            return @"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Abys - Offline AI Chat</title>
    <link rel=""stylesheet"" href=""style.css"">
</head>
<body>
    <div class=""chat-container"">
        <header class=""chat-header"">
            <div class=""header-content"">
                <h1><span class=""gradient-text"">Abys</span></h1>
                <p class=""subtitle"">Offline AI Assistant</p>
                <p class=""status offline"" id=""connection-status"">Connecting...</p>
            </div>
            <div class=""model-selector"">
                <select id=""model-select"">
                    <option value=""tinyllama"">TinyLlama (Fast)</option>
                    <option value=""llama3"">Llama 3</option>
                </select>
            </div>
        </header>
        <div class=""chat-messages"" id=""chat-messages"">
            <div class=""message system-message"">
                <div class=""message-content"">
                    <p>Welcome to <strong>Abys</strong>! Everything runs 100% offline on your machine.</p>
                </div>
            </div>
        </div>
        <div class=""chat-input-container"">
            <form id=""chat-form"">
                <textarea id=""user-input"" placeholder=""Ask Abys anything..."" rows=""1"" autofocus></textarea>
                <button type=""submit"" id=""send-button"" disabled>
                    <svg viewBox=""0 0 24 24"" width=""24"" height=""24"" stroke=""currentColor"" stroke-width=""2"" fill=""none"" stroke-linecap=""round"" stroke-linejoin=""round""><line x1=""22"" y1=""2"" x2=""11"" y2=""13""></line><polygon points=""22 2 15 22 11 13 2 9 22 2""></polygon></svg>
                </button>
            </form>
        </div>
    </div>
    <script src=""script.js""></script>
</body>
</html>";
        }

        private static string GetCss()
        {
            return @":root {
    --bg-color: #0a0e1a;
    --container-bg: rgba(16, 24, 48, 0.85);
    --border-color: rgba(100, 140, 255, 0.12);
    --text-main: #e8ecf4;
    --text-muted: #7a8baa;
    --accent: #646cff;
    --accent-hover: #535bf2;
    --user-msg-bg: linear-gradient(135deg, #646cff, #7c3aed);
    --ai-msg-bg: rgba(30, 40, 70, 0.9);
    --font-family: 'Segoe UI', system-ui, -apple-system, sans-serif;
}
* { box-sizing: border-box; margin: 0; padding: 0; }
body {
    font-family: var(--font-family);
    background-color: var(--bg-color);
    color: var(--text-main);
    height: 100vh;
    display: flex;
    justify-content: center;
    align-items: center;
    background-image:
        radial-gradient(ellipse at 20% 50%, rgba(100, 108, 255, 0.08) 0%, transparent 60%),
        radial-gradient(ellipse at 80% 20%, rgba(124, 58, 237, 0.08) 0%, transparent 60%);
    overflow: hidden;
}
.chat-container {
    width: 100%; max-width: 860px; height: 96vh;
    display: flex; flex-direction: column;
    background: var(--container-bg);
    backdrop-filter: blur(20px);
    border: 1px solid var(--border-color);
    border-radius: 20px;
    box-shadow: 0 0 80px rgba(100, 108, 255, 0.06), 0 20px 60px rgba(0,0,0,0.4);
    overflow: hidden;
}
.chat-header {
    padding: 18px 28px;
    border-bottom: 1px solid var(--border-color);
    display: flex; justify-content: space-between; align-items: center;
    background: rgba(10, 14, 26, 0.5);
}
.header-content h1 { font-size: 1.6rem; font-weight: 800; letter-spacing: 0.04em; }
.subtitle { font-size: 0.75rem; color: var(--text-muted); letter-spacing: 0.08em; text-transform: uppercase; margin-top: 2px; }
.gradient-text {
    background: linear-gradient(135deg, #646cff, #c084fc, #646cff);
    background-size: 200% 200%;
    -webkit-background-clip: text; -webkit-text-fill-color: transparent;
    animation: shimmer 4s ease-in-out infinite;
}
@keyframes shimmer { 0%, 100% { background-position: 0% 50%; } 50% { background-position: 100% 50%; } }
.status { font-size: 0.8rem; display: flex; align-items: center; gap: 6px; margin-top: 4px; }
.status::before { content: ''; display: inline-block; width: 7px; height: 7px; border-radius: 50%; flex-shrink: 0; }
.status.online { color: #34d399; }
.status.online::before { background-color: #34d399; box-shadow: 0 0 8px #34d399; }
.status.offline { color: #f87171; }
.status.offline::before { background-color: #f87171; box-shadow: 0 0 8px #f87171; }
.model-selector select {
    background: rgba(20, 28, 50, 0.9); color: var(--text-main);
    border: 1px solid var(--border-color); padding: 8px 14px;
    border-radius: 10px; outline: none; font-family: inherit;
    font-size: 0.85rem; cursor: pointer; appearance: none; transition: border-color 0.2s;
}
.model-selector select:hover { border-color: var(--accent); }
.chat-messages { flex: 1; overflow-y: auto; padding: 24px; display: flex; flex-direction: column; gap: 16px; scroll-behavior: smooth; }
.chat-messages::-webkit-scrollbar { width: 5px; }
.chat-messages::-webkit-scrollbar-thumb { background: rgba(100,108,255,0.15); border-radius: 10px; }
.message { display: flex; animation: fadeIn 0.25s ease-out forwards; opacity: 0; transform: translateY(8px); }
@keyframes fadeIn { to { opacity: 1; transform: translateY(0); } }
.message.user-message { justify-content: flex-end; }
.message.ai-message { justify-content: flex-start; }
.message.system-message { justify-content: center; }
.message-content { max-width: 78%; padding: 12px 18px; border-radius: 16px; line-height: 1.55; font-size: 0.92rem; word-wrap: break-word; }
.user-message .message-content { background: var(--user-msg-bg); color: white; border-bottom-right-radius: 4px; }
.ai-message .message-content { background: var(--ai-msg-bg); color: var(--text-main); border-bottom-left-radius: 4px; border: 1px solid var(--border-color); }
.system-message .message-content { background: transparent; color: var(--text-muted); font-size: 0.82rem; text-align: center; border: 1px dashed rgba(100,140,255,0.15); padding: 10px 20px; }
.chat-input-container { padding: 16px 24px; border-top: 1px solid var(--border-color); background: rgba(10, 14, 26, 0.7); }
#chat-form { display: flex; gap: 10px; align-items: flex-end; background: rgba(20, 28, 50, 0.8); border: 1px solid var(--border-color); border-radius: 16px; padding: 6px 10px; transition: border-color 0.2s; }
#chat-form:focus-within { border-color: var(--accent); box-shadow: 0 0 20px rgba(100,108,255,0.08); }
#user-input { flex: 1; background: transparent; border: none; color: var(--text-main); font-family: inherit; font-size: 0.95rem; padding: 8px; resize: none; outline: none; max-height: 120px; min-height: 24px; }
#user-input::placeholder { color: var(--text-muted); }
#send-button { background: var(--accent); color: white; border: none; width: 38px; height: 38px; border-radius: 50%; display: flex; justify-content: center; align-items: center; cursor: pointer; transition: all 0.2s; flex-shrink: 0; }
#send-button:hover:not(:disabled) { background: var(--accent-hover); transform: scale(1.08); }
#send-button:disabled { background: rgba(100,108,255,0.15); color: rgba(255,255,255,0.2); cursor: not-allowed; }
.loading-dots { display: flex; gap: 4px; align-items: center; height: 20px; }
.dot { width: 6px; height: 6px; background: var(--accent); border-radius: 50%; animation: bounce 1.2s infinite ease-in-out both; }
.dot:nth-child(1) { animation-delay: -0.32s; }
.dot:nth-child(2) { animation-delay: -0.16s; }
@keyframes bounce { 0%, 80%, 100% { transform: scale(0); opacity: 0.4; } 40% { transform: scale(1); opacity: 1; } }
.message-content pre { background: #080c18; padding: 12px; border-radius: 8px; overflow-x: auto; margin: 8px 0; border: 1px solid rgba(100,108,255,0.1); }
.message-content code { font-family: Consolas, Monaco, monospace; font-size: 0.88em; }
.message-content p { margin-bottom: 6px; }
.message-content p:last-child { margin-bottom: 0; }";
        }

        private static string GetJs()
        {
            return @"var chatMessages = document.getElementById('chat-messages');
var chatForm = document.getElementById('chat-form');
var userInput = document.getElementById('user-input');
var sendButton = document.getElementById('send-button');
var modelSelect = document.getElementById('model-select');
var connectionStatus = document.getElementById('connection-status');

var OLLAMA_URL = 'http://127.0.0.1:11434/api/chat';
var isGenerating = false;
var chatHistory = [];

function checkConnection() {
    var xhr = new XMLHttpRequest();
    xhr.open('GET', 'http://127.0.0.1:11434/api/tags', true);
    xhr.timeout = 3000;
    xhr.onload = function() {
        if (xhr.status === 200) {
            connectionStatus.textContent = 'Online \u2014 Running Locally';
            connectionStatus.className = 'status online';
            if (!isGenerating) sendButton.disabled = userInput.value.trim() === '';
        }
    };
    xhr.onerror = function() {
        connectionStatus.textContent = 'Offline \u2014 Starting server...';
        connectionStatus.className = 'status offline';
        sendButton.disabled = true;
    };
    xhr.ontimeout = xhr.onerror;
    xhr.send();
}

setInterval(checkConnection, 5000);
checkConnection();

userInput.addEventListener('input', function() {
    this.style.height = 'auto';
    this.style.height = this.scrollHeight + 'px';
    if (!isGenerating) sendButton.disabled = this.value.trim() === '';
});

userInput.addEventListener('keydown', function(e) {
    if (e.key === 'Enter' && !e.shiftKey) {
        e.preventDefault();
        if (!sendButton.disabled) chatForm.dispatchEvent(new Event('submit'));
    }
});

function addMessage(content, type) {
    var messageDiv = document.createElement('div');
    messageDiv.className = 'message ' + type + '-message';
    var contentDiv = document.createElement('div');
    contentDiv.className = 'message-content';
    contentDiv.innerHTML = type === 'ai' ? formatMarkdown(content) : content;
    messageDiv.appendChild(contentDiv);
    chatMessages.appendChild(messageDiv);
    chatMessages.scrollTop = chatMessages.scrollHeight;
    return contentDiv;
}

function formatMarkdown(text) {
    var html = text.replace(/</g, '&lt;').replace(/>/g, '&gt;');
    html = html.replace(/```([\s\S]*?)```/g, '<pre><code>$1</code></pre>');
    html = html.replace(/`([^`]+)`/g, '<code>$1</code>');
    html = html.replace(/\*\*([^*]+)\*\*/g, '<strong>$1</strong>');
    html = html.split('\n\n').map(function(p) { return '<p>' + p.replace(/\n/g, '<br>') + '</p>'; }).join('');
    return html;
}

function showLoading() {
    var d = document.createElement('div');
    d.className = 'message ai-message loading-message';
    d.id = 'loading-indicator';
    var c = document.createElement('div');
    c.className = 'message-content';
    c.innerHTML = '<div class=""loading-dots""><div class=""dot""></div><div class=""dot""></div><div class=""dot""></div></div>';
    d.appendChild(c);
    chatMessages.appendChild(d);
    chatMessages.scrollTop = chatMessages.scrollHeight;
}

function hideLoading() {
    var l = document.getElementById('loading-indicator');
    if (l) l.remove();
}

chatForm.addEventListener('submit', function(e) {
    e.preventDefault();
    var text = userInput.value.trim();
    if (!text || isGenerating) return;

    userInput.value = '';
    userInput.style.height = 'auto';
    sendButton.disabled = true;
    isGenerating = true;

    addMessage(text, 'user');
    chatHistory.push({ role: 'user', content: text });
    showLoading();

    var xhr = new XMLHttpRequest();
    xhr.open('POST', OLLAMA_URL, true);
    xhr.setRequestHeader('Content-Type', 'application/json');

    var aiResponse = '';
    var contentDiv = null;
    var lastLen = 0;

    xhr.onprogress = function() {
        if (!contentDiv) {
            hideLoading();
            contentDiv = addMessage('', 'ai');
        }
        var newData = xhr.responseText.substring(lastLen);
        lastLen = xhr.responseText.length;
        var lines = newData.split('\n');
        for (var i = 0; i < lines.length; i++) {
            if (lines[i].trim() === '') continue;
            try {
                var data = JSON.parse(lines[i]);
                if (data.message && data.message.content) {
                    aiResponse += data.message.content;
                    contentDiv.innerHTML = formatMarkdown(aiResponse);
                    chatMessages.scrollTop = chatMessages.scrollHeight;
                }
            } catch(ex) {}
        }
    };

    xhr.onload = function() {
        if (!contentDiv) { hideLoading(); contentDiv = addMessage(aiResponse || 'No response', 'ai'); }
        chatHistory.push({ role: 'assistant', content: aiResponse });
        isGenerating = false;
        sendButton.disabled = userInput.value.trim() === '';
        userInput.focus();
    };

    xhr.onerror = function() {
        hideLoading();
        addMessage('Error connecting to server.', 'system');
        chatHistory.pop();
        isGenerating = false;
        sendButton.disabled = userInput.value.trim() === '';
    };

    xhr.send(JSON.stringify({
        model: modelSelect.value,
        messages: chatHistory,
        stream: true,
        options: { num_ctx: 1024, temperature: 0.7 }
    }));
});";
        }
    }
}
