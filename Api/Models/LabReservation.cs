using System.ComponentModel.DataAnnotations;

namespace Api.Models
{
    public enum ReservationStatus
    {
        Scheduled = 1,  // مجدول
        Active = 2,     // نشط
        Completed = 3,  // مكتمل
        Cancelled = 4   // ملغي
    }

    public class LabReservation
    {
        public int Id { get; set; }
        
        public int LabId { get; set; } // المعمل
        
        public int? CourseId { get; set; } // الكورس (اختياري)
        
        public DateTime StartTime { get; set; } // وقت البداية
        
        public DateTime EndTime { get; set; } // وقت النهاية
        
        public ReservationStatus Status { get; set; } = ReservationStatus.Scheduled; // حالة الحجز
        
        [MaxLength(200)]
        public string Purpose { get; set; } = string.Empty; // الغرض من الحجز
        
        public int ReservedByUserId { get; set; } // محجوز بواسطة
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        [MaxLength(500)]
        public string Notes { get; set; } = string.Empty; // ملاحظات
        
        // Navigation Properties
        public virtual Lab Lab { get; set; } = null!;
        public virtual Course? Course { get; set; }
        public virtual User ReservedByUser { get; set; } = null!;
    }
}
