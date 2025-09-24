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
                    StudentId = registration.StudentId,
                    CourseRegistrationId = registration.Id,
                    BranchId = registration.Course.BranchId,
                    Amount = request.Amount,
                    PaymentMethod = request.PaymentMethod,
                    PaymentType = PaymentType.CourseFee,
                    PaymentSource = PaymentSource.CourseFee,
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
                    CreatedAt = DateTime.UtcNow,
                    Vendor = "إيرادات التدريب"
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

                // إضافة سجل الدفع إلى جدول المدفوعات
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var payment = new Payment
                {
                    StudentId = booking.StudentId,
                    WorkspaceBookingId = booking.Id,
                    BranchId = booking.BranchId,
                    Amount = request.Amount,
                    PaymentMethod = request.PaymentMethod,
                    PaymentType = PaymentType.Workspace,
                    PaymentSource = PaymentSource.Workspace,
                    PaymentDate = DateTime.UtcNow,
                    ProcessedByUserId = userId,
                    Notes = request.Notes ?? "دفعة مساحة عمل",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Payments.Add(payment);

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

                // إضافة سجل الدفع إلى جدول المدفوعات
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var payment = new Payment
                {
                    StudentId = order.StudentId,
                    BranchId = order.BranchId,
                    Amount = request.Amount,
                    PaymentMethod = request.PaymentMethod,
                    PaymentType = PaymentType.Cafeteria,
                    PaymentSource = PaymentSource.Cafeteria,
                    PaymentDate = DateTime.UtcNow,
                    ProcessedByUserId = userId,
                    Notes = request.Notes ?? "دفعة كافيتريا",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Payments.Add(payment);

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
                        StudentId = registration.StudentId,
                        CourseRegistrationId = registration.Id,
                        BranchId = registration.Course.BranchId,
                        Amount = diff,
                        PaymentMethod = request.PaymentMethod,
                        PaymentType = PaymentType.CourseFee,
                        PaymentSource = PaymentSource.CourseFee,
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
                        CreatedAt = DateTime.UtcNow,
                        Vendor = "إيرادات التدريب"
                    };
                    _context.Expenses.Add(expense);
                }
                // إذا قل المبلغ المدفوع (تم إلغاء جزء من الدفع)، أضف سجل دفع سالب
                else if (diff < 0)
                {
                    var refundPayment = new Payment
                    {
                        StudentId = registration.StudentId,
                        CourseRegistrationId = registration.Id,
                        BranchId = registration.Course.BranchId,
                        Amount = Math.Abs(diff), // مبلغ موجب للاسترداد
                        PaymentMethod = request.PaymentMethod,
                        PaymentType = PaymentType.CourseFee,
                        PaymentSource = PaymentSource.CourseFee,
                        PaymentDate = DateTime.UtcNow,
                        ProcessedByUserId = userId,
                        Notes = $"استرداد جزئي - {request.Notes}",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Payments.Add(refundPayment);

                    // إضافة سجل مصروف للاسترداد
                    var refundExpense = new Expense
                    {
                        Title = $"استرداد كورس '{registration.Course.Name}' - {registration.Student.FullName}",
                        Description = $"استرداد جزئي للطالب {registration.Student.FullName} من كورس {registration.Course.Name}",
                        Amount = diff, // مبلغ سالب للاسترداد
                        ExpenseDate = DateTime.UtcNow,
                        Category = ExpenseCategory.Training,
                        Status = ExpenseStatus.Paid,
                        Priority = ExpensePriority.Low,
                        PaymentMethod = request.PaymentMethod,
                        BranchId = registration.Course.BranchId ?? 1,
                        RequestedByUserId = userId,
                        ApprovedByUserId = userId,
                        ApprovedAt = DateTime.UtcNow,
                        Notes = $"استرداد من دفع رسوم الكورس - {request.Notes}",
                        CreatedAt = DateTime.UtcNow,
                        Vendor = "استردادات التدريب"
                    };
                    _context.Expenses.Add(refundExpense);
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
        public async Task<IActionResult> GetDetailedPayments([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null,
            [FromQuery] int? courseId = null, [FromQuery] int? studentId = null, [FromQuery] int? branchId = null)
        {
            try
            {
                var userBranchId = int.Parse(User.FindFirst("BranchId")?.Value ?? "0");
                var effectiveBranchId = branchId ?? userBranchId;

                // Get payments from Payments table
                var paymentsQuery = _context.Payments
                    .Include(p => p.CourseRegistration)
                        .ThenInclude(cr => cr.Student)
                    .Include(p => p.CourseRegistration)
                        .ThenInclude(cr => cr.Course)
                            .ThenInclude(c => c.Branch)
                    .Include(p => p.ProcessedByUser)
                    .Where(p => p.IsActive && p.CourseRegistration != null);

                // Filter by branch
                paymentsQuery = paymentsQuery.Where(p => p.CourseRegistration!.Course != null && p.CourseRegistration.Course.BranchId == effectiveBranchId);

                if (startDate.HasValue)
                    paymentsQuery = paymentsQuery.Where(p => p.PaymentDate >= startDate.Value);

                if (endDate.HasValue)
                    paymentsQuery = paymentsQuery.Where(p => p.PaymentDate <= endDate.Value);

                if (courseId.HasValue)
                    paymentsQuery = paymentsQuery.Where(p => p.CourseRegistration!.CourseId == courseId.Value);

                if (studentId.HasValue)
                    paymentsQuery = paymentsQuery.Where(p => p.CourseRegistration!.StudentId == studentId.Value);

                var payments = await paymentsQuery
                    .OrderByDescending(p => p.PaymentDate)
                    .Select(p => new
                    {
                        p.Id,
                        StudentName = p.CourseRegistration!.Student != null ? p.CourseRegistration.Student.FullName : string.Empty,
                        CourseName = p.CourseRegistration.Course != null ? p.CourseRegistration.Course.Name : string.Empty,
                        p.Amount,
                        PaymentMethod = p.PaymentMethod.ToString(),
                        PaymentMethodArabic = GetPaymentMethodArabic(p.PaymentMethod),
                        p.PaymentDate,
                        ProcessedBy = p.ProcessedByUser != null ? p.ProcessedByUser.FullName : string.Empty,
                        Notes = p.Notes ?? string.Empty,
                        RegistrationId = p.CourseRegistrationId ?? 0,
                        BranchName = p.CourseRegistration!.Course != null && p.CourseRegistration.Course.Branch != null ? p.CourseRegistration.Course.Branch.Name : string.Empty
                    })
                    .ToListAsync();

                // Get course registrations with completed payments only
                var registrationsQuery = _context.CourseRegistrations
                    .Include(cr => cr.Student)
                    .Include(cr => cr.Course)
                        .ThenInclude(c => c.Branch)
                    .Where(cr => cr.Course.BranchId == effectiveBranchId && 
                                cr.PaidAmount > 0 && 
                                cr.PaymentStatus == PaymentStatus.FullyPaid);

                if (startDate.HasValue)
                    registrationsQuery = registrationsQuery.Where(cr => cr.PaymentDate >= startDate.Value);

                if (endDate.HasValue)
                    registrationsQuery = registrationsQuery.Where(cr => cr.PaymentDate <= endDate.Value);

                if (courseId.HasValue)
                    registrationsQuery = registrationsQuery.Where(cr => cr.CourseId == courseId.Value);

                if (studentId.HasValue)
                    registrationsQuery = registrationsQuery.Where(cr => cr.StudentId == studentId.Value);

                var registrations = await registrationsQuery
                    .OrderByDescending(cr => cr.PaymentDate)
                    .Select(cr => new
                    {
                        Id = cr.Id + 1000000, // Unique ID for registrations
                        StudentName = cr.Student != null ? cr.Student.FullName : string.Empty,
                        CourseName = cr.Course != null ? cr.Course.Name : string.Empty,
                        Amount = cr.PaidAmount,
                        PaymentMethod = (cr.PaymentMethod ?? PaymentMethod.Cash).ToString(),
                        PaymentMethodArabic = GetPaymentMethodArabic(cr.PaymentMethod ?? PaymentMethod.Cash),
                        PaymentDate = cr.PaymentDate ?? cr.RegistrationDate,
                        ProcessedBy = "نظام",
                        Notes = cr.PaymentNotes ?? "دفعة كورس",
                        RegistrationId = cr.Id,
                        BranchName = cr.Course != null && cr.Course.Branch != null ? cr.Course.Branch.Name : string.Empty
                    })
                    .ToListAsync();

                // Combine both results
                var allPayments = payments.Concat(registrations).OrderByDescending(p => p.PaymentDate).ToList();

                return Ok(new { success = true, data = allPayments });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "حدث خطأ في الخادم", error = ex.Message });
            }
        }



        [HttpGet("all-transactions")]
        public async Task<IActionResult> GetAllTransactions([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var userBranchId = int.Parse(User.FindFirst("BranchId")?.Value ?? "0");
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

                // Get only actual payments (not expenses that are duplicates of payments)
                var paymentsQuery = _context.Payments
                    .Include(p => p.Student)
                    .Include(p => p.CourseRegistration)
                        .ThenInclude(cr => cr.Student)
                    .Include(p => p.CourseRegistration)
                        .ThenInclude(cr => cr.Course)
                            .ThenInclude(c => c.Branch)
                    .Include(p => p.ProcessedByUser)
                    .Where(p => p.IsActive);

                // Filter by branch if not admin
                if (userRole != "Admin")
                {
                    paymentsQuery = paymentsQuery.Where(p => 
                        (p.CourseRegistration != null && p.CourseRegistration.Course != null && p.CourseRegistration.Course.BranchId == userBranchId) ||
                        (p.Student != null && p.Student.BranchId == userBranchId)
                    );
                }

                if (startDate.HasValue)
                    paymentsQuery = paymentsQuery.Where(p => p.PaymentDate >= startDate.Value);

                if (endDate.HasValue)
                    paymentsQuery = paymentsQuery.Where(p => p.PaymentDate <= endDate.Value);

                // Execute query first, then project on client side
                var paymentsData = await paymentsQuery
                    .OrderByDescending(p => p.PaymentDate)
                    .ToListAsync();

                var payments = paymentsData.Select(p => new
                {
                    Id = p.Id,
                    StudentName = p.Student?.FullName ?? p.CourseRegistration?.Student?.FullName ?? string.Empty,
                    CourseName = p.CourseRegistration?.Course?.Name ?? string.Empty,
                    Amount = p.Amount,
                    PaymentMethod = p.PaymentMethod.ToString(),
                    PaymentMethodArabic = GetPaymentMethodArabic(p.PaymentMethod),
                    PaymentType = p.PaymentType.ToString(),
                    PaymentTypeArabic = GetPaymentTypeArabic(p.PaymentType),
                    PaymentDate = p.PaymentDate,
                    ProcessedBy = p.ProcessedByUser?.FullName ?? string.Empty,
                    Notes = p.Notes,
                    RegistrationId = p.CourseRegistrationId ?? 0,
                    BranchName = p.CourseRegistration?.Course?.Branch?.Name ?? string.Empty,
                    TransactionType = "income",
                    PaymentStatus = "paid",
                    Category = "التدريب",
                    Description = p.Notes ?? "دفعة تدريب",
                    InvoiceUrl = $"/api/invoices/payment/{p.Id}"
                }).ToList();

                // Get only actual expenses (not income records that duplicate payments)
                // Filter out expenses that are income from course payments
                var expensesQuery = _context.Expenses
                    .Include(e => e.Branch)
                    .Include(e => e.RequestedByUser)
                    .Where(e => e.BranchId == userBranchId && 
                               e.Category != ExpenseCategory.Training && // Exclude training income
                               e.Amount < 0); // Only actual expenses (negative amounts)

                if (startDate.HasValue)
                    expensesQuery = expensesQuery.Where(e => e.ExpenseDate >= startDate.Value);

                if (endDate.HasValue)
                    expensesQuery = expensesQuery.Where(e => e.ExpenseDate <= endDate.Value);

                // Execute query first, then project on client side
                var expensesData = await expensesQuery
                    .OrderByDescending(e => e.ExpenseDate)
                    .ToListAsync();

                var expenses = expensesData.Select(e => new
                {
                    Id = e.Id + 1000000, // Unique ID for expenses
                    StudentName = string.Empty,
                    CourseName = string.Empty,
                    Amount = Math.Abs(e.Amount), // Make positive for display
                    PaymentMethod = e.PaymentMethod.ToString(),
                    PaymentMethodArabic = GetPaymentMethodArabic(e.PaymentMethod),
                    PaymentType = "Expense",
                    PaymentTypeArabic = GetExpenseCategoryArabic(e.Category),
                    PaymentDate = e.ExpenseDate,
                    ProcessedBy = e.RequestedByUser?.FullName ?? string.Empty,
                    Notes = e.Description,
                    RegistrationId = 0,
                    BranchName = e.Branch?.Name ?? string.Empty,
                    TransactionType = "expense",
                    PaymentStatus = e.Status == ExpenseStatus.Paid ? "paid" : "pending",
                    Category = GetExpenseCategoryArabic(e.Category),
                    Description = e.Description,
                    InvoiceUrl = $"/api/invoices/expense/{e.Id}"
                }).ToList();

                // Combine both results
                var allTransactions = payments.Concat(expenses)
                    .OrderByDescending(t => t.PaymentDate)
                    .ToList();

                // Calculate totals - only from actual payments
                var totalIncome = payments.Sum(p => p.Amount);
                var totalExpenses = expenses.Sum(e => e.Amount);

                return Ok(new { 
                    success = true, 
                    data = allTransactions,
                    totalIncome,
                    totalExpenses,
                    netBalance = totalIncome - totalExpenses,
                    paymentCount = payments.Count,
                    expenseCount = expenses.Count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    message = "حدث خطأ في الخادم", 
                    error = ex.Message,
                    stackTrace = ex.StackTrace,
                    innerException = ex.InnerException?.Message
                });
            }
        }

        [HttpGet("clean-transactions")]
        public async Task<IActionResult> GetCleanTransactions([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var userBranchId = int.Parse(User.FindFirst("BranchId")?.Value ?? "0");
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

                // Get only actual payments from people who paid
                var paymentsQuery = _context.Payments
                    .Include(p => p.Student)
                    .Include(p => p.CourseRegistration)
                        .ThenInclude(cr => cr.Student)
                    .Include(p => p.CourseRegistration)
                        .ThenInclude(cr => cr.Course)
                            .ThenInclude(c => c.Branch)
                    .Include(p => p.ProcessedByUser)
                    .Where(p => p.IsActive && p.Amount > 0); // Only positive payments

                // Filter by branch if not admin
                if (userRole != "Admin")
                {
                    paymentsQuery = paymentsQuery.Where(p => 
                        (p.CourseRegistration != null && p.CourseRegistration.Course != null && p.CourseRegistration.Course.BranchId == userBranchId) ||
                        (p.Student != null && p.Student.BranchId == userBranchId)
                    );
                }

                if (startDate.HasValue)
                    paymentsQuery = paymentsQuery.Where(p => p.PaymentDate >= startDate.Value);

                if (endDate.HasValue)
                    paymentsQuery = paymentsQuery.Where(p => p.PaymentDate <= endDate.Value);

                // Execute query first, then project on client side
                var paymentsData = await paymentsQuery
                    .OrderByDescending(p => p.PaymentDate)
                    .ToListAsync();

                var transactions = paymentsData.Select(p => new
                {
                    Id = p.Id,
                    StudentName = p.Student?.FullName ?? p.CourseRegistration?.Student?.FullName ?? "غير محدد",
                    CourseName = p.CourseRegistration?.Course?.Name ?? "غير محدد",
                    Amount = p.Amount,
                    PaymentMethod = p.PaymentMethod.ToString(),
                    PaymentMethodArabic = GetPaymentMethodArabic(p.PaymentMethod),
                    PaymentType = p.PaymentType.ToString(),
                    PaymentTypeArabic = GetPaymentTypeArabic(p.PaymentType),
                    PaymentDate = p.PaymentDate,
                    ProcessedBy = p.ProcessedByUser?.FullName ?? "غير محدد",
                    Notes = p.Notes,
                    RegistrationId = p.CourseRegistrationId ?? 0,
                    BranchName = p.CourseRegistration?.Course?.Branch?.Name ?? "غير محدد",
                    TransactionType = "income",
                    PaymentStatus = "paid",
                    Category = "التدريب",
                    Description = p.Notes ?? "دفعة تدريب",
                    InvoiceUrl = $"/api/invoices/payment/{p.Id}"
                }).ToList();

                // Calculate totals
                var totalIncome = transactions.Sum(t => t.Amount);
                var totalTransactions = transactions.Count;

                return Ok(new { 
                    success = true, 
                    data = transactions,
                    totalIncome,
                    totalTransactions,
                    netBalance = totalIncome, // Only income, no expenses
                    message = "بيانات المدفوعات الصحيحة فقط"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    message = "حدث خطأ في الخادم", 
                    error = ex.Message
                });
            }
        }

        [HttpGet("student/{studentId}/history")]
        public async Task<IActionResult> GetStudentPaymentHistory(int studentId)
        {
            try
            {
                var payments = await _context.Payments
                    .Include(p => p.CourseRegistration)
                        .ThenInclude(cr => cr.Course)
                    .Include(p => p.ProcessedByUser)
                    .Where(p => p.StudentId == studentId && p.IsActive)
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

                var totalPaid = payments.Sum(p => p.Amount);

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        payments,
                        totalPaid,
                        paymentCount = payments.Count
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "حدث خطأ في الخادم", error = ex.Message });
            }
        }

