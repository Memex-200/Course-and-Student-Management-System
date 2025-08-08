using System.ComponentModel.DataAnnotations;

namespace Api.Models
{
    public enum CafeteriaOrderStatus
    {
        Pending = 1,        // في الانتظار
        Preparing = 2,      // قيد التحضير
        Ready = 3,          // جاهز
        Delivered = 4,      // تم التسليم
        Cancelled = 5       // ملغي
    }

    public class CafeteriaOrder
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string OrderNumber { get; set; } = string.Empty; // رقم الطلب
        
        public int? StudentId { get; set; } // الطالب (اختياري)
        
        public int? EmployeeId { get; set; } // الموظف (اختياري)
        
        [MaxLength(200)]
        public string CustomerName { get; set; } = string.Empty; // اسم العميل
        
        [MaxLength(20)]
        public string CustomerPhone { get; set; } = string.Empty; // هاتف العميل
        
        public DateTime OrderDate { get; set; } = DateTime.UtcNow; // تاريخ الطلب
        
        public CafeteriaOrderStatus Status { get; set; } = CafeteriaOrderStatus.Pending; // حالة الطلب

        public decimal DiscountAmount { get; set; } = 0; // الخصم

        public decimal SubTotal { get; set; } = 0; // المجموع الفرعي

        public decimal TaxAmount { get; set; } = 0; // الضريبة

        public decimal TotalAmount { get; set; } // المجموع الكلي
        
        public decimal PaidAmount { get; set; } = 0; // المبلغ المدفوع
        
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash; // طريقة الدفع
        
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Unpaid; // حالة الدفع
        
        public int BranchId { get; set; } // الفرع
        
        public int CreatedByUserId { get; set; } // أنشئ بواسطة
        
        public int? PreparedByUserId { get; set; } // حضر بواسطة
        
        public DateTime? PreparedAt { get; set; } // تاريخ التحضير
        
        public int? DeliveredByUserId { get; set; } // سلم بواسطة
        
        public DateTime? DeliveredAt { get; set; } // تاريخ التسليم
        
        [MaxLength(1000)]
        public string Notes { get; set; } = string.Empty; // ملاحظات
        
        [MaxLength(500)]
        public string CancellationReason { get; set; } = string.Empty; // سبب الإلغاء
        
        // Calculated Properties
        public decimal RemainingAmount => TotalAmount - PaidAmount;
        
        // Navigation Properties
        public virtual Student? Student { get; set; }
        public virtual Employee? Employee { get; set; }
        public virtual Branch Branch { get; set; } = null!;
        public virtual User CreatedByUser { get; set; } = null!;
        public virtual User? PreparedByUser { get; set; }
        public virtual User? DeliveredByUser { get; set; }
        public virtual ICollection<CafeteriaOrderItem> CafeteriaOrderItems { get; set; } = new List<CafeteriaOrderItem>();
    }
}
