using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using Api.Models;

namespace Api.Services
{
    public interface IEmailService
    {
        Task<bool> SendWelcomeEmailAsync(string toEmail, string studentName, string username, string password);
        Task<bool> SendCourseEnrollmentEmailAsync(string toEmail, string studentName, string courseName, DateTime startDate, DateTime endDate, string courseDescription);
    }

    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<EmailSettings> emailSettings, ILogger<EmailService> logger)
        {
            _emailSettings = emailSettings.Value;
            _logger = logger;
        }

        public async Task<bool> SendWelcomeEmailAsync(string toEmail, string studentName, string username, string password)
        {
            try
            {
                var subject = "مرحباً بك في أكاديمية الذكاء الاصطناعي والروبوتات";
                var body = $@"
                <html dir='rtl'>
                <body style='font-family: Arial, sans-serif; text-align: right; direction: rtl;'>
                    <div style='max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 10px;'>
                        <h2 style='color: #2563eb; text-align: center;'>أهلاً وسهلاً {studentName}</h2>
                        
                        <p>نرحب بك في أكاديمية الذكاء الاصطناعي والروبوتات!</p>
                        
                        <div style='background-color: #f8f9fa; padding: 20px; border-radius: 8px; margin: 20px 0;'>
                            <h3 style='color: #495057; margin-top: 0;'>بيانات الدخول الخاصة بك:</h3>
                            <p><strong>اسم المستخدم:</strong> {username}</p>
                            <p><strong>كلمة المرور:</strong> {password}</p>
                        </div>
                        
                        <div style='background-color: #fff3cd; padding: 15px; border-radius: 8px; border-left: 4px solid #ffc107;'>
                            <p style='margin: 0;'><strong>مهم:</strong> يرجى الاحتفاظ بهذه البيانات في مكان آمن وعدم مشاركتها مع أي شخص آخر.</p>
                        </div>
                        
                        <p>يمكنك الآن تسجيل الدخول إلى حسابك والاستفادة من جميع الخدمات المتاحة.</p>
                        
                        <div style='text-align: center; margin-top: 30px;'>
                            <p style='color: #6c757d;'>أكاديمية الذكاء الاصطناعي والروبوتات</p>
                            <p style='color: #6c757d; font-size: 12px;'>هذا إيميل تلقائي، يرجى عدم الرد عليه</p>
                        </div>
                    </div>
                </body>
                </html>";

                return await SendEmailAsync(toEmail, subject, body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending welcome email to {Email}", toEmail);
                return false;
            }
        }

        public async Task<bool> SendCourseEnrollmentEmailAsync(string toEmail, string studentName, string courseName, DateTime startDate, DateTime endDate, string courseDescription)
        {
            try
            {
                var subject = $"تأكيد التسجيل في كورس {courseName}";
                var body = $@"
                <html dir='rtl'>
                <body style='font-family: Arial, sans-serif; text-align: right; direction: rtl;'>
                    <div style='max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 10px;'>
                        <h2 style='color: #10b981; text-align: center;'>تم تسجيلك بنجاح!</h2>
                        
                        <p>عزيزي/عزيزتي {studentName},</p>
                        
                        <p>نتشرف بإبلاغك أنه تم تسجيلك بنجاح في الكورس التالي:</p>
                        
                        <div style='background-color: #f0f9ff; padding: 20px; border-radius: 8px; margin: 20px 0; border-left: 4px solid #10b981;'>
                            <h3 style='color: #1f2937; margin-top: 0;'>{courseName}</h3>
                            <p style='color: #6b7280; margin-bottom: 15px;'>{courseDescription}</p>
                            
                            <div style='display: grid; grid-template-columns: 1fr 1fr; gap: 10px;'>
                                <p><strong>تاريخ البداية:</strong> {startDate:yyyy/MM/dd}</p>
                                <p><strong>تاريخ النهاية:</strong> {endDate:yyyy/MM/dd}</p>
                            </div>
                        </div>
                        
                        <div style='background-color: #dcfce7; padding: 15px; border-radius: 8px;'>
                            <h4 style='color: #166534; margin-top: 0;'>الخطوات التالية:</h4>
                            <ul style='color: #166534;'>
                                <li>تسجيل الدخول إلى حسابك في الموقع</li>
                                <li>مراجعة تفاصيل الكورس والمواد التعليمية</li>
                                <li>التواصل مع المدرب في حالة وجود أي استفسارات</li>
                            </ul>
                        </div>
                        
                        <p>نتطلع إلى رؤيتك في الكورس ونتمنى لك تجربة تعليمية ممتعة ومفيدة!</p>
                        
                        <div style='text-align: center; margin-top: 30px;'>
                            <p style='color: #6c757d;'>أكاديمية الذكاء الاصطناعي والروبوتات</p>
                            <p style='color: #6c757d; font-size: 12px;'>للاستفسارات، يرجى التواصل معنا</p>
                        </div>
                    </div>
                </body>
                </html>";

                return await SendEmailAsync(toEmail, subject, body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending course enrollment email to {Email}", toEmail);
                return false;
            }
        }

        private async Task<bool> SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                using var client = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.SmtpPort);
                client.EnableSsl = _emailSettings.UseSSL;
                client.Credentials = new NetworkCredential(_emailSettings.SenderEmail, _emailSettings.SenderPassword);

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_emailSettings.SenderEmail, _emailSettings.SenderName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(toEmail);

                await client.SendMailAsync(mailMessage);
                _logger.LogInformation("Email sent successfully to {Email}", toEmail);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email}", toEmail);
                return false;
            }
        }
    }
} 