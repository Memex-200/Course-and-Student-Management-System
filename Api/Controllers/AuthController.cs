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
using System.Text.RegularExpressions;
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
        // Find admin user in database
        var adminUser = await _context.Users
            .Include(u => u.Branch)
            .FirstOrDefaultAsync(u => u.Username == request.Username && u.IsActive);

        if (adminUser == null)
            return BadRequest(new { message = "اسم المستخدم أو كلمة المرور غير صحيحة" });

        if (adminUser.Role != UserRole.Admin)
            return BadRequest(new { message = "هذا المستخدم ليس أدمن" });

        // Verify password with enhanced fallback mechanism
        var ok = VerifyPasswordWithEnhancedFallback(adminUser, request.Password);
        if (!ok)
            return BadRequest(new { message = "اسم المستخدم أو كلمة المرور غير صحيحة" });

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
                email = adminUser.Email,
                phone = adminUser.Phone,
                address = adminUser.Address,
                role = (int)adminUser.Role,
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
        // Find user in database
        var user = await _context.Users
            .Include(u => u.Branch)
            .FirstOrDefaultAsync(u => u.Username == request.Username && u.IsActive);

        if (user == null)
            return BadRequest(new { message = "اسم المستخدم أو كلمة المرور غير صحيحة" });

        // Verify password with enhanced fallback mechanism
        var ok = VerifyPasswordWithEnhancedFallback(user, request.Password);
        if (!ok)
            return BadRequest(new { message = "اسم المستخدم أو كلمة المرور غير صحيحة" });

        // Check if account is active
        if (!user.IsActive)
            return BadRequest(new { message = "الحساب غير مفعل" });

        // Generate JWT Token
        var jwtToken = GenerateJwtToken(user);

        // Return response based on user role
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
                email = user.Email,
                phone = user.Phone,
                address = user.Address,
                role = (int)user.Role,
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
        [Authorize(Roles = "Admin")]
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
                    success = true,
                    message = "تم إنشاء الحساب بنجاح",
                    token,
                    user = new
                    {
                        id = user.Id,
                        username = user.Username,
                        fullName = user.FullName,
                        email = user.Email,
                        phone = user.Phone,
                        address = user.Address,
                        role = (int)user.Role,
                        branchId = user.BranchId,
                        branchName = branch?.Name
                    }
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
                    success = true,
                    message = "تم تحديث البيانات بنجاح",
                    token,
                    user = new
                    {
                        id = user.Id,
                        username = user.Username,
                        fullName = user.FullName,
                        email = user.Email,
                        phone = user.Phone,
                        address = user.Address,
                        role = (int)user.Role,
                        branchId = user.BranchId,
                        branchName = user.Branch?.Name
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
                    success = true,
                    message = "تم تحديث دور المستخدم بنجاح",
                    token,
                    user = new
                    {
                        id = user.Id,
                        username = user.Username,
                        fullName = user.FullName,
                        email = user.Email,
                        phone = user.Phone,
                        address = user.Address,
                        role = (int)user.Role,
                        branchId = user.BranchId,
                        branchName = user.Branch?.Name
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
                var ok = VerifyPasswordWithEnhancedFallback(user, request.CurrentPassword);
                if (!ok)
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

        private static bool IsBcryptHash(string hash) =>
            !string.IsNullOrEmpty(hash) && (hash.StartsWith("$2a$") || hash.StartsWith("$2b$") || hash.StartsWith("$2y$"));

        private static bool IsSha256Hex(string hash) =>
            !string.IsNullOrEmpty(hash) && hash.Length == 64 && hash.All(Uri.IsHexDigit);

        private static bool IsBase64String(string s)
        {
            if (string.IsNullOrEmpty(s)) return false;
            s = s.Trim();
            return (s.Length % 4 == 0) && Regex.IsMatch(s, @"^[A-Za-z0-9\+/]+={0,2}$");
        }

        private bool VerifyPasswordAndMigrateIfNeeded(User user, string enteredPassword)
        {
            var storedHash = user.PasswordHash ?? string.Empty;

            // Case 1: BCrypt
            if (IsBcryptHash(storedHash))
                return BCrypt.Net.BCrypt.Verify(enteredPassword, storedHash);

            // Case 2: Legacy SHA256 hex
            if (IsSha256Hex(storedHash))
            {
                using var sha = SHA256.Create();
                var hashBytes = sha.ComputeHash(Encoding.UTF8.GetBytes(enteredPassword));
                var hex = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();

                if (hex == storedHash)
                {
                    user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(enteredPassword);
                    _context.SaveChanges();
                    return true;
                }
                return false;
            }

            // Case 3: Legacy SHA256 Base64
            if (IsBase64String(storedHash))
            {
                using var sha = SHA256.Create();
                var hashBytes = sha.ComputeHash(Encoding.UTF8.GetBytes(enteredPassword));
                var base64 = Convert.ToBase64String(hashBytes);

                if (base64 == storedHash)
                {
                    user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(enteredPassword);
                    _context.SaveChanges();
                    return true;
                }
                return false;
            }

            // Fallback: Try BCrypt anyway
            try
            {
                return BCrypt.Net.BCrypt.Verify(enteredPassword, storedHash);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Enhanced password verification with comprehensive fallback support
        /// Handles BCrypt, SHA256 hex, SHA256 Base64, and plain text migration
        /// </summary>
        private bool VerifyPasswordWithEnhancedFallback(User user, string enteredPassword)
        {
            var storedHash = user.PasswordHash ?? string.Empty;

            // Case 1: BCrypt verification (primary method)
            if (IsBcryptHash(storedHash))
            {
                try
                {
                    return BCrypt.Net.BCrypt.Verify(enteredPassword, storedHash);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"BCrypt verification failed for user {user.Username}: {ex.Message}");
                    return false;
                }
            }

            // Case 2: Legacy SHA256 hex verification
            if (IsSha256Hex(storedHash))
            {
                using var sha = SHA256.Create();
                var hashBytes = sha.ComputeHash(Encoding.UTF8.GetBytes(enteredPassword));
                var hex = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();

                if (hex == storedHash)
                {
                    // Migrate to BCrypt and save
                    user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(enteredPassword);
                    _context.SaveChanges();
                    _logger.LogInformation($"Migrated SHA256 hex password for user {user.Username} to BCrypt");
                    return true;
                }
                return false;
            }

            // Case 3: Legacy SHA256 Base64 verification
            if (IsBase64String(storedHash))
            {
                using var sha = SHA256.Create();
                var hashBytes = sha.ComputeHash(Encoding.UTF8.GetBytes(enteredPassword));
                var base64 = Convert.ToBase64String(hashBytes);

                if (base64 == storedHash)
                {
                    // Migrate to BCrypt and save
                    user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(enteredPassword);
                    _context.SaveChanges();
                    _logger.LogInformation($"Migrated SHA256 Base64 password for user {user.Username} to BCrypt");
                    return true;
                }
                return false;
            }

            // Case 4: Plain text comparison (for emergency admin access)
            // This should only be used for initial admin setup
            if (storedHash == enteredPassword)
            {
                // Immediately migrate to BCrypt
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(enteredPassword);
                _context.SaveChanges();
                _logger.LogWarning($"Migrated plain text password for user {user.Username} to BCrypt - SECURITY RISK RESOLVED");
                return true;
            }

            // Case 5: Try BCrypt verification as fallback (in case hash format is unrecognized)
            try
            {
                var result = BCrypt.Net.BCrypt.Verify(enteredPassword, storedHash);
                if (result)
                {
                    _logger.LogInformation($"BCrypt verification succeeded for user {user.Username} with unrecognized hash format");
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"All password verification methods failed for user {user.Username}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Check if current user has admin access
        /// </summary>
        [HttpGet("check-admin-access")]
        [Authorize]
        public IActionResult CheckAdminAccess()
        {
            try
            {
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var username = User.FindFirst(ClaimTypes.Name)?.Value;

                var isAdmin = userRole == "Admin";
                
                return Ok(new
                {
                    success = true,
                    isAdmin = isAdmin,
                    userRole = userRole,
                    userId = userId,
                    username = username,
                    message = isAdmin ? "Admin access granted" : "Admin access denied"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking admin access");
                return StatusCode(500, new { message = "حدث خطأ في الخادم" });
            }
        }

        /// <summary>
        /// Get admin dashboard data - Admin only
        /// </summary>
        [HttpGet("admin-dashboard")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAdminDashboard()
        {
            try
            {
                var totalUsers = await _context.Users.CountAsync();
                var totalStudents = await _context.Users.CountAsync(u => u.Role == UserRole.Student);
                var totalEmployees = await _context.Users.CountAsync(u => u.Role == UserRole.Employee);
                var totalAdmins = await _context.Users.CountAsync(u => u.Role == UserRole.Admin);
                var activeUsers = await _context.Users.CountAsync(u => u.IsActive);

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        totalUsers,
                        totalStudents,
                        totalEmployees,
                        totalAdmins,
                        activeUsers,
                        inactiveUsers = totalUsers - activeUsers
                    },
                    message = "Admin dashboard data retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting admin dashboard data");
                return StatusCode(500, new { message = "حدث خطأ في الخادم" });
            }
        }

        /// <summary>
        /// TEMPORARY ENDPOINT - ONE-TIME PASSWORD REHASHING
        /// This endpoint rehashes all admin user passwords using BCrypt.
        /// Should be removed after running once for security reasons.
        /// </summary>
        [HttpGet("rehash-admins")]
        [Authorize(Roles = "Admin")]
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
