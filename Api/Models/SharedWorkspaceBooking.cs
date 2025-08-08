using System.ComponentModel.DataAnnotations;

namespace Api.Models
{
    public enum SharedWorkspaceStatus
    {
        Available = 1,      // متاح
        Occupied = 2,       // مشغول
        Full = 3,           // ممتلئ
        Maintenance = 4     // صيانة
    }

    public enum BookingType
    {
        Individual = 1,     // فردي
        Group = 2,          // جماعي
        Study = 3,          // دراسة
        Meeting = 4,        // اجتماع
        Project = 5         // مشروع
    }

    // نموذج الـ Workspace المشترك
    public class SharedWorkspace
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty; // اسم المساحة
        
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty; // الوصف
        
        public int MaxCapacity { get; set; } = 10; // الحد الأقصى للأشخاص
        
        public int CurrentOccupancy { get; set; } = 0; // العدد الحالي
        
        public decimal HourlyRatePerPerson { get; set; } // السعر بالساعة للشخص الواحد
        
        public bool HasWifi { get; set; } = true; // واي فاي
        
        public bool HasPrinter { get; set; } = false; // طابعة
        
        public bool HasProjector { get; set; } = false; // بروجكتر
        
        public bool HasWhiteboard { get; set; } = true; // سبورة
        
        [MaxLength(500)]
        public string Equipment { get; set; } = string.Empty; // المعدات المتاحة
        
        public SharedWorkspaceStatus Status { get; set; } = SharedWorkspaceStatus.Available;
        
        public int BranchId { get; set; } // الفرع
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation Properties
        public virtual Branch Branch { get; set; } = null!;
        public virtual ICollection<SharedWorkspaceBooking> Bookings { get; set; } = new List<SharedWorkspaceBooking>();
    }

    // نموذج الحجز المشترك
    public class SharedWorkspaceBooking
    {
        public int Id { get; set; }
        
        public int SharedWorkspaceId { get; set; } // المساحة المشتركة
        
        public int? StudentId { get; set; } // الطالب (اختياري)
        
        [MaxLength(100)]
        public string CustomerName { get; set; } = string.Empty; // اسم العميل
        
        [MaxLength(20)]
        public string CustomerPhone { get; set; } = string.Empty; // رقم الهاتف
        
        [MaxLength(100)]
        public string CustomerEmail { get; set; } = string.Empty; // البريد الإلكتروني
        
        public DateTime StartTime { get; set; } // وقت البداية
        
        public DateTime EndTime { get; set; } // وقت النهاية
        
        public int NumberOfPeople { get; set; } = 1; // عدد الأشخاص
        
        public decimal HourlyRate { get; set; } // السعر بالساعة
        
        public decimal TotalHours { get; set; } // إجمالي الساعات
        
        public decimal TotalAmount { get; set; } // المبلغ الإجمالي
        
        public decimal PaidAmount { get; set; } = 0; // المبلغ المدفوع
        
        public decimal RemainingAmount => TotalAmount - PaidAmount; // المبلغ المتبقي
        
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
        
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;
        
        public DateTime? PaymentDate { get; set; }
        
        public BookingType BookingType { get; set; } = BookingType.Individual;
        
        public WorkspaceBookingStatus Status { get; set; } = WorkspaceBookingStatus.Pending;
        
        [MaxLength(500)]
        public string Purpose { get; set; } = string.Empty; // الغرض من الحجز
        
        [MaxLength(1000)]
        public string Notes { get; set; } = string.Empty; // ملاحظات
        
        [MaxLength(500)]
        public string RequiredEquipment { get; set; } = string.Empty; // المعدات المطلوبة
        
        public bool NeedsInternet { get; set; } = true; // يحتاج إنترنت
        
        public bool NeedsPrinter { get; set; } = false; // يحتاج طابعة
        
        public bool NeedsProjector { get; set; } = false; // يحتاج بروجكتر
        
        public DateTime? CheckInTime { get; set; } // وقت الدخول الفعلي
        
        public DateTime? CheckOutTime { get; set; } // وقت الخروج الفعلي
        
        public int BranchId { get; set; } // الفرع
        
        public int BookedByUserId { get; set; } // محجوز بواسطة
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation Properties
        public virtual SharedWorkspace SharedWorkspace { get; set; } = null!;
        public virtual Student? Student { get; set; }
        public virtual Branch Branch { get; set; } = null!;
        public virtual User BookedByUser { get; set; } = null!;
        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }

    // نموذج لتتبع الحضور في الوقت الفعلي
    public class WorkspaceOccupancy
    {
        public int Id { get; set; }
        
        public int SharedWorkspaceId { get; set; } // المساحة
        
        public int SharedWorkspaceBookingId { get; set; } // الحجز
        
        public DateTime CheckInTime { get; set; } = DateTime.UtcNow; // وقت الدخول
        
        public DateTime? CheckOutTime { get; set; } // وقت الخروج
        
        public int ActualPeopleCount { get; set; } = 1; // العدد الفعلي للأشخاص
        
        public bool IsActive { get; set; } = true; // نشط حالياً
        
        [MaxLength(500)]
        public string Notes { get; set; } = string.Empty; // ملاحظات
        
        // Navigation Properties
        public virtual SharedWorkspace SharedWorkspace { get; set; } = null!;
        public virtual SharedWorkspaceBooking Booking { get; set; } = null!;
    }
}
