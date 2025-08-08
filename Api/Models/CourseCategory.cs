using System.ComponentModel.DataAnnotations;

namespace Api.Models
{
    public class CourseCategory
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty; // اسم الفئة
        
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty; // وصف الفئة
        
        public int MinAge { get; set; } // الحد الأدنى للعمر
        
        public int MaxAge { get; set; } // الحد الأقصى للعمر
        
        public bool IsActive { get; set; } = true; // نشط أم لا
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation Properties
        public virtual ICollection<Course> Courses { get; set; } = new List<Course>();
    }
}
