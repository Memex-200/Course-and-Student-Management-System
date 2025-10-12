using Api.Data;
using Api.Models;
using Api.DTOs;
using Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CoursesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly ILogger<CoursesController> _logger;

        public CoursesController(ApplicationDbContext context, IEmailService emailService, ILogger<CoursesController> logger)
        {
            _context = context;
            _emailService = emailService;
            _logger = logger;
        }

        [HttpGet("admin-overview")]
        public async Task<IActionResult> GetAdminOverview()
        {
            try
            {
                // Get all courses without authorization for admin overview
                var courses = await _context.Courses
                    .Include(c => c.CourseCategory)
                    .Include(c => c.Branch)
                    .Include(c => c.Instructor)
                        .ThenInclude(i => i.User)
                    .Include(c => c.CourseRegistrations)
                    .Select(c => new
                    {
                        c.Id,
                        c.Name,
                        c.Description,
                        c.Price,
                        c.SessionsCount,
                        c.MaxStudents,
                        CurrentStudents = c.CourseRegistrations.Count(cr => cr.PaymentStatus != PaymentStatus.Cancelled),
                        c.StartDate,
                        c.EndDate,
                        c.Status,
                        BranchName = c.Branch != null ? c.Branch.Name : "غير محدد",
                        InstructorName = c.Instructor != null && c.Instructor.User != null ? c.Instructor.User.FullName : "غير محدد",
                        c.IsActive,
                        c.CreatedAt
                    })
                    .OrderByDescending(c => c.CreatedAt)
                    .ToListAsync();

                return Ok(new { success = true, data = courses });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting admin overview");
                return StatusCode(500, new { success = false, message = "حدث خطأ في الخادم", error = ex.Message });
            }
        }

        [HttpGet]
        [Authorize(Policy = "AdminOrEmployee")]
        public async Task<IActionResult> GetCourses([FromQuery] int? branchId = null, [FromQuery] int? categoryId = null)
        {
            try
            {
                // Log request details
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
                var userBranchIdClaim = User.FindFirst("BranchId")?.Value;
                _logger.LogInformation("GetCourses called by user with role {UserRole} and branchId {BranchIdClaim}", userRole, userBranchIdClaim);
                _logger.LogInformation("Query params - branchId: {BranchId}, categoryId: {CategoryId}", branchId, categoryId);

                // Log all claims
                foreach (var claim in User.Claims)
                {
                    _logger.LogDebug("Claim: {ClaimType} = {ClaimValue}", claim.Type, claim.Value);
                }

                // Get total count before any filtering
                var totalCount = await _context.Courses.CountAsync();
                _logger.LogInformation("Total courses in database (before filters): {TotalCount}", totalCount);

                // Log some sample courses
                var sampleCourses = await _context.Courses
                    .Take(5)
                    .Select(c => new { c.Id, c.Name, c.BranchId, c.CourseCategoryId, c.IsActive })
                    .ToListAsync();
                
                _logger.LogDebug("Sample courses from database: {SampleCourses}", System.Text.Json.JsonSerializer.Serialize(sampleCourses));

                var query = _context.Courses
                    .Include(c => c.CourseCategory)
                    .Include(c => c.Branch)
                    .Include(c => c.Instructor)
                        .ThenInclude(i => i.User)
                    .Include(c => c.Room)
                    .Include(c => c.Lab)
                    .Include(c => c.CourseRegistrations)
                    .AsQueryable();

                _logger.LogDebug("Initial query created with includes");

                // REMOVE branch filtering: show all courses to all users

                // Filter by category only if requested
                if (categoryId.HasValue)
                {
                    query = query.Where(c => c.CourseCategoryId == categoryId.Value);
                    _logger.LogInformation("Filtered by category: {CategoryId}", categoryId.Value);
                }

                // Log the SQL query
                var sql = query.ToQueryString();
                _logger.LogDebug("Generated SQL query: {SQL}", sql);

                var courses = await query
                    .OrderByDescending(c => c.CreatedAt)
                    .Select(c => new CourseDTO
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Description = c.Description,
                        CourseCategoryId = c.CourseCategoryId,
                        CourseCategoryName = c.CourseCategory != null && c.CourseCategory.Name != null ? c.CourseCategory.Name : string.Empty,
                        Price = c.Price,
                        SessionsCount = c.SessionsCount,
                        MaxStudents = c.MaxStudents,
                        CurrentStudents = c.CourseRegistrations != null ? c.CourseRegistrations.Count(cr => cr.PaymentStatus != PaymentStatus.Cancelled) : 0,
                        StartDate = c.StartDate,
                        EndDate = c.EndDate,
                        Status = c.Status,
                        BranchId = c.BranchId,
                        BranchName = c.Branch != null && c.Branch.Name != null ? c.Branch.Name : string.Empty,
                        InstructorId = c.InstructorId,
                        InstructorName = c.Instructor != null && c.Instructor.User != null && c.Instructor.User.FullName != null ? c.Instructor.User.FullName : string.Empty,
                        LabId = c.LabId,
                        LabName = c.Lab != null && c.Lab.Name != null ? c.Lab.Name : string.Empty,
                        RoomId = c.RoomId,
                        RoomName = c.Room != null && c.Room.Name != null ? c.Room.Name : string.Empty,
                        DriveLink = c.DriveLink,
                        Schedule = c.Schedule ?? string.Empty,
                        Content = c.Content ?? string.Empty,
                        Prerequisites = c.Prerequisites ?? string.Empty,
                        CourseDays = c.CourseDays ?? string.Empty,
                        StartTime = c.StartTime ?? string.Empty,
                        EndTime = c.EndTime ?? string.Empty,
                        IsActive = c.IsActive,
                        CreatedAt = c.CreatedAt,
                        Notes = c.Notes ?? string.Empty
                    })
                    .ToListAsync();

                _logger.LogInformation("Found {CourseCount} courses after applying filters", courses.Count);

                return Ok(new { success = true, data = courses });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting courses. Query params - branchId: {BranchId}, categoryId: {CategoryId}", branchId, categoryId);
                return StatusCode(500, new { success = false, message = "حدث خطأ في الخادم", error = ex.Message });
            }
        }

        // GET: api/courses/all
        [HttpGet("all")]
        public async Task<IActionResult> GetAllCourses()
        {
            try
            {
                var courses = await _context.Courses
                    .Where(c => c.IsActive)
                    .OrderBy(c => c.Name)
                    .Select(c => new
                    {
                        c.Id,
                        c.Name,
                        c.Description,
                        c.Price
                    })
                    .ToListAsync();

                return Ok(courses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all courses");
                return StatusCode(500, new { message = "حدث خطأ في الخادم", error = ex.Message });
            }
        }

        [HttpGet("test-data")]
        [Authorize(Policy = "AdminOrEmployee")]
        public async Task<IActionResult> GetTestData()
        {
            try
            {
                var data = new
                {
                    Branches = await _context.Branches.Select(b => new { b.Id, b.Name }).ToListAsync(),
                    Categories = await _context.CourseCategories.Select(c => new { c.Id, c.Name }).ToListAsync(),
                    Rooms = await _context.Rooms.Select(r => new { r.Id, r.Name, r.BranchId }).ToListAsync(),
                    Labs = await _context.Labs.Select(l => new { l.Id, l.Name, l.BranchId }).ToListAsync(),
                    Employees = await _context.Employees
                        .Where(e => e.EmployeeRole == Models.EmployeeRole.Instructor)
                        .Select(e => new { e.Id, e.FullName, e.BranchId })
                        .ToListAsync()
                };

                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "حدث خطأ في الخادم", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        [Authorize(Policy = "AdminOrEmployee")]
        public async Task<IActionResult> GetCourse(int id)
        {
            try
            {
                var course = await _context.Courses
                    .Include(c => c.CourseCategory)
                    .Include(c => c.Branch)
                    .Include(c => c.Instructor)
                        .ThenInclude(i => i.User)
                    .Include(c => c.Room)
                    .Include(c => c.Lab)
                    .Include(c => c.CourseRegistrations)
                        .ThenInclude(cr => cr.Student)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (course == null)
                    return NotFound(new { success = false, message = "الكورس غير موجود" });

                // Debug logging
                _logger.LogDebug("Course {CourseId} found with {RegistrationCount} registrations.", id, course.CourseRegistrations?.Count ?? 0);

                var result = new CourseDTO
                {
                    Id = course.Id,
                    Name = course.Name,
                    Description = course.Description,
                    CourseCategoryId = course.CourseCategoryId.GetValueOrDefault(),
                    CourseCategoryName = course.CourseCategory != null ? course.CourseCategory.Name : string.Empty,
                    Price = course.Price,
                    SessionsCount = course.SessionsCount,
                    MaxStudents = course.MaxStudents,
                    CurrentStudents = course.CourseRegistrations.Count(cr => cr.PaymentStatus != PaymentStatus.Cancelled),
                    StartDate = course.StartDate,
                    EndDate = course.EndDate,
                    Status = course.Status,
                    BranchId = course.BranchId.GetValueOrDefault(),
                    BranchName = course.Branch != null ? course.Branch.Name : string.Empty,
                    InstructorId = course.InstructorId,
                    InstructorName = course.Instructor != null && course.Instructor.User != null ? course.Instructor.User.FullName : string.Empty,
                    LabId = course.LabId,
                    LabName = course.Lab != null ? course.Lab.Name : string.Empty,
                    RoomId = course.RoomId,
                    RoomName = course.Room != null ? course.Room.Name : string.Empty,
                    DriveLink = course.DriveLink,
                    Schedule = course.Schedule,
                    Content = course.Content,
                    Prerequisites = course.Prerequisites,
                    CourseDays = course.CourseDays,
                    StartTime = course.StartTime,
                    EndTime = course.EndTime,
                    IsActive = course.IsActive,
                    CreatedAt = course.CreatedAt,
                    Notes = course.Notes,
                    EnrolledStudents = course.CourseRegistrations
                        .Where(cr => cr.PaymentStatus != PaymentStatus.Cancelled)
                        .Select(cr => {
                            return new EnrolledStudentDTO {
                            Id = cr.Student.Id,
                            RegistrationId = cr.Id,
                                FullName = cr.Student.FullName ?? "اسم غير متوفر",
                            PaidAmount = cr.PaidAmount,
                            RemainingAmount = cr.RemainingAmount,
                            PaymentStatus = cr.PaymentStatus.ToString(),
                            PaymentStatusArabic = cr.PaymentStatus switch
                            {
                                PaymentStatus.FullyPaid => "مدفوع بالكامل",
                                PaymentStatus.PartiallyPaid => "مدفوع جزئياً",
                                PaymentStatus.Unpaid => "غير مدفوع",
                                PaymentStatus.Pending => "في الانتظار",
                                PaymentStatus.Cancelled => "ملغي",
                                _ => cr.PaymentStatus.ToString()
                            },
                                Phone = cr.Student.Phone ?? "غير متوفر",
                                PaymentMethod = cr.PaymentMethod?.ToString() ?? "Cash",
                                PaymentMethodArabic = cr.PaymentMethod switch
                                {
                                    PaymentMethod.Cash => "نقدي",
                                    PaymentMethod.InstaPay => "انستا باي",
                                    PaymentMethod.Fawry => "فوري",
                                    null => "نقدي",
                                    _ => cr.PaymentMethod.ToString()
                                }
                            };
                        }).ToList()
                };

                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "حدث خطأ في الخادم", error = ex.Message });
            }
        }

        [HttpPost]
        [Authorize(Policy = "AdminOrEmployee")]
        public async Task<IActionResult> CreateCourse([FromBody] CreateCourseDTO request)
        {
            try
            {
                // Validate required fields
                if (string.IsNullOrWhiteSpace(request.Name))
                    return BadRequest(new { success = false, message = "اسم الدورة مطلوب" });

                if (request.Price < 0)
                    return BadRequest(new { success = false, message = "السعر يجب أن يكون أكبر من أو يساوي صفر" });

                if (request.SessionsCount <= 0)
                    return BadRequest(new { success = false, message = "عدد الحصص يجب أن يكون أكبر من صفر" });

                if (request.MaxStudents <= 0)
                    return BadRequest(new { success = false, message = "الحد الأقصى للطلاب يجب أن يكون أكبر من صفر" });

                if (request.StartDate >= request.EndDate)
                    return BadRequest(new { success = false, message = "تاريخ البداية يجب أن يكون قبل تاريخ النهاية" });

                // Validate foreign keys if provided
                if (request.CourseCategoryId.HasValue)
                {
                    var category = await _context.CourseCategories.FindAsync(request.CourseCategoryId.Value);
                    if (category == null)
                        return BadRequest(new { success = false, message = "فئة الدورة غير موجودة" });
                }

                if (request.BranchId.HasValue)
                {
                    var branch = await _context.Branches.FindAsync(request.BranchId.Value);
                    if (branch == null)
                        return BadRequest(new { success = false, message = "الفرع غير موجود" });
                }

                if (request.InstructorId.HasValue)
                {
                    var instructor = await _context.Employees.FindAsync(request.InstructorId.Value);
                    if (instructor == null)
                        return BadRequest(new { success = false, message = "المدرب غير موجود" });
                }

                if (request.RoomId.HasValue)
                {
                    var room = await _context.Rooms.FindAsync(request.RoomId.Value);
                    if (room == null)
                        return BadRequest(new { success = false, message = "القاعة غير موجودة" });
                }

                if (request.LabId.HasValue)
                {
                    var lab = await _context.Labs.FindAsync(request.LabId.Value);
                    if (lab == null)
                        return BadRequest(new { success = false, message = "المعمل غير موجود" });
                }

                var course = new Course
                {
                    Name = request.Name,
                    Description = request.Description,
                    CourseCategoryId = request.CourseCategoryId,
                    Price = request.Price,
                    SessionsCount = request.SessionsCount,
                    MaxStudents = request.MaxStudents,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    BranchId = request.BranchId,
                    InstructorId = request.InstructorId,
                    RoomId = request.RoomId,
                    LabId = request.LabId,
                    Content = request.Content,
                    Prerequisites = request.Prerequisites,
                    CourseDays = request.CourseDays,
                    StartTime = request.StartTime,
                    EndTime = request.EndTime,
                    Notes = request.Notes,
                    DriveLink = request.DriveLink,
                    Status = CourseStatus.Planned,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                // Generate schedule string
                course.Schedule = $"{request.CourseDays}، {request.StartTime}-{request.EndTime}";

                _context.Courses.Add(course);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "تم إنشاء الكورس بنجاح", data = new { courseId = course.Id } });
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, new { success = false, message = "خطأ في حفظ البيانات في قاعدة البيانات", error = ex.InnerException?.Message ?? ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "حدث خطأ في الخادم", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "AdminOrEmployee")]
        public async Task<IActionResult> UpdateCourse(int id, [FromBody] UpdateCourseDTO request)
        {
            try
            {
                var course = await _context.Courses.FindAsync(id);
                if (course == null)
                    return NotFound(new { success = false, message = "الكورس غير موجود" });

                // Update only provided fields
                if (request.Name != null) course.Name = request.Name;
                if (request.Description != null) course.Description = request.Description;
                if (request.CourseCategoryId.HasValue) course.CourseCategoryId = request.CourseCategoryId.Value;
                if (request.Price.HasValue) course.Price = request.Price.Value;
                if (request.SessionsCount.HasValue) course.SessionsCount = request.SessionsCount.Value;
                if (request.MaxStudents.HasValue) course.MaxStudents = request.MaxStudents.Value;
                if (request.StartDate.HasValue) course.StartDate = request.StartDate.Value;
                if (request.EndDate.HasValue) course.EndDate = request.EndDate.Value;
                if (request.Status.HasValue) course.Status = request.Status.Value;
                if (request.BranchId.HasValue) course.BranchId = request.BranchId.Value;
                if (request.InstructorId.HasValue) course.InstructorId = request.InstructorId;
                if (request.RoomId.HasValue) course.RoomId = request.RoomId;
                if (request.LabId.HasValue) course.LabId = request.LabId;
                if (request.Content != null) course.Content = request.Content;
                if (request.Prerequisites != null) course.Prerequisites = request.Prerequisites;
                if (request.CourseDays != null) course.CourseDays = request.CourseDays;
                if (request.StartTime != null) course.StartTime = request.StartTime;
                if (request.EndTime != null) course.EndTime = request.EndTime;
                if (request.Notes != null) course.Notes = request.Notes;
                if (request.DriveLink != null) course.DriveLink = request.DriveLink;
                if (request.IsActive.HasValue) course.IsActive = request.IsActive.Value;

                // Update schedule string if any schedule-related field is updated
                if (request.CourseDays != null || request.StartTime != null || request.EndTime != null)
                {
                    course.Schedule = $"{course.CourseDays}، {course.StartTime}-{course.EndTime}";
                }

                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "تم تحديث الكورس بنجاح" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "حدث خطأ في الخادم", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "AdminOrEmployee")]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            try
            {
                var course = await _context.Courses
                    .Include(c => c.CourseRegistrations)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (course == null)
                    return NotFound(new { success = false, message = "الكورس غير موجود" });

                if (course.CourseRegistrations.Any())
                    return BadRequest(new { success = false, message = "لا يمكن حذف الكورس لوجود طلاب مسجلين به" });

                _context.Courses.Remove(course);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "تم حذف الكورس بنجاح" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "حدث خطأ في الخادم", error = ex.Message });
            }
        }

        [HttpGet("{id}/available-students")]
        [Authorize(Policy = "AdminOrEmployee")]
        public async Task<IActionResult> GetAvailableStudentsForCourse(int id)
        {
            try
            {
                var course = await _context.Courses.FindAsync(id);
                if (course == null)
                    return NotFound(new { success = false, message = "الكورس غير موجود" });

                // Get user's branch
                var userBranchId = int.Parse(User.FindFirst("BranchId")?.Value ?? "0");

                // Get students who are not already registered for this course
                var availableStudents = await _context.Students
                    .Where(s => s.IsActive && 
                               s.BranchId == userBranchId &&
                               !s.CourseRegistrations.Any(cr => cr.CourseId == id && cr.PaymentStatus != PaymentStatus.Cancelled))
                    .Select(s => new
                    {
                        s.Id,
                        s.FullName,
                        s.Phone,
                        s.Email,
                        s.Age,
                        s.Gender
                    })
                    .OrderBy(s => s.FullName)
                    .ToListAsync();

                return Ok(new { success = true, data = availableStudents });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "حدث خطأ في الخادم", error = ex.Message });
            }
        }

