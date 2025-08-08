using Api.Data;
using Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "AdminOnly")]
    public class EmployeesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public EmployeesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/employees?role=Instructor
        [HttpGet]
        public async Task<IActionResult> GetEmployees([FromQuery] EmployeeRole? role = null)
        {
            var query = _context.Employees.Include(e => e.User).AsQueryable();
            if (role.HasValue)
                query = query.Where(e => e.EmployeeRole == role.Value);
            var employees = await query.Select(e => new {
                e.Id,
                e.FullName,
                e.EmployeeRole,
                e.BranchId,
                e.IsActive,
                Username = e.User.Username,
                e.User.Email,
                e.User.Phone
            }).ToListAsync();
            return Ok(new { success = true, data = employees });
        }

        // POST: api/employees
        [HttpPost]
        public async Task<IActionResult> CreateEmployee([FromBody] CreateEmployeeRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.FullName) || string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest(new { success = false, message = "الاسم واسم المستخدم وكلمة المرور مطلوبة" });

            // تحقق إذا كان المستخدم موجود مسبقًا
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
            User user;
            if (existingUser != null)
            {
                // إذا كان موجودًا، استخدمه مباشرة
                user = existingUser;
            }
            else
            {
                // إذا لم يكن موجودًا، أنشئ حساب جديد
                user = new User
                {
                    FullName = request.FullName,
                    Username = request.Username,
                    Email = request.Email ?? string.Empty,
                    Phone = request.Phone ?? string.Empty,
                    Address = request.Address ?? string.Empty,
                    PasswordHash = HashPassword(request.Password),
                    Role = UserRole.Employee,
                    UserRole = UserRole.Employee,
                    BranchId = request.BranchId,
                    IsActive = true
                };
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            }

            // Create Employee
            var employee = new Employee
            {
                UserId = user.Id,
                FullName = request.FullName,
                EmployeeRole = EmployeeRole.Instructor,
                Position = "مدرب",
                Salary = 0,
                HireDate = DateTime.UtcNow,
                BranchId = request.BranchId,
                IsActive = true,
                Notes = request.Notes ?? string.Empty
            };
            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "تم إضافة المدرب بنجاح", data = new { employee.Id, employee.FullName } });
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password + "AIRoboticsAcademy2025"));
            return Convert.ToBase64String(hashedBytes);
        }
    }

    public class CreateEmployeeRequest
    {
        [Required]
        public string FullName { get; set; } = string.Empty;
        [Required]
        public string Username { get; set; } = string.Empty;
        [Required]
        public string Password { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public int BranchId { get; set; }
        public string? Notes { get; set; }
    }
} 