using System.ComponentModel.DataAnnotations;

namespace Api.Models
{
    public class StudentGrade
    {
        public int Id { get; set; }
        
        public int StudentId { get; set; } // الطالب
        
        public int CourseId { get; set; } // الكورس
        
        [Range(0, 100)]
        public decimal Grade { get; set; } // الدرجة من 100
        
        [MaxLength(500)]
        public string Notes { get; set; } = string.Empty; // ملاحظات
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        public int CreatedByUserId { get; set; } // أنشئ بواسطة
        
        public int? UpdatedByUserId { get; set; } // عدل بواسطة
        
        // Navigation Properties
        public virtual Student Student { get; set; } = null!;
        public virtual Course Course { get; set; } = null!;
        public virtual User CreatedByUser { get; set; } = null!;
        public virtual User? UpdatedByUser { get; set; }
    }
}
