using Api.Data;
using Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text;
using Api.Services;
using BCryptNet = BCrypt.Net.BCrypt;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AttendanceController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AttendanceController> _logger;
        private readonly IPasswordGeneratorService _passwordGenerator;

        public AttendanceController(ApplicationDbContext context, ILogger<AttendanceController> logger, IPasswordGeneratorService passwordGenerator)
        {
            _context = context;
            _logger = logger;
            _passwordGenerator = passwordGenerator;
        }

        [HttpGet("courses/{courseId}")]
        public async Task<IActionResult> GetCourseAttendance(int courseId, [FromQuery] DateTime? sessionDate = null)
        {
            try
            {
                var course = await _context.Courses
                    .Include(c => c.CourseRegistrations)
                        .ThenInclude(cr => cr.Student)
                    .FirstOrDefaultAsync(c => c.Id == courseId);

                if (course == null)
                    return NotFound(new { message = "الكورس غير موجود" });

                var query = _context.Attendances
                    .Where(a => a.CourseId == courseId);

                if (sessionDate.HasValue)
                {
                    var startOfDay = sessionDate.Value.Date;
                    var endOfDay = startOfDay.AddDays(1);
                    query = query.Where(a => a.SessionDate >= startOfDay && a.SessionDate < endOfDay);
                }

                var attendanceRecordsRaw = await query
                    .OrderByDescending(a => a.SessionDate)
                    .ThenBy(a => a.StudentId)
                    .Select(a => new
                    {
                        a.Id,
                        a.SessionDate,
                        Status = a.Status.ToString(),
                        a.CheckInTime,
                        a.CheckOutTime,
                        a.Notes,
                        StudentId = a.StudentId
                    })
                    .ToListAsync();

                var attendanceRecords = attendanceRecordsRaw
                    .Select(a => new
                    {
                        a.Id,
                        a.SessionDate,
                        a.Status,
                        StatusArabic = GetAttendanceStatusArabic(Enum.Parse<AttendanceStatus>(a.Status)),
                        a.CheckInTime,
                        a.CheckOutTime,
                        a.Notes,
                        a.StudentId
                    })
                    .ToList();

                // Get registered students for the course
                var registeredStudents = course.CourseRegistrations
                    .Where(cr => cr.PaymentStatus != PaymentStatus.Cancelled)
                    .Select(cr => new
                    {
                        Id = cr.Student.Id,
                        FullName = cr.Student.FullName,
                        Phone = cr.Student.Phone,
                        RegistrationDate = cr.RegistrationDate
                    })
                    .ToList();

                // تحقق من اكتمال بيانات الطلاب
                foreach (var s in registeredStudents)
                {
                    if (s.Id == 0 || string.IsNullOrWhiteSpace(s.FullName))
                    {
                        _logger.LogWarning("[AttendanceController] Student with incomplete data found: Id={StudentId}, FullName={StudentFullName}", s.Id, s.FullName);
                    }
                }

                return Ok(new
                {
                    Course = new { course.Id, course.Name },
                    RegisteredStudents = registeredStudents,
                    AttendanceRecords = attendanceRecords
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting course attendance for course {CourseId}", courseId);
                return StatusCode(500, new {
                    message = "حدث خطأ في الخادم",
                    error = ex.Message,
                });
            }
        }

        [HttpPost("courses/{courseId}/session")]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> CreateAttendanceSession(int courseId, [FromBody] CreateAttendanceSessionRequest request)
        {
            try
            {
                var course = await _context.Courses
                    .Include(c => c.CourseRegistrations)
                        .ThenInclude(cr => cr.Student)
                    .FirstOrDefaultAsync(c => c.Id == courseId);

                if (course == null)
                    return NotFound(new { message = "الكورس غير موجود" });

                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

                // Check if session already exists for this date
                var existingSession = await _context.Attendances
                    .AnyAsync(a => a.CourseId == courseId && 
                                  a.SessionDate.Date == request.SessionDate.Date);

                if (existingSession)
                    return BadRequest(new { message = "جلسة الحضور موجودة بالفعل لهذا التاريخ" });

                var attendanceRecords = new List<Attendance>();

                // Create attendance records for all registered students
                var registeredStudents = course.CourseRegistrations
                    .Where(cr => cr.PaymentStatus != PaymentStatus.Cancelled)
                    .Select(cr => cr.Student)
                    .ToList();

                foreach (var student in registeredStudents)
                {
                    var studentAttendance = request.StudentAttendances
                        .FirstOrDefault(sa => sa.StudentId == student.Id);

                    var attendance = new Attendance
                    {
                        StudentId = student.Id,
                        CourseId = courseId,
                        AttendanceDate = request.SessionDate,
                        SessionDate = request.SessionDate,
                        Status = studentAttendance?.Status ?? AttendanceStatus.Absent,
                        CheckInTime = studentAttendance?.CheckInTime?.TimeOfDay,
                        CheckOutTime = studentAttendance?.CheckOutTime?.TimeOfDay,
                        Notes = studentAttendance?.Notes ?? string.Empty,
                        RecordedByUserId = userId,
                        CreatedAt = DateTime.UtcNow
                    };

                    attendanceRecords.Add(attendance);

                    // --- منطق الدفع عند السيشن الثانية ---
                    // احسب عدد حضور الطالب في هذا الكورس (قبل إضافة الحضور الحالي)
                    var previousAttendances = _context.Attendances
                        .Count(a => a.StudentId == student.Id && a.CourseId == courseId);

                    // ابحث عن تسجيل الكورس لهذا الطالب
                    var courseRegistration = course.CourseRegistrations
                        .FirstOrDefault(cr => cr.StudentId == student.Id);

                    if (courseRegistration != null && previousAttendances == 1 && courseRegistration.PaymentStatus != PaymentStatus.FullyPaid)
                    {
                        // 1. تحديث حالة الدفع
                        courseRegistration.PaidAmount = course.Price;
                        courseRegistration.PaymentStatus = PaymentStatus.FullyPaid;
                        courseRegistration.PaymentDate = DateTime.UtcNow;
                        courseRegistration.PaymentMethod = PaymentMethod.Cash; // أو حسب المطلوب

                        // 2. إنشاء سجل دفع
                        var payment = new Payment
                        {
                            CourseRegistrationId = courseRegistration.Id,
                            Amount = course.Price,
                            PaymentMethod = PaymentMethod.Cash, // أو حسب المطلوب
                            PaymentDate = DateTime.UtcNow,
                            ProcessedByUserId = userId,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow,
                            Notes = $"دفع تلقائي عند حضور السيشن الثانية للطالب {student.FullName}"
                        };
                        _context.Payments.Add(payment);

                        // 3. إنشاء مصروف (Expense) بقيمة الكورس
                        var expense = new Expense
                        {
                            Title = $"مصروف كورس {course.Name} - الطالب {student.FullName}",
                            Description = $"دفع الطالب {student.FullName} قيمة كورس {course.Name}",
                            Category = ExpenseCategory.Training,
                            Amount = course.Price,
                            ExpenseDate = DateTime.UtcNow,
                            Priority = ExpensePriority.Medium,
                            Vendor = string.Empty,
                            PaymentMethod = PaymentMethod.Cash,
                            ReceiptNumber = string.Empty,
                            Notes = $"دفع تلقائي عند حضور السيشن الثانية للطالب {student.FullName}",
                            IsRecurring = false,
                            BranchId = course.BranchId ?? 1,
                            RequestedByUserId = userId,
                            Status = ExpenseStatus.Paid,
                            CreatedAt = DateTime.UtcNow
                        };
                        _context.Expenses.Add(expense);

                        // 4. إنشاء حساب مستخدم للطالب إذا لم يكن لديه
                        if (student.UserId == null || student.UserId == 0)
                        {
                            // توليد اسم مستخدم وكلمة مرور عشوائية
                            var username = _passwordGenerator.GenerateUsername(student.FullName);
                            var password = _passwordGenerator.GenerateRandomPassword();
                            var passwordHash = BCryptNet.HashPassword(password); // Use BCrypt for consistency and security
                            var email = string.IsNullOrWhiteSpace(student.Email) ? $"student{student.Id}@company.com" : student.Email;

                            var newUser = new User
                            {
                                FullName = student.FullName,
                                Username = username,
                                Email = email,
                                Phone = student.Phone,

                                PasswordHash = passwordHash,
                                Role = UserRole.Student,
                                UserRole = UserRole.Student,
                                BranchId = student.BranchId,
                                IsActive = true,
                                CreatedAt = DateTime.UtcNow
                            };
                            _context.Users.Add(newUser);
                            _context.SaveChanges(); // لحفظ الـ Id

                            student.UserId = newUser.Id;
                            _context.SaveChanges();

                            // TODO: إرسال بيانات الدخول (username/password) للطالب عبر الإيميل أو SMS
                        }
                    }
                }

                _context.Attendances.AddRange(attendanceRecords);
                await _context.SaveChangesAsync();

                return Ok(new { message = "تم تسجيل الحضور بنجاح", recordsCount = attendanceRecords.Count });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "حدث خطأ في الخادم", error = ex.Message });
            }
        }

        [HttpPut("courses/{courseId}/session")]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> UpdateAttendanceSession(int courseId, [FromBody] UpdateAttendanceSessionRequest request)
        {
            try
            {
                var course = await _context.Courses.FindAsync(courseId);
                if (course == null)
                    return NotFound(new { message = "الكورس غير موجود" });

                // Get existing attendance records for the session date
                var startOfDay = request.SessionDate.Date;
                var endOfDay = startOfDay.AddDays(1);

                var existingRecords = await _context.Attendances
                    .Where(a => a.CourseId == courseId && 
                               a.SessionDate >= startOfDay && 
                               a.SessionDate < endOfDay)
                    .ToListAsync();

                if (!existingRecords.Any())
                    return NotFound(new { message = "جلسة الحضور غير موجودة لهذا التاريخ" });

                // Update attendance records
                foreach (var studentAttendance in request.StudentAttendances)
                {
                    var existingRecord = existingRecords
                        .FirstOrDefault(er => er.StudentId == studentAttendance.StudentId);

                    if (existingRecord != null)
                    {
                        existingRecord.Status = studentAttendance.Status;
                        existingRecord.CheckInTime = studentAttendance.CheckInTime?.TimeOfDay;
                        existingRecord.CheckOutTime = studentAttendance.CheckOutTime?.TimeOfDay;
                        existingRecord.Notes = studentAttendance.Notes ?? string.Empty;
                        // Remove UpdatedAt as it doesn't exist in the model
                    }
                }

                await _context.SaveChangesAsync();

                return Ok(new { message = "تم تحديث الحضور بنجاح" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "حدث خطأ في الخادم", error = ex.Message });
            }
        }

        [HttpGet("students/{studentId}")]
        public async Task<IActionResult> GetStudentAttendance(int studentId, [FromQuery] int? courseId = null, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var student = await _context.Students.FindAsync(studentId);
                if (student == null)
                    return NotFound(new { message = "الطالب غير موجود" });

                var query = _context.Attendances
                    .Include(a => a.Course)
                    .Where(a => a.StudentId == studentId);

                if (courseId.HasValue)
                    query = query.Where(a => a.CourseId == courseId.Value);

                if (startDate.HasValue)
                    query = query.Where(a => a.SessionDate >= startDate.Value);

                if (endDate.HasValue)
                    query = query.Where(a => a.SessionDate <= endDate.Value);

                // Get raw data for statistics first
                var rawAttendanceData = await query.ToListAsync();

                var attendanceRecords = rawAttendanceData
                    .OrderByDescending(a => a.SessionDate)
                    .Select(a => new
                    {
                        a.Id,
                        a.SessionDate,
                        Status = a.Status.ToString(),
                        StatusArabic = GetAttendanceStatusArabic(a.Status),
                        a.CheckInTime,
                        a.CheckOutTime,
                        a.Notes,
                        Course = new
                        {
                            a.Course.Id,
                            a.Course.Name
                        }
                    })
                    .ToList();

                var statistics = new
                {
                    TotalSessions = rawAttendanceData.Count,
                    PresentSessions = rawAttendanceData.Count(a => a.Status == AttendanceStatus.Present),
                    AbsentSessions = rawAttendanceData.Count(a => a.Status == AttendanceStatus.Absent),
                    LateSessions = rawAttendanceData.Count(a => a.Status == AttendanceStatus.Late),
                    AttendanceRate = rawAttendanceData.Any()
                        ? Math.Round((double)rawAttendanceData.Count(a => a.Status == AttendanceStatus.Present || a.Status == AttendanceStatus.Late) / rawAttendanceData.Count * 100, 2)
                        : 0
                };

                return Ok(new
                {
                    Student = new { student.Id, student.FullName },
                    Statistics = statistics,
                    AttendanceRecords = attendanceRecords
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "حدث خطأ في الخادم", error = ex.Message });
            }
        }

        [HttpGet("reports/course/{courseId}")]
        public async Task<IActionResult> GetCourseAttendanceReport(int courseId, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var course = await _context.Courses
                    .Include(c => c.CourseRegistrations)
                        .ThenInclude(cr => cr.Student)
                    .FirstOrDefaultAsync(c => c.Id == courseId);

                if (course == null)
                    return NotFound(new { message = "الكورس غير موجود" });

                var query = _context.Attendances
                    .Include(a => a.Student)
                    .Where(a => a.CourseId == courseId);

                if (startDate.HasValue)
                    query = query.Where(a => a.SessionDate >= startDate.Value);

                if (endDate.HasValue)
                    query = query.Where(a => a.SessionDate <= endDate.Value);

                var attendanceData = await query.ToListAsync();

                // Group by student
                var studentReports = course.CourseRegistrations
                    .Where(cr => cr.PaymentStatus != PaymentStatus.Cancelled)
                    .Select(cr => cr.Student)
                    .Select(student => new
                    {
                        Student = new { student.Id, student.FullName, student.Phone },
                        TotalSessions = attendanceData.Count(a => a.StudentId == student.Id),
                        PresentSessions = attendanceData.Count(a => a.StudentId == student.Id && a.Status == AttendanceStatus.Present),
                        AbsentSessions = attendanceData.Count(a => a.StudentId == student.Id && a.Status == AttendanceStatus.Absent),
                        LateSessions = attendanceData.Count(a => a.StudentId == student.Id && a.Status == AttendanceStatus.Late),
                        AttendanceRate = attendanceData.Any(a => a.StudentId == student.Id)
                            ? Math.Round((double)attendanceData.Count(a => a.StudentId == student.Id && (a.Status == AttendanceStatus.Present || a.Status == AttendanceStatus.Late)) / attendanceData.Count(a => a.StudentId == student.Id) * 100, 2)
                            : 0
                    })
                    .OrderByDescending(sr => sr.AttendanceRate)
                    .ToList();

                // Overall statistics
                var overallStats = new
                {
                    TotalStudents = studentReports.Count,
                    TotalSessions = attendanceData.Select(a => a.SessionDate.Date).Distinct().Count(),
                    AverageAttendanceRate = studentReports.Any() ? Math.Round(studentReports.Average(sr => sr.AttendanceRate), 2) : 0,
                    HighAttendanceStudents = studentReports.Count(sr => sr.AttendanceRate >= 80),
                    LowAttendanceStudents = studentReports.Count(sr => sr.AttendanceRate < 60)
                };

                return Ok(new
                {
                    Course = new { course.Id, course.Name },
                    OverallStatistics = overallStats,
                    StudentReports = studentReports
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "حدث خطأ في الخادم", error = ex.Message });
            }
        }

        [HttpDelete("{attendanceId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteAttendanceRecord(int attendanceId)
        {
            try
            {
                var attendance = await _context.Attendances.FindAsync(attendanceId);
                if (attendance == null)
                    return NotFound(new { message = "سجل الحضور غير موجود" });

                _context.Attendances.Remove(attendance);
                await _context.SaveChangesAsync();

                return Ok(new { message = "تم حذف سجل الحضور بنجاح" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "حدث خطأ في الخادم", error = ex.Message });
            }
        }

        private string GetAttendanceStatusArabic(AttendanceStatus status)
        {
            return status switch
            {
                AttendanceStatus.Present => "حاضر",
                AttendanceStatus.Absent => "غائب",
                AttendanceStatus.Late => "متأخر",
                AttendanceStatus.Excused => "معذور",
                _ => status.ToString()
            };
        }

    }

    public class CreateAttendanceSessionRequest
    {
        public DateTime SessionDate { get; set; }
        public List<StudentAttendanceRequest> StudentAttendances { get; set; } = new();
    }

    public class UpdateAttendanceSessionRequest
    {
        public DateTime SessionDate { get; set; }
        public List<StudentAttendanceRequest> StudentAttendances { get; set; } = new();
    }

    public class StudentAttendanceRequest
    {
        public int StudentId { get; set; }
        public AttendanceStatus Status { get; set; }
        public DateTime? CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
        public string Notes { get; set; } = string.Empty;
    }
}
