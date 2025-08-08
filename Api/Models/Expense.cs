using System.ComponentModel.DataAnnotations;

namespace Api.Models
{
    public enum ExpenseCategory
    {
        Equipment = 1,          // معدات
        Maintenance = 2,        // صيانة
        Utilities = 3,          // مرافق (كهرباء، مياه، إنترنت)
        Supplies = 4,           // مستلزمات
        Marketing = 5,          // تسويق
        Salaries = 6,           // رواتب
        Rent = 7,               // إيجار
        Transportation = 8,     // مواصلات
        Food = 9,               // طعام (كافيتريا)
        Training = 10,          // تدريب
        Software = 11,          // برمجيات
        Insurance = 12,         // تأمين
        Legal = 13,             // قانونية
        Other = 14              // أخرى
    }

    public enum ExpenseStatus
    {
        Pending = 1,            // في الانتظار
        Approved = 2,           // موافق عليه
        Rejected = 3,           // مرفوض
        Paid = 4                // مدفوع
    }

    public enum ExpensePriority
    {
        Low = 1,                // منخفضة
        Medium = 2,             // متوسطة
        High = 3,               // عالية
        Urgent = 4              // عاجلة
    }

    public class Expense
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty; // عنوان المصروف
        
        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty; // الوصف
        
        public decimal Amount { get; set; } // المبلغ
        
        public ExpenseCategory Category { get; set; } // الفئة
        
        public DateTime ExpenseDate { get; set; } // تاريخ المصروف
        
        public ExpenseStatus Status { get; set; } = ExpenseStatus.Pending; // الحالة

        public ExpensePriority Priority { get; set; } = ExpensePriority.Medium; // الأولوية

        public int BranchId { get; set; } // الفرع
        
        public int RequestedByUserId { get; set; } // طلب بواسطة
        
        public int? ApprovedByUserId { get; set; } // موافق عليه بواسطة

        public DateTime? ApprovedAt { get; set; } // تاريخ الموافقة

        [MaxLength(500)]
        public string RejectionReason { get; set; } = string.Empty; // سبب الرفض
        
        [MaxLength(100)]
        public string ReceiptNumber { get; set; } = string.Empty; // رقم الإيصال
        
        [MaxLength(200)]
        public string Vendor { get; set; } = string.Empty; // المورد

        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash; // طريقة الدفع
        
        [MaxLength(500)]
        public string AttachmentPath { get; set; } = string.Empty; // مسار المرفق
        
        public bool IsRecurring { get; set; } = false; // متكرر

        [MaxLength(100)]
        public string RecurrencePattern { get; set; } = string.Empty; // نمط التكرار

        public DateTime? NextRecurrenceDate { get; set; } // تاريخ التكرار التالي
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        [MaxLength(1000)]
        public string Notes { get; set; } = string.Empty; // ملاحظات
        
        // Navigation Properties
        public virtual Branch Branch { get; set; } = null!;
        public virtual User RequestedByUser { get; set; } = null!;
        public virtual User? ApprovedByUser { get; set; }
    }
}
