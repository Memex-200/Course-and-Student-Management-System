using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Api.Data;
using Api.Models;
using System.Security.Claims;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SharedWorkspaceController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SharedWorkspaceController(ApplicationDbContext context)
        {
            _context = context;
        }

        // الحصول على جميع المساحات المشتركة
        [HttpGet("spaces")]
        public async Task<IActionResult> GetSharedWorkspaces()
        {
            try
            {
                var userBranchId = int.Parse(User.FindFirst("BranchId")?.Value ?? "0");
                
                // Only Assiut branch has shared workspace
                var branch = await _context.Branches.FindAsync(userBranchId);
                if (branch?.Name != "أسيوط")
                {
                    return BadRequest(new { message = "خدمة الـ Workspace المشترك متاحة فقط في فرع أسيوط" });
                }

                var workspaces = await _context.SharedWorkspaces
                    .Where(sw => sw.BranchId == userBranchId && sw.IsActive)
                    .Select(sw => new
                    {
                        sw.Id,
                        sw.Name,
                        sw.Description,
                        sw.MaxCapacity,
                        sw.CurrentOccupancy,
                        AvailableSpots = sw.MaxCapacity - sw.CurrentOccupancy,
                        sw.HourlyRatePerPerson,
                        sw.HasWifi,
                        sw.HasPrinter,
                        sw.HasProjector,
                        sw.HasWhiteboard,
                        sw.Equipment,
                        Status = sw.Status.ToString(),
                        StatusArabic = GetSharedWorkspaceStatusArabic(sw.Status),
                        ActiveBookings = sw.Bookings.Count(b => b.Status == WorkspaceBookingStatus.InProgress),
                        TodayBookings = sw.Bookings.Count(b => b.StartTime.Date == DateTime.Today)
                    })
                    .ToListAsync();

                return Ok(workspaces);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "حدث خطأ في الخادم", error = ex.Message });
            }
        }

        // الحصول على تفاصيل مساحة مشتركة محددة
        [HttpGet("spaces/{id}")]
        public async Task<IActionResult> GetSharedWorkspace(int id)
        {
            try
            {
                var userBranchId = int.Parse(User.FindFirst("BranchId")?.Value ?? "0");
                
                var workspace = await _context.SharedWorkspaces
                    .Include(sw => sw.Bookings.Where(b => b.StartTime.Date >= DateTime.Today))
                        .ThenInclude(b => b.Student)
                    .FirstOrDefaultAsync(sw => sw.Id == id && sw.BranchId == userBranchId);

                if (workspace == null)
                    return NotFound(new { message = "المساحة المشتركة غير موجودة" });

                var result = new
                {
                    workspace.Id,
                    workspace.Name,
                    workspace.Description,
                    workspace.MaxCapacity,
                    workspace.CurrentOccupancy,
                    AvailableSpots = workspace.MaxCapacity - workspace.CurrentOccupancy,
                    workspace.HourlyRatePerPerson,
                    workspace.HasWifi,
                    workspace.HasPrinter,
                    workspace.HasProjector,
                    workspace.HasWhiteboard,
                    workspace.Equipment,
                    Status = workspace.Status.ToString(),
                    StatusArabic = GetSharedWorkspaceStatusArabic(workspace.Status),
                    UpcomingBookings = workspace.Bookings
                        .Where(b => b.StartTime >= DateTime.Now)
                        .OrderBy(b => b.StartTime)
                        .Select(b => new
                        {
                            b.Id,
                            Customer = b.Student?.FullName ?? b.CustomerName,
                            b.StartTime,
                            b.EndTime,
                            b.NumberOfPeople,
                            BookingType = b.BookingType.ToString(),
                            BookingTypeArabic = GetBookingTypeArabic(b.BookingType),
                            Status = b.Status.ToString(),
                            StatusArabic = GetWorkspaceBookingStatusArabic(b.Status)
                        }).ToList()
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "حدث خطأ في الخادم", error = ex.Message });
            }
        }

        // إنشاء حجز مشترك جديد
        [HttpPost("book")]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> CreateSharedWorkspaceBooking([FromBody] CreateSharedWorkspaceBookingRequest request)
        {
            try
            {
                var userBranchId = int.Parse(User.FindFirst("BranchId")?.Value ?? "0");
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

                // التحقق من المساحة المشتركة
                var workspace = await _context.SharedWorkspaces
                    .FirstOrDefaultAsync(sw => sw.Id == request.SharedWorkspaceId && sw.BranchId == userBranchId);

                if (workspace == null)
                    return NotFound(new { message = "المساحة المشتركة غير موجودة" });

                // التحقق من السعة المتاحة
                var currentOccupancy = await GetCurrentOccupancy(request.SharedWorkspaceId, request.StartTime, request.EndTime);
                if (currentOccupancy + request.NumberOfPeople > workspace.MaxCapacity)
                {
                    return BadRequest(new { 
                        message = $"عدد الأشخاص المطلوب ({request.NumberOfPeople}) يتجاوز السعة المتاحة. السعة المتاحة: {workspace.MaxCapacity - currentOccupancy}" 
                    });
                }

                // حساب التكلفة
                var totalHours = (decimal)(request.EndTime - request.StartTime).TotalHours;
                var totalAmount = totalHours * request.NumberOfPeople * workspace.HourlyRatePerPerson;

                var booking = new SharedWorkspaceBooking
                {
                    SharedWorkspaceId = request.SharedWorkspaceId,
                    StudentId = request.StudentId,
                    CustomerName = request.CustomerName,
                    CustomerPhone = request.CustomerPhone,
                    CustomerEmail = request.CustomerEmail,
                    StartTime = request.StartTime,
                    EndTime = request.EndTime,
                    NumberOfPeople = request.NumberOfPeople,
                    HourlyRate = workspace.HourlyRatePerPerson,
                    TotalHours = totalHours,
                    TotalAmount = totalAmount,
                    PaidAmount = request.PaidAmount,
                    PaymentMethod = request.PaymentMethod,
                    PaymentStatus = request.PaidAmount >= totalAmount
                        ? PaymentStatus.FullyPaid
                        : request.PaidAmount > 0
                            ? PaymentStatus.PartiallyPaid
                            : PaymentStatus.Pending,
                    BookingType = request.BookingType,
                    Purpose = request.Purpose,
                    Notes = request.Notes,
                    RequiredEquipment = request.RequiredEquipment,
                    NeedsInternet = request.NeedsInternet,
                    NeedsPrinter = request.NeedsPrinter,
                    NeedsProjector = request.NeedsProjector,
                    BranchId = userBranchId,
                    BookedByUserId = userId,
                    Status = WorkspaceBookingStatus.Confirmed
                };

                _context.SharedWorkspaceBookings.Add(booking);

                // إضافة سجل دفع إذا تم الدفع
                if (request.PaidAmount > 0)
                {
                    var payment = new Payment
                    {
                        SharedWorkspaceBookingId = booking.Id,
                        Amount = request.PaidAmount,
                        PaymentMethod = request.PaymentMethod,
                        PaymentDate = DateTime.UtcNow,
                        ProcessedByUserId = userId,
                        Notes = "دفعة حجز الـ Workspace المشترك"
                    };

                    _context.Payments.Add(payment);
                }

                await _context.SaveChangesAsync();

                return Ok(new { 
                    message = "تم حجز الـ Workspace المشترك بنجاح", 
                    bookingId = booking.Id,
                    totalAmount = totalAmount,
                    paidAmount = request.PaidAmount,
                    remainingAmount = totalAmount - request.PaidAmount
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "حدث خطأ في الخادم", error = ex.Message });
            }
        }

        // الحصول على جميع الحجوزات
        [HttpGet("bookings")]
        public async Task<IActionResult> GetSharedWorkspaceBookings([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var userBranchId = int.Parse(User.FindFirst("BranchId")?.Value ?? "0");
                
                var query = _context.SharedWorkspaceBookings
                    .Include(swb => swb.SharedWorkspace)
                    .Include(swb => swb.Student)
                    .Where(swb => swb.BranchId == userBranchId);

                if (startDate.HasValue)
                    query = query.Where(swb => swb.StartTime >= startDate.Value);

                if (endDate.HasValue)
                    query = query.Where(swb => swb.EndTime <= endDate.Value);

                var bookings = await query
                    .OrderByDescending(swb => swb.StartTime)
                    .Select(swb => new
                    {
                        swb.Id,
                        Workspace = swb.SharedWorkspace.Name,
                        Customer = swb.Student != null ? swb.Student.FullName : swb.CustomerName,
                        swb.CustomerPhone,
                        swb.StartTime,
                        swb.EndTime,
                        swb.NumberOfPeople,
                        swb.TotalHours,
                        swb.HourlyRate,
                        swb.TotalAmount,
                        swb.PaidAmount,
                        RemainingAmount = swb.RemainingAmount,
                        PaymentStatus = swb.PaymentStatus.ToString(),
                        PaymentStatusArabic = GetPaymentStatusArabic(swb.PaymentStatus),
                        BookingType = swb.BookingType.ToString(),
                        BookingTypeArabic = GetBookingTypeArabic(swb.BookingType),
                        Status = swb.Status.ToString(),
                        StatusArabic = GetWorkspaceBookingStatusArabic(swb.Status),
                        swb.Purpose,
                        swb.CheckInTime,
                        swb.CheckOutTime,
                        swb.CreatedAt
                    })
                    .ToListAsync();

                return Ok(bookings);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "حدث خطأ في الخادم", error = ex.Message });
            }
        }

        // تسجيل الدخول للمساحة المشتركة
        [HttpPost("bookings/{bookingId}/checkin")]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> CheckInToWorkspace(int bookingId, [FromBody] CheckInRequest request)
        {
            try
            {
                var booking = await _context.SharedWorkspaceBookings
                    .Include(swb => swb.SharedWorkspace)
                    .FirstOrDefaultAsync(swb => swb.Id == bookingId);

                if (booking == null)
                    return NotFound(new { message = "الحجز غير موجود" });

                if (booking.Status != WorkspaceBookingStatus.Confirmed)
                    return BadRequest(new { message = "الحجز غير مؤكد" });

                // تسجيل الدخول
                booking.CheckInTime = DateTime.UtcNow;
                booking.Status = WorkspaceBookingStatus.InProgress;

                // إنشاء سجل حضور
                var occupancy = new WorkspaceOccupancy
                {
                    SharedWorkspaceId = booking.SharedWorkspaceId,
                    SharedWorkspaceBookingId = booking.Id,
                    CheckInTime = DateTime.UtcNow,
                    ActualPeopleCount = request.ActualPeopleCount > 0 ? request.ActualPeopleCount : booking.NumberOfPeople,
                    Notes = request.Notes
                };

                _context.WorkspaceOccupancies.Add(occupancy);

                // تحديث العدد الحالي في المساحة
                await UpdateWorkspaceOccupancy(booking.SharedWorkspaceId);

                await _context.SaveChangesAsync();

                return Ok(new { message = "تم تسجيل الدخول بنجاح" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "حدث خطأ في الخادم", error = ex.Message });
            }
        }

        // تسجيل الخروج من المساحة المشتركة
        [HttpPost("bookings/{bookingId}/checkout")]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> CheckOutFromWorkspace(int bookingId, [FromBody] CheckOutRequest request)
        {
            try
            {
                var booking = await _context.SharedWorkspaceBookings
                    .Include(swb => swb.SharedWorkspace)
                    .FirstOrDefaultAsync(swb => swb.Id == bookingId);

                if (booking == null)
                    return NotFound(new { message = "الحجز غير موجود" });

                if (booking.Status != WorkspaceBookingStatus.InProgress)
                    return BadRequest(new { message = "الحجز ليس قيد التنفيذ" });

                // تسجيل الخروج
                booking.CheckOutTime = DateTime.UtcNow;
                booking.Status = WorkspaceBookingStatus.Completed;

                // تحديث سجل الحضور
                var occupancy = await _context.WorkspaceOccupancies
                    .FirstOrDefaultAsync(wo => wo.SharedWorkspaceBookingId == bookingId && wo.IsActive);

                if (occupancy != null)
                {
                    occupancy.CheckOutTime = DateTime.UtcNow;
                    occupancy.IsActive = false;
                    occupancy.Notes += $"\n{request.Notes}";
                }

                // تحديث العدد الحالي في المساحة
                await UpdateWorkspaceOccupancy(booking.SharedWorkspaceId);

                await _context.SaveChangesAsync();

                return Ok(new { message = "تم تسجيل الخروج بنجاح" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "حدث خطأ في الخادم", error = ex.Message });
            }
        }

        // دالة مساعدة لحساب العدد الحالي في فترة زمنية معينة
        private async Task<int> GetCurrentOccupancy(int workspaceId, DateTime startTime, DateTime endTime)
        {
            var overlappingBookings = await _context.SharedWorkspaceBookings
                .Where(swb => swb.SharedWorkspaceId == workspaceId &&
                             swb.Status != WorkspaceBookingStatus.Cancelled &&
                             swb.StartTime < endTime && swb.EndTime > startTime)
                .SumAsync(swb => swb.NumberOfPeople);

            return overlappingBookings;
        }

        // دالة مساعدة لتحديث العدد الحالي في المساحة
        private async Task UpdateWorkspaceOccupancy(int workspaceId)
        {
            var workspace = await _context.SharedWorkspaces.FindAsync(workspaceId);
            if (workspace != null)
            {
                var currentOccupancy = await _context.WorkspaceOccupancies
                    .Where(wo => wo.SharedWorkspaceId == workspaceId && wo.IsActive)
                    .SumAsync(wo => wo.ActualPeopleCount);

                workspace.CurrentOccupancy = currentOccupancy;
                workspace.Status = currentOccupancy >= workspace.MaxCapacity 
                    ? SharedWorkspaceStatus.Full 
                    : currentOccupancy > 0 
                        ? SharedWorkspaceStatus.Occupied 
                        : SharedWorkspaceStatus.Available;
            }
        }

        // Helper Methods
        private string GetSharedWorkspaceStatusArabic(SharedWorkspaceStatus status)
        {
            return status switch
            {
                SharedWorkspaceStatus.Available => "متاح",
                SharedWorkspaceStatus.Occupied => "مشغول",
                SharedWorkspaceStatus.Full => "ممتلئ",
                SharedWorkspaceStatus.Maintenance => "صيانة",
                _ => status.ToString()
            };
        }

        private string GetBookingTypeArabic(BookingType type)
        {
            return type switch
            {
                BookingType.Individual => "فردي",
                BookingType.Group => "جماعي",
                BookingType.Study => "دراسة",
                BookingType.Meeting => "اجتماع",
                BookingType.Project => "مشروع",
                _ => type.ToString()
            };
        }

        private string GetWorkspaceBookingStatusArabic(WorkspaceBookingStatus status)
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
    }

    // Request Models
    public class CreateSharedWorkspaceBookingRequest
    {
        public int SharedWorkspaceId { get; set; }
        public int? StudentId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int NumberOfPeople { get; set; } = 1;
        public decimal PaidAmount { get; set; } = 0;
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;
        public BookingType BookingType { get; set; } = BookingType.Individual;
        public string Purpose { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public string RequiredEquipment { get; set; } = string.Empty;
        public bool NeedsInternet { get; set; } = true;
        public bool NeedsPrinter { get; set; } = false;
        public bool NeedsProjector { get; set; } = false;
    }

    public class CheckInRequest
    {
        public int ActualPeopleCount { get; set; }
        public string Notes { get; set; } = string.Empty;
    }

    public class CheckOutRequest
    {
        public string Notes { get; set; } = string.Empty;
    }
}
