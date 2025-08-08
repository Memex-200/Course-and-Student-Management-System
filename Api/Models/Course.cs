using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace Api.Models
{
    public enum CourseStatus
    {
        Planned = 1,      // مخطط
        Active = 2,       // نشط
        Completed = 3,    // مكتمل
        Cancelled = 4     // ملغي
    }

    public class Course
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty; // اسم الكورس
        
        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty; // وصف الكورس
        
        public int? CourseCategoryId { get; set; } // فئة الكورس
        
        public decimal Price { get; set; } // سعر الكورس
        
        public int SessionsCount { get; set; } // عدد الحصص
        
        public int MaxStudents { get; set; } // الحد الأقصى للطلاب
        
        public DateTime StartDate { get; set; } // تاريخ البداية
        
        public DateTime EndDate { get; set; } // تاريخ النهاية
        
        public CourseStatus Status { get; set; } = CourseStatus.Planned; // حالة الكورس
        
        public int? BranchId { get; set; } // الفرع
        
        public int? InstructorId { get; set; } // المدرس
        
        public int? LabId { get; set; } // المعمل

        public int? RoomId { get; set; } // الغرفة

        [MaxLength(100)]
        public string Schedule { get; set; } = string.Empty; // الجدول الزمني المعروض

        [MaxLength(4000)]
        public string Content { get; set; } = string.Empty; // محتوى الكورس

        [MaxLength(1000)]
        public string Prerequisites { get; set; } = string.Empty; // المتطلبات الأساسية

        // Schedule Details
        public string CourseDays { get; set; } = string.Empty; // أيام الدورة (مفصولة بفواصل)
        public string StartTime { get; set; } = string.Empty; // وقت بداية المحاضرة
        public string EndTime { get; set; } = string.Empty; // وقت نهاية المحاضرة
        
        public bool IsActive { get; set; } = true; // نشط أم لا
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        [MaxLength(500)]
        public string Notes { get; set; } = string.Empty; // ملاحظات
        
        // Navigation Properties
        public virtual CourseCategory? CourseCategory { get; set; }
        public virtual Branch? Branch { get; set; }
        public virtual Employee? Instructor { get; set; }
        public virtual Lab? Lab { get; set; }
        public virtual Room? Room { get; set; }
        public virtual ICollection<CourseRegistration> CourseRegistrations { get; set; } = new List<CourseRegistration>();
        public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
    }
}