#if DEBUG
        [HttpPost("seed-test-data")]
        [Authorize(Policy = "AdminOrEmployee")]
        public async Task<IActionResult> SeedTestData()
        {
            try
            {
                // Check if test data already exists
                if (await _context.CourseRegistrations.AnyAsync())
                {
                    return Ok(new { success = true, message = "البيانات التجريبية موجودة بالفعل" });
                }

                var testCourse = await _context.Courses.FirstOrDefaultAsync();
                var testStudents = await _context.Students.Take(3).ToListAsync();

                if (testCourse == null || !testStudents.Any())
                {
                    return BadRequest(new { success = false, message = "لا توجد كورسات أو طلاب للاختبار" });
                }

                foreach (var student in testStudents)
                {
                    var registration = new CourseRegistration
                    {
                        CourseId = testCourse.Id,
                        StudentId = student.Id,
                        RegistrationDate = DateTime.UtcNow.AddDays(-10),
                        TotalAmount = testCourse.Price,
                        PaidAmount = testCourse.Price * 0.7m,
                        PaymentStatus = PaymentStatus.PartiallyPaid,
                        PaymentMethod = PaymentMethod.Cash,
                        Notes = "تسجيل تجريبي"
                    };
                    _context.CourseRegistrations.Add(registration);
                }

                await _context.SaveChangesAsync();
                return Ok(new { success = true, message = "تم إضافة البيانات التجريبية بنجاح" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "حدث خطأ في الخادم", error = ex.Message });
            }
        }

        [HttpGet("available")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> GetAvailableCourses()
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

                if (user?.StudentId == null)
                    return BadRequest(new { success = false, message = "المستخدم ليس طالباً" });

                var student = await _context.Students.FirstOrDefaultAsync(s => s.Id == user.StudentId);
                if (student == null)
                    return NotFound(new { success = false, message = "بيانات الطالب غير موجودة" });

                // Get all active courses that the student is NOT already registered in
                var availableCourses = await _context.Courses
                    .Include(c => c.CourseCategory)
                    .Include(c => c.Branch)
                    .Include(c => c.Instructor)
                        .ThenInclude(i => i.User)
                    .Include(c => c.CourseRegistrations)
                    .Where(c => c.IsActive && 
                               !c.CourseRegistrations.Any(cr => cr.StudentId == student.Id && cr.PaymentStatus != PaymentStatus.Cancelled)) // Not already registered
                    .Select(c => new
                    {
                        c.Id,
                        c.Name,
                        c.Description,
                        CourseCategoryName = c.CourseCategory != null ? c.CourseCategory.Name : string.Empty,
                        c.Price,
                        c.SessionsCount,
                        c.MaxStudents,
                        CurrentStudents = c.CourseRegistrations.Count(cr => cr.PaymentStatus != PaymentStatus.Cancelled),
                        AvailableSeats = c.MaxStudents - c.CourseRegistrations.Count(cr => cr.PaymentStatus != PaymentStatus.Cancelled),
                        c.StartDate,
                        c.EndDate,
                        c.Status,
                        BranchName = c.Branch != null ? c.Branch.Name : string.Empty,
                        InstructorName = c.Instructor != null && c.Instructor.User != null ? c.Instructor.User.FullName : string.Empty,
                        c.Schedule,
                        c.Content,
                        c.Prerequisites,
                        c.CourseDays,
                        c.StartTime,
                        c.EndTime,
                        c.CreatedAt
                    })
                    .OrderByDescending(c => c.CreatedAt)
                    .ToListAsync();

                return Ok(new { 
                    success = true, 
                    message = availableCourses.Count > 0 ? "تم جلب الكورسات المتاحة بنجاح" : "لا توجد كورسات متاحة للتسجيل",
                    data = availableCourses 
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "حدث خطأ في الخادم", error = ex.Message });
            }
        }

        [HttpGet("categories")]
        [Authorize(Policy = "AdminOrEmployee")]
        public async Task<IActionResult> GetCourseCategories()
        {
            try
            {
                var categories = await _context.CourseCategories
                    .Where(cc => cc.IsActive)
                    .OrderBy(cc => cc.MinAge)
                    .Select(cc => new
                    {
                        cc.Id,
                        cc.Name,
                        cc.Description,
                        cc.MinAge,
                        cc.MaxAge
                    })
                    .ToListAsync();

                return Ok(new { success = true, data = categories });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "حدث خطأ في الخادم", error = ex.Message });
            }
        }


        [HttpPost("update-trainer-names")]
        [Authorize(Policy = "AdminOrEmployee")]
        public async Task<IActionResult> UpdateTrainerNames()
        {
            try
            {
                await Scripts.UpdateTrainerName.UpdateTrainerNames(_context);
                return Ok(new { success = true, message = "تم تحديث أسماء المدربين بنجاح" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "حدث خطأ في تحديث أسماء المدربين", error = ex.Message });
            }
        }

        [HttpGet("test-trainers")]
        [Authorize(Policy = "AdminOrEmployee")]
        public async Task<IActionResult> TestTrainers()
        {
            try
            {
                await Scripts.TestTrainers.TestCurrentTrainers(_context);
                return Ok(new { success = true, message = "تم اختبار المدربين بنجاح" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "حدث خطأ في اختبار المدربين", error = ex.Message });
            }
        }
#endif
    }
}
