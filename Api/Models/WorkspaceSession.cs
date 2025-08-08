using System.ComponentModel.DataAnnotations;

namespace Api.Models
{
    public enum SessionStatus
    {
        Active = 1,     // نشط
        Completed = 2,  // مكتمل
        Cancelled = 3   // ملغي
    }

    public class WorkspaceSession
    {
        public int Id { get; set; }
        
        public int WorkspaceDeskId { get; set; } // المكتب
        
        public int? StudentId { get; set; } // الطالب (اختياري)
        
        [MaxLength(100)]
        public string CustomerName { get; set; } = string.Empty; // اسم العميل
        
        [MaxLength(20)]
        public string CustomerPhone { get; set; } = string.Empty; // هاتف العميل
        
        public DateTime StartTime { get; set; } = DateTime.UtcNow; // وقت البداية
        
        public DateTime? EndTime { get; set; } // وقت النهاية
        
        public TimeSpan? Duration => EndTime?.Subtract(StartTime); // المدة
        
        public bool UsedInternet { get; set; } = false; // استخدم إنترنت
        
        public bool UsedLaptop { get; set; } = false; // استخدم لابتوب
        
        public decimal InternetCost { get; set; } = 0; // تكلفة الإنترنت
        
        public decimal LaptopCost { get; set; } = 0; // تكلفة اللابتوب
        
        public decimal CafeteriaCost { get; set; } = 0; // تكلفة الكافيتريا
        
        public decimal TotalCost { get; set; } = 0; // التكلفة الإجمالية
        
        public SessionStatus Status { get; set; } = SessionStatus.Active; // حالة الجلسة
        
        public int CreatedByUserId { get; set; } // أنشئ بواسطة
        
        public int BranchId { get; set; } // الفرع
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        [MaxLength(500)]
        public string Notes { get; set; } = string.Empty; // ملاحظات
        
        // Navigation Properties
        public virtual WorkspaceDesk WorkspaceDesk { get; set; } = null!;
        public virtual Student? Student { get; set; }
        public virtual User CreatedByUser { get; set; } = null!;
        public virtual Branch Branch { get; set; } = null!;
        public virtual ICollection<WorkspaceInvoiceItem> WorkspaceInvoiceItems { get; set; } = new List<WorkspaceInvoiceItem>();
    }
}
