using Api.Data;
using Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    
    public class WorkspaceController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public WorkspaceController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("bookings")]
        public async Task<IActionResult> GetWorkspaceBookings([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var userBranchId = int.Parse(User.FindFirst("BranchId")?.Value ?? "0");
                
                // Only Assiut branch has workspace
                var branch = await _context.Branches.FindAsync(userBranchId);
                if (branch?.Name != "أسيوط")
                {
                    return BadRequest(new { message = "خدمة الـ Workspace متاحة فقط في فرع أسيوط" });
                }

                var query = _context.WorkspaceBookings
                    .Include(wb => wb.Student)
                    .Include(wb => wb.Room)
                    .Include(wb => wb.Branch)
                    .Where(wb => wb.BranchId == userBranchId);

                if (startDate.HasValue)
                    query = query.Where(wb => wb.StartTime >= startDate.Value);

                if (endDate.HasValue)
                    query = query.Where(wb => wb.EndTime <= endDate.Value);

                var bookings = await query
                    .OrderByDescending(wb => wb.StartTime)
                    .Select(wb => new
                    {
                        wb.Id,
                        wb.StartTime,
                        wb.EndTime,
                        wb.HourlyRate,
                        TotalHours = wb.HoursBooked,
                        wb.TotalAmount,
                        wb.PaidAmount,
                        PaymentStatus = wb.PaymentStatus.ToString(),
                        PaymentStatusArabic = GetPaymentStatusArabic(wb.PaymentStatus),
                        PaymentMethod = wb.PaymentMethod.ToString(),
                        PaymentMethodArabic = GetPaymentMethodArabic(wb.PaymentMethod),
                        Status = wb.Status.ToString(),
                        StatusArabic = GetWorkspaceStatusArabic(wb.Status),
                        Purpose = "حجز مساحة عمل",
                        wb.Notes,
                        Student = wb.Student != null ? wb.Student.FullName : null,
                        Room = wb.Room != null ? wb.Room.Name : null,
                        RemainingAmount = wb.RemainingAmount,
                        wb.CreatedAt
                    })
                    .ToListAsync();

                return Ok(bookings);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "حدث خطأ في الخادم", error = ex.Message });
            }
        }

        [HttpPost("book")]
        
        public async Task<IActionResult> CreateWorkspaceBooking([FromBody] CreateWorkspaceBookingRequest request)
        {
            try
            {
                var userBranchId = int.Parse(User.FindFirst("BranchId")?.Value ?? "0");
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

                // Only Assiut branch has workspace
                var branch = await _context.Branches.FindAsync(userBranchId);
                if (branch?.Name != "أسيوط")
                {
                    return BadRequest(new { message = "خدمة الـ Workspace متاحة فقط في فرع أسيوط" });
                }

                // Calculate total hours and amount
                var totalHours = (decimal)(request.EndTime - request.StartTime).TotalHours;
                var totalAmount = totalHours * request.HourlyRate;

                var booking = new WorkspaceBooking
                {
                    StudentId = request.StudentId ?? 0,
                    RoomId = 1, // Default room for now
                    BookingDate = DateTime.UtcNow.Date,
                    StartTime = request.StartTime,
                    EndTime = request.EndTime,
                    HoursBooked = (int)totalHours,
                    HourlyRate = request.HourlyRate,
                    TotalAmount = totalAmount,
                    PaidAmount = request.PaidAmount,
                    RemainingAmount = totalAmount - request.PaidAmount,
                    PaymentMethod = request.PaymentMethod,
                    PaymentStatus = request.PaidAmount >= totalAmount
                        ? PaymentStatus.FullyPaid
                        : request.PaidAmount > 0
                            ? PaymentStatus.PartiallyPaid
                            : PaymentStatus.Unpaid,
                    Notes = request.Notes,
                    BranchId = userBranchId,
                    BookedByUserId = userId,
                    Status = WorkspaceBookingStatus.Confirmed,
                    CreatedAt = DateTime.UtcNow
                };

                _context.WorkspaceBookings.Add(booking);

                // Add payment record if amount paid
                if (request.PaidAmount > 0)
                {
                    var payment = new Payment
                    {
                        WorkspaceBookingId = booking.Id,
                        Amount = request.PaidAmount,
                        PaymentMethod = request.PaymentMethod,
                        PaymentDate = DateTime.UtcNow,
                        ProcessedByUserId = userId,
                        Notes = "دفعة حجز الـ Workspace"
                    };

                    _context.Payments.Add(payment);
                }

                await _context.SaveChangesAsync();

                return Ok(new { message = "تم حجز الـ Workspace بنجاح", bookingId = booking.Id });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "حدث خطأ في الخادم", error = ex.Message });
            }
        }

        [HttpPut("bookings/{id}/end")]
        
        public async Task<IActionResult> EndWorkspaceBooking(int id, [FromBody] EndWorkspaceBookingRequest request)
        {
            try
            {
                var booking = await _context.WorkspaceBookings.FindAsync(id);
                if (booking == null)
                    return NotFound(new { message = "الحجز غير موجود" });

                if (booking.Status != WorkspaceBookingStatus.InProgress)
                    return BadRequest(new { message = "الحجز غير نشط" });

                var actualEndTime = request.ActualEndTime ?? DateTime.UtcNow;
                var actualTotalHours = (int)(actualEndTime - booking.StartTime).TotalHours;
                var actualTotalAmount = actualTotalHours * booking.HourlyRate;

                booking.EndTime = actualEndTime;
                booking.HoursBooked = actualTotalHours;
                booking.TotalAmount = actualTotalAmount;
                booking.RemainingAmount = actualTotalAmount - booking.PaidAmount;
                booking.Status = WorkspaceBookingStatus.Completed;
                booking.Notes += $"\nانتهى في: {actualEndTime:yyyy-MM-dd HH:mm}";

                // Update payment status based on new total
                if (booking.PaidAmount >= actualTotalAmount)
                    booking.PaymentStatus = PaymentStatus.FullyPaid;
                else if (booking.PaidAmount > 0)
                    booking.PaymentStatus = PaymentStatus.PartiallyPaid;
                else
                    booking.PaymentStatus = PaymentStatus.Unpaid;

                await _context.SaveChangesAsync();

                return Ok(new 
                { 
                    message = "تم إنهاء الحجز بنجاح", 
                    actualTotalHours = actualTotalHours,
                    actualTotalAmount = actualTotalAmount,
                    remainingAmount = actualTotalAmount - booking.PaidAmount
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "حدث خطأ في الخادم", error = ex.Message });
            }
        }

        [HttpPost("bookings/{id}/payment")]
        
        public async Task<IActionResult> AddWorkspacePayment(int id, [FromBody] AddPaymentRequest request)
        {
            try
            {
                var booking = await _context.WorkspaceBookings.FindAsync(id);
                if (booking == null)
                    return NotFound(new { message = "الحجز غير موجود" });

                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

                var payment = new Payment
                {
                    WorkspaceBookingId = id,
                    Amount = request.Amount,
                    PaymentMethod = request.PaymentMethod,
                    PaymentDate = DateTime.UtcNow,
                    ProcessedByUserId = userId,
                    Notes = request.Notes
                };

                _context.Payments.Add(payment);

                // Update booking payment status
                booking.PaidAmount += request.Amount;
                if (booking.PaidAmount >= booking.TotalAmount)
                    booking.PaymentStatus = PaymentStatus.FullyPaid;
                else if (booking.PaidAmount > 0)
                    booking.PaymentStatus = PaymentStatus.PartiallyPaid;

                await _context.SaveChangesAsync();

                return Ok(new { message = "تم إضافة الدفعة بنجاح", paymentId = payment.Id });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "حدث خطأ في الخادم", error = ex.Message });
            }
        }

        [HttpGet("invoices")]
        public async Task<IActionResult> GetWorkspaceInvoices([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var userBranchId = int.Parse(User.FindFirst("BranchId")?.Value ?? "0");

                var query = _context.WorkspaceBookings
                    .Include(wb => wb.Student)
                    .Include(wb => wb.Room)
                    .Where(wb => wb.BranchId == userBranchId && wb.Status == WorkspaceBookingStatus.Completed);

                if (startDate.HasValue)
                    query = query.Where(wb => wb.CreatedAt >= startDate.Value);

                if (endDate.HasValue)
                    query = query.Where(wb => wb.CreatedAt <= endDate.Value);

                var invoices = await query
                    .OrderByDescending(wb => wb.CreatedAt)
                    .Select(wb => new
                    {
                        wb.Id,
                        InvoiceNumber = $"WS-{wb.Id:D6}",
                        wb.StartTime,
                        wb.EndTime,
                        TotalHours = wb.HoursBooked,
                        wb.HourlyRate,
                        wb.TotalAmount,
                        wb.PaidAmount,
                        RemainingAmount = wb.RemainingAmount,
                        PaymentStatus = wb.PaymentStatus.ToString(),
                        PaymentStatusArabic = GetPaymentStatusArabic(wb.PaymentStatus),
                        Customer = wb.Student != null ? wb.Student.FullName : "زبون",
                        CustomerPhone = wb.Student != null ? wb.Student.Phone : "",
                        Purpose = "حجز مساحة عمل",
                        wb.CreatedAt
                    })
                    .ToListAsync();

                return Ok(invoices);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "حدث خطأ في الخادم", error = ex.Message });
            }
        }

        [HttpGet("statistics")]
        public async Task<IActionResult> GetWorkspaceStatistics([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var userBranchId = int.Parse(User.FindFirst("BranchId")?.Value ?? "0");

                var query = _context.WorkspaceBookings
                    .Where(wb => wb.BranchId == userBranchId);

                if (startDate.HasValue)
                    query = query.Where(wb => wb.CreatedAt >= startDate.Value);

                if (endDate.HasValue)
                    query = query.Where(wb => wb.CreatedAt <= endDate.Value);

                var bookings = await query.ToListAsync();

                var statistics = new
                {
                    TotalBookings = bookings.Count,
                    ActiveBookings = bookings.Count(b => b.Status == WorkspaceBookingStatus.InProgress),
                    CompletedBookings = bookings.Count(b => b.Status == WorkspaceBookingStatus.Completed),
                    TotalHours = bookings.Sum(b => b.HoursBooked),
                    TotalRevenue = bookings.Sum(b => b.TotalAmount),
                    TotalPaid = bookings.Sum(b => b.PaidAmount),
                    TotalPending = bookings.Sum(b => b.RemainingAmount),
                    AverageHourlyRate = bookings.Any() ? bookings.Average(b => b.HourlyRate) : 0,
                    AverageSessionDuration = bookings.Any() ? bookings.Average(b => b.HoursBooked) : 0
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
                PaymentStatus.PartiallyPaid => "مدفوع جزئياً",
                PaymentStatus.FullyPaid => "مدفوع بالكامل",
                PaymentStatus.Unpaid => "غير مدفوع",
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

        private string GetWorkspaceStatusArabic(WorkspaceBookingStatus status)
        {
            return status switch
            {
                WorkspaceBookingStatus.Pending => "في الانتظار",
                WorkspaceBookingStatus.Confirmed => "مؤكد",
                WorkspaceBookingStatus.InProgress => "قيد التنفيذ",
                WorkspaceBookingStatus.Completed => "مكتمل",
                WorkspaceBookingStatus.Cancelled => "ملغي",
                _ => status.ToString()
            };
        }
    }

    public class CreateWorkspaceBookingRequest
    {
        public int? StudentId { get; set; }
        public int? EmployeeId { get; set; }

        [MaxLength(100)]
        public string CustomerName { get; set; } = string.Empty;

        [MaxLength(20)]
        public string CustomerPhone { get; set; } = string.Empty;

        [Required]
        public DateTime StartTime { get; set; }

        [Required]
        public DateTime EndTime { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal HourlyRate { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal PaidAmount { get; set; }

        [Required]
        public PaymentMethod PaymentMethod { get; set; }

        [MaxLength(500)]
        public string Purpose { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string Notes { get; set; } = string.Empty;
    }

    public class EndWorkspaceBookingRequest
    {
        public DateTime? ActualEndTime { get; set; }
    }

    public class AddPaymentRequest
    {
        [Required]
        [Range(0, double.MaxValue)]
        public decimal Amount { get; set; }

        [Required]
        public PaymentMethod PaymentMethod { get; set; }

        [MaxLength(500)]
        public string Notes { get; set; } = string.Empty;
    }
}
