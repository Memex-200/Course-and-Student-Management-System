using System.ComponentModel.DataAnnotations;

namespace Api.Models
{
    public enum InvoiceItemType
    {
        DeskUsage = 1,    // استخدام المكتب
        Internet = 2,     // إنترنت
        Laptop = 3,       // لابتوب
        Cafeteria = 4     // كافيتريا
    }

    public class WorkspaceInvoiceItem
    {
        public int Id { get; set; }
        
        public int WorkspaceInvoiceId { get; set; } // الفاتورة
        
        public int? WorkspaceSessionId { get; set; } // الجلسة (اختياري)
        
        public InvoiceItemType ItemType { get; set; } // نوع العنصر
        
        [Required]
        [MaxLength(200)]
        public string Description { get; set; } = string.Empty; // الوصف
        
        public decimal Quantity { get; set; } = 1; // الكمية
        
        public decimal UnitPrice { get; set; } // سعر الوحدة
        
        public decimal TotalPrice => Quantity * UnitPrice; // السعر الإجمالي
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation Properties
        public virtual WorkspaceInvoice WorkspaceInvoice { get; set; } = null!;
        public virtual WorkspaceSession? WorkspaceSession { get; set; }
    }
}
