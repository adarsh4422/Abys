const chatMessages = document.getElementById('chat-messages');
const chatForm = document.getElementById('chat-form');
const userInput = document.getElementById('user-input');
const sendButton = document.getElementById('send-button');
const modelSelect = document.getElementById('model-select');
const connectionStatus = document.getElementById('connection-status');

const OLLAMA_URL = 'http://127.0.0.1:11434/api/chat';

let isGenerating = false;
let chatHistory = [];

// Check connection
async function checkConnection() {
    try {
        const response = await fetch('http://127.0.0.1:11434/api/tags');
        if (response.ok) {
            connectionStatus.textContent = 'Connected (Offline Mode)';
            connectionStatus.className = 'status online';
            if (!isGenerating) sendButton.disabled = userInput.value.trim() === '';
            return true;
        }
    } catch (error) {
        connectionStatus.textContent = 'Server not running. Run LocalAIChat.exe';
        connectionStatus.className = 'status offline';
        sendButton.disabled = true;
    }
    return false;
}

// Check connection periodically
setInterval(checkConnection, 5000);
checkConnection();

// Auto-resize textarea
userInput.addEventListener('input', function() {
    this.style.height = 'auto';
    this.style.height = (this.scrollHeight) + 'px';
    if (!isGenerating) {
        sendButton.disabled = this.value.trim() === '';
    }
});

userInput.addEventListener('keydown', function(e) {
    if (e.key === 'Enter' && !e.shiftKey) {
        e.preventDefault();
        if (!sendButton.disabled) chatForm.dispatchEvent(new Event('submit'));
    }
});

function addMessage(content, type) {
    const messageDiv = document.createElement('div');
    messageDiv.className = `message ${type}-message`;
    
    const contentDiv = document.createElement('div');
    contentDiv.className = 'message-content';
    
    let formattedContent = content;
    if (type === 'ai') {
        formattedContent = formatMarkdown(content);
    }
    
    contentDiv.innerHTML = formattedContent;
    messageDiv.appendChild(contentDiv);
    chatMessages.appendChild(messageDiv);
    
    chatMessages.scrollTop = chatMessages.scrollHeight;
    return contentDiv;
}

function formatMarkdown(text) {
    let html = text.replace(/</g, '&lt;').replace(/>/g, '&gt;');
    html = html.replace(/```([\s\S]*?)```/g, '<pre><code>$1</code></pre>');
    html = html.replace(/`([^`]+)`/g, '<code>$1</code>');
    html = html.replace(/\*\*([^*]+)\*\*/g, '<strong>$1</strong>');
    html = html.split('\n\n').map(p => `<p>${p.replace(/\n/g, '<br>')}</p>`).join('');
    return html;
}

function showLoading() {
    const messageDiv = document.createElement('div');
    messageDiv.className = `message ai-message loading-message`;
    messageDiv.id = 'loading-indicator';
    
    const contentDiv = document.createElement('div');
    contentDiv.className = 'message-content';
    contentDiv.innerHTML = `
        <div class="loading-dots">
            <div class="dot"></div><div class="dot"></div><div class="dot"></div>
        </div>
    `;
    
    messageDiv.appendChild(contentDiv);
    chatMessages.appendChild(messageDiv);
    chatMessages.scrollTop = chatMessages.scrollHeight;
}

function hideLoading() {
    const loader = document.getElementById('loading-indicator');
    if (loader) loader.remove();
}

chatForm.addEventListener('submit', async (e) => {
    e.preventDefault();
    
    const text = userInput.value.trim();
    if (!text || isGenerating) return;
    
    const isConnected = await checkConnection();
    if (!isConnected) {
        alert("The AI server is not running. Please run LocalAIChat.exe first.");
        return;
    }
    
    userInput.value = '';
    userInput.style.height = 'auto';
    sendButton.disabled = true;
    isGenerating = true;
    
    addMessage(text, 'user');
    
    // Add user message to history
    chatHistory.push({ role: 'user', content: text });
    
    showLoading();
    
    try {
        const response = await fetch(OLLAMA_URL, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                model: modelSelect.value,
                messages: chatHistory,
                stream: true
            })
        });
        
        hideLoading();
        
        if (!response.ok) {
            throw new Error(`Server error: ${response.status}`);
        }
        
        const contentDiv = addMessage('', 'ai');
        let aiResponse = '';
        
        const reader = response.body.getReader();
        const decoder = new TextDecoder('utf-8');
        
        while (true) {
            const { done, value } = await reader.read();
            if (done) break;
            
            const chunk = decoder.decode(value, { stream: true });
            const lines = chunk.split('\n').filter(line => line.trim() !== '');
            
            for (const line of lines) {
                const data = JSON.parse(line);
                if (data.message && data.message.content) {
                    aiResponse += data.message.content;
                    contentDiv.innerHTML = formatMarkdown(aiResponse);
                    chatMessages.scrollTop = chatMessages.scrollHeight;
                }
            }
        }
        
        chatHistory.push({ role: 'assistant', content: aiResponse });
        
    } catch (error) {
        hideLoading();
        addMessage(`Error: ${error.message}. Is the server still running?`, 'system');
        chatHistory.pop(); // Remove the user message from history if failed
    } finally {
        isGenerating = false;
        sendButton.disabled = userInput.value.trim() === '';
        userInput.focus();
    }
});
