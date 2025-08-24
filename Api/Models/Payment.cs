using System.ComponentModel.DataAnnotations;

namespace Api.Models
{
    public enum PaymentType
    {
        CourseFee = 1,        // رسوم الكورس
        Cafeteria = 2,        // الكافيتريا
        Workspace = 3,        // مساحة العمل
        Equipment = 4,        // المعدات
        Other = 5             // أخرى
    }

    public class Payment
    {
        public int Id { get; set; }
        
        public int? StudentId { get; set; } // الطالب (مباشر)
        
        public int? CourseRegistrationId { get; set; } // تسجيل الكورس

        public int? WorkspaceBookingId { get; set; } // حجز مساحة العمل

        public int? SharedWorkspaceBookingId { get; set; } // حجز المساحة المشتركة

        public int? BranchId { get; set; } // الفرع

        public decimal Amount { get; set; } // المبلغ
        
        public PaymentMethod PaymentMethod { get; set; } // طريقة الدفع
        
        public PaymentType PaymentType { get; set; } = PaymentType.CourseFee; // نوع الدفع
        
        public PaymentSource PaymentSource { get; set; } = PaymentSource.CourseFee; // مصدر الدفع
        
        public DateTime PaymentDate { get; set; } = DateTime.UtcNow; // تاريخ الدفع
        
        [MaxLength(100)]
        public string TransactionReference { get; set; } = string.Empty; // مرجع المعاملة
        
        public int ProcessedByUserId { get; set; } // معالج بواسطة
        
        public bool IsActive { get; set; } = true; // نشط أم لا
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        [MaxLength(500)]
        public string Notes { get; set; } = string.Empty; // ملاحظات
        
        // Navigation Properties
        public virtual Student? Student { get; set; }
        public virtual CourseRegistration? CourseRegistration { get; set; }
        public virtual WorkspaceBooking? WorkspaceBooking { get; set; }
        public virtual SharedWorkspaceBooking? SharedWorkspaceBooking { get; set; }
        public virtual Branch? Branch { get; set; }
        public virtual User ProcessedByUser { get; set; } = null!;
    }
}
