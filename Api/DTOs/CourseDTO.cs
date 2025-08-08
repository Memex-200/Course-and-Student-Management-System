using Api.Models;
using System.ComponentModel.DataAnnotations;

namespace Api.DTOs
{
    public class CourseDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int? CourseCategoryId { get; set; }
        public string CourseCategoryName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int SessionsCount { get; set; }
        public int MaxStudents { get; set; }
        public int CurrentStudents { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public CourseStatus Status { get; set; }
        public string StatusArabic => Status switch
        {
            CourseStatus.Planned => "مخطط",
            CourseStatus.Active => "نشط",
            CourseStatus.Completed => "مكتمل",
            CourseStatus.Cancelled => "ملغي",
            _ => "غير معروف"
        };
        public int? BranchId { get; set; }
        public string BranchName { get; set; } = string.Empty;
        public int? InstructorId { get; set; }
        public string InstructorName { get; set; } = string.Empty;
        public int? LabId { get; set; }
        public string LabName { get; set; } = string.Empty;
        public int? RoomId { get; set; }
        public string RoomName { get; set; } = string.Empty;
        public string Schedule { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Prerequisites { get; set; } = string.Empty;
        public string CourseDays { get; set; } = string.Empty;
        public string StartTime { get; set; } = string.Empty;
        public string EndTime { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Notes { get; set; } = string.Empty;
        public List<EnrolledStudentDTO> EnrolledStudents { get; set; } = new();
    }

    public class EnrolledStudentDTO
    {
        public int Id { get; set; } // studentId
        public int RegistrationId { get; set; } // CourseRegistration.Id
        public string FullName { get; set; } = string.Empty;
        public decimal PaidAmount { get; set; }
        public decimal RemainingAmount { get; set; }
        public string PaymentStatus { get; set; } = string.Empty;
        public string PaymentStatusArabic { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
        public string PaymentMethodArabic { get; set; } = string.Empty;
    }

    public class CreateCourseDTO
    {
        [Required(ErrorMessage = "اسم الدورة مطلوب")]
        [MaxLength(200, ErrorMessage = "اسم الدورة يجب ألا يتجاوز 200 حرف")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1000, ErrorMessage = "الوصف يجب ألا يتجاوز 1000 حرف")]
        public string Description { get; set; } = string.Empty;

        public int? CourseCategoryId { get; set; }

        [Required(ErrorMessage = "السعر مطلوب")]
        [Range(0, double.MaxValue, ErrorMessage = "السعر يجب أن يكون أكبر من أو يساوي صفر")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "عدد الحصص مطلوب")]
        [Range(1, int.MaxValue, ErrorMessage = "عدد الحصص يجب أن يكون أكبر من صفر")]
        public int SessionsCount { get; set; }

        [Required(ErrorMessage = "الحد الأقصى للطلاب مطلوب")]
        [Range(1, int.MaxValue, ErrorMessage = "الحد الأقصى للطلاب يجب أن يكون أكبر من صفر")]
        public int MaxStudents { get; set; }

        [Required(ErrorMessage = "تاريخ البداية مطلوب")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "تاريخ النهاية مطلوب")]
        public DateTime EndDate { get; set; }

        public int? BranchId { get; set; }

        public int? InstructorId { get; set; }
        public int? LabId { get; set; }
        public int? RoomId { get; set; }

        [MaxLength(4000, ErrorMessage = "محتوى الدورة يجب ألا يتجاوز 4000 حرف")]
        public string Content { get; set; } = string.Empty;

        [MaxLength(1000, ErrorMessage = "المتطلبات الأساسية يجب ألا تتجاوز 1000 حرف")]
        public string Prerequisites { get; set; } = string.Empty;

        [Required(ErrorMessage = "أيام الدورة مطلوبة")]
        [MaxLength(200, ErrorMessage = "أيام الدورة يجب ألا تتجاوز 200 حرف")]
        public string CourseDays { get; set; } = string.Empty;

        [Required(ErrorMessage = "وقت البداية مطلوب")]
        [MaxLength(50, ErrorMessage = "وقت البداية يجب ألا يتجاوز 50 حرف")]
        public string StartTime { get; set; } = string.Empty;

        [Required(ErrorMessage = "وقت النهاية مطلوب")]
        [MaxLength(50, ErrorMessage = "وقت النهاية يجب ألا يتجاوز 50 حرف")]
        public string EndTime { get; set; } = string.Empty;

        [MaxLength(500, ErrorMessage = "الملاحظات يجب ألا تتجاوز 500 حرف")]
        public string Notes { get; set; } = string.Empty;
    }

    public class UpdateCourseDTO
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public int? CourseCategoryId { get; set; }
        public decimal? Price { get; set; }
        public int? SessionsCount { get; set; }
        public int? MaxStudents { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public CourseStatus? Status { get; set; }
        public int? BranchId { get; set; }
        public int? InstructorId { get; set; }
        public int? LabId { get; set; }
        public int? RoomId { get; set; }
        public string? Content { get; set; }
        public string? Prerequisites { get; set; }
        public string? CourseDays { get; set; }
        public string? StartTime { get; set; }
        public string? EndTime { get; set; }
        public string? Notes { get; set; }
        public bool? IsActive { get; set; }
    }
} 