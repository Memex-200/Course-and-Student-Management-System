# AI Robotics Academy - Windows Deployment Script
# This script deploys the React frontend and .NET API to the VPS server

param(
    [string]$ServerIP = "72.60.39.178",
    [string]$ServerUser = "root",
    [string]$Domain = "airobotics.site",
    [int]$ApiPort = 5227
)

Write-Host "🚀 Starting deployment process..." -ForegroundColor Green

Write-Host "📋 Deployment Configuration:" -ForegroundColor Green
Write-Host "Server IP: $ServerIP"
Write-Host "Domain: $Domain"
Write-Host "API Port: $ApiPort"
Write-Host ""

# Step 1: Build and deploy Backend API
Write-Host "🔧 Step 1: Building .NET API..." -ForegroundColor Yellow
Set-Location "Api"
dotnet publish -c Release -o "../publish/api"
Write-Host "✅ API build completed" -ForegroundColor Green

# Step 2: Build Frontend
Write-Host "🔧 Step 2: Building React Frontend..." -ForegroundColor Yellow
Set-Location "../AI-Robotics-Frontend-main"

# Create production environment file
"VITE_API_URL=https://$Domain/api" | Out-File -FilePath ".env.production" -Encoding UTF8

# Install dependencies and build
npm install
npm run build
Write-Host "✅ Frontend build completed" -ForegroundColor Green

# Step 3: Copy files to server
Write-Host "🔧 Step 3: Copying files to server..." -ForegroundColor Yellow

# Create deployment directory on server
ssh "$ServerUser@$ServerIP" "mkdir -p /var/www/airobotics"

# Copy API files
Write-Host "Copying API files..."
scp -r "../publish/api/*" "$ServerUser@$ServerIP:/var/www/airobotics/api/"

# Copy Frontend files
Write-Host "Copying Frontend files..."
scp -r "dist/*" "$ServerUser@$ServerIP:/var/www/airobotics/frontend/"

Write-Host "✅ Files copied to server" -ForegroundColor Green

# Step 4: Setup systemd service for API
Write-Host "🔧 Step 4: Setting up API service..." -ForegroundColor Yellow

$serviceConfig = @"
[Unit]
Description=AI Robotics Academy API
After=network.target

[Service]
Type=simple
User=www-data
WorkingDirectory=/var/www/airobotics/api
ExecStart=/usr/bin/dotnet /var/www/airobotics/api/Api.dll --urls=http://0.0.0.0:$ApiPort
Restart=always
RestartSec=10
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false

[Install]
WantedBy=multi-user.target
"@

ssh "$ServerUser@$ServerIP" "echo '$serviceConfig' > /etc/systemd/system/airobotics-api.service"

# Enable and start the service
ssh "$ServerUser@$ServerIP" "systemctl daemon-reload && systemctl enable airobotics-api && systemctl start airobotics-api"

Write-Host "✅ API service configured and started" -ForegroundColor Green

# Step 5: Setup Nginx
Write-Host "🔧 Step 5: Setting up Nginx..." -ForegroundColor Yellow

$nginxConfig = @"
server {
    listen 80;
    server_name $Domain www.$Domain;
    
    # Frontend
    location / {
        root /var/www/airobotics/frontend;
        index index.html;
        try_files `$uri `$uri/ /index.html;
    }
    
    # API
    location /api/ {
        proxy_pass http://localhost:$ApiPort/api/;
        proxy_http_version 1.1;
        proxy_set_header Upgrade `$http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host `$host;
        proxy_set_header X-Real-IP `$remote_addr;
        proxy_set_header X-Forwarded-For `$proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto `$scheme;
        proxy_cache_bypass `$http_upgrade;
    }
}
"@

ssh "$ServerUser@$ServerIP" "echo '$nginxConfig' > /etc/nginx/sites-available/airobotics"
ssh "$ServerUser@$ServerIP" "ln -sf /etc/nginx/sites-available/airobotics /etc/nginx/sites-enabled/ && nginx -t && systemctl reload nginx"

Write-Host "✅ Nginx configured" -ForegroundColor Green

# Step 6: Setup SSL with Certbot
Write-Host "🔧 Step 6: Setting up SSL certificate..." -ForegroundColor Yellow
ssh "$ServerUser@$ServerIP" "certbot --nginx -d $Domain -d www.$Domain --non-interactive --agree-tos --email ezzmosataf@gmail.com"

Write-Host "✅ SSL certificate configured" -ForegroundColor Green

# Step 7: Final verification
Write-Host "🔧 Step 7: Verifying deployment..." -ForegroundColor Yellow

# Check API status
$apiStatus = ssh "$ServerUser@$ServerIP" "systemctl is-active airobotics-api"
Write-Host "API Service Status: $apiStatus"

# Check Nginx status
$nginxStatus = ssh "$ServerUser@$ServerIP" "systemctl is-active nginx"
Write-Host "Nginx Status: $nginxStatus"

Write-Host "🎉 Deployment completed successfully!" -ForegroundColor Green
Write-Host "🌐 Your website should be available at: https://$Domain" -ForegroundColor Green
Write-Host "🔗 API endpoint: https://$Domain/api" -ForegroundColor Green
