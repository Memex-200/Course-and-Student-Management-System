using System.ComponentModel.DataAnnotations;

namespace Api.Models
{
    public enum CafeteriaItemCategory
    {
        Beverages = 1,      // مشروبات
        Snacks = 2,         // وجبات خفيفة
        Meals = 3,          // وجبات
        Desserts = 4,       // حلويات
        Fruits = 5,         // فواكه
        Other = 6           // أخرى
    }

    public class CafeteriaItem
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty; // اسم المنتج
        
        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty; // الوصف
        
        public CafeteriaItemCategory Category { get; set; } // الفئة
        
        public decimal Price { get; set; } // السعر
        
        public decimal Cost { get; set; } = 0; // التكلفة
        
        [MaxLength(50)]
        public string Unit { get; set; } = "قطعة"; // الوحدة

        public int StockQuantity { get; set; } = 0; // الكمية المتاحة

        public int MinimumStock { get; set; } = 0; // الحد الأدنى للمخزون

        public bool IsAvailable { get; set; } = true; // متاح أم لا

        public bool IsActive { get; set; } = true; // نشط أم لا

        public int BranchId { get; set; } // الفرع

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        [MaxLength(500)]
        public string Notes { get; set; } = string.Empty; // ملاحظات
        
        // Navigation Properties
        public virtual Branch Branch { get; set; } = null!;
        public virtual ICollection<CafeteriaOrder> CafeteriaOrders { get; set; } = new List<CafeteriaOrder>();
        public virtual ICollection<CafeteriaOrderItem> CafeteriaOrderItems { get; set; } = new List<CafeteriaOrderItem>();
    }
}
