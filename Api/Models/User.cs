using System.ComponentModel.DataAnnotations;

namespace Api.Models
{
    public enum UserRole
    {
        Admin = 1,
        Employee = 2,
        Student = 3
    }

    public class User
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty; // الاسم الكامل
        
        [Required]
        [MaxLength(50)]
        public string Username { get; set; } = string.Empty; // اسم المستخدم

        [Required]
        [MaxLength(100)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty; // البريد الإلكتروني
        
        [Required]
        [MaxLength(20)]
        public string Phone { get; set; } = string.Empty; // رقم الهاتف

        [MaxLength(200)]
        public string Address { get; set; } = string.Empty; // العنوان
        
        [Required]
        public string PasswordHash { get; set; } = string.Empty; // كلمة المرور مشفرة
        
        public UserRole Role { get; set; } // دور المستخدم

        public UserRole UserRole { get; set; } // دور المستخدم (للتوافق مع الكود)
        
        public int BranchId { get; set; } // الفرع التابع له
        
        public int? StudentId { get; set; } // معرف الطالب (في حالة كان المستخدم طالب)
        
        public bool IsActive { get; set; } = true; // نشط أم لا
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? LastLoginAt { get; set; } // آخر تسجيل دخول
        
        // Navigation Properties
        public virtual Branch Branch { get; set; } = null!;
    }
}
