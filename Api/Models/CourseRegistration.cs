using System.ComponentModel.DataAnnotations;

namespace Api.Models
{
    public enum PaymentStatus
    {
        Pending = 1,       // في الانتظار
        Unpaid = 2,        // غير مدفوع
        PartiallyPaid = 3, // مدفوع جزئياً
        FullyPaid = 4,     // مدفوع بالكامل
        Cancelled = 5      // ملغي
    }

    public enum PaymentMethod
    {
        Cash = 1,      // كاش
        InstaPay = 2,  // انستا باي
        Fawry = 3      // فوري
    }

    public class CourseRegistration
    {
        public int Id { get; set; }
        
        public int StudentId { get; set; } // الطالب
        
        public int CourseId { get; set; } // الكورس
        
        public DateTime RegistrationDate { get; set; } = DateTime.UtcNow; // تاريخ التسجيل
        
        public decimal TotalAmount { get; set; } // المبلغ الإجمالي
        
        public decimal PaidAmount { get; set; } // المبلغ المدفوع
        
        public decimal RemainingAmount => TotalAmount - PaidAmount; // المبلغ المتبقي
        
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Unpaid; // حالة الدفع
        
        public PaymentMethod? PaymentMethod { get; set; } // طريقة الدفع

        public DateTime? PaymentDate { get; set; } // تاريخ الدفع

        [MaxLength(500)]
        public string PaymentNotes { get; set; } = string.Empty; // ملاحظات الدفع

        public bool IsActive { get; set; } = true; // نشط أم لا
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        [MaxLength(500)]
        public string Notes { get; set; } = string.Empty; // ملاحظات
        
        // Navigation Properties
        public virtual Student Student { get; set; } = null!;
        public virtual Course Course { get; set; } = null!;
        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}
