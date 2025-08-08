# إعداد خدمة الإيميل للأكاديمية

## الخطوات المطلوبة لتفعيل خدمة الإيميل:

### 1. إعداد حساب Gmail للإرسال

1. انتقل إلى حسابك في Gmail
2. اذهب إلى **إعدادات الحساب** → **الأمان**
3. قم بتفعيل **التحقق بخطوتين** إذا لم يكن مفعلاً
4. اذهب إلى **كلمات مرور التطبيقات** (App Passwords)
5. أنشئ كلمة مرور جديدة للتطبيق واحفظها

### 2. تحديث ملف appsettings.json

قم بتحديث قسم `EmailSettings` في ملف `appsettings.json`:

```json
"EmailSettings": {
  "SmtpServer": "smtp.gmail.com",
  "SmtpPort": 587,
  "SenderEmail": "your-academy-email@gmail.com",
  "SenderPassword": "your-app-password-here",
  "SenderName": "أكاديمية الذكاء الاصطناعي والروبوتات",
  "UseSSL": true
}
```

### 3. أو استخدام متغيرات البيئة (أكثر أماناً)

بدلاً من وضع البيانات الحساسة في `appsettings.json`، يمكنك استخدام متغيرات البيئة:

```bash
# في Windows PowerShell
$env:EmailSettings__SenderEmail="your-academy-email@gmail.com"
$env:EmailSettings__SenderPassword="your-app-password-here"

# أو في Command Prompt
set EmailSettings__SenderEmail=your-academy-email@gmail.com
set EmailSettings__SenderPassword=your-app-password-here
```

### 4. بدائل أخرى لخدمة الإيميل

#### استخدام Outlook/Hotmail:
```json
"EmailSettings": {
  "SmtpServer": "smtp-mail.outlook.com",
  "SmtpPort": 587,
  "SenderEmail": "your-email@outlook.com",
  "SenderPassword": "your-password",
  "SenderName": "أكاديمية الذكاء الاصطناعي والروبوتات",
  "UseSSL": true
}
```

#### استخدام SendGrid (خدمة مدفوعة):
```json
"EmailSettings": {
  "SmtpServer": "smtp.sendgrid.net",
  "SmtpPort": 587,
  "SenderEmail": "your-verified-email@yourdomain.com",
  "SenderPassword": "your-sendgrid-api-key",
  "SenderName": "أكاديمية الذكاء الاصطناعي والروبوتات",
  "UseSSL": true
}
```

## الميزات المتاحة:

### 1. إيميل ترحيبي للطلاب الجدد
- يتم إرساله تلقائياً عند إضافة طالب جديد
- يحتوي على بيانات الدخول (اسم المستخدم وكلمة المرور)
- تصميم جميل باللغة العربية

### 2. إيميل التسجيل في الكورسات
- يتم إرساله عند تسجيل الطالب في كورس جديد
- يحتوي على تفاصيل الكورس والمواعيد
- إرشادات للخطوات التالية

## اختبار الإعداد:

1. قم بتشغيل الباك إند
2. أضف طالب جديد من الواجهة
3. تحقق من أن الإيميل تم إرساله
4. سجل الطالب في كورس واختبر إيميل التسجيل

## استكشاف الأخطاء:

### إذا لم تصل الإيميلات:
1. تحقق من صحة بيانات الإيميل والباسورد
2. تحقق من أن الـ App Password تم إنشاؤه بشكل صحيح
3. تحقق من الـ Logs في الكونسول
4. تحقق من مجلد الـ Spam

### أخطاء شائعة:
- `Authentication failed`: بيانات دخول خاطئة
- `Mailbox unavailable`: الإيميل المرسل إليه غير صحيح
- `Connection timeout`: مشكلة في الاتصال بالإنترنت أو الـ Firewall

## الأمان:

⚠️ **هام**: لا تضع بيانات الإيميل الحساسة في ملفات الكود المرفوعة على Git
- استخدم متغيرات البيئة
- أو استخدم ملف `appsettings.Development.json` منفصل (غير مرفوع على Git) 