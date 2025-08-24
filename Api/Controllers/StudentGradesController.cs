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
    [Authorize]
    public class StudentGradesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public StudentGradesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/studentgrades
        [HttpGet]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> GetGrades([FromQuery] int? studentId = null, [FromQuery] int? courseId = null)
        {
            try
            {
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
                        StudentName = sg.Student.FullName,
                        sg.CourseId,
                        CourseName = sg.Course.Name,
                        sg.Grade,
                        sg.Notes,
                        sg.CreatedAt,
                        sg.UpdatedAt,
                        CreatedBy = sg.CreatedByUser.FullName,
                        UpdatedBy = sg.UpdatedByUser != null ? sg.UpdatedByUser.FullName : null
                    })
                    .ToListAsync();

                return Ok(new { success = true, data = grades });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "حدث خطأ في الخادم", error = ex.Message });
            }
        }

        // GET: api/studentgrades/all
        [HttpGet("all")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> GetAllGrades()
        {
            try
            {
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
                        StudentName = sg.Student.FullName,
                        sg.CourseId,
                        CourseName = sg.Course.Name,
                        sg.Grade,
                        sg.Notes,
                        sg.CreatedAt,
                        sg.UpdatedAt,
                        CreatedBy = sg.CreatedByUser.FullName,
                        UpdatedBy = sg.UpdatedByUser != null ? sg.UpdatedByUser.FullName : null
                    })
                    .ToListAsync();

                return Ok(grades);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "حدث خطأ في الخادم", error = ex.Message });
            }
        }

        // GET: api/studentgrades/student/{studentId}
        [HttpGet("student/{studentId}")]
        public async Task<IActionResult> GetStudentGrades(int studentId)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var userRole = User.FindFirst("Role")?.Value;

                // للطلاب: يمكنهم فقط رؤية درجاتهم
                if (userRole == "Student")
                {
                    var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == userId);
                    if (student == null || student.Id != studentId)
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

                return Ok(new { success = true, data = grades });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "حدث خطأ في الخادم", error = ex.Message });
            }
        }

        // GET: api/studentgrades/{id}
        [HttpGet("{id}")]
        [Authorize(Policy = "AdminOnly")]
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
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> CreateGrade([FromBody] CreateGradeRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { success = false, message = "بيانات غير صحيحة" });
                }

                // التحقق من وجود الطالب والكورس
                var student = await _context.Students.FindAsync(request.StudentId);
                if (student == null)
                {
                    return BadRequest(new { success = false, message = "الطالب غير موجود" });
                }

                var course = await _context.Courses.FindAsync(request.CourseId);
                if (course == null)
                {
                    return BadRequest(new { success = false, message = "الكورس غير موجود" });
                }

                // التحقق من عدم وجود درجة سابقة للطالب في نفس الكورس
                var existingGrade = await _context.StudentGrades
                    .FirstOrDefaultAsync(sg => sg.StudentId == request.StudentId && sg.CourseId == request.CourseId);

                if (existingGrade != null)
                {
                    return BadRequest(new { success = false, message = "يوجد درجة سابقة للطالب في هذا الكورس" });
                }

                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

                var grade = new StudentGrade
                {
                    StudentId = request.StudentId,
                    CourseId = request.CourseId,
                    Grade = request.Grade,
                    Notes = request.Notes ?? string.Empty,
                    CreatedByUserId = userId,
                    UpdatedByUserId = userId
                };

                _context.StudentGrades.Add(grade);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "تم إضافة الدرجة بنجاح", data = new { grade.Id } });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "حدث خطأ في الخادم", error = ex.Message });
            }
        }

        // PUT: api/studentgrades/{id}
        [HttpPut("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> UpdateGrade(int id, [FromBody] UpdateGradeRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { success = false, message = "بيانات غير صحيحة" });
                }

                var grade = await _context.StudentGrades.FindAsync(id);
                if (grade == null)
                {
                    return NotFound(new { success = false, message = "الدرجة غير موجودة" });
                }

                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

                grade.Grade = request.Grade;
                grade.Notes = request.Notes ?? string.Empty;
                grade.UpdatedAt = DateTime.UtcNow;
                grade.UpdatedByUserId = userId;

                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "تم تحديث الدرجة بنجاح" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "حدث خطأ في الخادم", error = ex.Message });
            }
        }

        // DELETE: api/studentgrades/{id}
        [HttpDelete("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> DeleteGrade(int id)
        {
            try
            {
                var grade = await _context.StudentGrades.FindAsync(id);
                if (grade == null)
                {
                    return NotFound(new { success = false, message = "الدرجة غير موجودة" });
                }

                _context.StudentGrades.Remove(grade);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "تم حذف الدرجة بنجاح" });
            }
            catch (Exception ex)
            {
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
