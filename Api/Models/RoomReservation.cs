using System.ComponentModel.DataAnnotations;

namespace Api.Models
{
    public enum ReservationType
    {
        Course = 1,       // كورس
        Meeting = 2,      // اجتماع
        Workshop = 3,     // ورشة عمل
        Event = 4,        // فعالية
        Maintenance = 5,  // صيانة
        Other = 6         // أخرى
    }

    public enum RoomReservationStatus
    {
        Scheduled = 1,    // مجدول
        InProgress = 2,   // قيد التنفيذ
        Completed = 3,    // مكتمل
        Cancelled = 4,    // ملغي
        NoShow = 5        // لم يحضر
    }

    public class RoomReservation
    {
        public int Id { get; set; }
        
        public int RoomId { get; set; } // الغرفة
        
        public int? CourseId { get; set; } // الكورس (اختياري)
        
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty; // عنوان الحجز
        
        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty; // وصف الحجز
        
        public ReservationType ReservationType { get; set; } // نوع الحجز
        
        public DateTime StartDateTime { get; set; } // تاريخ ووقت البداية
        
        public DateTime EndDateTime { get; set; } // تاريخ ووقت النهاية
        
        public TimeSpan Duration => EndDateTime - StartDateTime; // المدة
        
        public int ExpectedAttendees { get; set; } = 0; // عدد الحضور المتوقع
        
        public int ActualAttendees { get; set; } = 0; // عدد الحضور الفعلي
        
        public RoomReservationStatus Status { get; set; } = RoomReservationStatus.Scheduled; // حالة الحجز
        
        public int ReservedByUserId { get; set; } // محجوز بواسطة
        
        public int? InstructorId { get; set; } // المدرس (اختياري)
        
        [MaxLength(500)]
        public string RequiredEquipment { get; set; } = string.Empty; // المعدات المطلوبة
        
        [MaxLength(500)]
        public string SpecialRequirements { get; set; } = string.Empty; // متطلبات خاصة
        
        public bool IsRecurring { get; set; } = false; // متكرر أم لا
        
        [MaxLength(100)]
        public string RecurrencePattern { get; set; } = string.Empty; // نمط التكرار
        
        public DateTime? RecurrenceEndDate { get; set; } // تاريخ انتهاء التكرار
        
        public bool SendReminder { get; set; } = true; // إرسال تذكير
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; } // تاريخ آخر تحديث
        
        [MaxLength(1000)]
        public string Notes { get; set; } = string.Empty; // ملاحظات
        
        [MaxLength(500)]
        public string CancellationReason { get; set; } = string.Empty; // سبب الإلغاء
        
        // Navigation Properties
        public virtual Room Room { get; set; } = null!;
        public virtual Course? Course { get; set; }
        public virtual User ReservedByUser { get; set; } = null!;
        public virtual Employee? Instructor { get; set; }
    }
}