#if DEBUG
        [HttpPost("populate-student-ids")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PopulateStudentIds()
        {
            try
            {
                await Scripts.PopulateStudentIdInPayments.PopulateStudentIds(_context);
                return Ok(new { success = true, message = "تم تحديث StudentId لجميع المدفوعات بنجاح" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "حدث خطأ في تحديث StudentId", error = ex.Message });
            }
        }

        [HttpPost("populate-payment-data")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PopulatePaymentData()
        {
            try
            {
                await Scripts.PopulatePaymentData.Execute(_context);
                return Ok(new { success = true, message = "تم تحديث بيانات المدفوعات بنجاح" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "حدث خطأ في تحديث بيانات المدفوعات", error = ex.Message });
            }
        }

        [HttpPost("clean-duplicate-expenses")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CleanDuplicateExpenses()
        {
            try
            {
                // Find and remove duplicate expense records that are income from course payments
                var duplicateExpenses = await _context.Expenses
                    .Where(e => e.Category == ExpenseCategory.Training && e.Amount > 0)
                    .ToListAsync();

                var removedCount = 0;
                foreach (var expense in duplicateExpenses)
                {
                    // Check if there's a corresponding payment record
                    var hasPayment = await _context.Payments
                        .AnyAsync(p => p.Amount == expense.Amount && 
                                      p.PaymentDate.Date == expense.ExpenseDate.Date &&
                                      p.Notes.Contains("دفعة") || p.Notes.Contains("تسجيل"));

                    if (hasPayment)
                    {
                        _context.Expenses.Remove(expense);
                        removedCount++;
                    }
                }

                await _context.SaveChangesAsync();

                return Ok(new { 
                    success = true, 
                    message = $"تم حذف {removedCount} سجل مصروف مكرر",
                    removedCount = removedCount
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    success = false, 
                    message = "حدث خطأ في تنظيف البيانات المكررة", 
                    error = ex.Message 
                });
            }
        }

        [HttpPost("add-sample-data")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddSampleData()
        {
            try
            {
                var script = new Api.Scripts.AddSamplePayments(_context);
                await script.ExecuteAsync();
                
                return Ok(new { message = "تم إضافة بيانات تجريبية بنجاح" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "حدث خطأ في الخادم", error = ex.Message });
            }
        }
#endif

        [HttpGet("debug-payments")]
        [Authorize(Policy = "AdminOrEmployee")]
        public async Task<IActionResult> GetDebugPayments()
        {
            try
            {
                var userBranchId = int.Parse(User.FindFirst("BranchId")?.Value ?? "0");
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

                // Get all payments with full details
                var payments = await _context.Payments
                    .Include(p => p.Student)
                    .Include(p => p.CourseRegistration)
                        .ThenInclude(cr => cr.Course)
                            .ThenInclude(c => c.Branch)
                    .Include(p => p.ProcessedByUser)
                    .Where(p => p.IsActive)
                    .Select(p => new
                    {
                        p.Id,
                        p.StudentId,
                        p.CourseRegistrationId,
                        p.BranchId,
                        StudentName = p.Student != null ? p.Student.FullName : "No Student",
                        CourseName = p.CourseRegistration != null && p.CourseRegistration.Course != null ? p.CourseRegistration.Course.Name : "No Course",
                        BranchName = p.CourseRegistration != null && p.CourseRegistration.Course != null && p.CourseRegistration.Course.Branch != null ? p.CourseRegistration.Course.Branch.Name : "No Branch",
                        p.Amount,
                        p.PaymentMethod,
                        p.PaymentType,
                        p.PaymentDate,
                        ProcessedBy = p.ProcessedByUser != null ? p.ProcessedByUser.FullName : "System",
                        p.Notes,
                        p.IsActive,
                        p.CreatedAt
                    })
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync();

                // Get all expenses
                var expenses = await _context.Expenses
                    .Include(e => e.Branch)
                    .Include(e => e.RequestedByUser)
                    .Select(e => new
                    {
                        e.Id,
                        e.Title,
                        e.Description,
                        e.Amount,
                        e.Category,
                        e.ExpenseDate,
                        e.Status,
                        e.BranchId,
                        BranchName = e.Branch != null ? e.Branch.Name : "No Branch",
                        RequestedBy = e.RequestedByUser != null ? e.RequestedByUser.FullName : "System",
                        e.CreatedAt
                    })
                    .OrderByDescending(e => e.CreatedAt)
                    .ToListAsync();

                // Get course registrations with payments
                var registrations = await _context.CourseRegistrations
                    .Include(cr => cr.Student)
                    .Include(cr => cr.Course)
                        .ThenInclude(c => c.Branch)
                    .Where(cr => cr.PaidAmount > 0)
                    .Select(cr => new
                    {
                        cr.Id,
                        cr.StudentId,
                        cr.CourseId,
                        StudentName = cr.Student != null ? cr.Student.FullName : "No Student",
                        CourseName = cr.Course != null ? cr.Course.Name : "No Course",
                        BranchName = cr.Course != null && cr.Course.Branch != null ? cr.Course.Branch.Name : "No Branch",
                        cr.TotalAmount,
                        cr.PaidAmount,
                        cr.PaymentStatus,
                        cr.PaymentMethod,
                        cr.PaymentDate,
                        cr.CreatedAt
                    })
                    .OrderByDescending(cr => cr.CreatedAt)
                    .ToListAsync();

                return Ok(new { 
                    success = true, 
                    userBranchId,
                    userRole,
                    totalPayments = payments.Count,
                    totalExpensesCount = expenses.Count,
                    totalRegistrations = registrations.Count,
                    payments = payments.Take(10), // Show first 10 for debugging
                    expenses = expenses.Take(10), // Show first 10 for debugging
                    registrations = registrations.Take(10) // Show first 10 for debugging
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "حدث خطأ في الخادم", error = ex.Message });
            }
        }

        [HttpGet("debug-simple")]
        [Authorize(Policy = "AdminOrEmployee")]
        public async Task<IActionResult> GetDebugSimple()
        {
            try
            {
                var userBranchId = int.Parse(User.FindFirst("BranchId")?.Value ?? "0");
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
                
                // Test database connection
                var paymentsCount = await _context.Payments.CountAsync();
                var expensesCount = await _context.Expenses.CountAsync();
                var studentsCount = await _context.Students.CountAsync();
                
                // Check payment data with student relationships
                var paymentsWithStudents = await _context.Payments
                    .Include(p => p.Student)
                    .Include(p => p.CourseRegistration)
                        .ThenInclude(cr => cr.Student)
                    .Take(5)
                    .Select(p => new
                    {
                        p.Id,
                        p.StudentId,
                        StudentName = p.Student != null ? p.Student.FullName : "No Student",
                        CourseRegistrationId = p.CourseRegistrationId,
                        CourseRegistrationStudentName = p.CourseRegistration != null && p.CourseRegistration.Student != null ? p.CourseRegistration.Student.FullName : "No Course Registration Student",
                        p.Amount,
                        p.PaymentDate
                    })
                    .ToListAsync();
                
                // Simple test - just return basic info
                return Ok(new { 
                    success = true, 
                    userBranchId, 
                    userRole,
                    message = "Debug endpoint working",
                    databaseStats = new {
                        paymentsCount,
                        expensesCount,
                        studentsCount
                    },
                    samplePayments = paymentsWithStudents
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Debug error", error = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        [HttpGet("debug-all-transactions")]
        [Authorize(Policy = "AdminOrEmployee")]
        public async Task<IActionResult> GetDebugAllTransactions([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var userBranchId = int.Parse(User.FindFirst("BranchId")?.Value ?? "0");
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

                // Get all payments - fetch data first, then project
                var paymentsQuery = _context.Payments
                    .Include(p => p.Student)
                    .Include(p => p.CourseRegistration)
                        .ThenInclude(cr => cr.Student)
                    .Include(p => p.CourseRegistration)
                        .ThenInclude(cr => cr.Course)
                            .ThenInclude(c => c.Branch)
                    .Include(p => p.ProcessedByUser)
                    .Where(p => p.IsActive);

                // Filter by branch if not admin
                if (userRole != "Admin")
                {
                    paymentsQuery = paymentsQuery.Where(p => 
                        (p.CourseRegistration != null && p.CourseRegistration.Course != null && p.CourseRegistration.Course.BranchId == userBranchId) ||
                        (p.Student != null && p.Student.BranchId == userBranchId)
                    );
                }

                if (startDate.HasValue)
                    paymentsQuery = paymentsQuery.Where(p => p.PaymentDate >= startDate.Value);

                if (endDate.HasValue)
                    paymentsQuery = paymentsQuery.Where(p => p.PaymentDate <= endDate.Value);

                // Execute query first, then project on client side
                var paymentsData = await paymentsQuery
                    .OrderByDescending(p => p.PaymentDate)
                    .ToListAsync();

                var payments = paymentsData.Select(p => new
                {
                    Id = p.Id,
                    StudentName = p.Student?.FullName ?? p.CourseRegistration?.Student?.FullName ?? string.Empty,
                    CourseName = p.CourseRegistration?.Course?.Name ?? string.Empty,
                    Amount = p.Amount,
                    PaymentMethod = p.PaymentMethod.ToString(),
                    PaymentMethodArabic = GetPaymentMethodArabic(p.PaymentMethod),
                    PaymentType = p.PaymentType.ToString(),
                    PaymentTypeArabic = GetPaymentTypeArabic(p.PaymentType),
                    PaymentDate = p.PaymentDate,
                    ProcessedBy = p.ProcessedByUser?.FullName ?? string.Empty,
                    Notes = p.Notes,
                    RegistrationId = p.CourseRegistrationId ?? 0,
                    BranchName = p.CourseRegistration?.Course?.Branch?.Name ?? string.Empty,
                    TransactionType = "income",
                    PaymentStatus = "paid",
                    Category = "التدريب",
                    Description = p.Notes ?? "دفعة تدريب",
                    InvoiceUrl = $"/api/invoices/payment/{p.Id}"
                }).ToList();

                // Get only actual expenses (not income records that duplicate payments)
                var expensesQuery = _context.Expenses
                    .Include(e => e.Branch)
                    .Include(e => e.RequestedByUser)
                    .Where(e => e.BranchId == userBranchId && 
                               e.Category != ExpenseCategory.Training && // Exclude training income
                               e.Amount < 0); // Only actual expenses (negative amounts)

                if (startDate.HasValue)
                    expensesQuery = expensesQuery.Where(e => e.ExpenseDate >= startDate.Value);

                if (endDate.HasValue)
                    expensesQuery = expensesQuery.Where(e => e.ExpenseDate <= endDate.Value);

                // Execute query first, then project on client side
                var expensesData = await expensesQuery
                    .OrderByDescending(e => e.ExpenseDate)
                    .ToListAsync();

                var expenses = expensesData.Select(e => new
                {
                    Id = e.Id + 1000000, // Unique ID for expenses
                    StudentName = string.Empty,
                    CourseName = string.Empty,
                    Amount = Math.Abs(e.Amount),
                    PaymentMethod = e.PaymentMethod.ToString(),
                    PaymentMethodArabic = GetPaymentMethodArabic(e.PaymentMethod),
                    PaymentType = "Expense",
                    PaymentTypeArabic = GetExpenseCategoryArabic(e.Category),
                    PaymentDate = e.ExpenseDate,
                    ProcessedBy = e.RequestedByUser?.FullName ?? string.Empty,
                    Notes = e.Description,
                    RegistrationId = 0,
                    BranchName = e.Branch?.Name ?? string.Empty,
                    TransactionType = e.Amount >= 0 ? "income" : "expense",
                    PaymentStatus = e.Status == ExpenseStatus.Paid ? "paid" : "pending",
                    Category = GetExpenseCategoryArabic(e.Category),
                    Description = e.Description,
                    InvoiceUrl = $"/api/invoices/expense/{e.Id}"
                }).ToList();

                // Combine both results
                var allTransactions = payments.Concat(expenses)
                    .OrderByDescending(t => t.PaymentDate)
                    .ToList();

                // Calculate totals - only from actual payments
                var totalIncome = payments.Sum(p => p.Amount);
                var totalExpenses = expenses.Sum(e => e.Amount);

                return Ok(new { 
                    success = true, 
                    userBranchId,
                    userRole,
                    totalPayments = payments.Count,
                    totalExpensesCount = expenses.Count,
                    totalTransactions = allTransactions.Count,
                    totalIncome,
                    totalExpenses,
                    netBalance = totalIncome - totalExpenses,
                    payments = payments.Take(5), // Show first 5 for debugging
                    expenses = expenses.Take(5), // Show first 5 for debugging
                    allTransactions = allTransactions.Take(10) // Show first 10 for debugging
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "حدث خطأ في الخادم", error = ex.Message });
            }
        }

        [HttpPost("test-create-payment")]
        [Authorize(Policy = "AdminOrEmployee")]
        public async Task<IActionResult> TestCreatePayment()
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var userBranchId = int.Parse(User.FindFirst("BranchId")?.Value ?? "0");

                // Find a course registration to test with
                var registration = await _context.CourseRegistrations
                    .Include(cr => cr.Student)
                    .Include(cr => cr.Course)
                    .FirstOrDefaultAsync(cr => cr.PaidAmount > 0);

                if (registration == null)
                {
                    return BadRequest(new { message = "لا توجد تسجيلات كورسات للاختبار" });
                }

                // Create a test payment record
                var testPayment = new Payment
                {
                    StudentId = registration.StudentId,
                    CourseRegistrationId = registration.Id,
                    BranchId = registration.Course.BranchId,
                    Amount = 100, // Test amount
                    PaymentMethod = PaymentMethod.Cash,
                    PaymentType = PaymentType.CourseFee,
                    PaymentSource = PaymentSource.CourseFee,
                    PaymentDate = DateTime.UtcNow,
                    ProcessedByUserId = userId,
                    Notes = "اختبار إنشاء سجل دفع",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Payments.Add(testPayment);
                await _context.SaveChangesAsync();

                return Ok(new { 
                    success = true, 
                    message = "تم إنشاء سجل دفع تجريبي بنجاح",
                    paymentId = testPayment.Id,
                    studentId = testPayment.StudentId,
                    courseRegistrationId = testPayment.CourseRegistrationId,
                    branchId = testPayment.BranchId,
                    amount = testPayment.Amount,
                    userBranchId = userBranchId
                });
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

        private string GetPaymentSourceArabic(PaymentSource source)
        {
            return source switch
            {
                PaymentSource.Tuition => "الرسوم الدراسية",
                PaymentSource.CourseFee => "رسوم الكورس",
                PaymentSource.Cafeteria => "الكافيتريا",
                PaymentSource.Workspace => "مساحة العمل",
                PaymentSource.Equipment => "المعدات",
                PaymentSource.Other => "أخرى",
                _ => source.ToString()
            };
        }

        private string GetExpenseCategoryArabic(ExpenseCategory category)
        {
            return category switch
            {
                ExpenseCategory.Equipment => "معدات",
                ExpenseCategory.Maintenance => "صيانة",
                ExpenseCategory.Utilities => "مرافق",
                ExpenseCategory.Supplies => "مستلزمات",
                ExpenseCategory.Marketing => "تسويق",
                ExpenseCategory.Salaries => "رواتب",
                ExpenseCategory.Rent => "إيجار",
                ExpenseCategory.Transportation => "مواصلات",
                ExpenseCategory.Food => "طعام",
                ExpenseCategory.Training => "تدريب",
                ExpenseCategory.Software => "برمجيات",
                ExpenseCategory.Insurance => "تأمين",
                ExpenseCategory.Legal => "قانونية",
                ExpenseCategory.Other => "أخرى",
                _ => category.ToString()
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
        public decimal TotalPaid { get; set; }
        public decimal OutstandingBalance { get; set; }
        public List<object> PaymentHistory { get; set; } = new();
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
        public string? DriveLink { get; set; }
    }
}
