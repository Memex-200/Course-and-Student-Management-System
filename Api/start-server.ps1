Write-Host "Starting AI Robotics Academy API Server..." -ForegroundColor Green
Set-Location "e:\Work\AI Robotics APP\ai-main\ai-main\Api"
Write-Host "Current directory: $(Get-Location)" -ForegroundColor Yellow
Write-Host "Running dotnet run..." -ForegroundColor Yellow
dotnet run --launch-profile http
Write-Host "Server stopped." -ForegroundColor Red
Read-Host "Press Enter to exit"
