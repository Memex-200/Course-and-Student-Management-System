using System.ComponentModel.DataAnnotations;

namespace Api.Models
{
    public enum RoomType
    {
        Classroom = 1,    // فصل دراسي
        Lab = 2,          // معمل
        Meeting = 3,      // قاعة اجتماعات
        Workshop = 4      // ورشة عمل
    }

    public class Room
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty; // اسم الغرفة
        
        [Required]
        [MaxLength(50)]
        public string RoomNumber { get; set; } = string.Empty; // رقم الغرفة
        
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty; // وصف الغرفة
        
        public RoomType RoomType { get; set; } // نوع الغرفة
        
        public int Capacity { get; set; } // السعة القصوى
        
        public int BranchId { get; set; } // الفرع
        
        public bool IsActive { get; set; } = true; // نشط أم لا
        
        [MaxLength(1000)]
        public string Equipment { get; set; } = string.Empty; // المعدات المتاحة
        
        [MaxLength(500)]
        public string Location { get; set; } = string.Empty; // الموقع في المبنى
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        [MaxLength(500)]
        public string Notes { get; set; } = string.Empty; // ملاحظات
        
        // Navigation Properties
        public virtual Branch Branch { get; set; } = null!;
        public virtual ICollection<RoomReservation> RoomReservations { get; set; } = new List<RoomReservation>();
        public virtual ICollection<Course> CoursesInRoom { get; set; } = new List<Course>();
    }
}
