$ErrorActionPreference = "Stop"

$ollamaDir = "$PSScriptRoot\ollama"
$ollamaExe = "$ollamaDir\ollama.exe"
$modelName = "llama3"

# 1. Download and Extract Ollama if not present
if (-not (Test-Path $ollamaExe)) {
    Write-Host "Ollama not found. Downloading standalone version..." -ForegroundColor Cyan
    $zipPath = "$PSScriptRoot\ollama-windows-amd64.zip"
    
    if (-not (Test-Path $zipPath)) {
        Write-Host "Downloading ollama-windows-amd64.zip... (This might take a moment)"
        Invoke-WebRequest -Uri "https://github.com/ollama/ollama/releases/latest/download/ollama-windows-amd64.zip" -OutFile $zipPath
    }
    
    Write-Host "Extracting Ollama..." -ForegroundColor Cyan
    Expand-Archive -Path $zipPath -DestinationPath $ollamaDir -Force
    Remove-Item $zipPath -Force
}

# 2. Start Ollama Server
Write-Host "Starting Ollama Server..." -ForegroundColor Cyan
$env:OLLAMA_ORIGINS = "*"

# Stop any existing ollama instances to avoid port conflicts
Stop-Process -Name "ollama" -ErrorAction SilentlyContinue

Start-Process -FilePath $ollamaExe -ArgumentList "serve" -WindowStyle Hidden -PassThru

# Wait for server to start
Start-Sleep -Seconds 3

# 3. Pull the model
Write-Host "Downloading/Verifying model '$modelName'... (This will take a few minutes depending on your connection)" -ForegroundColor Cyan
& $ollamaExe pull $modelName

Write-Host "Model ready!" -ForegroundColor Green

# 4. Open the UI
$uiPath = "$PSScriptRoot\index.html"
Write-Host "Opening Chat UI: $uiPath" -ForegroundColor Cyan
Start-Process "file:///$($uiPath.Replace('\', '/'))"

Write-Host "Setup Complete! The server is running in the background." -ForegroundColor Green
Write-Host "To stop the server later, you can run: Stop-Process -Name 'ollama'" -ForegroundColor Yellow
