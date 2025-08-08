using System.ComponentModel.DataAnnotations;

namespace Api.Models
{
    public enum EquipmentStatus
    {
        Available = 1,          // متاح
        InUse = 2,              // قيد الاستخدام
        Reserved = 3,           // محجوز
        UnderMaintenance = 4,   // تحت الصيانة
        OutOfOrder = 5          // معطل
    }

    public enum EquipmentCondition
    {
        Excellent = 1,      // ممتاز
        Good = 2,           // جيد
        Fair = 3,           // مقبول
        Poor = 4,           // سيء
        Damaged = 5         // تالف
    }

    public class Equipment
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty; // اسم المعدة
        
        [Required]
        [MaxLength(100)]
        public string SerialNumber { get; set; } = string.Empty; // الرقم التسلسلي
        
        [MaxLength(100)]
        public string Model { get; set; } = string.Empty; // الموديل
        
        [MaxLength(100)]
        public string Brand { get; set; } = string.Empty; // الماركة

        [MaxLength(100)]
        public string Manufacturer { get; set; } = string.Empty; // الشركة المصنعة
        
        [MaxLength(50)]
        public string Category { get; set; } = string.Empty; // الفئة
        
        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty; // الوصف
        
        public decimal PurchasePrice { get; set; } = 0; // سعر الشراء
        
        public DateTime PurchaseDate { get; set; } // تاريخ الشراء
        
        public DateTime? WarrantyExpiry { get; set; } // انتهاء الضمان
        
        public EquipmentStatus Status { get; set; } = EquipmentStatus.Available; // الحالة
        
        public EquipmentCondition Condition { get; set; } = EquipmentCondition.Excellent; // الحالة الفيزيائية
        
        public int BranchId { get; set; } // الفرع
        
        public int? RoomId { get; set; } // الغرفة
        
        public int? AssignedToUserId { get; set; } // مخصص لمستخدم
        
        [MaxLength(500)]
        public string Location { get; set; } = string.Empty; // الموقع
        
        public DateTime? LastMaintenanceDate { get; set; } // آخر صيانة
        
        public DateTime? NextMaintenanceDate { get; set; } // الصيانة القادمة
        
        [MaxLength(1000)]
        public string MaintenanceNotes { get; set; } = string.Empty; // ملاحظات الصيانة
        
        public bool IsActive { get; set; } = true; // نشط
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        [MaxLength(1000)]
        public string Notes { get; set; } = string.Empty; // ملاحظات
        
        // Navigation Properties
        public virtual Branch Branch { get; set; } = null!;
        public virtual Room? Room { get; set; }
        public virtual User? AssignedToUser { get; set; }
        public virtual ICollection<EquipmentReservation> EquipmentReservations { get; set; } = new List<EquipmentReservation>();
    }
}
