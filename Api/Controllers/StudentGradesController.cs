using Api.Data;
using Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    
    public class StudentGradesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public StudentGradesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/studentgrades
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetGrades([FromQuery] int? studentId = null, [FromQuery] int? courseId = null)
        {
            try
            {
                Console.WriteLine("[StudentGradesController] GET /api/studentgrades called");
                Console.WriteLine($"Query params => studentId: {studentId}, courseId: {courseId}");
                var query = _context.StudentGrades
                    .Include(sg => sg.Student)
                    .Include(sg => sg.Course)
                    .Include(sg => sg.CreatedByUser)
                    .Include(sg => sg.UpdatedByUser)
                    .AsQueryable();

                if (studentId.HasValue)
                    query = query.Where(sg => sg.StudentId == studentId.Value);

                if (courseId.HasValue)
                    query = query.Where(sg => sg.CourseId == courseId.Value);

                var grades = await query
                    .OrderByDescending(sg => sg.UpdatedAt)
                    .Select(sg => new
                    {
                        sg.Id,
                        sg.StudentId,
                        StudentName = sg.Student != null ? sg.Student.FullName : null,
                        sg.CourseId,
                        CourseName = sg.Course != null ? sg.Course.Name : null,
                        sg.Grade,
                        sg.Notes,
                        sg.CreatedAt,
                        sg.UpdatedAt,
                        CreatedBy = sg.CreatedByUser != null ? sg.CreatedByUser.FullName : null,
                        UpdatedBy = sg.UpdatedByUser != null ? sg.UpdatedByUser.FullName : null
                    })
                    .ToListAsync();

                Console.WriteLine($"[StudentGradesController] GET list count: {grades.Count}");
                return Ok(new { success = true, data = grades });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[StudentGradesController] Error in GetGrades: {ex.Message}\n{ex.StackTrace}");
                return StatusCode(500, new { success = false, message = "حدث خطأ في الخادم", error = ex.Message });
            }
        }

        // GET: api/studentgrades/all
        [HttpGet("all")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllGrades()
        {
            try
            {
                Console.WriteLine("[StudentGradesController] GET /api/studentgrades/all called");
                var grades = await _context.StudentGrades
                    .Include(sg => sg.Student)
                    .Include(sg => sg.Course)
                    .Include(sg => sg.CreatedByUser)
                    .Include(sg => sg.UpdatedByUser)
                    .OrderByDescending(sg => sg.UpdatedAt)
                    .Select(sg => new
                    {
                        sg.Id,
                        sg.StudentId,
                        StudentName = sg.Student != null ? sg.Student.FullName : null,
                        sg.CourseId,
                        CourseName = sg.Course != null ? sg.Course.Name : null,
                        sg.Grade,
                        sg.Notes,
                        sg.CreatedAt,
                        sg.UpdatedAt,
                        CreatedBy = sg.CreatedByUser != null ? sg.CreatedByUser.FullName : null,
                        UpdatedBy = sg.UpdatedByUser != null ? sg.UpdatedByUser.FullName : null
                    })
                    .ToListAsync();

                Console.WriteLine($"[StudentGradesController] GET all count: {grades.Count}");
                return Ok(new { success = true, data = grades });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[StudentGradesController] Error in GetAllGrades: {ex.Message}\n{ex.StackTrace}");
                return StatusCode(500, new { success = false, message = "حدث خطأ في الخادم", error = ex.Message });
            }
        }

        // GET: api/studentgrades/student/{studentId}
        [HttpGet("student/{studentId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetStudentGrades(int studentId)
        {
            try
            {
                Console.WriteLine($"[StudentGradesController] GET /api/studentgrades/student/{studentId} called");
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var userRole = User.FindFirst("Role")?.Value;

                // للطلاب: يمكنهم فقط رؤية درجاتهم
                if (userRole == "Student")
                {
                    var student = await _context.Students.FirstOrDefaultAsync(s => s.Id == studentId);
                    if (student == null)
                    {
                        return Forbid();
                    }
                    
                    // التحقق من أن الطالب مرتبط بالمستخدم الحالي
                    var user = await _context.Users.FirstOrDefaultAsync(u => u.StudentId == studentId);
                    if (user == null || user.Id != userId)
                    {
                        return Forbid();
                    }
                }

                var grades = await _context.StudentGrades
                    .Include(sg => sg.Course)
                    .Where(sg => sg.StudentId == studentId)
                    .OrderByDescending(sg => sg.UpdatedAt)
                    .Select(sg => new
                    {
                        sg.Id,
                        sg.CourseId,
                        CourseName = sg.Course.Name,
                        sg.Grade,
                        sg.Notes,
                        sg.UpdatedAt
                    })
                    .ToListAsync();

                Console.WriteLine($"[StudentGradesController] student {studentId} grades count: {grades.Count}");
                return Ok(new { success = true, data = grades });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[StudentGradesController] Error in GetStudentGrades: {ex.Message}\n{ex.StackTrace}");
                return StatusCode(500, new { success = false, message = "حدث خطأ في الخادم", error = ex.Message });
            }
        }

        // GET: api/studentgrades/{id}
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetGrade(int id)
        {
            try
            {
                var grade = await _context.StudentGrades
                    .Include(sg => sg.Student)
                    .Include(sg => sg.Course)
                    .Include(sg => sg.CreatedByUser)
                    .Include(sg => sg.UpdatedByUser)
                    .FirstOrDefaultAsync(sg => sg.Id == id);

                if (grade == null)
                {
                    return NotFound(new { success = false, message = "الدرجة غير موجودة" });
                }

                var result = new
                {
                    grade.Id,
                    grade.StudentId,
                    StudentName = grade.Student.FullName,
                    grade.CourseId,
                    CourseName = grade.Course.Name,
                    grade.Grade,
                    grade.Notes,
                    grade.CreatedAt,
                    grade.UpdatedAt,
                    CreatedBy = grade.CreatedByUser.FullName,
                    UpdatedBy = grade.UpdatedByUser != null ? grade.UpdatedByUser.FullName : null
                };

                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "حدث خطأ في الخادم", error = ex.Message });
            }
        }

        // POST: api/studentgrades
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> CreateGrade([FromBody] CreateGradeRequest request)
        {
            try
            {
                Console.WriteLine($"=== CreateGrade Request ===");
                Console.WriteLine($"Request: {System.Text.Json.JsonSerializer.Serialize(request)}");
                Console.WriteLine($"ModelState.IsValid: {ModelState.IsValid}");
                
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    Console.WriteLine($"ModelState errors: {string.Join(", ", errors)}");
                    return BadRequest(new { success = false, message = "بيانات غير صحيحة", errors = errors });
                }

                // التحقق من وجود الطالب والكورس
                Console.WriteLine($"Checking student with ID: {request.StudentId}");
                var student = await _context.Students.FindAsync(request.StudentId);
                if (student == null)
                {
                    Console.WriteLine($"Student not found with ID: {request.StudentId}");
                    return BadRequest(new { success = false, message = "الطالب غير موجود" });
                }
                Console.WriteLine($"Student found: {student.FullName}");

                Console.WriteLine($"Checking course with ID: {request.CourseId}");
                var course = await _context.Courses.FindAsync(request.CourseId);
                if (course == null)
                {
                    Console.WriteLine($"Course not found with ID: {request.CourseId}");
                    return BadRequest(new { success = false, message = "الكورس غير موجود" });
                }
                Console.WriteLine($"Course found: {course.Name}");

                // التحقق من عدم وجود درجة سابقة للطالب في نفس الكورس
                var existingGrade = await _context.StudentGrades
                    .FirstOrDefaultAsync(sg => sg.StudentId == request.StudentId && sg.CourseId == request.CourseId);

                if (existingGrade != null)
                {
                    Console.WriteLine($"Existing grade found for student {request.StudentId} in course {request.CourseId}");
                    return BadRequest(new { success = false, message = "يوجد درجة سابقة للطالب في هذا الكورس" });
                }

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                Console.WriteLine($"User ID claim: {userIdClaim}");
                
                int userId;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out userId) || userId <= 0 || !await _context.Users.AnyAsync(u => u.Id == userId))
                {
                    // Auth may be disabled in debug: pick a safe existing user to satisfy FK
                    userId = await _context.Users.OrderBy(u => u.Id).Select(u => u.Id).FirstOrDefaultAsync();
                    if (userId <= 0)
                    {
                        return StatusCode(500, new { success = false, message = "لا يوجد مستخدم صالح لتنفيذ العملية. الرجاء إنشاء مستخدم واحد على الأقل." });
                    }
                }

                Console.WriteLine($"Creating grade for user ID: {userId}");
                var grade = new StudentGrade
                {
                    StudentId = request.StudentId,
                    CourseId = request.CourseId,
                    Grade = request.Grade,
                    Notes = request.Notes ?? string.Empty,
                    CreatedByUserId = userId,
                    UpdatedByUserId = userId
                };

                Console.WriteLine($"Adding grade to context...");
                _context.StudentGrades.Add(grade);
                
                Console.WriteLine($"Saving changes...");
                await _context.SaveChangesAsync();
                Console.WriteLine($"Grade saved successfully with ID: {grade.Id}");

                return Ok(new { success = true, message = "تم إضافة الدرجة بنجاح", data = new { grade.Id } });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in CreateGrade: {ex.Message}\n{ex.StackTrace}");
                return StatusCode(500, new { success = false, message = "حدث خطأ في إضافة الدرجة", error = ex.Message });
            }
        }

        // PUT: api/studentgrades/{id}
        [HttpPut("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> UpdateGrade(int id, [FromBody] UpdateGradeRequest request)
        {
            try
            {
                Console.WriteLine($"[StudentGradesController] PUT /api/studentgrades/{id} called");
                Console.WriteLine($"Payload: {System.Text.Json.JsonSerializer.Serialize(request)}");
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { success = false, message = "بيانات غير صحيحة" });
                }

                var grade = await _context.StudentGrades.FindAsync(id);
                if (grade == null)
                {
                    return NotFound(new { success = false, message = "الدرجة غير موجودة" });
                }

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                int userId;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out userId) || userId <= 0 || !await _context.Users.AnyAsync(u => u.Id == userId))
                {
                    userId = await _context.Users.OrderBy(u => u.Id).Select(u => u.Id).FirstOrDefaultAsync();
                    if (userId <= 0)
                    {
                        return StatusCode(500, new { success = false, message = "لا يوجد مستخدم صالح لتنفيذ العملية. الرجاء إنشاء مستخدم واحد على الأقل." });
                    }
                }

                grade.Grade = request.Grade;
                grade.Notes = request.Notes ?? string.Empty;
                grade.UpdatedAt = DateTime.UtcNow;
                grade.UpdatedByUserId = userId;

                await _context.SaveChangesAsync();
                Console.WriteLine($"[StudentGradesController] Grade {id} updated successfully");

                return Ok(new { success = true, message = "تم تحديث الدرجة بنجاح" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[StudentGradesController] Error in UpdateGrade: {ex.Message}\n{ex.StackTrace}");
                return StatusCode(500, new { success = false, message = "حدث خطأ في الخادم", error = ex.Message });
            }
        }

        // DELETE: api/studentgrades/{id}
        [HttpDelete("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> DeleteGrade(int id)
        {
            try
            {
                Console.WriteLine($"[StudentGradesController] DELETE /api/studentgrades/{id} called");
                var grade = await _context.StudentGrades.FindAsync(id);
                if (grade == null)
                {
                    return NotFound(new { success = false, message = "الدرجة غير موجودة" });
                }

                _context.StudentGrades.Remove(grade);
                await _context.SaveChangesAsync();

                Console.WriteLine($"[StudentGradesController] Grade {id} deleted successfully");
                return Ok(new { success = true, message = "تم حذف الدرجة بنجاح" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[StudentGradesController] Error in DeleteGrade: {ex.Message}\n{ex.StackTrace}");
                return StatusCode(500, new { success = false, message = "حدث خطأ في الخادم", error = ex.Message });
            }
        }
    }

    public class CreateGradeRequest
    {
        public int StudentId { get; set; }
        public int CourseId { get; set; }
        public decimal Grade { get; set; }
        public string? Notes { get; set; }
    }

    public class UpdateGradeRequest
    {
        public decimal Grade { get; set; }
        public string? Notes { get; set; }
    }
}
