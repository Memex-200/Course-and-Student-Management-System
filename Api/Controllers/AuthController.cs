using Api.Data;
using Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using BCrypt.Net;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;

        public AuthController(ApplicationDbContext context, IConfiguration configuration, ILogger<AuthController> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpGet("test")]
        public IActionResult Test()
        {
            return Ok(new { message = "API is working!", timestamp = DateTime.UtcNow });
        }

       [HttpPost("admin-login")]
public async Task<IActionResult> AdminLogin([FromBody] AdminLoginRequest request)
{
    try
    {
        // أولوية لحساب الأدمن الأساسي الثابت (اختياري)
        if (request.Username == "admin" && request.Password == "123456")
        {
            var defaultAdmin = await _context.Users
                .Include(u => u.Branch)
                .FirstOrDefaultAsync(u => u.Username == "admin" && u.IsActive);

            if (defaultAdmin == null)
            {
                return BadRequest(new { message = "حساب المدير الافتراضي غير موجود" });
            }

            var defaultToken = GenerateJwtToken(defaultAdmin);
            return Ok(new
            {
                success = true,
                message = "تم تسجيل الدخول كمدير النظام",
                token = defaultToken,
                user = new
                {
                    id = defaultAdmin.Id,
                    username = defaultAdmin.Username,
                    fullName = defaultAdmin.FullName,
                    role = defaultAdmin.Role.ToString(),
                    branchId = defaultAdmin.BranchId,
                    branchName = defaultAdmin.Branch?.Name
                }
            });
        }

        // لو مش الأدمن الأساسي، نفحص الأدمنز من قاعدة البيانات
        var adminUser = await _context.Users
            .Include(u => u.Branch)
            .FirstOrDefaultAsync(u => u.Username == request.Username && u.IsActive);

        if (adminUser == null)
            return BadRequest(new { message = "اسم المستخدم غير موجود" });

        if (adminUser.Role != UserRole.Admin)
            return BadRequest(new { message = "هذا المستخدم ليس أدمن" });

        if (!VerifyPassword(request.Password, adminUser.PasswordHash))
            return BadRequest(new { message = "كلمة المرور غير صحيحة" });

        var token = GenerateJwtToken(adminUser);

        return Ok(new
        {
            success = true,
            message = "تم تسجيل الدخول بنجاح كأدمن",
            token = token,
            user = new
            {
                id = adminUser.Id,
                username = adminUser.Username,
                fullName = adminUser.FullName,
                role = adminUser.Role.ToString(),
                branchId = adminUser.BranchId,
                branchName = adminUser.Branch?.Name
            }
        });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error in admin login");
        return StatusCode(500, new { message = "حدث خطأ في الخادم" });
    }
}
[HttpPost("login")]
public async Task<IActionResult> Login([FromBody] LoginRequest request)
{
    try
    {
        // ✅ 1. لو المستخدم هو الأدمن الأساسي (افتراضي)
        if (request.Username == "admin" && request.Password == "123456")
        {
            var defaultAdmin = await _context.Users
                .Include(u => u.Branch)
                .FirstOrDefaultAsync(u => u.Username == "admin" && u.IsActive);

            if (defaultAdmin == null)
                return BadRequest(new { message = "حساب المدير الافتراضي غير موجود" });

            var token = GenerateJwtToken(defaultAdmin);
            return Ok(new
            {
                success = true,
                message = "تم تسجيل الدخول كمدير النظام",
                token,
                user = new
                {
                    id = defaultAdmin.Id,
                    username = defaultAdmin.Username,
                    fullName = defaultAdmin.FullName,
                    role = defaultAdmin.Role.ToString(),
                    branchId = defaultAdmin.BranchId,
                    branchName = defaultAdmin.Branch?.Name
                }
            });
        }

        // ✅ 2. نحاول نجيب المستخدم من قاعدة البيانات
        var user = await _context.Users
            .Include(u => u.Branch)
            .FirstOrDefaultAsync(u => u.Username == request.Username && u.IsActive);

        if (user == null)
            return BadRequest(new { message = "اسم المستخدم غير موجود" });

        // ✅ 3. نتحقق من كلمة المرور
        if (!VerifyPassword(request.Password, user.PasswordHash))
            return BadRequest(new { message = "كلمة المرور غير صحيحة" });

        // ✅ 4. نتحقق من الحالة
        if (!user.IsActive)
            return BadRequest(new { message = "الحساب غير مفعل" });

        // ✅ 5. نولّد JWT Token
        var jwtToken = GenerateJwtToken(user);

        // ✅ 6. نرجع التفاصيل حسب الدور
        string roleMessage = user.Role switch
        {
            UserRole.Admin => "تم تسجيل الدخول كأدمن",
            UserRole.Employee => "تم تسجيل الدخول كموظف",
            UserRole.Student => "تم تسجيل الدخول كطالب",
            _ => "تم تسجيل الدخول بنجاح"
        };

        return Ok(new
        {
            success = true,
            message = roleMessage,
            token = jwtToken,
            user = new
            {
                id = user.Id,
                username = user.Username,
                fullName = user.FullName,
                role = user.Role.ToString(),
                branchId = user.BranchId,
                branchName = user.Branch?.Name
            }
        });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error in unified login");
        return StatusCode(500, new { message = "حدث خطأ أثناء تسجيل الدخول" });
    }
}


        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                // Check if username already exists
                if (await _context.Users.AnyAsync(u => u.Username == request.Username))
                {
                    return BadRequest(new { message = "اسم المستخدم موجود بالفعل" });
                }

                // Check if email already exists
                if (!string.IsNullOrEmpty(request.Email) && 
                    await _context.Users.AnyAsync(u => u.Email == request.Email))
                {
                    return BadRequest(new { message = "البريد الإلكتروني موجود بالفعل" });
                }

                var user = new User
                {
                    Username = request.Username,
                    FullName = request.FullName,
                    Email = request.Email,
                    Phone = request.Phone,
                    Role = request.UserRole,
                    BranchId = request.BranchId,
                    PasswordHash = HashPassword(request.Password),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Load the branch information for the response
                var branch = await _context.Branches.FindAsync(user.BranchId);

                // Generate JWT token for the new user
                var token = GenerateJwtToken(user);

                // Log token claims for debugging
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(token);
                _logger.LogInformation($"Generated token claims for new user: {string.Join(", ", jwtToken.Claims.Select(c => $"{c.Type}={c.Value}"))}");

                // Return the same format as login endpoint
                return Ok(new
                {
                    token,
                    user = new
                    {
                        user.Id,
                        user.Username,
                        user.FullName,
                        user.Email,
                        user.Phone,
                        user.Address,
                        Role = (int)user.UserRole, // Changed from UserRole to Role to match frontend
                        Branch = branch?.Name,
                        user.BranchId
                    },
                    message = "تم إنشاء الحساب بنجاح"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "حدث خطأ في الخادم", error = ex.Message });
            }
        }

        [Authorize]
        [HttpPut("update")]
        public async Task<IActionResult> UpdateUser([FromBody] UpdateUserRequest request)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
                var user = await _context.Users
                    .Include(u => u.Branch)
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                {
                    return NotFound(new { message = "المستخدم غير موجود" });
                }

                // Check if email is being changed and if it's already taken
                if (!string.IsNullOrEmpty(request.Email) && 
                    request.Email != user.Email && 
                    await _context.Users.AnyAsync(u => u.Email == request.Email))
                {
                    return BadRequest(new { message = "البريد الإلكتروني موجود بالفعل" });
                }

                // Update user properties
                user.FullName = request.FullName ?? user.FullName;
                user.Email = request.Email ?? user.Email;
                user.Phone = request.Phone ?? user.Phone;
                user.Address = request.Address ?? user.Address;

                await _context.SaveChangesAsync();

                // Generate new token with updated claims
                var token = GenerateJwtToken(user);

                return Ok(new
                {
                    token,
                    user = new
                    {
                        user.Id,
                        user.Username,
                        user.FullName,
                        user.Email,
                        user.Phone,
                        user.Address,
                        Role = (int)user.Role,
                        Branch = user.Branch?.Name,
                        user.BranchId
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "حدث خطأ في الخادم", error = ex.Message });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("update-role/{userId}")]
        public async Task<IActionResult> UpdateUserRole(int userId, [FromBody] UpdateUserRoleRequest request)
        {
            try
            {
                var user = await _context.Users
                    .Include(u => u.Branch)
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                {
                    return NotFound(new { message = "المستخدم غير موجود" });
                }

                user.UserRole = request.NewRole;
                await _context.SaveChangesAsync();

                // Generate new token with updated claims
                var token = GenerateJwtToken(user);

                return Ok(new
                {
                    message = "تم تحديث دور المستخدم بنجاح",
                    token,
                    user = new
                    {
                        user.Id,
                        user.Username,
                        user.FullName,
                        user.Email,
                        user.Phone,
                        user.Address,
                        UserRole = (int)user.UserRole,
                        Branch = user.Branch?.Name,
                        user.BranchId
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "حدث خطأ في الخادم", error = ex.Message });
            }
        }

        private string GenerateJwtToken(User user)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var secretKey = jwtSettings["Key"];
            var key = Encoding.ASCII.GetBytes(secretKey!);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email ?? ""),
                new Claim(ClaimTypes.Role, user.Role == UserRole.Admin ? "Admin" : user.Role == UserRole.Employee ? "Employee" : "Student"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("BranchId", user.BranchId.ToString()),
                new Claim("FullName", user.FullName)
            };

            _logger.LogInformation($"Generating token with claims: {string.Join(", ", claims.Select(c => $"{c.Type}={c.Value}"))}");

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(1),
                Issuer = jwtSettings["Issuer"],
                Audience = jwtSettings["Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            _logger.LogInformation($"Generated token: {tokenString}");
            return tokenString;
        }

        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            try
            {
                // Get current user ID from token
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return BadRequest(new { message = "معرف المستخدم غير صحيح" });
                }

                // Get user from database
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return NotFound(new { message = "المستخدم غير موجود" });
                }

                // Verify current password
                if (!VerifyPassword(request.CurrentPassword, user.PasswordHash))
                {
                    return BadRequest(new { message = "كلمة المرور الحالية غير صحيحة" });
                }

                // Update password
                user.PasswordHash = HashPassword(request.NewPassword);
                await _context.SaveChangesAsync();

                return Ok(new { message = "تم تغيير كلمة المرور بنجاح" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during password change");
                return StatusCode(500, new { message = "حدث خطأ في الخادم", error = ex.Message });
            }
        }

        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        private bool VerifyPassword(string password, string hash)
        {
            try
            {
                return BCrypt.Net.BCrypt.Verify(password, hash);
            }
            catch
            {
                // Fallback for old SHA256 hashes
                using var sha256 = System.Security.Cryptography.SHA256.Create();
                var hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password + "AIRoboticsAcademy2025"));
                var oldHash = Convert.ToBase64String(hashedBytes);
                return oldHash == hash;
            }
        }

        /// <summary>
        /// TEMPORARY ENDPOINT - ONE-TIME PASSWORD REHASHING
        /// This endpoint rehashes all admin user passwords using BCrypt.
        /// Should be removed after running once for security reasons.
        /// </summary>
        [HttpGet("rehash-admins")]
        public async Task<IActionResult> RehashAdminPasswords()
        {
            try
            {
                // Find all users with Role == UserRole.Admin
                var adminUsers = await _context.Users
                    .Where(u => u.Role == UserRole.Admin)
                    .ToListAsync();

                if (!adminUsers.Any())
                {
                    return Ok(new { message = "No admin users found to rehash" });
                }

                // Known admin credentials mapping
                var adminCredentials = new Dictionary<string, string>
                {
                    { "admin", "123456" },
                    { "ibrahem", "Ibrahem@123!" },
                    { "karem", "Karem@123!" },
                    { "ahmed", "Ahmed@123!" },
                    { "mira", "Mira@123!" },
                    { "memex", "Eman@123!" }
                };

                var rehashedCount = 0;
                var rehashedUsers = new List<string>();

                foreach (var adminUser in adminUsers)
                {
                    // Get the known password for this admin user
                    if (adminCredentials.TryGetValue(adminUser.Username, out var knownPassword))
                    {
                        // Re-hash the known password using BCrypt
                        adminUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(knownPassword);
                        rehashedCount++;
                        rehashedUsers.Add(adminUser.Username);
                    }
                }

                // Save all changes
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = $"Successfully rehashed passwords for {rehashedCount} admin users",
                    rehashedUsers = rehashedUsers,
                    totalAdminUsers = adminUsers.Count,
                    note = "This endpoint should be removed after running once for security reasons"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during admin password rehashing");
                return StatusCode(500, new { message = "حدث خطأ في الخادم أثناء إعادة تشفير كلمات المرور", error = ex.Message });
            }
        }
    }

    public class LoginRequest
    {
        [Required]
        [MaxLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;
    }

    public class AdminLoginRequest
    {
        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }

    public class RegisterRequest
    {
        [Required]
        [MaxLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [EmailAddress]
        [MaxLength(100)]
        public string? Email { get; set; }

        [Required]
        [MaxLength(20)]
        public string Phone { get; set; } = string.Empty;

        [Required]
        public UserRole UserRole { get; set; }

        [Required]
        public int BranchId { get; set; }
    }

    public class UpdateUserRequest
    {
        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [EmailAddress]
        [MaxLength(100)]
        public string? Email { get; set; }

        [MaxLength(20)]
        public string? Phone { get; set; }

        [MaxLength(200)]
        public string? Address { get; set; }
    }

    public class UpdateUserRoleRequest
    {
        [Required]
        public UserRole NewRole { get; set; }
    }

    public class ChangePasswordRequest
    {
        [Required]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string NewPassword { get; set; } = string.Empty;
    }
}
