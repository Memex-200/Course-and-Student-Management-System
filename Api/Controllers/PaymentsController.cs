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
    public class PaymentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PaymentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("course-registrations")]
        public async Task<IActionResult> GetCoursePayments([FromQuery] PaymentStatus? status = null, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var userBranchId = int.Parse(User.FindFirst("BranchId")?.Value ?? "0");
                var query = _context.CourseRegistrations
                    .Include(cr => cr.Student)
                    .Include(cr => cr.Course)
                    .Where(cr => cr.Course.BranchId == userBranchId);

                if (status.HasValue)
                    query = query.Where(cr => cr.PaymentStatus == status.Value);

                if (startDate.HasValue)
                    query = query.Where(cr => cr.RegistrationDate >= startDate.Value);

                if (endDate.HasValue)
                    query = query.Where(cr => cr.RegistrationDate <= endDate.Value);

                var payments = await query
                    .OrderByDescending(cr => cr.RegistrationDate)
                    .Select(cr => new
                    {
                        cr.Id,
                        Student = cr.Student.FullName,
                        Course = cr.Course.Name,
                        cr.TotalAmount,
                        cr.PaidAmount,
                        RemainingAmount = cr.RemainingAmount,
                        PaymentStatus = cr.PaymentStatus.ToString(),
                        PaymentStatusArabic = GetPaymentStatusArabic(cr.PaymentStatus),
                        cr.PaymentMethod,
                        cr.RegistrationDate,
                        cr.PaymentDate,
                        cr.PaymentNotes
                    })
                    .ToListAsync();

                return Ok(payments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "حدث خطأ في الخادم", error = ex.Message });
            }
        }

        [HttpGet("workspace-bookings")]
        public async Task<IActionResult> GetWorkspacePayments([FromQuery] PaymentStatus? status = null, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var userBranchId = int.Parse(User.FindFirst("BranchId")?.Value ?? "0");
                
                // Check if this is Assiut branch (workspace only available there)
                var branch = await _context.Branches.FindAsync(userBranchId);
                if (branch?.Name != "عسيوط")
                    return BadRequest(new { message = "خدمة الورك سبيس متاحة فقط في فرع عسيوط" });

                var query = _context.WorkspaceBookings
                    .Include(wb => wb.Student)
                    .Where(wb => wb.BranchId == userBranchId);

                if (status.HasValue)
                    query = query.Where(wb => wb.PaymentStatus == status.Value);

                if (startDate.HasValue)
                    query = query.Where(wb => wb.BookingDate >= startDate.Value);

                if (endDate.HasValue)
                    query = query.Where(wb => wb.BookingDate <= endDate.Value);

                var payments = await query
                    .OrderByDescending(wb => wb.BookingDate)
                    .Select(wb => new
                    {
                        wb.Id,
                        Student = wb.Student.FullName,
                        wb.BookingDate,
                        wb.StartTime,
                        wb.EndTime,
                        wb.HoursBooked,
                        wb.HourlyRate,
                        wb.TotalAmount,
                        wb.PaidAmount,
                        RemainingAmount = wb.RemainingAmount,
                        PaymentStatus = wb.PaymentStatus.ToString(),
                        PaymentStatusArabic = GetPaymentStatusArabic(wb.PaymentStatus),
                        PaymentMethod = wb.PaymentMethod.ToString(),
                        PaymentMethodArabic = GetPaymentMethodArabic(wb.PaymentMethod),
                        wb.PaymentDate,
                        wb.PaymentNotes
                    })
                    .ToListAsync();

                return Ok(payments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "حدث خطأ في الخادم", error = ex.Message });
            }
        }

        [HttpGet("cafeteria-orders")]
        public async Task<IActionResult> GetCafeteriaPayments([FromQuery] PaymentStatus? status = null, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var userBranchId = int.Parse(User.FindFirst("BranchId")?.Value ?? "0");
                var query = _context.CafeteriaOrders
                    .Include(co => co.Student)
                    .Include(co => co.Employee)
                        .ThenInclude(e => e.User)
                    .Where(co => co.BranchId == userBranchId);

                if (status.HasValue)
                    query = query.Where(co => co.PaymentStatus == status.Value);

                if (startDate.HasValue)
                    query = query.Where(co => co.OrderDate >= startDate.Value);

                if (endDate.HasValue)
                    query = query.Where(co => co.OrderDate <= endDate.Value);

                var payments = await query
                    .OrderByDescending(co => co.OrderDate)
                    .Select(co => new
                    {
                        co.Id,
                        co.OrderNumber,
                        Customer = co.Student != null ? co.Student.FullName :
                                  co.Employee != null ? co.Employee.User.FullName : co.CustomerName,
                        co.OrderDate,
                        co.TotalAmount,
                        co.PaidAmount,
                        RemainingAmount = co.RemainingAmount,
                        PaymentStatus = co.PaymentStatus.ToString(),
                        PaymentStatusArabic = GetPaymentStatusArabic(co.PaymentStatus),
                        PaymentMethod = co.PaymentMethod.ToString(),
                        PaymentMethodArabic = GetPaymentMethodArabic(co.PaymentMethod)
                    })
                    .ToListAsync();

                return Ok(payments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "حدث خطأ في الخادم", error = ex.Message });
            }
        }

        [HttpPost("course-registrations/{registrationId}/payment")]
        [Authorize(Policy = "AdminOrEmployee")]
        public async Task<IActionResult> ProcessCoursePayment(int registrationId, [FromBody] ProcessPaymentRequest request)
        {
            try
            {
                var registration = await _context.CourseRegistrations
                    .Include(cr => cr.Student)
                    .Include(cr => cr.Course)
                    .FirstOrDefaultAsync(cr => cr.Id == registrationId);

                if (registration == null)
                    return NotFound(new { message = "التسجيل غير موجود" });

                if (registration.PaymentStatus == PaymentStatus.FullyPaid)
                    return BadRequest(new { message = "تم دفع المبلغ بالكامل بالفعل" });

                var remainingAmount = registration.RemainingAmount;
                if (request.Amount > remainingAmount)
                    return BadRequest(new { message = "المبلغ المدفوع أكبر من المبلغ المستحق" });

                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

                registration.PaidAmount += request.Amount;
                registration.PaymentMethod = request.PaymentMethod;
                registration.PaymentDate = DateTime.UtcNow;
                registration.PaymentNotes = request.Notes;

                // Update payment status
                if (registration.PaidAmount >= registration.TotalAmount)
                    registration.PaymentStatus = PaymentStatus.FullyPaid;
                else if (registration.PaidAmount > 0)
                    registration.PaymentStatus = PaymentStatus.PartiallyPaid;

                // إضافة سجل الدفع إلى جدول المدفوعات
                var payment = new Payment
                {
                    CourseRegistrationId = registration.Id,
                    Amount = request.Amount,
                    PaymentMethod = request.PaymentMethod,
                    PaymentDate = DateTime.UtcNow,
                    ProcessedByUserId = userId,
                    Notes = request.Notes ?? "دفعة كورس",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Payments.Add(payment);

                // إضافة سجل إيراد إلى جدول المصروفات
                var expense = new Expense
                {
                    Title = $"إيراد كورس '{registration.Course.Name}' - {registration.Student.FullName}",
                    Description = $"دفعة من الطالب {registration.Student.FullName} لكورس {registration.Course.Name}",
                    Amount = request.Amount, // مبلغ موجب للإيراد
                    ExpenseDate = DateTime.UtcNow,
                    Category = ExpenseCategory.Training,
                    Status = ExpenseStatus.Paid,
                    Priority = ExpensePriority.Low,
                    PaymentMethod = request.PaymentMethod,
                    BranchId = registration.Course.BranchId ?? 1,
                    RequestedByUserId = userId,
                    ApprovedByUserId = userId,
                    ApprovedAt = DateTime.UtcNow,
                    Notes = $"إيراد من دفع رسوم الكورس - {request.Notes}",
                    CreatedAt = DateTime.UtcNow
                };
                _context.Expenses.Add(expense);

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "تم تسجيل الدفعة بنجاح",
                    paidAmount = request.Amount,
                    totalPaid = registration.PaidAmount,
                    remainingAmount = registration.RemainingAmount,
                    paymentStatus = registration.PaymentStatus.ToString()
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "حدث خطأ في الخادم", error = ex.Message });
            }
        }

        [HttpPost("workspace-bookings/{bookingId}/payment")]
        [Authorize(Policy = "AdminOrEmployee")]
        public async Task<IActionResult> ProcessWorkspacePayment(int bookingId, [FromBody] ProcessPaymentRequest request)
        {
            try
            {
                var booking = await _context.WorkspaceBookings
                    .Include(wb => wb.Student)
                    .FirstOrDefaultAsync(wb => wb.Id == bookingId);

                if (booking == null)
                    return NotFound(new { message = "الحجز غير موجود" });

                if (booking.PaymentStatus == PaymentStatus.FullyPaid)
                    return BadRequest(new { message = "تم دفع المبلغ بالكامل بالفعل" });

                var remainingAmount = booking.RemainingAmount;
                if (request.Amount > remainingAmount)
                    return BadRequest(new { message = "المبلغ المدفوع أكبر من المبلغ المستحق" });

                booking.PaidAmount += request.Amount;
                booking.PaymentMethod = request.PaymentMethod;
                booking.PaymentDate = DateTime.UtcNow;
                booking.PaymentNotes = request.Notes;

                // Update payment status
                if (booking.PaidAmount >= booking.TotalAmount)
                    booking.PaymentStatus = PaymentStatus.FullyPaid;
                else if (booking.PaidAmount > 0)
                    booking.PaymentStatus = PaymentStatus.PartiallyPaid;

                await _context.SaveChangesAsync();

                return Ok(new 
                { 
                    message = "تم تسجيل الدفعة بنجاح", 
                    paidAmount = request.Amount,
                    totalPaid = booking.PaidAmount,
                    remainingAmount = booking.RemainingAmount,
                    paymentStatus = booking.PaymentStatus.ToString()
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "حدث خطأ في الخادم", error = ex.Message });
            }
        }

        [HttpPost("cafeteria-orders/{orderId}/payment")]
        [Authorize(Policy = "AdminOrEmployee")]
        public async Task<IActionResult> ProcessCafeteriaPayment(int orderId, [FromBody] ProcessPaymentRequest request)
        {
            try
            {
                var order = await _context.CafeteriaOrders.FindAsync(orderId);
                if (order == null)
                    return NotFound(new { message = "الطلب غير موجود" });

                if (order.PaymentStatus == PaymentStatus.FullyPaid)
                    return BadRequest(new { message = "تم دفع المبلغ بالكامل بالفعل" });

                var remainingAmount = order.RemainingAmount;
                if (request.Amount > remainingAmount)
                    return BadRequest(new { message = "المبلغ المدفوع أكبر من المبلغ المستحق" });

                order.PaidAmount += request.Amount;
                order.PaymentMethod = request.PaymentMethod;

                // Update payment status
                if (order.PaidAmount >= order.TotalAmount)
                    order.PaymentStatus = PaymentStatus.FullyPaid;
                else if (order.PaidAmount > 0)
                    order.PaymentStatus = PaymentStatus.PartiallyPaid;

                await _context.SaveChangesAsync();

                return Ok(new 
                { 
                    message = "تم تسجيل الدفعة بنجاح", 
                    paidAmount = request.Amount,
                    totalPaid = order.PaidAmount,
                    remainingAmount = order.RemainingAmount,
                    paymentStatus = order.PaymentStatus.ToString()
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "حدث خطأ في الخادم", error = ex.Message });
            }
        }

        [HttpPut("course-registrations/{registrationId}/update-payment")]
        [Authorize(Policy = "AdminOrEmployee")]
        public async Task<IActionResult> UpdateCoursePayment(int registrationId, [FromBody] UpdatePaymentRequest request)
        {
            try
            {
                var registration = await _context.CourseRegistrations
                    .Include(cr => cr.Student)
                    .Include(cr => cr.Course)
                    .FirstOrDefaultAsync(cr => cr.Id == registrationId);

                if (registration == null)
                    return NotFound(new { message = "التسجيل غير موجود" });

                // التحقق من أن المبلغ المدفوع الجديد لا يتجاوز المبلغ الإجمالي
                if (request.PaidAmount > registration.TotalAmount)
                    return BadRequest(new { message = "المبلغ المدفوع لا يمكن أن يتجاوز المبلغ الإجمالي" });

                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

                // منطق إضافة سجل مصروفات إذا زاد المبلغ المدفوع
                var previousPaidAmount = registration.PaidAmount;
                var newPaidAmount = request.PaidAmount;
                var diff = newPaidAmount - previousPaidAmount;

                if (diff > 0)
                {
                    // إضافة سجل الدفع إلى جدول المدفوعات
                    var payment = new Payment
                    {
                        CourseRegistrationId = registration.Id,
                        Amount = diff,
                        PaymentMethod = request.PaymentMethod,
                        PaymentDate = DateTime.UtcNow,
                        ProcessedByUserId = userId,
                        Notes = request.Notes ?? "تحديث دفعة كورس",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Payments.Add(payment);

                    // إضافة سجل إيراد إلى جدول المصروفات
                    var expense = new Expense
                    {
                        Title = $"إيراد كورس '{registration.Course.Name}' - {registration.Student.FullName}",
                        Description = $"تحديث دفعة من الطالب {registration.Student.FullName} لكورس {registration.Course.Name}",
                        Amount = diff, // مبلغ موجب للإيراد
                        ExpenseDate = DateTime.UtcNow,
                        Category = ExpenseCategory.Training,
                        Status = ExpenseStatus.Paid,
                        Priority = ExpensePriority.Low,
                        PaymentMethod = request.PaymentMethod,
                        BranchId = registration.Course.BranchId ?? 1,
                        RequestedByUserId = userId,
                        ApprovedByUserId = userId,
                        ApprovedAt = DateTime.UtcNow,
                        Notes = $"تحديث إيراد من دفع رسوم الكورس - {request.Notes}",
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Expenses.Add(expense);
                }

                // تحديث المبلغ المدفوع
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

                return Ok(new 
                { 
                    message = "تم تحديث حالة الدفع بنجاح", 
                    paidAmount = registration.PaidAmount,
                    remainingAmount = registration.RemainingAmount,
                    paymentStatus = registration.PaymentStatus.ToString(),
                    paymentStatusArabic = GetPaymentStatusArabic(registration.PaymentStatus),
                    paymentMethod = registration.PaymentMethod,
                    paymentMethodArabic = GetPaymentMethodArabic(registration.PaymentMethod ?? PaymentMethod.Cash)
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "حدث خطأ في الخادم", error = ex.Message });
            }
        }

        [HttpDelete("course-registrations/{registrationId}")]
        [Authorize(Policy = "AdminOrEmployee")]
        public async Task<IActionResult> RemoveCourseRegistration(int registrationId)
        {
            try
            {
                var registration = await _context.CourseRegistrations
                    .Include(cr => cr.Payments)
                    .FirstOrDefaultAsync(cr => cr.Id == registrationId);
                
            if (registration == null)
                    return NotFound(new { success = false, message = "تسجيل الطالب غير موجود" });

                // حذف جميع المدفوعات المرتبطة بالتسجيل أولاً
                if (registration.Payments.Any())
                {
                    _context.Payments.RemoveRange(registration.Payments);
                }

                // حذف التسجيل نفسه
            _context.CourseRegistrations.Remove(registration);
            await _context.SaveChangesAsync();
                
                return Ok(new { success = true, message = "تم إلغاء تسجيل الطالب من الكورس بنجاح" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "حدث خطأ في الخادم", error = ex.Message });
            }
        }

        [HttpGet("detailed-payments")]
        [Authorize(Policy = "AdminOrEmployee")]
        public async Task<IActionResult> GetDetailedPayments([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var userBranchId = int.Parse(User.FindFirst("BranchId")?.Value ?? "0");
                var query = _context.Payments
                    .Include(p => p.CourseRegistration)
                        .ThenInclude(cr => cr.Student)
                    .Include(p => p.CourseRegistration)
                        .ThenInclude(cr => cr.Course)
                    .Include(p => p.ProcessedByUser)
                    .Where(p => p.IsActive);

                // Filter by branch
                query = query.Where(p => p.CourseRegistration.Course.BranchId == userBranchId);

                if (startDate.HasValue)
                    query = query.Where(p => p.PaymentDate >= startDate.Value);

                if (endDate.HasValue)
                    query = query.Where(p => p.PaymentDate <= endDate.Value);

                var payments = await query
                    .OrderByDescending(p => p.PaymentDate)
                    .Select(p => new
                    {
                        p.Id,
                        StudentName = p.CourseRegistration.Student.FullName,
                        CourseName = p.CourseRegistration.Course.Name,
                        p.Amount,
                        PaymentMethod = p.PaymentMethod.ToString(),
                        PaymentMethodArabic = GetPaymentMethodArabic(p.PaymentMethod),
                        p.PaymentDate,
                        ProcessedBy = p.ProcessedByUser.FullName,
                        p.Notes,
                        RegistrationId = p.CourseRegistrationId
                    })
                    .ToListAsync();

                return Ok(new { success = true, data = payments });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "حدث خطأ في الخادم", error = ex.Message });
            }
        }

        [HttpGet("statistics")]
        public async Task<IActionResult> GetPaymentStatistics([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var userBranchId = int.Parse(User.FindFirst("BranchId")?.Value ?? "0");

                // Course payments
                var courseQuery = _context.CourseRegistrations
                    .Include(cr => cr.Course)
                    .Where(cr => cr.Course.BranchId == userBranchId);

                if (startDate.HasValue)
                    courseQuery = courseQuery.Where(cr => cr.RegistrationDate >= startDate.Value);
                if (endDate.HasValue)
                    courseQuery = courseQuery.Where(cr => cr.RegistrationDate <= endDate.Value);

                var coursePayments = await courseQuery.ToListAsync();

                // Workspace payments (only for Assiut branch)
                var workspacePayments = new List<WorkspaceBooking>();
                var branch = await _context.Branches.FindAsync(userBranchId);
                if (branch?.Name == "عسيوط")
                {
                    var workspaceQuery = _context.WorkspaceBookings
                        .Where(wb => wb.BranchId == userBranchId);

                    if (startDate.HasValue)
                        workspaceQuery = workspaceQuery.Where(wb => wb.BookingDate >= startDate.Value);
                    if (endDate.HasValue)
                        workspaceQuery = workspaceQuery.Where(wb => wb.BookingDate <= endDate.Value);

                    workspacePayments = await workspaceQuery.ToListAsync();
                }

                // Cafeteria payments
                var cafeteriaQuery = _context.CafeteriaOrders
                    .Where(co => co.BranchId == userBranchId);

                if (startDate.HasValue)
                    cafeteriaQuery = cafeteriaQuery.Where(co => co.OrderDate >= startDate.Value);
                if (endDate.HasValue)
                    cafeteriaQuery = cafeteriaQuery.Where(co => co.OrderDate <= endDate.Value);

                var cafeteriaPayments = await cafeteriaQuery.ToListAsync();

                var statistics = new
                {
                    CoursePayments = new
                    {
                        TotalRegistrations = coursePayments.Count,
                        TotalAmount = coursePayments.Sum(cp => cp.TotalAmount),
                        PaidAmount = coursePayments.Sum(cp => cp.PaidAmount),
                        RemainingAmount = coursePayments.Sum(cp => cp.RemainingAmount),
                        FullyPaidCount = coursePayments.Count(cp => cp.PaymentStatus == PaymentStatus.FullyPaid),
                        PartiallyPaidCount = coursePayments.Count(cp => cp.PaymentStatus == PaymentStatus.PartiallyPaid),
                        UnpaidCount = coursePayments.Count(cp => cp.PaymentStatus == PaymentStatus.Unpaid)
                    },
                    WorkspacePayments = new
                    {
                        TotalBookings = workspacePayments.Count,
                        TotalAmount = workspacePayments.Sum(wp => wp.TotalAmount),
                        PaidAmount = workspacePayments.Sum(wp => wp.PaidAmount),
                        RemainingAmount = workspacePayments.Sum(wp => wp.RemainingAmount),
                        FullyPaidCount = workspacePayments.Count(wp => wp.PaymentStatus == PaymentStatus.FullyPaid),
                        PartiallyPaidCount = workspacePayments.Count(wp => wp.PaymentStatus == PaymentStatus.PartiallyPaid),
                        UnpaidCount = workspacePayments.Count(wp => wp.PaymentStatus == PaymentStatus.Unpaid)
                    },
                    CafeteriaPayments = new
                    {
                        TotalOrders = cafeteriaPayments.Count,
                        TotalAmount = cafeteriaPayments.Sum(cp => cp.TotalAmount),
                        PaidAmount = cafeteriaPayments.Sum(cp => cp.PaidAmount),
                        RemainingAmount = cafeteriaPayments.Sum(cp => cp.RemainingAmount),
                        FullyPaidCount = cafeteriaPayments.Count(cp => cp.PaymentStatus == PaymentStatus.FullyPaid),
                        PartiallyPaidCount = cafeteriaPayments.Count(cp => cp.PaymentStatus == PaymentStatus.PartiallyPaid),
                        UnpaidCount = cafeteriaPayments.Count(cp => cp.PaymentStatus == PaymentStatus.Unpaid)
                    },
                    OverallSummary = new
                    {
                        TotalRevenue = coursePayments.Sum(cp => cp.PaidAmount) + 
                                      workspacePayments.Sum(wp => wp.PaidAmount) + 
                                      cafeteriaPayments.Sum(cp => cp.PaidAmount),
                        TotalOutstanding = coursePayments.Sum(cp => cp.RemainingAmount) + 
                                          workspacePayments.Sum(wp => wp.RemainingAmount) + 
                                          cafeteriaPayments.Sum(cp => cp.RemainingAmount)
                    }
                };

                return Ok(statistics);
            }
            catch (Exception ex)
            {
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
    }

    public class ProcessPaymentRequest
    {
        public decimal Amount { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public string Notes { get; set; } = string.Empty;
    }

    public class UpdatePaymentRequest
    {
        public decimal PaidAmount { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public string Notes { get; set; } = string.Empty;
    }

    public class UpdateRegistrationStatusRequest
    {
        public string Status { get; set; } = string.Empty; // "active", "completed", "dropped"
    }

    public class CreateStudentAccountResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
    }

    public class StudentDashboardDTO
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public List<StudentCourseDTO> Courses { get; set; } = new();
    }

    public class StudentCourseDTO
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalSessions { get; set; }
        public int AttendedSessions { get; set; }
        public int AbsentSessions { get; set; }
        public bool IsCompleted { get; set; }
        public string PaymentStatus { get; set; } = string.Empty;
        public string PaymentStatusArabic { get; set; } = string.Empty;
        public DateTime? NextSessionDate { get; set; }
        public string Schedule { get; set; } = string.Empty;
        public string InstructorName { get; set; } = string.Empty;
        public double Progress { get; set; }
        public string? CertificateUrl { get; set; }
        public int? ExamScore { get; set; }
    }
}
