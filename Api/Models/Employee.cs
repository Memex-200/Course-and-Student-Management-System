using System.ComponentModel.DataAnnotations;

namespace Api.Models
{
    public enum EmployeeRole
    {
        Manager = 1,      // مدير
        Instructor = 2,   // مدرس
        Reception = 3     // استقبال
    }

    public class Employee
    {
        public int Id { get; set; }
        
        public int UserId { get; set; } // ربط مع جدول المستخدمين
        
        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty; // الاسم الكامل
        
        public EmployeeRole EmployeeRole { get; set; } // دور الموظف
        
        [MaxLength(100)]
        public string Position { get; set; } = string.Empty; // المنصب
        
        public decimal Salary { get; set; } // الراتب
        
        public DateTime HireDate { get; set; } // تاريخ التوظيف
        
        public int BranchId { get; set; } // الفرع
        
        public bool IsActive { get; set; } = true; // نشط أم لا
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        [MaxLength(500)]
        public string Notes { get; set; } = string.Empty; // ملاحظات
        
        // Navigation Properties
        public virtual User User { get; set; } = null!;
        public virtual Branch Branch { get; set; } = null!;
        public virtual ICollection<Course> CoursesAsInstructor { get; set; } = new List<Course>();
    }
}
