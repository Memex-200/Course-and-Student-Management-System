using System.ComponentModel.DataAnnotations;

namespace Api.Models
{
    public class Lab
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty; // اسم المعمل
        
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty; // وصف المعمل
        
        public int Capacity { get; set; } // السعة
        
        public int BranchId { get; set; } // الفرع
        
        public bool IsActive { get; set; } = true; // نشط أم لا
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        [MaxLength(500)]
        public string Equipment { get; set; } = string.Empty; // المعدات المتاحة
        
        // Navigation Properties
        public virtual Branch Branch { get; set; } = null!;
        public virtual ICollection<Course> Courses { get; set; } = new List<Course>();
        public virtual ICollection<LabReservation> LabReservations { get; set; } = new List<LabReservation>();
    }
}
