#!/bin/bash

# AI Robotics Academy - Deployment Script
# This script deploys the React frontend and .NET API to the VPS server

echo "🚀 Starting deployment process..."

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Server configuration
SERVER_IP="72.60.39.178"
SERVER_USER="root"
DOMAIN="airobotics.site"
API_PORT="5227"

echo -e "${GREEN}📋 Deployment Configuration:${NC}"
echo "Server IP: $SERVER_IP"
echo "Domain: $DOMAIN"
echo "API Port: $API_PORT"
echo ""

# Step 1: Build and deploy Backend API
echo -e "${YELLOW}🔧 Step 1: Building .NET API...${NC}"
cd Api
dotnet publish -c Release -o ../publish/api
echo -e "${GREEN}✅ API build completed${NC}"

# Step 2: Build Frontend
echo -e "${YELLOW}🔧 Step 2: Building React Frontend...${NC}"
cd ../AI-Robotics-Frontend-main

# Create production environment file
echo "VITE_API_URL=https://$DOMAIN/api" > .env.production

# Install dependencies and build
npm install
npm run build
echo -e "${GREEN}✅ Frontend build completed${NC}"

# Step 3: Copy files to server
echo -e "${YELLOW}🔧 Step 3: Copying files to server...${NC}"

# Create deployment directory on server
ssh $SERVER_USER@$SERVER_IP "mkdir -p /var/www/airobotics"

# Copy API files
echo "Copying API files..."
scp -r ../publish/api/* $SERVER_USER@$SERVER_IP:/var/www/airobotics/api/

# Copy Frontend files
echo "Copying Frontend files..."
scp -r dist/* $SERVER_USER@$SERVER_IP:/var/www/airobotics/frontend/

echo -e "${GREEN}✅ Files copied to server${NC}"

# Step 4: Setup systemd service for API
echo -e "${YELLOW}🔧 Step 4: Setting up API service...${NC}"

ssh $SERVER_USER@$SERVER_IP "cat > /etc/systemd/system/airobotics-api.service << 'EOF'
[Unit]
Description=AI Robotics Academy API
After=network.target

[Service]
Type=simple
User=www-data
WorkingDirectory=/var/www/airobotics/api
ExecStart=/usr/bin/dotnet /var/www/airobotics/api/Api.dll --urls=http://0.0.0.0:$API_PORT
Restart=always
RestartSec=10
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false

[Install]
WantedBy=multi-user.target
EOF"

# Enable and start the service
ssh $SERVER_USER@$SERVER_IP "systemctl daemon-reload && systemctl enable airobotics-api && systemctl start airobotics-api"

echo -e "${GREEN}✅ API service configured and started${NC}"

# Step 5: Setup Nginx
echo -e "${YELLOW}🔧 Step 5: Setting up Nginx...${NC}"

ssh $SERVER_USER@$SERVER_IP "cat > /etc/nginx/sites-available/airobotics << 'EOF'
server {
    listen 80;
    server_name $DOMAIN www.$DOMAIN;
    
    # Frontend
    location / {
        root /var/www/airobotics/frontend;
        index index.html;
        try_files \$uri \$uri/ /index.html;
    }
    
    # API
    location /api/ {
        proxy_pass http://localhost:$API_PORT/api/;
        proxy_http_version 1.1;
        proxy_set_header Upgrade \$http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host \$host;
        proxy_set_header X-Real-IP \$remote_addr;
        proxy_set_header X-Forwarded-For \$proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto \$scheme;
        proxy_cache_bypass \$http_upgrade;
    }
}
EOF"

# Enable the site
ssh $SERVER_USER@$SERVER_IP "ln -sf /etc/nginx/sites-available/airobotics /etc/nginx/sites-enabled/ && nginx -t && systemctl reload nginx"

echo -e "${GREEN}✅ Nginx configured${NC}"

# Step 6: Setup SSL with Certbot
echo -e "${YELLOW}🔧 Step 6: Setting up SSL certificate...${NC}"
ssh $SERVER_USER@$SERVER_IP "certbot --nginx -d $DOMAIN -d www.$DOMAIN --non-interactive --agree-tos --email ezzmosataf@gmail.com"

echo -e "${GREEN}✅ SSL certificate configured${NC}"

# Step 7: Final verification
echo -e "${YELLOW}🔧 Step 7: Verifying deployment...${NC}"

# Check API status
API_STATUS=$(ssh $SERVER_USER@$SERVER_IP "systemctl is-active airobotics-api")
echo "API Service Status: $API_STATUS"

# Check Nginx status
NGINX_STATUS=$(ssh $SERVER_USER@$SERVER_IP "systemctl is-active nginx")
echo "Nginx Status: $NGINX_STATUS"

echo -e "${GREEN}🎉 Deployment completed successfully!${NC}"
echo -e "${GREEN}🌐 Your website should be available at: https://$DOMAIN${NC}"
echo -e "${GREEN}🔗 API endpoint: https://$DOMAIN/api${NC}"
