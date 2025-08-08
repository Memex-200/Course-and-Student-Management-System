using System.ComponentModel.DataAnnotations;

namespace Api.Models
{
    public class CafeteriaOrderItem
    {
        public int Id { get; set; }
        
        public int CafeteriaOrderId { get; set; } // الطلب
        
        public int CafeteriaItemId { get; set; } // المنتج
        
        public int Quantity { get; set; } // الكمية
        
        public decimal UnitPrice { get; set; } // سعر الوحدة
        
        public decimal TotalPrice => Quantity * UnitPrice; // السعر الإجمالي
        
        [MaxLength(500)]
        public string Notes { get; set; } = string.Empty; // ملاحظات خاصة
        
        [MaxLength(500)]
        public string Customization { get; set; } = string.Empty; // تخصيص (مثل: بدون سكر، إضافة كريمة)
        
        // Navigation Properties
        public virtual CafeteriaOrder CafeteriaOrder { get; set; } = null!;
        public virtual CafeteriaItem CafeteriaItem { get; set; } = null!;
    }
}
