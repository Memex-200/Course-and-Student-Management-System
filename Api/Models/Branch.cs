using System.ComponentModel.DataAnnotations;

namespace Api.Models
{
    public class Branch
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty; // اسم الفرع
        
        [MaxLength(200)]
        public string Address { get; set; } = string.Empty; // عنوان الفرع
        
        [MaxLength(20)]
        public string Phone { get; set; } = string.Empty; // هاتف الفرع
        
        public bool HasWorkspace { get; set; } // هل يحتوي على workspace
        public bool HasSharedWorkspace { get; set; } // هل يحتوي على مساحة عمل مشتركة
        public bool HasRooms { get; set; } // هل يحتوي على قاعات

        public bool IsActive { get; set; } = true; // نشط أم لا

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation Properties
        public virtual ICollection<Course> Courses { get; set; } = new List<Course>();
        public virtual ICollection<Student> Students { get; set; } = new List<Student>();
        public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
        public virtual ICollection<WorkspaceSession> WorkspaceSessions { get; set; } = new List<WorkspaceSession>();
        public virtual ICollection<Lab> Labs { get; set; } = new List<Lab>();
    }
}
