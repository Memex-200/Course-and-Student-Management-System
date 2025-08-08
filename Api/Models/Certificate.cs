using System.ComponentModel.DataAnnotations;

namespace Api.Models
{
    public class Certificate
    {
        public int Id { get; set; }
        
        public int StudentId { get; set; }
        
        public int CourseId { get; set; }
        
        [MaxLength(200)]
        public string CertificateNumber { get; set; } = string.Empty;
        
        [MaxLength(500)]
        public string CertificateUrl { get; set; } = string.Empty;
        
        public DateTime IssueDate { get; set; } = DateTime.UtcNow;
        
        public int? ExamScore { get; set; } // درجة الامتحان من 100
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        [MaxLength(500)]
        public string Notes { get; set; } = string.Empty;
        
        // Navigation Properties
        public virtual Student Student { get; set; } = null!;
        public virtual Course Course { get; set; } = null!;
    }
}
