using System.ComponentModel.DataAnnotations;

namespace Api.Models
{
    public enum WorkspaceBookingStatus
    {
        Pending = 1,            // في الانتظار
        Confirmed = 2,          // مؤكد
        InProgress = 3,         // قيد التنفيذ
        Completed = 4,          // مكتمل
        Cancelled = 5           // ملغي
    }

    public class WorkspaceBooking
    {
        public int Id { get; set; }
        
        public int StudentId { get; set; } // الطالب
        
        public int RoomId { get; set; } // الغرفة
        
        public DateTime BookingDate { get; set; } // تاريخ الحجز
        
        public DateTime StartTime { get; set; } // وقت البداية
        
        public DateTime EndTime { get; set; } // وقت النهاية
        
        public int HoursBooked { get; set; } // عدد الساعات المحجوزة
        
        public decimal HourlyRate { get; set; } // سعر الساعة
        
        public decimal TotalAmount { get; set; } // المبلغ الإجمالي
        
        public decimal PaidAmount { get; set; } = 0; // المبلغ المدفوع
        
        public decimal RemainingAmount { get; set; } = 0; // المبلغ المتبقي
        
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending; // حالة الدفع
        
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash; // طريقة الدفع
        
        public DateTime? PaymentDate { get; set; } // تاريخ الدفع
        
        [MaxLength(500)]
        public string PaymentNotes { get; set; } = string.Empty; // ملاحظات الدفع
        
        public WorkspaceBookingStatus Status { get; set; } = WorkspaceBookingStatus.Pending; // حالة الحجز
        
        public int BranchId { get; set; } // الفرع
        
        public int BookedByUserId { get; set; } // محجوز بواسطة
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        [MaxLength(1000)]
        public string Notes { get; set; } = string.Empty; // ملاحظات
        
        // Navigation Properties
        public virtual Student Student { get; set; } = null!;
        public virtual Room Room { get; set; } = null!;
        public virtual Branch Branch { get; set; } = null!;
        public virtual User BookedByUser { get; set; } = null!;
    }
}
