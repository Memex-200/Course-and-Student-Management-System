using System.ComponentModel.DataAnnotations;

namespace Api.Models
{
    public class WorkspaceDesk
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string DeskNumber { get; set; } = string.Empty; // رقم المكتب
        
        [MaxLength(200)]
        public string Description { get; set; } = string.Empty; // وصف المكتب
        
        public int BranchId { get; set; } // الفرع
        
        public bool HasInternet { get; set; } = true; // يحتوي على إنترنت
        
        public bool HasLaptop { get; set; } = false; // يحتوي على لابتوب
        
        public decimal HourlyRate { get; set; } // السعر بالساعة
        
        public bool IsActive { get; set; } = true; // نشط أم لا
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation Properties
        public virtual Branch Branch { get; set; } = null!;
        public virtual ICollection<WorkspaceSession> WorkspaceSessions { get; set; } = new List<WorkspaceSession>();
    }
}
