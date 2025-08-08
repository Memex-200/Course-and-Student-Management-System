using System.ComponentModel.DataAnnotations;

namespace Api.Models
{
    public class WorkspaceInvoice
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string InvoiceNumber { get; set; } = string.Empty; // رقم الفاتورة
        
        public int WorkspaceSessionId { get; set; } // جلسة الـ workspace
        
        [MaxLength(100)]
        public string CustomerName { get; set; } = string.Empty; // اسم العميل
        
        [MaxLength(20)]
        public string CustomerPhone { get; set; } = string.Empty; // هاتف العميل
        
        public DateTime InvoiceDate { get; set; } = DateTime.UtcNow; // تاريخ الفاتورة
        
        public decimal SubTotal { get; set; } // المجموع الفرعي
        
        public decimal TaxAmount { get; set; } = 0; // ضريبة
        
        public decimal DiscountAmount { get; set; } = 0; // خصم
        
        public decimal TotalAmount { get; set; } // المجموع الإجمالي
        
        public PaymentMethod PaymentMethod { get; set; } // طريقة الدفع
        
        public bool IsPaid { get; set; } = false; // مدفوع أم لا
        
        public int BranchId { get; set; } // الفرع
        
        public int CreatedByUserId { get; set; } // أنشئ بواسطة
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        [MaxLength(500)]
        public string Notes { get; set; } = string.Empty; // ملاحظات
        
        // Navigation Properties
        public virtual WorkspaceSession WorkspaceSession { get; set; } = null!;
        public virtual Branch Branch { get; set; } = null!;
        public virtual User CreatedByUser { get; set; } = null!;
        public virtual ICollection<WorkspaceInvoiceItem> WorkspaceInvoiceItems { get; set; } = new List<WorkspaceInvoiceItem>();
    }
}
