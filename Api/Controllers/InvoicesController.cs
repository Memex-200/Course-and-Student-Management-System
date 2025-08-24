using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using Api.Models;
using Api.Data;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class InvoicesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<InvoicesController> _logger;

        public InvoicesController(ApplicationDbContext context, ILogger<InvoicesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("payment/{paymentId}")]
        [Authorize(Policy = "AdminOrEmployee")]
        public async Task<IActionResult> GetPaymentInvoice(int paymentId)
        {
            try
            {
                var payment = await _context.Payments
                    .Include(p => p.CourseRegistration)
                        .ThenInclude(cr => cr.Student)
                    .Include(p => p.CourseRegistration)
                        .ThenInclude(cr => cr.Course)
                    .Include(p => p.ProcessedByUser)
                    .FirstOrDefaultAsync(p => p.Id == paymentId);

                if (payment == null)
                    return NotFound(new { message = "الدفعة غير موجودة" });

                var invoiceNumber = $"INV-{payment.Id:D6}";
                var invoiceData = new
                {
                    InvoiceNumber = invoiceNumber,
                    InvoiceDate = payment.PaymentDate.ToString("yyyy-MM-dd"),
                    StudentName = payment.CourseRegistration.Student?.FullName ?? "غير محدد",
                    CourseName = payment.CourseRegistration.Course?.Name ?? "غير محدد",
                    Amount = payment.Amount,
                    PaymentMethod = GetPaymentMethodArabic(payment.PaymentMethod),
                    PaymentDate = payment.PaymentDate.ToString("yyyy-MM-dd"),
                    PaymentStatus = "مدفوع",
                    ProcessedBy = payment.ProcessedByUser?.FullName ?? "غير محدد",
                    Notes = payment.Notes ?? "دفعة كورس"
                };

                var pdfBytes = GenerateInvoicePDF(invoiceData, "payment");
                return File(pdfBytes, "application/pdf", $"invoice-{invoiceNumber}.pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating payment invoice");
                return StatusCode(500, new { message = "حدث خطأ في الخادم", error = ex.Message });
            }
        }

        [HttpGet("expense/{expenseId}")]
        [Authorize(Policy = "AdminOrEmployee")]
        public async Task<IActionResult> GetExpenseInvoice(int expenseId)
        {
            try
            {
                var expense = await _context.Expenses
                    .Include(e => e.Branch)
                    .Include(e => e.RequestedByUser)
                    .FirstOrDefaultAsync(e => e.Id == expenseId);

                if (expense == null)
                    return NotFound(new { message = "المصروف غير موجود" });

                var invoiceNumber = $"EXP-{expense.Id:D6}";
                var invoiceData = new
                {
                    InvoiceNumber = invoiceNumber,
                    InvoiceDate = expense.ExpenseDate.ToString("yyyy-MM-dd"),
                    StudentName = "مصروف تشغيلي",
                    CourseName = expense.Category.ToString(),
                    Amount = Math.Abs(expense.Amount),
                    PaymentMethod = GetPaymentMethodArabic(expense.PaymentMethod),
                    PaymentDate = expense.ExpenseDate.ToString("yyyy-MM-dd"),
                    PaymentStatus = expense.Status == ExpenseStatus.Paid ? "مدفوع" : "غير مدفوع",
                    ProcessedBy = expense.RequestedByUser?.FullName ?? "غير محدد",
                    Notes = expense.Notes ?? expense.Description
                };

                var pdfBytes = GenerateInvoicePDF(invoiceData, "expense");
                return File(pdfBytes, "application/pdf", $"expense-{invoiceNumber}.pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating expense invoice");
                return StatusCode(500, new { message = "حدث خطأ في الخادم", error = ex.Message });
            }
        }

        private byte[] GenerateInvoicePDF(dynamic invoiceData, string type)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                Document document = new Document(PageSize.A4, 25, 25, 30, 30);
                PdfWriter writer = PdfWriter.GetInstance(document, ms);

                document.Open();

                // Add Arabic font support
                BaseFont bf = BaseFont.CreateFont("C:\\Windows\\Fonts\\arial.ttf", BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                Font titleFont = new Font(bf, 18, Font.BOLD);
                Font headerFont = new Font(bf, 12, Font.BOLD);
                Font normalFont = new Font(bf, 10, Font.NORMAL);

                // Header
                Paragraph title = new Paragraph("فاتورة مالية", titleFont);
                title.Alignment = Element.ALIGN_CENTER;
                document.Add(title);
                document.Add(new Paragraph(" ")); // Spacing

                // Invoice Info
                PdfPTable infoTable = new PdfPTable(2);
                infoTable.WidthPercentage = 100;
                infoTable.SetWidths(new float[] { 1f, 1f });

                infoTable.AddCell(new PdfPCell(new Phrase($"رقم الفاتورة: {invoiceData.InvoiceNumber}", headerFont)) { Border = 0 });
                infoTable.AddCell(new PdfPCell(new Phrase($"التاريخ: {invoiceData.InvoiceDate}", headerFont)) { Border = 0 });

                document.Add(infoTable);
                document.Add(new Paragraph(" ")); // Spacing

                // Transaction Details
                PdfPTable detailsTable = new PdfPTable(2);
                detailsTable.WidthPercentage = 100;
                detailsTable.SetWidths(new float[] { 1f, 2f });

                AddTableRow(detailsTable, "اسم الطالب:", invoiceData.StudentName, headerFont, normalFont);
                AddTableRow(detailsTable, "الكورس:", invoiceData.CourseName, headerFont, normalFont);
                AddTableRow(detailsTable, "المبلغ:", $"{invoiceData.Amount} جنيه", headerFont, normalFont);
                AddTableRow(detailsTable, "طريقة الدفع:", invoiceData.PaymentMethod, headerFont, normalFont);
                AddTableRow(detailsTable, "تاريخ الدفع:", invoiceData.PaymentDate, headerFont, normalFont);
                AddTableRow(detailsTable, "حالة الدفع:", invoiceData.PaymentStatus, headerFont, normalFont);
                AddTableRow(detailsTable, "تمت المعالجة بواسطة:", invoiceData.ProcessedBy, headerFont, normalFont);

                if (!string.IsNullOrEmpty(invoiceData.Notes))
                {
                    AddTableRow(detailsTable, "ملاحظات:", invoiceData.Notes, headerFont, normalFont);
                }

                document.Add(detailsTable);

                document.Close();
                return ms.ToArray();
            }
        }

        private void AddTableRow(PdfPTable table, string label, string value, Font labelFont, Font valueFont)
        {
            table.AddCell(new PdfPCell(new Phrase(label, labelFont)) { Border = 0, PaddingBottom = 5 });
            table.AddCell(new PdfPCell(new Phrase(value, valueFont)) { Border = 0, PaddingBottom = 5 });
        }

        private string GetPaymentMethodArabic(PaymentMethod method)
        {
            return method switch
            {
                PaymentMethod.Cash => "نقدي",
                PaymentMethod.InstaPay => "انستا باي",
                PaymentMethod.Fawry => "فوري",
                _ => method.ToString()
            };
        }
    }
}
