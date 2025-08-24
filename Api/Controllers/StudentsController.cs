using Api.Data;
using Api.Models;
using Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.Extensions.Logging; // Added for logging
using System.Linq; // Added for string.Join
using BCrypt.Net;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class StudentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<StudentsController> _logger; // Added logger field
        private readonly IEmailService _emailService;
        private readonly IPasswordGeneratorService _passwordGenerator;

        public StudentsController(ApplicationDbContext context, ILogger<StudentsController> logger, IEmailService emailService, IPasswordGeneratorService passwordGenerator) // Added email and password services
        {
            _context = context;
            _logger = logger; // Initialize logger
            _emailService = emailService;
            _passwordGenerator = passwordGenerator;
        }

        [HttpGet]
        public async Task<IActionResult> GetStudents([FromQuery] int? branchId = null, [FromQuery] string? search = null)
        {
            try
            {
                _logger.LogInformation($"Getting students. BranchId: {branchId}, Search: {search}");
                
                var userBranchId = int.Parse(User.FindFirst("BranchId")?.Value ?? "0");
                _logger.LogInformation($"User branch ID: {userBranchId}");
                
                var query = _context.Students
                    .Include(s => s.Branch)
                    .Include(s => s.CourseRegistrations)
                    .AsQueryable();

                // Filter by branch
                if (branchId.HasValue)
                    query = query.Where(s => s.BranchId == branchId.Value);
                else if (userBranchId > 0)
                    query = query.Where(s => s.BranchId == userBranchId);

                // Search filter
                if (!string.IsNullOrEmpty(search))
                {
                    search = search.Trim().ToLower();
                    query = query.Where(s => 
                        s.FullName.ToLower().Contains(search) || 
                        s.Phone.Contains(search) || 
                        s.ParentPhone.Contains(search) ||
                        s.Email.ToLower().Contains(search)
                    );
                }

                var students = await query
                    .OrderByDescending(s => s.CreatedAt)
                    .Select(s => new
                    {
                        s.Id,
                        s.FullName,
                        s.Phone,
                        s.Email,
                        s.Age,
                        s.AgeGroup,
                        s.Level,
                        s.Gender,
                        s.School,
                        s.ParentName,
                        s.ParentPhone,
                        s.EmergencyContact,
                        s.EmergencyPhone,
                        s.MedicalConditions,
                        s.BranchId,
                        s.IsActive,
                        branch = new { id = s.Branch.Id, name = s.Branch.Name },
                        registeredCourses = s.CourseRegistrations.Count(cr => cr.PaymentStatus != PaymentStatus.Cancelled),
                        totalRegistrations = s.CourseRegistrations.Count,
                        cancelledRegistrations = s.CourseRegistrations.Count(cr => cr.PaymentStatus == PaymentStatus.Cancelled),
                        s.CreatedAt
                    })
                    .ToListAsync();

                // Transform the data after database query
                var transformedStudents = students.Select(s => new
                {
                    s.Id,
                    s.FullName,
                    s.Phone,
                    s.Email,
                    s.Age,
                    AgeGroup = GetAgeGroupArabic(s.AgeGroup),
                    Level = GetLevelArabic(s.Level),
                    s.Gender,
                    s.School,
                    s.ParentName,
                    s.ParentPhone,
                    s.EmergencyContact,
                    s.EmergencyPhone,
                    s.MedicalConditions,
                    s.BranchId,
                    s.IsActive,
                    s.branch,
                    s.registeredCourses,
                    s.totalRegistrations,
                    s.cancelledRegistrations,
                    s.CreatedAt
                }).ToList();

                // Log registration counts for debugging
                foreach (var student in transformedStudents)
                {
                    _logger.LogInformation($"Student {student.Id} ({student.FullName}): Total registrations: {student.totalRegistrations}, Cancelled: {student.cancelledRegistrations}, Active: {student.registeredCourses}");
                }

                _logger.LogInformation($"Found {transformedStudents.Count} students");
                
                return Ok(new { 
                    success = true,
                    message = transformedStudents.Count > 0 ? "تم جلب بيانات الطلاب بنجاح" : "لا يوجد طلاب",
                    data = transformedStudents
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting students");
                return StatusCode(500, new { 
                    success = false,
                    message = "حدث خطأ في الخادم",
                    error = ex.Message,
                    data = new List<object>()
                });
            }
        }

        [HttpPost]
        [Authorize(Policy = "AdminOrEmployee")]
        public async Task<IActionResult> CreateStudent([FromBody] CreateStudentRequest request)
        {
            if (request == null)
            {
                _logger.LogError("CreateStudent: Request is null");
                return BadRequest(new { message = "بيانات الطلب فارغة" });
            }

            try
            {
                var userBranchId = int.Parse(User.FindFirst("BranchId")?.Value ?? "0");

                // Check if phone already exists
                var phoneExists = await _context.Students.AnyAsync(s => s.Phone == request.Phone);
                if (phoneExists)
                {
                    _logger.LogWarning($"Phone number {request.Phone} already exists - returning BadRequest");
                    return BadRequest(new { message = "رقم الهاتف موجود بالفعل" });
                }

                // Check if email already exists
                if (!string.IsNullOrEmpty(request.Email))
                {
                    var emailExists = await _context.Students.AnyAsync(s => s.Email == request.Email);
                    if (emailExists)
                    {
                        _logger.LogWarning($"Email {request.Email} already exists - returning BadRequest");
                        return BadRequest(new { message = "الإيميل موجود بالفعل" });
                    }
                }

                var student = new Student
                {
                    FullName = request.FullName,
                    Phone = request.Phone,
                    Email = request.Email,
                    Age = request.Age,
                    AgeGroup = DetermineAgeGroup(request.Age),
                    Level = request.Level,
                    Gender = request.Gender,
                    School = request.School,
                    ParentName = request.ParentName,
                    ParentPhone = request.ParentPhone,
                    EmergencyContact = request.EmergencyContact,
                    EmergencyPhone = request.EmergencyPhone,
                    MedicalConditions = request.MedicalConditions,
                    BranchId = userBranchId,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Students.Add(student);
                await _context.SaveChangesAsync();

                // Use the centralized method to create the user account
                var (user, _) = await CreateUserAccountForStudentAsync(student);

                _logger.LogInformation($"Student created successfully with ID: {student.Id}");
                return Ok(new { 
                    message = "تم إضافة الطالب بنجاح وإرسال بيانات الدخول عبر الإيميل", 
                    studentId = student.Id,
                    username = user?.Username,
                    emailSent = !string.IsNullOrEmpty(request.Email)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating student");
                return StatusCode(500, new { message = "حدث خطأ في الخادم", error = ex.Message });
            }
        }

        [HttpPost("{id}/create-account")]
        [Authorize(Policy = "AdminOrEmployee")]
        public async Task<IActionResult> CreateAccountForExistingStudent(int id)
        {
            try
            {
                var student = await _context.Students
                    .Include(s => s.User)
                    .FirstOrDefaultAsync(s => s.Id == id);
                
                if (student == null)
                    return NotFound(new { message = "الطالب غير موجود" });

                // Check if student already has an account
                if (student.User != null)
                    return BadRequest(new { message = "الطالب لديه حساب مسبقاً" });

                // Use the centralized method to create the user account
                var (user, _) = await CreateUserAccountForStudentAsync(student);

                if (user == null)
                {
                    return StatusCode(500, new { message = "فشل إنشاء حساب المستخدم" });
                }

                _logger.LogInformation($"User account created successfully for student ID: {student.Id}");
                return Ok(new { 
                    message = "تم إنشاء حساب للطالب بنجاح وإرسال بيانات الدخول عبر الإيميل", 
                    studentId = student.Id,
                    username = user.Username,
                    emailSent = !string.IsNullOrEmpty(student.Email)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating account for student {id}");
                return StatusCode(500, new { message = "حدث خطأ في الخادم", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "AdminOrEmployee")]  // We'll define this policy in Program.cs
        public async Task<IActionResult> UpdateStudent(int id, [FromBody] UpdateStudentRequest request)
        {
            try
            {
                var student = await _context.Students.FindAsync(id);
                if (student == null)
                    return NotFound(new { message = "الطالب غير موجود" });

                // Check if phone already exists (excluding current student)
                if (await _context.Students.AnyAsync(s => s.Phone == request.Phone && s.Id != id))
                {
                    return BadRequest(new { message = "رقم الهاتف موجود بالفعل" });
                }

                student.FullName = request.FullName;
                student.Phone = request.Phone;
                student.Email = request.Email;
                student.Age = request.Age;
                student.AgeGroup = DetermineAgeGroup(request.Age);
                student.Level = request.Level;
                student.Gender = request.Gender;
                student.School = request.School;
                student.ParentName = request.ParentName;
                student.ParentPhone = request.ParentPhone;
                student.EmergencyContact = request.EmergencyContact;
                student.EmergencyPhone = request.EmergencyPhone;
                student.MedicalConditions = request.MedicalConditions;
                student.IsActive = request.IsActive;

                await _context.SaveChangesAsync();

                return Ok(new { message = "تم تحديث بيانات الطالب بنجاح" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "حدث خطأ في الخادم", error = ex.Message });
            }
        }

        [HttpPost("{studentId}/register-course")]
        [Authorize(Policy = "AdminOrEmployee")]  // We'll define this policy in Program.cs
        public async Task<IActionResult> RegisterStudentToCourse(int studentId, [FromBody] RegisterCourseRequest request)
        {
            try
            {
                var student = await _context.Students.FindAsync(studentId);
                if (student == null)
                    return NotFound(new { message = "الطالب غير موجود" });

                var course = await _context.Courses
                    .Include(c => c.CourseRegistrations)
                    .FirstOrDefaultAsync(c => c.Id == request.CourseId);

                if (course == null)
                    return NotFound(new { message = "الكورس غير موجود" });

                // Check if already registered
                if (await _context.CourseRegistrations
                    .AnyAsync(cr => cr.StudentId == studentId && cr.CourseId == request.CourseId && 
                                   cr.PaymentStatus != PaymentStatus.Cancelled))
                {
                    return BadRequest(new { message = "الطالب مسجل بالفعل في هذا الكورس" });
                }

                // Check course capacity
                var currentRegistrations = course.CourseRegistrations
                    .Count(cr => cr.PaymentStatus != PaymentStatus.Cancelled);

                if (currentRegistrations >= course.MaxStudents)
                {
                    return BadRequest(new { message = "الكورس مكتمل العدد" });
                }

                var registration = new CourseRegistration
                {
                    StudentId = studentId,
                    CourseId = request.CourseId,
                    RegistrationDate = DateTime.UtcNow,
                    TotalAmount = request.TotalAmount ?? course.Price,
                    PaidAmount = request.PaidAmount,
                    PaymentMethod = request.PaymentMethod,
                    PaymentStatus = request.PaidAmount >= (request.TotalAmount ?? course.Price) 
                        ? PaymentStatus.FullyPaid 
                        : request.PaidAmount > 0 
                            ? PaymentStatus.PartiallyPaid 
                            : PaymentStatus.Unpaid,
                    Notes = request.Notes
                };

                _context.CourseRegistrations.Add(registration);
                await _context.SaveChangesAsync(); // Save first to get the ID

                // Add payment record if amount paid
                if (request.PaidAmount > 0)
                {
                    var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                    var userBranchId = int.Parse(User.FindFirst("BranchId")?.Value ?? "0");

                    // Store a payment record
                    var payment = new Payment
                    {
                        CourseRegistrationId = registration.Id,
                        Amount = request.PaidAmount,
                        PaymentMethod = request.PaymentMethod,
                        PaymentDate = DateTime.UtcNow,
                        ProcessedByUserId = userId,
                        Notes = "دفعة التسجيل"
                    };
                    _context.Payments.Add(payment);

                    // Also store an income record in expenses to appear in the financial ledger
                    var incomeExpense = new Expense
                    {
                        Title = $"إيراد كورس '{course.Name}' - {student.FullName}",
                        Description = $"دفعة من الطالب {student.FullName} لكورس {course.Name}",
                        Amount = request.PaidAmount, // Positive amount as income
                        ExpenseDate = DateTime.UtcNow,
                        Category = ExpenseCategory.Training,
                        Status = ExpenseStatus.Paid,
                        Priority = ExpensePriority.Low,
                        PaymentMethod = request.PaymentMethod,
                        BranchId = course.BranchId ?? userBranchId,
                        RequestedByUserId = userId,
                        ApprovedByUserId = userId,
                        ApprovedAt = DateTime.UtcNow,
                        Notes = $"إيراد من دفع رسوم الكورس - {request.Notes}",
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Expenses.Add(incomeExpense);
                }

                await _context.SaveChangesAsync();

                // Send course enrollment email
                if (!string.IsNullOrEmpty(student.Email))
                {
                    try
                    {
                        var emailSent = await _emailService.SendCourseEnrollmentEmailAsync(
                            student.Email,
                            student.FullName,
                            course.Name,
                            course.StartDate,
                            course.EndDate,
                            course.Description ?? "كورس في أكاديمية الذكاء الاصطناعي والروبوتات"
                        );
                        
                        if (emailSent)
                        {
                            _logger.LogInformation($"Course enrollment email sent successfully to {student.Email}");
                        }
                        else
                        {
                            _logger.LogWarning($"Failed to send course enrollment email to {student.Email}");
                        }
                    }
                    catch (Exception emailEx)
                    {
                        _logger.LogError(emailEx, $"Error sending course enrollment email to {student.Email}");
                    }
                }

                _logger.LogInformation($"Successfully registered student {studentId} to course {request.CourseId}. Registration ID: {registration.Id}");

                return Ok(new { message = "تم تسجيل الطالب في الكورس بنجاح", registrationId = registration.Id });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "حدث خطأ في الخادم", error = ex.Message });
            }
        }

        [HttpPut("course-registrations/{registrationId}/update-payment")]
        [Authorize(Policy = "AdminOrEmployee")]
        public async Task<IActionResult> UpdateCourseRegistrationPayment(int registrationId, [FromBody] UpdatePaymentRequest request)
        {
            var registration = await _context.CourseRegistrations.FindAsync(registrationId);
            if (registration == null)
                return NotFound(new { message = "تسجيل الكورس غير موجود" });

            registration.PaidAmount = request.PaidAmount;
            registration.PaymentMethod = request.PaymentMethod;
            registration.PaymentDate = DateTime.UtcNow;
            registration.PaymentNotes = request.Notes;

            // تحديث حالة الدفع بناءً على المبلغ المدفوع
            if (registration.PaidAmount >= registration.TotalAmount)
                registration.PaymentStatus = PaymentStatus.FullyPaid;
            else if (registration.PaidAmount > 0)
                registration.PaymentStatus = PaymentStatus.PartiallyPaid;
            else
                registration.PaymentStatus = PaymentStatus.Unpaid;

            await _context.SaveChangesAsync();
            return Ok(new { message = "تم تحديث حالة الدفع بنجاح" });
        }

        [HttpPut("course-registrations/{registrationId}/update-status")]
        [Authorize(Policy = "AdminOrEmployee")]
        public async Task<IActionResult> UpdateCourseRegistrationStatus(int registrationId, [FromBody] UpdateRegistrationStatusRequest request)
        {
            try
            {
                var registration = await _context.CourseRegistrations.FindAsync(registrationId);
                if (registration == null)
                    return NotFound(new { message = "تسجيل الكورس غير موجود" });

                // تحديث حالة التسجيل
                switch (request.Status.ToLower())
                {
                    case "active":
                        registration.PaymentStatus = registration.PaidAmount >= registration.TotalAmount
                            ? PaymentStatus.FullyPaid
                            : registration.PaidAmount > 0
                                ? PaymentStatus.PartiallyPaid
                                : PaymentStatus.Unpaid;
                        break;
                    case "completed":
                        // يمكن إضافة منطق خاص بالإكمال هنا
                        break;
                    case "dropped":
                        registration.PaymentStatus = PaymentStatus.Cancelled;
                        break;
                    default:
                        return BadRequest(new { message = "حالة التسجيل غير صحيحة" });
                }

                await _context.SaveChangesAsync();
                return Ok(new { success = true, message = "تم تحديث حالة التسجيل بنجاح" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "حدث خطأ في الخادم", error = ex.Message });
            }
        }

        [HttpPost("course-registrations/{registrationId}/create-account")]
        [Authorize(Policy = "AdminOrEmployee")]
        public async Task<IActionResult> CreateStudentAccount(int registrationId)
        {
            try
            {
                var registration = await _context.CourseRegistrations
                    .Include(cr => cr.Student)
                    .Include(cr => cr.Course)
                    .FirstOrDefaultAsync(cr => cr.Id == registrationId);

                if (registration == null)
                    return NotFound(new { message = "تسجيل الكورس غير موجود" });

                // التحقق من أن الطالب دفع المبلغ المطلوب
                if (registration.PaymentStatus == PaymentStatus.Unpaid)
                    return BadRequest(new { message = "يجب دفع رسوم الكورس أولاً لإنشاء الحساب" });

                var student = registration.Student;

                // التحقق من وجود حساب مسبق
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.StudentId == student.Id);
                if (existingUser != null)
                    return BadRequest(new { message = "يوجد حساب مسبق لهذا الطالب",
                        username = existingUser.Username });

                // إنشاء username فريد
                var baseUsername = GenerateUsername(student.FullName, student.Phone);
                var username = await EnsureUniqueUsername(baseUsername);

                // إنشاء password عشوائي
                var password = GenerateRandomPassword();
                var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

                // إنشاء المستخدم الجديد
                var newUser = new User
                {
                    Username = username,
                    PasswordHash = hashedPassword,
                    FullName = student.FullName,
                    Email = student.Email,
                    Phone = student.Phone,

                    UserRole = UserRole.Student,
                    BranchId = registration.Course.BranchId ?? 1,
                    StudentId = student.Id,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Created account for student {student.FullName} with username {username}");

                return Ok(new {
                    success = true,
                    message = "تم إنشاء حساب الطالب بنجاح",
                    username = username,
                    password = password,
                    studentName = student.FullName,
                    courseName = registration.Course.Name
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating student account: {ex.Message}");
                return StatusCode(500, new { message = "حدث خطأ في الخادم", error = ex.Message });
            }
        }

#if DEBUG
        [HttpPost("create-accounts-for-existing")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> CreateAccountsForExistingStudents()
        {
            try
            {
                var studentsWithoutAccounts = await _context.Students
                    .Where(s => s.IsActive && !_context.Users.Any(u => u.StudentId == s.Id))
                    .ToListAsync();

                var createdAccounts = new List<object>();

                foreach (var student in studentsWithoutAccounts)
                {
                    if (string.IsNullOrEmpty(student.Email))
                    {
                        _logger.LogWarning($"Student {student.Id} ({student.FullName}) has no email address, skipping account creation");
                        continue;
                    }

                    var (user, _) = await CreateUserAccountForStudentAsync(student);

                    if (user != null)
                    {
                        createdAccounts.Add(new
                        {
                            studentId = student.Id,
                            studentName = student.FullName,
                            email = student.Email,
                            username = user.Username,
                            emailSent = !string.IsNullOrEmpty(user.Email)
                        });
                    }
                }

                return Ok(new
                {
                    message = $"تم إنشاء {createdAccounts.Count} حساب جديد للطلاب الموجودين",
                    totalStudentsProcessed = studentsWithoutAccounts.Count,
                    accountsCreated = createdAccounts.Count,
                    accounts = createdAccounts
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating accounts for existing students");
                return StatusCode(500, new { message = "حدث خطأ في إنشاء الحسابات", error = ex.Message });
            }
        }
#endif

        [HttpPost("create-account/{studentId}")]
        [Authorize(Policy = "AdminOrEmployee")]
        public async Task<IActionResult> CreateAccountForStudent(int studentId)
        {
            try
            {
                var student = await _context.Students.FindAsync(studentId);
                if (student == null)
                    return NotFound(new { message = "الطالب غير موجود" });

                // Check if account already exists
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.StudentId == studentId);
                if (existingUser != null)
                    return BadRequest(new { message = "الطالب لديه حساب بالفعل", username = existingUser.Username });

                if (string.IsNullOrEmpty(student.Email))
                    return BadRequest(new { message = "الطالب ليس لديه عنوان إيميل" });

                var (user, password) = await CreateUserAccountForStudentAsync(student);

                return Ok(new
                {
                    message = "تم إنشاء الحساب بنجاح وإرسال بيانات الدخول",
                    studentId = student.Id,
                    studentName = student.FullName,
                    username = user?.Username,
                    password = password, // فقط للاختبار - في الإنتاج لا نُرجع كلمة المرور
                    emailSent = !string.IsNullOrEmpty(user?.Email)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating account for student {studentId}");
                return StatusCode(500, new { message = "حدث خطأ في إنشاء الحساب", error = ex.Message });
            }
        }

        /// <summary>
        /// Centralized method to create a user account for a student.
        /// </summary>
        private async Task<(User? user, string? plainTextPassword)> CreateUserAccountForStudentAsync(Student student)
        {
            try
            {
                // Generate username and password
                var username = _passwordGenerator.GenerateUsername(student.FullName);
                var password = _passwordGenerator.GenerateRandomPassword();
                var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

                // Ensure username is unique
                var existingUserCount = await _context.Users.CountAsync(u => u.Username.StartsWith(username));
                if (existingUserCount > 0)
                {
                    username = username + (existingUserCount + 1);
                }

                var user = new User
                {
                    Username = username,
                    PasswordHash = hashedPassword,
                    FullName = student.FullName,
                    Email = student.Email,
                    Phone = student.Phone,
                    Role = UserRole.Student,
                    BranchId = student.BranchId,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    StudentId = student.Id
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Send welcome email
                if (!string.IsNullOrEmpty(student.Email))
                {
                    var emailSent = await _emailService.SendWelcomeEmailAsync(student.Email, student.FullName, username, password);
                    if (emailSent)
                    {
                        _logger.LogInformation("Welcome email sent successfully to {Email}", student.Email);
                    }
                    else
                    {
                        _logger.LogWarning("Failed to send welcome email to {Email}", student.Email);
                    }
                }

                return (user, password);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CreateUserAccountForStudentAsync for student {StudentId}", student.Id);
                return (null, null);
            }
        }

        [HttpGet("{studentId}/dashboard")]
        [Authorize(Policy = "AdminOrEmployee")]
        public async Task<IActionResult> GetStudentDashboard(int studentId)
        {
            try
            {
                var student = await _context.Students
                    .Include(s => s.CourseRegistrations)
                        .ThenInclude(cr => cr.Course)
                    .Include(s => s.Attendances)
                        .ThenInclude(a => a.Course)
                    .FirstOrDefaultAsync(s => s.Id == studentId);

                if (student == null)
                    return NotFound(new { message = "الطالب غير موجود" });

                var dashboardData = new StudentDashboardDTO
                {
                    StudentId = student.Id,
                    StudentName = student.FullName,
                    Phone = student.Phone,
                    Email = student.Email,
                    Courses = student.CourseRegistrations
                        .Where(cr => cr.PaymentStatus != PaymentStatus.Cancelled)
                        .Select(cr => new StudentCourseDTO
                        {
                            CourseId = cr.Course.Id,
                            CourseName = cr.Course.Name,
                            StartDate = cr.Course.StartDate,
                            EndDate = cr.Course.EndDate,
                            TotalSessions = cr.Course.SessionsCount,
                            AttendedSessions = student.Attendances
                                .Count(a => a.CourseId == cr.Course.Id && a.Status == AttendanceStatus.Present),
                            AbsentSessions = student.Attendances
                                .Count(a => a.CourseId == cr.Course.Id && a.Status == AttendanceStatus.Absent),
                            IsCompleted = cr.Course.Status == CourseStatus.Completed,
                            PaymentStatus = cr.PaymentStatus.ToString(),
                            PaymentStatusArabic = GetPaymentStatusArabic(cr.PaymentStatus),
                            NextSessionDate = GetNextSessionDate(cr.Course),
                            Schedule = cr.Course.Schedule,
                            InstructorName = cr.Course.Instructor?.FullName ?? "غير محدد",
                            Progress = CalculateCourseProgress(cr.Course, student.Attendances.Where(a => a.CourseId == cr.Course.Id).ToList())
                        }).ToList()
                };

                return Ok(new { success = true, data = dashboardData });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting student dashboard: {ex.Message}");
                return StatusCode(500, new { message = "حدث خطأ في الخادم", error = ex.Message });
            }
        }

        private string GetPaymentStatusArabic(PaymentStatus status)
        {
            return status switch
            {
                PaymentStatus.Pending => "في الانتظار",
                PaymentStatus.Unpaid => "غير مدفوع",
                PaymentStatus.PartiallyPaid => "مدفوع جزئياً",
                PaymentStatus.FullyPaid => "مدفوع بالكامل",
                PaymentStatus.Cancelled => "ملغي",
                _ => status.ToString()
            };
        }

        private string GetPaymentMethodArabic(PaymentMethod method)
        {
            return method switch
            {
                PaymentMethod.Cash => "نقدي",
                PaymentMethod.InstaPay => "انستا باي",
                PaymentMethod.Fawry => "فوري",
                _ => method.ToString()
            };
        }

        private string GetPaymentTypeArabic(PaymentType type)
        {
            return type switch
            {
                PaymentType.CourseFee => "رسوم الكورس",
                PaymentType.Cafeteria => "الكافيتريا",
                PaymentType.Workspace => "مساحة العمل",
                PaymentType.Equipment => "المعدات",
                PaymentType.Other => "أخرى",
                _ => type.ToString()
            };
        }

        private DateTime? GetNextSessionDate(Course course)
        {
            // منطق حساب موعد الجلسة القادمة بناءً على جدول الكورس
            // هذا مثال بسيط - يمكن تطويره أكثر
            if (course.Status == CourseStatus.Active && DateTime.Now < course.EndDate)
            {
                return DateTime.Now.AddDays(7); // مثال: الجلسة القادمة بعد أسبوع
            }
            return null;
        }

        private double CalculateCourseProgress(Course course, List<Attendance> attendances)
        {
            if (course.SessionsCount == 0) return 0;

            var attendedSessions = attendances.Count(a => a.Status == AttendanceStatus.Present);
            return Math.Round((double)attendedSessions / course.SessionsCount * 100, 1);
        }

        [HttpGet("my-dashboard")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> GetMyDashboard()
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

                if (user?.StudentId == null)
                    return BadRequest(new { message = "المستخدم ليس طالباً" });

                var student = await _context.Students
                    .Include(s => s.CourseRegistrations)
                        .ThenInclude(cr => cr.Course)
                    .Include(s => s.Attendances)
                        .ThenInclude(a => a.Course)
                    .FirstOrDefaultAsync(s => s.Id == user.StudentId);

                // جلب الشهادات
                var certificates = await _context.Certificates
                    .Where(c => c.StudentId == user.StudentId)
                    .ToListAsync();

                if (student == null)
                    return NotFound(new { message = "بيانات الطالب غير موجودة" });

                // جلب سجل المدفوعات
                var payments = await _context.Payments
                    .Include(p => p.CourseRegistration)
                        .ThenInclude(cr => cr.Course)
                    .Include(p => p.ProcessedByUser)
                    .Where(p => p.StudentId == student.Id && p.IsActive)
                    .OrderByDescending(p => p.PaymentDate)
                    .Select(p => new
                    {
                        p.Id,
                        p.Amount,
                        PaymentMethod = p.PaymentMethod.ToString(),
                        PaymentMethodArabic = GetPaymentMethodArabic(p.PaymentMethod),
                        PaymentType = p.PaymentType.ToString(),
                        PaymentTypeArabic = GetPaymentTypeArabic(p.PaymentType),
                        p.PaymentDate,
                        p.Notes,
                        CourseName = p.CourseRegistration != null ? p.CourseRegistration.Course.Name : null,
                        ProcessedBy = p.ProcessedByUser.FullName
                    })
                    .ToListAsync();

                // حساب إجمالي المدفوع والمستحق
                var totalPaid = payments.Sum(p => p.Amount);
                var totalOwed = student.CourseRegistrations
                    .Where(cr => cr.PaymentStatus != PaymentStatus.Cancelled)
                    .Sum(cr => cr.TotalAmount);
                var outstandingBalance = totalOwed - totalPaid;

                var dashboardData = new StudentDashboardDTO
                {
                    StudentId = student.Id,
                    StudentName = student.FullName,
                    Phone = student.Phone,
                    Email = student.Email,
                    TotalPaid = totalPaid,
                    OutstandingBalance = outstandingBalance,
                    PaymentHistory = payments.Take(5).Cast<object>().ToList(), // آخر 5 مدفوعات
                    Courses = student.CourseRegistrations
                        .Where(cr => cr.PaymentStatus != PaymentStatus.Cancelled)
                        .Select(cr =>
                        {
                            var certificate = certificates.FirstOrDefault(c => c.CourseId == cr.Course.Id);
                            return new StudentCourseDTO
                            {
                                CourseId = cr.Course.Id,
                                CourseName = cr.Course.Name,
                                StartDate = cr.Course.StartDate,
                                EndDate = cr.Course.EndDate,
                                TotalSessions = cr.Course.SessionsCount,
                                AttendedSessions = student.Attendances
                                    .Count(a => a.CourseId == cr.Course.Id && a.Status == AttendanceStatus.Present),
                                AbsentSessions = student.Attendances
                                    .Count(a => a.CourseId == cr.Course.Id && a.Status == AttendanceStatus.Absent),
                                IsCompleted = cr.Course.Status == CourseStatus.Completed,
                                PaymentStatus = cr.PaymentStatus.ToString(),
                                PaymentStatusArabic = GetPaymentStatusArabic(cr.PaymentStatus),
                                NextSessionDate = GetNextSessionDate(cr.Course),
                                Schedule = cr.Course.Schedule,
                                InstructorName = cr.Course.Instructor?.FullName ?? "غير محدد",
                                Progress = CalculateCourseProgress(cr.Course, student.Attendances.Where(a => a.CourseId == cr.Course.Id).ToList()),
                                CertificateUrl = certificate?.CertificateUrl,
                                ExamScore = certificate?.ExamScore
                            };
                        }).ToList()
                };

                return Ok(new { success = true, data = dashboardData });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting student dashboard: {ex.Message}");
                return StatusCode(500, new { message = "حدث خطأ في الخادم", error = ex.Message });
            }
        }

        [HttpDelete("course-registrations/{registrationId}")]
        [Authorize(Policy = "AdminOrEmployee")]
        public async Task<IActionResult> DeleteCourseRegistration(int registrationId)
        {
            var registration = await _context.CourseRegistrations.FindAsync(registrationId);
            if (registration == null)
                return NotFound(new { message = "تسجيل الكورس غير موجود" });

            _context.CourseRegistrations.Remove(registration);
            await _context.SaveChangesAsync();
            return Ok(new { message = "تم حذف تسجيل الطالب من الكورس بنجاح" });
        }

        [HttpDelete("{id:int}")]
        [Authorize(Policy = "AdminOrEmployee")]
        public async Task<IActionResult> DeleteStudent(int id)
        {
            try
            {
                _logger.LogInformation($"Attempting to delete student with ID: {id}");
                
                var student = await _context.Students
                    .Include(s => s.CourseRegistrations)
                    .Include(s => s.Attendances)
                    .Include(s => s.WorkspaceBookings)
                    .Include(s => s.CafeteriaOrders)
                    .FirstOrDefaultAsync(s => s.Id == id);

                // Find the associated user by StudentId
                var associatedUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.StudentId == id);

                if (student == null)
                {
                    _logger.LogWarning($"Student with ID {id} not found");
                    return NotFound(new { success = false, message = "الطالب غير موجود" });
                }

                // Check if student has any active course registrations
                var hasActiveRegistrations = student.CourseRegistrations
                    .Any(cr => cr.PaymentStatus != PaymentStatus.Cancelled);
                
                if (hasActiveRegistrations)
                {
                    _logger.LogWarning($"Cannot delete student {id} because they have active course registrations");
                    return BadRequest(new { 
                        success = false, 
                        message = "لا يمكن حذف الطالب لأنه مسجل في كورسات نشطة" 
                    });
                }

                // Remove related records first
                if (student.CourseRegistrations.Any())
                {
                    _context.CourseRegistrations.RemoveRange(student.CourseRegistrations);
                }
                if (student.Attendances.Any())
                {
                    _context.Attendances.RemoveRange(student.Attendances);
                }
                if (student.WorkspaceBookings.Any())
                {
                    _context.WorkspaceBookings.RemoveRange(student.WorkspaceBookings);
                }
                if (student.CafeteriaOrders.Any())
                {
                    _context.CafeteriaOrders.RemoveRange(student.CafeteriaOrders);
                }

                // Remove the associated User account first (if exists)
                if (associatedUser != null)
                {
                    _logger.LogInformation($"Deleting associated user account with ID: {associatedUser.Id}, Email: {associatedUser.Email}");
                    _context.Users.Remove(associatedUser);
                }

                // Remove the student
                _context.Students.Remove(student);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Successfully deleted student with ID: {id} and associated user account");
                return Ok(new { 
                    success = true, 
                    message = "تم حذف الطالب وحساب المستخدم بنجاح" 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting student with ID: {id}");
                return StatusCode(500, new { 
                    success = false, 
                    message = "حدث خطأ في الخادم", 
                    error = ex.Message 
                });
            }
        }

        [HttpPost("course-registrations/{registrationId}/issue-certificate")]
        [Authorize(Policy = "AdminOrEmployee")]
        public async Task<IActionResult> IssueCertificate(int registrationId, [FromBody] IssueCertificateRequest request)
        {
            try
            {
                var registration = await _context.CourseRegistrations
                    .Include(cr => cr.Student)
                    .Include(cr => cr.Course)
                    .FirstOrDefaultAsync(cr => cr.Id == registrationId);

                if (registration == null)
                    return NotFound(new { message = "تسجيل الكورس غير موجود" });

                // التحقق من أن الكورس مكتمل
                if (registration.Course.Status != CourseStatus.Completed)
                    return BadRequest(new { message = "لا يمكن إصدار شهادة لكورس غير مكتمل" });

                // التحقق من عدم وجود شهادة مسبقة
                var existingCertificate = await _context.Certificates
                    .FirstOrDefaultAsync(c => c.StudentId == registration.StudentId && c.CourseId == registration.CourseId);

                if (existingCertificate != null)
                    return BadRequest(new { message = "تم إصدار شهادة مسبقاً لهذا الطالب في هذا الكورس" });

                // إنشاء رقم شهادة فريد
                var certificateNumber = GenerateCertificateNumber(registration.Course.Name, registration.Student.FullName);

                // إنشاء الشهادة
                var certificate = new Certificate
                {
                    StudentId = registration.StudentId,
                    CourseId = registration.CourseId,
                    CertificateNumber = certificateNumber,
                    CertificateUrl = $"/certificates/{certificateNumber}.pdf",
                    IssueDate = DateTime.UtcNow,
                    ExamScore = request.ExamScore,
                    Notes = request.Notes,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Certificates.Add(certificate);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Certificate issued for student {registration.Student.FullName} in course {registration.Course.Name}");

                return Ok(new
                {
                    success = true,
                    message = "تم إصدار الشهادة بنجاح",
                    certificateNumber = certificateNumber,
                    certificateUrl = certificate.CertificateUrl,
                    examScore = certificate.ExamScore
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error issuing certificate: {ex.Message}");
                return StatusCode(500, new { message = "حدث خطأ في الخادم", error = ex.Message });
            }
        }

        private AgeGroup DetermineAgeGroup(int age)
        {
            return age switch
            {
                >= 4 and <= 6 => AgeGroup.GroupA,
                >= 7 and <= 10 => AgeGroup.GroupB,
                >= 11 and <= 12 => AgeGroup.GroupC,
                >= 13 and <= 17 => AgeGroup.GroupD,
                _ => AgeGroup.GroupA
            };
        }

        private static string GetAgeGroupArabic(AgeGroup ageGroup)
        {
            return ageGroup switch
            {
                AgeGroup.GroupA => "الفئة أ (4-6 سنوات)",
                AgeGroup.GroupB => "الفئة ب (7-10 سنوات)",
                AgeGroup.GroupC => "الفئة ج (11-12 سنة)",
                AgeGroup.GroupD => "الفئة د (13-17 سنة)",
                _ => "غير محدد"
            };
        }

        private static string GetLevelArabic(StudentLevel level)
        {
            return level switch
            {
                StudentLevel.Level1 => "المستوى الأول",
                StudentLevel.Level2 => "المستوى الثاني",
                StudentLevel.Level3 => "المستوى الثالث",
                _ => "غير محدد"
            };
        }

        private string GenerateCertificateNumber(string courseName, string studentName)
        {
            var courseCode = courseName.Length >= 3 ? courseName.Substring(0, 3).ToUpper() : courseName.ToUpper();
            var studentCode = studentName.Replace(" ", "").Length >= 3 ?
                studentName.Replace(" ", "").Substring(0, 3).ToUpper() :
                studentName.Replace(" ", "").ToUpper();
            var timestamp = DateTime.Now.ToString("yyyyMMdd");
            var random = new Random().Next(100, 999);

            return $"CERT-{courseCode}-{studentCode}-{timestamp}-{random}";
        }
    }

    public class CreateStudentRequest
    {
        [Required(ErrorMessage = "الاسم الكامل مطلوب")]
        [MaxLength(100, ErrorMessage = "الاسم الكامل يجب ألا يتجاوز 100 حرف")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "رقم الهاتف مطلوب")]
        [MaxLength(20, ErrorMessage = "رقم الهاتف يجب ألا يتجاوز 20 رقم")]
        [RegularExpression(@"^01[0-2,5]{1}[0-9]{8}$", ErrorMessage = "رقم هاتف غير صحيح (01xxxxxxxxx)")]
        public string Phone { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "بريد إلكتروني غير صحيح")]
        [MaxLength(100, ErrorMessage = "البريد الإلكتروني يجب ألا يتجاوز 100 حرف")]
        public string? Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "العمر مطلوب")]
        [Range(4, 17, ErrorMessage = "العمر يجب أن يكون بين 4 و 17 سنة")]
        public int Age { get; set; }

        public AgeGroup AgeGroup { get; set; } // سيتم تحديده تلقائياً بناءً على العمر

        [Required(ErrorMessage = "المستوى مطلوب")]
        public StudentLevel Level { get; set; }

        [Required(ErrorMessage = "الجنس مطلوب")]
        [MaxLength(10, ErrorMessage = "الجنس يجب ألا يتجاوز 10 أحرف")]
        public string Gender { get; set; } = string.Empty;

        [MaxLength(100, ErrorMessage = "اسم المدرسة يجب ألا يتجاوز 100 حرف")]
        public string School { get; set; } = string.Empty;

        [Required(ErrorMessage = "اسم ولي الأمر مطلوب")]
        [MaxLength(100, ErrorMessage = "اسم ولي الأمر يجب ألا يتجاوز 100 حرف")]
        public string ParentName { get; set; } = string.Empty;

        [Required(ErrorMessage = "رقم هاتف ولي الأمر مطلوب")]
        [MaxLength(20, ErrorMessage = "رقم هاتف ولي الأمر يجب ألا يتجاوز 20 رقم")]
        [RegularExpression(@"^01[0-2,5]{1}[0-9]{8}$", ErrorMessage = "رقم هاتف غير صحيح (01xxxxxxxxx)")]
        public string ParentPhone { get; set; } = string.Empty;

        [MaxLength(100, ErrorMessage = "اسم جهة الاتصال يجب ألا يتجاوز 100 حرف")]
        public string EmergencyContact { get; set; } = string.Empty;

        [MaxLength(20, ErrorMessage = "رقم هاتف الطوارئ يجب ألا يتجاوز 20 رقم")]
        [RegularExpression(@"^01[0-2,5]{1}[0-9]{8}$", ErrorMessage = "رقم هاتف غير صحيح (01xxxxxxxxx)")]
        public string EmergencyPhone { get; set; } = string.Empty;

        [MaxLength(500, ErrorMessage = "الحالة الطبية يجب ألا تتجاوز 500 حرف")]
        public string MedicalConditions { get; set; } = string.Empty;
    }

    public class UpdateStudentRequest : CreateStudentRequest
    {
        public bool IsActive { get; set; }
    }

    public class RegisterCourseRequest
    {
        [Required]
        public int CourseId { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? TotalAmount { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal PaidAmount { get; set; }

        [Required]
        public PaymentMethod PaymentMethod { get; set; }

        [MaxLength(500)]
        public string Notes { get; set; } = string.Empty;
    }

    public class IssueCertificateRequest
    {
        [Range(0, 100, ErrorMessage = "درجة الامتحان يجب أن تكون بين 0 و 100")]
        public int? ExamScore { get; set; }

        [MaxLength(500, ErrorMessage = "الملاحظات يجب ألا تتجاوز 500 حرف")]
        public string Notes { get; set; } = string.Empty;
    }
}
