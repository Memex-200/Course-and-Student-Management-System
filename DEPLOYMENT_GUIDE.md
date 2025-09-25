# 🚀 دليل نشر موقع أكاديمية الذكاء الاصطناعي والروبوتات

## 📋 المتطلبات المسبقة

1. **VPS Server**: Hostinger VPS (IP: 72.60.39.178)
2. **Domain**: airobotics.site
3. **Database**: MySQL موجود ومُعد
4. **SSH Access**: متصل بالسيرفر

## 🔧 خطوات النشر

### الخطوة 1: إعداد السيرفر

#### 1.1 تحديث النظام

```bash
sudo apt update && sudo apt upgrade -y
```

#### 1.2 تثبيت المتطلبات

```bash
# تثبيت .NET 8
wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
sudo apt update
sudo apt install -y dotnet-sdk-8.0

# تثبيت Node.js
curl -fsSL https://deb.nodesource.com/setup_18.x | sudo -E bash -
sudo apt install -y nodejs

# تثبيت Nginx
sudo apt install -y nginx

# تثبيت Certbot للـ SSL
sudo apt install -y certbot python3-certbot-nginx
```

#### 1.3 إعداد Firewall

```bash
sudo ufw allow 22
sudo ufw allow 80
sudo ufw allow 443
sudo ufw allow 5227
sudo ufw --force enable
```

### الخطوة 2: إعداد قاعدة البيانات

تأكد من أن قاعدة البيانات MySQL تعمل وأن لديك:

- Database: `airobotics_db`
- User: `aiuser`
- Password: `StrongPasswordHere!`

### الخطوة 3: رفع الملفات

#### 3.1 رفع Backend API

```bash
# إنشاء مجلد النشر
mkdir -p /var/www/airobotics/api

# رفع ملفات API (من الكمبيوتر المحلي)
scp -r Api/* root@72.60.39.178:/var/www/airobotics/api/
```

#### 3.2 رفع Frontend

```bash
# بناء المشروع أولاً (على الكمبيوتر المحلي)
cd AI-Robotics-Frontend-main
echo "VITE_API_URL=https://airobotics.site/api" > .env.production
npm install
npm run build

# رفع الملفات المبنية
scp -r dist/* root@72.60.39.178:/var/www/airobotics/frontend/
```

### الخطوة 4: إعداد Backend Service

#### 4.1 إنشاء Systemd Service

```bash
sudo tee /etc/systemd/system/airobotics-api.service > /dev/null << 'EOF'
[Unit]
Description=AI Robotics Academy API
After=network.target

[Service]
Type=simple
User=www-data
WorkingDirectory=/var/www/airobotics/api
ExecStart=/usr/bin/dotnet /var/www/airobotics/api/Api.dll --urls=http://0.0.0.0:5227
Restart=always
RestartSec=10
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false

[Install]
WantedBy=multi-user.target
EOF
```

#### 4.2 تشغيل الخدمة

```bash
sudo systemctl daemon-reload
sudo systemctl enable airobotics-api
sudo systemctl start airobotics-api
sudo systemctl status airobotics-api
```

### الخطوة 5: إعداد Nginx

#### 5.1 إنشاء Nginx Configuration

```bash
sudo tee /etc/nginx/sites-available/airobotics > /dev/null << 'EOF'
server {
    listen 80;
    server_name airobotics.site www.airobotics.site;

    # Frontend
    location / {
        root /var/www/airobotics/frontend;
        index index.html;
        try_files $uri $uri/ /index.html;
    }

    # API
    location /api/ {
        proxy_pass http://localhost:5227/api/;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_cache_bypass $http_upgrade;
    }
}
EOF
```

#### 5.2 تفعيل الموقع

```bash
sudo ln -sf /etc/nginx/sites-available/airobotics /etc/nginx/sites-enabled/
sudo rm -f /etc/nginx/sites-enabled/default
sudo nginx -t
sudo systemctl reload nginx
```

### الخطوة 6: إعداد SSL Certificate

```bash
sudo certbot --nginx -d airobotics.site -d www.airobotics.site --non-interactive --agree-tos --email ezzmosataf@gmail.com
```

### الخطوة 7: التحقق من النشر

#### 7.1 فحص الخدمات

```bash
# فحص API Service
sudo systemctl status airobotics-api

# فحص Nginx
sudo systemctl status nginx

# فحص المنافذ المفتوحة
sudo netstat -tlnp | grep :5227
sudo netstat -tlnp | grep :80
sudo netstat -tlnp | grep :443
```

#### 7.2 فحص الـ Logs

```bash
# API Logs
sudo journalctl -u airobotics-api -f

# Nginx Logs
sudo tail -f /var/log/nginx/access.log
sudo tail -f /var/log/nginx/error.log
```

## 🎯 النتائج المتوقعة

بعد اكتمال النشر، يجب أن يكون:

1. **Frontend**: متاح على `https://airobotics.site`
2. **API**: متاح على `https://airobotics.site/api`
3. **SSL**: شهادة SSL صالحة
4. **Database**: متصل ويعمل

## 🔧 استكشاف الأخطاء

### مشاكل شائعة:

1. **API لا يعمل**:

   ```bash
   sudo systemctl restart airobotics-api
   sudo journalctl -u airobotics-api -f
   ```

2. **Nginx لا يعمل**:

   ```bash
   sudo nginx -t
   sudo systemctl restart nginx
   ```

3. **Database Connection**:

   - تأكد من إعدادات الاتصال في `appsettings.Production.json`
   - تأكد من أن MySQL يعمل

4. **SSL Issues**:
   ```bash
   sudo certbot renew --dry-run
   ```

## 📝 ملفات مهمة

- **API Config**: `/var/www/airobotics/api/appsettings.Production.json`
- **Nginx Config**: `/etc/nginx/sites-available/airobotics`
- **Service Config**: `/etc/systemd/system/airobotics-api.service`
- **Frontend Files**: `/var/www/airobotics/frontend/`

## 🚀 النشر السريع

يمكنك استخدام الـ scripts المُعدة:

### على Linux/Mac:

```bash
chmod +x deploy.sh
./deploy.sh
```

### على Windows:

```powershell
.\deploy.ps1
```

---

**ملاحظة**: تأكد من تحديث معلومات قاعدة البيانات والإيميل في ملفات الإعدادات قبل النشر.
