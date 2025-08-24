using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api.Models
{
    public class Student
    {
        public int Id { get; set; }
        
        public int? UserId { get; set; } // ربط مع جدول المستخدمين (اختياري)
        
        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty; // الاسم الكامل
        
        public int Age { get; set; } // العمر

        public DateTime DateOfBirth { get; set; } // تاريخ الميلاد

        public AgeGroup AgeGroup { get; set; } // الفئة العمرية

        public StudentLevel Level { get; set; } // المستوى

        [MaxLength(20)]
        public string Phone { get; set; } = string.Empty; // رقم الهاتف

        [MaxLength(100)]
        public string Email { get; set; } = string.Empty; // البريد الإلكتروني

        [MaxLength(10)]
        public string Gender { get; set; } = string.Empty; // الجنس
        
        [MaxLength(100)]
        public string School { get; set; } = string.Empty; // المدرسة
        
        [Required]
        [MaxLength(20)]
        public string ParentPhone { get; set; } = string.Empty; // رقم ولي الأمر
        
        [Required]
        [MaxLength(100)]
        public string ParentName { get; set; } = string.Empty; // اسم ولي الأمر

        [MaxLength(100)]
        public string EmergencyContact { get; set; } = string.Empty; // جهة الاتصال الطارئة

        [MaxLength(20)]
        public string EmergencyPhone { get; set; } = string.Empty; // رقم الطوارئ

        [MaxLength(500)]
        public string MedicalConditions { get; set; } = string.Empty; // الحالات الطبية
        
        public bool AcceptsDomesticTravel { get; set; } // يقبل السفر داخل مصر
        
        public bool AcceptsInternationalTravel { get; set; } // يقبل السفر خارج مصر
        
        public int BranchId { get; set; } // الفرع
        
        public bool IsActive { get; set; } = true; // نشط أم لا

        public DateTime RegistrationDate { get; set; } = DateTime.UtcNow; // تاريخ التسجيل

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        [MaxLength(500)]
        public string Notes { get; set; } = string.Empty; // ملاحظات
        
        // Navigation Properties
        public virtual User User { get; set; } = null!;
        public virtual Branch Branch { get; set; } = null!;
        public virtual ICollection<CourseRegistration> CourseRegistrations { get; set; } = new List<CourseRegistration>();
        public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
        public virtual ICollection<WorkspaceBooking> WorkspaceBookings { get; set; } = new List<WorkspaceBooking>();
        public virtual ICollection<CafeteriaOrder> CafeteriaOrders { get; set; } = new List<CafeteriaOrder>();
        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}
