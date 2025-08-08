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
    [Authorize(Roles = "Admin,Employee")]
    public class ReportsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("financial")]
        public async Task<IActionResult> GetFinancialReport([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var userBranchId = int.Parse(User.FindFirst("BranchId")?.Value ?? "0");
                var branch = await _context.Branches.FindAsync(userBranchId);

                // Set default date range if not provided
                startDate ??= DateTime.UtcNow.AddMonths(-1);
                endDate ??= DateTime.UtcNow;

                // Course Revenue
                var courseRevenue = await _context.CourseRegistrations
                    .Include(cr => cr.Course)
                    .Where(cr => cr.Course.BranchId == userBranchId &&
                                cr.RegistrationDate >= startDate &&
                                cr.RegistrationDate <= endDate)
                    .SumAsync(cr => cr.PaidAmount);

                // Workspace Revenue (only for Assiut)
                decimal workspaceRevenue = 0;
                if (branch?.Name == "عسيوط")
                {
                    workspaceRevenue = await _context.WorkspaceBookings
                        .Where(wb => wb.BranchId == userBranchId &&
                                    wb.BookingDate >= startDate &&
                                    wb.BookingDate <= endDate)
                        .SumAsync(wb => wb.PaidAmount);
                }

                // Cafeteria Revenue
                var cafeteriaRevenue = await _context.CafeteriaOrders
                    .Where(co => co.BranchId == userBranchId &&
                                co.OrderDate >= startDate &&
                                co.OrderDate <= endDate)
                    .SumAsync(co => co.PaidAmount);

                // Expenses
                var expenses = await _context.Expenses
                    .Where(e => e.BranchId == userBranchId &&
                               e.ExpenseDate >= startDate &&
                               e.ExpenseDate <= endDate &&
                               e.Status == ExpenseStatus.Paid)
                    .SumAsync(e => e.Amount);

                // Equipment Damage Costs
                var equipmentDamageCosts = await _context.EquipmentReservations
                    .Include(er => er.Equipment)
                    .Where(er => er.Equipment.BranchId == userBranchId &&
                                er.ActualEndDateTime >= startDate &&
                                er.ActualEndDateTime <= endDate &&
                                er.DamageCost > 0)
                    .SumAsync(er => er.DamageCost);

                var totalRevenue = courseRevenue + workspaceRevenue + cafeteriaRevenue;
                var totalExpenses = expenses + equipmentDamageCosts;
                var netProfit = totalRevenue - totalExpenses;

                var report = new
                {
                    Period = new { StartDate = startDate, EndDate = endDate },
                    Branch = branch?.Name,
                    Revenue = new
                    {
                        CourseRevenue = courseRevenue,
                        WorkspaceRevenue = workspaceRevenue,
                        CafeteriaRevenue = cafeteriaRevenue,
                        TotalRevenue = totalRevenue
                    },
                    Expenses = new
                    {
                        OperationalExpenses = expenses,
                        EquipmentDamageCosts = equipmentDamageCosts,
                        TotalExpenses = totalExpenses
                    },
                    Profitability = new
                    {
                        NetProfit = netProfit,
                        ProfitMargin = totalRevenue > 0 ? Math.Round(((double)netProfit / (double)totalRevenue) * 100, 2) : 0
                    }
                };

                return Ok(report);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "حدث خطأ في الخادم", error = ex.Message });
            }
        }

        [HttpGet("courses")]
        public async Task<IActionResult> GetCoursesReport([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var userBranchId = int.Parse(User.FindFirst("BranchId")?.Value ?? "0");

                startDate ??= DateTime.UtcNow.AddMonths(-3);
                endDate ??= DateTime.UtcNow;

                var coursesData = await _context.Courses
                    .Include(c => c.CourseRegistrations)
                        .ThenInclude(cr => cr.Student)
                    .Include(c => c.Attendances)
                    .Where(c => c.BranchId == userBranchId &&
                               c.StartDate >= startDate &&
                               c.StartDate <= endDate)
                    .ToListAsync();

                var courseReports = coursesData.Select(course => new
                {
                    Course = new { course.Id, course.Name, course.StartDate, course.EndDate, course.Status },
                    Registrations = new
                    {
                        TotalRegistrations = course.CourseRegistrations.Count,
                        ActiveRegistrations = course.CourseRegistrations.Count(cr => cr.PaymentStatus != PaymentStatus.Cancelled),
                        FullyPaidRegistrations = course.CourseRegistrations.Count(cr => cr.PaymentStatus == PaymentStatus.FullyPaid),
                        Revenue = course.CourseRegistrations.Sum(cr => cr.PaidAmount),
                        OutstandingAmount = course.CourseRegistrations.Sum(cr => cr.RemainingAmount)
                    },
                    Attendance = new
                    {
                        TotalSessions = course.Attendances.Select(a => a.SessionDate.Date).Distinct().Count(),
                        AverageAttendanceRate = course.Attendances.Any() 
                            ? Math.Round((double)course.Attendances.Count(a => a.Status == AttendanceStatus.Present || a.Status == AttendanceStatus.Late) / course.Attendances.Count() * 100, 2)
                            : 0
                    }
                }).ToList();

                var summary = new
                {
                    TotalCourses = courseReports.Count,
                    ActiveCourses = courseReports.Count(cr => cr.Course.Status == CourseStatus.Active),
                    CompletedCourses = courseReports.Count(cr => cr.Course.Status == CourseStatus.Completed),
                    TotalStudents = courseReports.Sum(cr => cr.Registrations.ActiveRegistrations),
                    TotalRevenue = courseReports.Sum(cr => cr.Registrations.Revenue),
                    TotalOutstanding = courseReports.Sum(cr => cr.Registrations.OutstandingAmount),
                    AverageAttendanceRate = courseReports.Any() 
                        ? Math.Round(courseReports.Average(cr => cr.Attendance.AverageAttendanceRate), 2) 
                        : 0
                };

                return Ok(new
                {
                    Period = new { StartDate = startDate, EndDate = endDate },
                    Summary = summary,
                    CourseDetails = courseReports
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "حدث خطأ في الخادم", error = ex.Message });
            }
        }

        [HttpGet("students")]
        public async Task<IActionResult> GetStudentsReport([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var userBranchId = int.Parse(User.FindFirst("BranchId")?.Value ?? "0");

                startDate ??= DateTime.UtcNow.AddMonths(-6);
                endDate ??= DateTime.UtcNow;

                var studentsData = await _context.Students
                    .Include(s => s.CourseRegistrations)
                        .ThenInclude(cr => cr.Course)
                    .Include(s => s.Attendances)
                    .Include(s => s.WorkspaceBookings)
                    .Include(s => s.CafeteriaOrders)
                    .Where(s => s.BranchId == userBranchId &&
                               s.RegistrationDate >= startDate &&
                               s.RegistrationDate <= endDate)
                    .ToListAsync();

                var studentReports = studentsData.Select(student => new
                {
                    Student = new 
                    { 
                        student.Id, 
                        student.FullName, 
                        student.Age, 
                        student.Phone, 
                        student.RegistrationDate 
                    },
                    Courses = new
                    {
                        TotalCourses = student.CourseRegistrations.Count,
                        ActiveCourses = student.CourseRegistrations.Count(cr => cr.Course.Status == CourseStatus.Active),
                        CompletedCourses = student.CourseRegistrations.Count(cr => cr.Course.Status == CourseStatus.Completed),
                        TotalSpent = student.CourseRegistrations.Sum(cr => cr.PaidAmount),
                        OutstandingAmount = student.CourseRegistrations.Sum(cr => cr.RemainingAmount)
                    },
                    Attendance = new
                    {
                        TotalSessions = student.Attendances.Count,
                        PresentSessions = student.Attendances.Count(a => a.Status == AttendanceStatus.Present),
                        AttendanceRate = student.Attendances.Any() 
                            ? Math.Round((double)student.Attendances.Count(a => a.Status == AttendanceStatus.Present || a.Status == AttendanceStatus.Late) / student.Attendances.Count * 100, 2)
                            : 0
                    },
                    WorkspaceUsage = new
                    {
                        TotalBookings = student.WorkspaceBookings.Count,
                        TotalHours = student.WorkspaceBookings.Sum(wb => wb.HoursBooked),
                        TotalSpent = student.WorkspaceBookings.Sum(wb => wb.PaidAmount)
                    },
                    CafeteriaUsage = new
                    {
                        TotalOrders = student.CafeteriaOrders.Count,
                        TotalSpent = student.CafeteriaOrders.Sum(co => co.PaidAmount)
                    },
                    TotalLifetimeValue = student.CourseRegistrations.Sum(cr => cr.PaidAmount) +
                                       student.WorkspaceBookings.Sum(wb => wb.PaidAmount) +
                                       student.CafeteriaOrders.Sum(co => co.PaidAmount)
                }).ToList();

                var summary = new
                {
                    TotalStudents = studentReports.Count,
                    ActiveStudents = studentReports.Count(sr => sr.Courses.ActiveCourses > 0),
                    TotalRevenue = studentReports.Sum(sr => sr.TotalLifetimeValue),
                    AverageLifetimeValue = studentReports.Any() 
                        ? Math.Round(studentReports.Average(sr => sr.TotalLifetimeValue), 2) 
                        : 0,
                    AverageAttendanceRate = studentReports.Any() 
                        ? Math.Round(studentReports.Average(sr => sr.Attendance.AttendanceRate), 2) 
                        : 0,
                    TopStudentsByValue = studentReports
                        .OrderByDescending(sr => sr.TotalLifetimeValue)
                        .Take(10)
                        .Select(sr => new { sr.Student.FullName, sr.TotalLifetimeValue })
                        .ToList()
                };

                return Ok(new
                {
                    Period = new { StartDate = startDate, EndDate = endDate },
                    Summary = summary,
                    StudentDetails = studentReports.OrderByDescending(sr => sr.TotalLifetimeValue).ToList()
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "حدث خطأ في الخادم", error = ex.Message });
            }
        }

        [HttpGet("equipment")]
        public async Task<IActionResult> GetEquipmentReport()
        {
            try
            {
                var userBranchId = int.Parse(User.FindFirst("BranchId")?.Value ?? "0");

                var equipmentData = await _context.Equipment
                    .Include(e => e.EquipmentReservations)
                    .Where(e => e.BranchId == userBranchId)
                    .ToListAsync();

                var equipmentReports = equipmentData.Select(equipment => new
                {
                    Equipment = new 
                    { 
                        equipment.Id, 
                        equipment.Name, 
                        equipment.Status, 
                        equipment.Condition,
                        equipment.PurchaseDate,
                        equipment.PurchasePrice
                    },
                    Usage = new
                    {
                        TotalReservations = equipment.EquipmentReservations.Count,
                        ActiveReservations = equipment.EquipmentReservations.Count(er => er.Status == EquipmentReservationStatus.InUse),
                        CompletedReservations = equipment.EquipmentReservations.Count(er => er.Status == EquipmentReservationStatus.Returned),
                        TotalDamageCost = equipment.EquipmentReservations.Sum(er => er.DamageCost),
                        UtilizationRate = equipment.EquipmentReservations.Any() 
                            ? Math.Round((double)equipment.EquipmentReservations.Count(er => er.Status == EquipmentReservationStatus.Returned) / equipment.EquipmentReservations.Count * 100, 2)
                            : 0
                    },
                    Maintenance = new
                    {
                        equipment.LastMaintenanceDate,
                        equipment.NextMaintenanceDate,
                        IsMaintenanceDue = equipment.NextMaintenanceDate.HasValue && equipment.NextMaintenanceDate.Value <= DateTime.UtcNow
                    }
                }).ToList();

                var summary = new
                {
                    TotalEquipment = equipmentReports.Count,
                    AvailableEquipment = equipmentReports.Count(er => er.Equipment.Status == EquipmentStatus.Available),
                    InUseEquipment = equipmentReports.Count(er => er.Equipment.Status == EquipmentStatus.InUse),
                    MaintenanceEquipment = equipmentReports.Count(er => er.Equipment.Status == EquipmentStatus.UnderMaintenance),
                    EquipmentNeedingMaintenance = equipmentReports.Count(er => er.Maintenance.IsMaintenanceDue),
                    TotalDamageCosts = equipmentReports.Sum(er => er.Usage.TotalDamageCost),
                    AverageUtilizationRate = equipmentReports.Any() 
                        ? Math.Round(equipmentReports.Average(er => er.Usage.UtilizationRate), 2) 
                        : 0
                };

                return Ok(new
                {
                    Summary = summary,
                    EquipmentDetails = equipmentReports
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "حدث خطأ في الخادم", error = ex.Message });
            }
        }

        [HttpGet("dashboard")]
        [Authorize(Policy = "AdminOrEmployee")]
        public async Task<IActionResult> GetDashboardStats()
        {
            try
            {
                var userBranchId = int.Parse(User.FindFirst("BranchId")?.Value ?? "0");

                // Real data from DB
                var totalStudents = await _context.Students.CountAsync(s => s.BranchId == userBranchId);
                var totalCourses = await _context.Courses.CountAsync(c => c.BranchId == userBranchId);
                var activeCourses = await _context.Courses.CountAsync(c => c.BranchId == userBranchId && c.Status == CourseStatus.Active); // Status 2 = Active
                var totalRevenue = await _context.CourseRegistrations
                    .Where(cr => cr.Course.BranchId == userBranchId && cr.PaymentStatus != PaymentStatus.Cancelled)
                    .SumAsync(cr => (decimal?)cr.PaidAmount) ?? 0;
                var monthlyRevenue = await _context.CourseRegistrations
                    .Where(cr => cr.Course.BranchId == userBranchId && cr.PaymentStatus != PaymentStatus.Cancelled && cr.PaymentDate != null && cr.PaymentDate.Value.Month == DateTime.Now.Month && cr.PaymentDate.Value.Year == DateTime.Now.Year)
                    .SumAsync(cr => (decimal?)cr.PaidAmount) ?? 0;
                var activeWorkspaceBookings = await _context.WorkspaceBookings.CountAsync(wb => wb.BranchId == userBranchId && wb.EndTime > DateTime.Now);
                var pendingPayments = await _context.CourseRegistrations.CountAsync(cr => cr.Course.BranchId == userBranchId && cr.PaymentStatus == PaymentStatus.Pending);
                var upcomingClasses = await _context.Courses.CountAsync(c => c.BranchId == userBranchId && c.StartDate > DateTime.Now);

                var stats = new
                {
                    totalStudents,
                    totalCourses,
                    activeCourses,
                    totalRevenue,
                    monthlyRevenue,
                    activeWorkspaceBookings,
                    pendingPayments,
                    upcomingClasses
                };
                return Ok(new { success = true, data = stats });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "حدث خطأ في الخادم", error = ex.Message });
            }
        }
    }
}
