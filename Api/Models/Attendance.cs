using System.ComponentModel.DataAnnotations;

namespace Api.Models
{
    public enum AttendanceStatus
    {
        Present = 1,  // حاضر
        Absent = 2,   // غائب
        Late = 3,     // متأخر
        Excused = 4   // معذور
    }

    public class Attendance
    {
        public int Id { get; set; }
        
        public int StudentId { get; set; } // الطالب
        
        public int CourseId { get; set; } // الكورس
        
        public DateTime AttendanceDate { get; set; } // تاريخ الحضور

        public DateTime SessionDate { get; set; } // تاريخ الجلسة

        public AttendanceStatus Status { get; set; } // حالة الحضور
        
        public TimeSpan? CheckInTime { get; set; } // وقت الدخول
        
        public TimeSpan? CheckOutTime { get; set; } // وقت الخروج
        
        public int RecordedByUserId { get; set; } // مسجل بواسطة
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        [MaxLength(500)]
        public string Notes { get; set; } = string.Empty; // ملاحظات
        
        // Navigation Properties
        public virtual Student Student { get; set; } = null!;
        public virtual Course Course { get; set; } = null!;
        public virtual User RecordedByUser { get; set; } = null!;
    }
}
