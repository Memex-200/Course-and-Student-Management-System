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
    
    public class RoomsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public RoomsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetRooms([FromQuery] int? branchId = null)
        {
            try
            {
                var userBranchId = int.Parse(User.FindFirst("BranchId")?.Value ?? "0");
                var query = _context.Rooms
                    .Include(r => r.Branch)
                    .AsQueryable();

                // Filter by branch
                if (branchId.HasValue)
                    query = query.Where(r => r.BranchId == branchId.Value);
                else
                    query = query.Where(r => r.BranchId == userBranchId);

                var rooms = await query
                    .Where(r => r.IsActive)
                    .OrderBy(r => r.RoomNumber)
                    .Select(r => new
                    {
                        r.Id,
                        r.Name,
                        r.RoomNumber,
                        r.Description,
                        r.RoomType,
                        r.Capacity,
                        r.Equipment,
                        r.Location,
                        Branch = r.Branch.Name,
                        CurrentReservations = r.RoomReservations
                            .Where(rr => rr.Status == RoomReservationStatus.Scheduled || rr.Status == RoomReservationStatus.InProgress)
                            .Count()
                    })
                    .ToListAsync();

                return Ok(rooms);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "حدث خطأ في الخادم", error = ex.Message });
            }
        }

        [HttpGet("{id}/reservations")]
        public async Task<IActionResult> GetRoomReservations(int id, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var room = await _context.Rooms.FindAsync(id);
                if (room == null)
                    return NotFound(new { message = "الغرفة غير موجودة" });

                var query = _context.RoomReservations
                    .Include(rr => rr.ReservedByUser)
                    .Include(rr => rr.Course)
                    .Include(rr => rr.Instructor)
                        .ThenInclude(i => i!.User)
                    .Where(rr => rr.RoomId == id);

                if (startDate.HasValue)
                    query = query.Where(rr => rr.StartDateTime >= startDate.Value);

                if (endDate.HasValue)
                    query = query.Where(rr => rr.EndDateTime <= endDate.Value);

                var reservations = await query
                    .OrderBy(rr => rr.StartDateTime)
                    .Select(rr => new
                    {
                        rr.Id,
                        rr.Title,
                        rr.Description,
                        rr.ReservationType,
                        rr.StartDateTime,
                        rr.EndDateTime,
                        rr.Status,
                        rr.ExpectedAttendees,
                        rr.ActualAttendees,
                        ReservedBy = rr.ReservedByUser.FullName,
                        Course = rr.Course != null ? rr.Course.Name : null,
                        Instructor = rr.Instructor != null ? rr.Instructor.User.FullName : null,
                        rr.RequiredEquipment,
                        rr.Notes
                    })
                    .ToListAsync();

                return Ok(reservations);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "حدث خطأ في الخادم", error = ex.Message });
            }
        }

        [HttpPost("{id}/reservations")]
        
        public async Task<IActionResult> CreateReservation(int id, [FromBody] CreateReservationRequest request)
        {
            try
            {
                var room = await _context.Rooms.FindAsync(id);
                if (room == null)
                    return NotFound(new { message = "الغرفة غير موجودة" });

                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

                // Check for conflicts
                var hasConflict = await _context.RoomReservations
                    .AnyAsync(rr => rr.RoomId == id &&
                                   rr.Status != RoomReservationStatus.Cancelled &&
                                   ((request.StartDateTime >= rr.StartDateTime && request.StartDateTime < rr.EndDateTime) ||
                                    (request.EndDateTime > rr.StartDateTime && request.EndDateTime <= rr.EndDateTime) ||
                                    (request.StartDateTime <= rr.StartDateTime && request.EndDateTime >= rr.EndDateTime)));

                if (hasConflict)
                    return BadRequest(new { message = "يوجد تعارض في المواعيد مع حجز آخر" });

                var reservation = new RoomReservation
                {
                    RoomId = id,
                    Title = request.Title,
                    Description = request.Description,
                    ReservationType = request.ReservationType,
                    StartDateTime = request.StartDateTime,
                    EndDateTime = request.EndDateTime,
                    ExpectedAttendees = request.ExpectedAttendees,
                    ReservedByUserId = userId,
                    CourseId = request.CourseId,
                    InstructorId = request.InstructorId,
                    RequiredEquipment = request.RequiredEquipment,
                    SpecialRequirements = request.SpecialRequirements,
                    Notes = request.Notes,
                    Status = RoomReservationStatus.Scheduled,
                    CreatedAt = DateTime.UtcNow
                };

                _context.RoomReservations.Add(reservation);
                await _context.SaveChangesAsync();

                return Ok(new { message = "تم إنشاء الحجز بنجاح", reservationId = reservation.Id });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "حدث خطأ في الخادم", error = ex.Message });
            }
        }

        [HttpPut("reservations/{reservationId}")]
        
        public async Task<IActionResult> UpdateReservation(int reservationId, [FromBody] UpdateReservationRequest request)
        {
            try
            {
                var reservation = await _context.RoomReservations.FindAsync(reservationId);
                if (reservation == null)
                    return NotFound(new { message = "الحجز غير موجود" });

                // Check for conflicts (excluding current reservation)
                var hasConflict = await _context.RoomReservations
                    .AnyAsync(rr => rr.RoomId == reservation.RoomId &&
                                   rr.Id != reservationId &&
                                   rr.Status != RoomReservationStatus.Cancelled &&
                                   ((request.StartDateTime >= rr.StartDateTime && request.StartDateTime < rr.EndDateTime) ||
                                    (request.EndDateTime > rr.StartDateTime && request.EndDateTime <= rr.EndDateTime) ||
                                    (request.StartDateTime <= rr.StartDateTime && request.EndDateTime >= rr.EndDateTime)));

                if (hasConflict)
                    return BadRequest(new { message = "يوجد تعارض في المواعيد مع حجز آخر" });

                reservation.Title = request.Title;
                reservation.Description = request.Description;
                reservation.ReservationType = request.ReservationType;
                reservation.StartDateTime = request.StartDateTime;
                reservation.EndDateTime = request.EndDateTime;
                reservation.ExpectedAttendees = request.ExpectedAttendees;
                reservation.CourseId = request.CourseId;
                reservation.InstructorId = request.InstructorId;
                reservation.RequiredEquipment = request.RequiredEquipment;
                reservation.SpecialRequirements = request.SpecialRequirements;
                reservation.Notes = request.Notes;
                reservation.Status = request.Status;
                reservation.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(new { message = "تم تحديث الحجز بنجاح" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "حدث خطأ في الخادم", error = ex.Message });
            }
        }

        [HttpDelete("reservations/{reservationId}")]
        
        public async Task<IActionResult> CancelReservation(int reservationId, [FromBody] CancelReservationRequest request)
        {
            try
            {
                var reservation = await _context.RoomReservations.FindAsync(reservationId);
                if (reservation == null)
                    return NotFound(new { message = "الحجز غير موجود" });

                reservation.Status = RoomReservationStatus.Cancelled;
                reservation.CancellationReason = request.CancellationReason;
                reservation.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(new { message = "تم إلغاء الحجز بنجاح" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "حدث خطأ في الخادم", error = ex.Message });
            }
        }

        [HttpGet("availability")]
        public async Task<IActionResult> CheckAvailability([FromQuery] int roomId, [FromQuery] DateTime startDateTime, [FromQuery] DateTime endDateTime)
        {
            try
            {
                var hasConflict = await _context.RoomReservations
                    .AnyAsync(rr => rr.RoomId == roomId &&
                                   rr.Status != RoomReservationStatus.Cancelled &&
                                   ((startDateTime >= rr.StartDateTime && startDateTime < rr.EndDateTime) ||
                                    (endDateTime > rr.StartDateTime && endDateTime <= rr.EndDateTime) ||
                                    (startDateTime <= rr.StartDateTime && endDateTime >= rr.EndDateTime)));

                return Ok(new { isAvailable = !hasConflict });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "حدث خطأ في الخادم", error = ex.Message });
            }
        }
    }

    public class CreateReservationRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public ReservationType ReservationType { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public int ExpectedAttendees { get; set; }
        public int? CourseId { get; set; }
        public int? InstructorId { get; set; }
        public string RequiredEquipment { get; set; } = string.Empty;
        public string SpecialRequirements { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    }

    public class UpdateReservationRequest : CreateReservationRequest
    {
        public RoomReservationStatus Status { get; set; }
    }

    public class CancelReservationRequest
    {
        public string CancellationReason { get; set; } = string.Empty;
    }
}
