using System.ComponentModel.DataAnnotations;

namespace Api.Models
{
    public enum EquipmentReservationStatus
    {
        Pending = 1,        // في الانتظار
        Approved = 2,       // موافق عليه
        InUse = 3,          // قيد الاستخدام
        Returned = 4,       // تم الإرجاع
        Cancelled = 5,      // ملغي
        Overdue = 6         // متأخر
    }

    public class EquipmentReservation
    {
        public int Id { get; set; }
        
        public int EquipmentId { get; set; } // المعدة
        
        public int ReservedByUserId { get; set; } // محجوز بواسطة
        
        public int? CourseId { get; set; } // الكورس (اختياري)
        
        [Required]
        [MaxLength(200)]
        public string Purpose { get; set; } = string.Empty; // الغرض من الحجز
        
        public DateTime ReservationDate { get; set; } // تاريخ الحجز
        
        public DateTime StartDateTime { get; set; } // تاريخ ووقت البداية
        
        public DateTime EndDateTime { get; set; } // تاريخ ووقت النهاية
        
        public DateTime? ActualReturnDateTime { get; set; } // تاريخ الإرجاع الفعلي
        
        public EquipmentReservationStatus Status { get; set; } = EquipmentReservationStatus.Pending;
        
        public int? ApprovedByUserId { get; set; } // موافق عليه بواسطة
        
        public DateTime? ApprovedAt { get; set; } // تاريخ الموافقة

        public DateTime? ActualStartDateTime { get; set; } // تاريخ البداية الفعلي

        public DateTime? ActualEndDateTime { get; set; } // تاريخ النهاية الفعلي
        
        [MaxLength(1000)]
        public string Notes { get; set; } = string.Empty; // ملاحظات
        
        [MaxLength(1000)]
        public string ReturnNotes { get; set; } = string.Empty; // ملاحظات الإرجاع
        
        public bool IsEquipmentDamaged { get; set; } = false; // هل المعدة تالفة
        
        [MaxLength(1000)]
        public string DamageDescription { get; set; } = string.Empty; // وصف التلف
        
        public decimal? DamageCost { get; set; } // تكلفة التلف
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation Properties
        public virtual Equipment Equipment { get; set; } = null!;
        public virtual User ReservedByUser { get; set; } = null!;
        public virtual User? ApprovedByUser { get; set; }
        public virtual Course? Course { get; set; }
    }
}
