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
    
    public class EquipmentController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public EquipmentController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetEquipment([FromQuery] int? branchId = null, [FromQuery] EquipmentStatus? status = null)
        {
            try
            {
                var userBranchId = int.Parse(User.FindFirst("BranchId")?.Value ?? "0");
                var query = _context.Equipment
                    .Include(e => e.Branch)
                    .Include(e => e.Room)
                    .Include(e => e.AssignedToUser)
                    .AsQueryable();

                // Filter by branch
                if (branchId.HasValue)
                    query = query.Where(e => e.BranchId == branchId.Value);
                else
                    query = query.Where(e => e.BranchId == userBranchId);

                // Filter by status
                if (status.HasValue)
                    query = query.Where(e => e.Status == status.Value);

                var equipment = await query
                    .OrderBy(e => e.Name)
                    .Select(e => new
                    {
                        e.Id,
                        e.Name,
                        e.SerialNumber,
                        e.Model,
                        e.Manufacturer,
                        Status = e.Status.ToString(),
                        StatusArabic = GetEquipmentStatusArabic(e.Status),
                        Condition = e.Condition.ToString(),
                        ConditionArabic = GetEquipmentConditionArabic(e.Condition),
                        e.PurchaseDate,
                        e.PurchasePrice,
                        e.WarrantyExpiry,
                        Branch = e.Branch.Name,
                        Room = e.Room != null ? e.Room.Name : null,
                        AssignedTo = e.AssignedToUser != null ? e.AssignedToUser.FullName : null,
                        e.LastMaintenanceDate,
                        e.NextMaintenanceDate,
                        ActiveReservations = e.EquipmentReservations
                            .Count(er => er.Status == EquipmentReservationStatus.Approved || 
                                        er.Status == EquipmentReservationStatus.InUse)
                    })
                    .ToListAsync();

                return Ok(equipment);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "حدث خطأ في الخادم", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetEquipmentDetails(int id)
        {
            try
            {
                var equipment = await _context.Equipment
                    .Include(e => e.Branch)
                    .Include(e => e.Room)
                    .Include(e => e.AssignedToUser)
                    .Include(e => e.EquipmentReservations)
                        .ThenInclude(er => er.ReservedByUser)
                    .FirstOrDefaultAsync(e => e.Id == id);

                if (equipment == null)
                    return NotFound(new { message = "المعدة غير موجودة" });

                var result = new
                {
                    equipment.Id,
                    equipment.Name,
                    equipment.SerialNumber,
                    equipment.Model,
                    equipment.Manufacturer,
                    equipment.Description,
                    Status = equipment.Status.ToString(),
                    StatusArabic = GetEquipmentStatusArabic(equipment.Status),
                    Condition = equipment.Condition.ToString(),
                    ConditionArabic = GetEquipmentConditionArabic(equipment.Condition),
                    equipment.PurchaseDate,
                    equipment.PurchasePrice,
                    equipment.WarrantyExpiry,
                    equipment.LastMaintenanceDate,
                    equipment.NextMaintenanceDate,
                    equipment.MaintenanceNotes,
                    Branch = new { equipment.Branch.Id, equipment.Branch.Name },
                    Room = equipment.Room != null ? new { equipment.Room.Id, equipment.Room.Name } : null,
                    AssignedTo = equipment.AssignedToUser != null ? 
                        new { equipment.AssignedToUser.Id, equipment.AssignedToUser.FullName } : null,
                    RecentReservations = equipment.EquipmentReservations
                        .OrderByDescending(er => er.StartDateTime)
                        .Take(10)
                        .Select(er => new
                        {
                            er.Id,
                            er.Purpose,
                            er.StartDateTime,
                            er.EndDateTime,
                            Status = er.Status.ToString(),
                            StatusArabic = GetEquipmentReservationStatusArabic(er.Status),
                            ReservedBy = er.ReservedByUser.FullName,
                            er.DamageCost,
                            er.ReturnNotes
                        }).ToList()
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "حدث خطأ في الخادم", error = ex.Message });
            }
        }

        [HttpPost]
        
        public async Task<IActionResult> CreateEquipment([FromBody] CreateEquipmentRequest request)
        {
            try
            {
                var userBranchId = int.Parse(User.FindFirst("BranchId")?.Value ?? "0");

                // Check if serial number already exists
                if (await _context.Equipment.AnyAsync(e => e.SerialNumber == request.SerialNumber))
                {
                    return BadRequest(new { message = "الرقم التسلسلي موجود بالفعل" });
                }

                var equipment = new Equipment
                {
                    Name = request.Name,
                    SerialNumber = request.SerialNumber,
                    Model = request.Model,
                    Manufacturer = request.Manufacturer,
                    Description = request.Description,
                    PurchaseDate = request.PurchaseDate,
                    PurchasePrice = request.PurchasePrice,
                    WarrantyExpiry = request.WarrantyExpiry,
                    Status = EquipmentStatus.Available,
                    Condition = EquipmentCondition.Good,
                    BranchId = userBranchId,
                    RoomId = request.RoomId,
                    AssignedToUserId = request.AssignedToUserId,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Equipment.Add(equipment);
                await _context.SaveChangesAsync();

                return Ok(new { message = "تم إضافة المعدة بنجاح", equipmentId = equipment.Id });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "حدث خطأ في الخادم", error = ex.Message });
            }
        }

        [HttpPost("{id}/reserve")]
        
        public async Task<IActionResult> ReserveEquipment(int id, [FromBody] ReserveEquipmentRequest request)
        {
            try
            {
                var equipment = await _context.Equipment.FindAsync(id);
                if (equipment == null)
                    return NotFound(new { message = "المعدة غير موجودة" });

                if (equipment.Status != EquipmentStatus.Available)
                    return BadRequest(new { message = "المعدة غير متاحة للحجز" });

                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

                // Check for conflicts
                var hasConflict = await _context.EquipmentReservations
                    .AnyAsync(er => er.EquipmentId == id &&
                                   (er.Status == EquipmentReservationStatus.Approved || 
                                    er.Status == EquipmentReservationStatus.InUse) &&
                                   ((request.StartDateTime >= er.StartDateTime && request.StartDateTime < er.EndDateTime) ||
                                    (request.EndDateTime > er.StartDateTime && request.EndDateTime <= er.EndDateTime) ||
                                    (request.StartDateTime <= er.StartDateTime && request.EndDateTime >= er.EndDateTime)));

                if (hasConflict)
                    return BadRequest(new { message = "يوجد تعارض في المواعيد مع حجز آخر" });

                var reservation = new EquipmentReservation
                {
                    EquipmentId = id,
                    Purpose = request.Purpose,
                    StartDateTime = request.StartDateTime,
                    EndDateTime = request.EndDateTime,
                    ReservedByUserId = userId,
                    CourseId = request.CourseId,
                    Notes = request.Notes,
                    Status = EquipmentReservationStatus.Pending,
                    CreatedAt = DateTime.UtcNow
                };

                _context.EquipmentReservations.Add(reservation);
                await _context.SaveChangesAsync();

                return Ok(new { message = "تم إنشاء طلب الحجز بنجاح", reservationId = reservation.Id });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "حدث خطأ في الخادم", error = ex.Message });
            }
        }

        [HttpPut("reservations/{reservationId}/approve")]
        
        public async Task<IActionResult> ApproveReservation(int reservationId)
        {
            try
            {
                var reservation = await _context.EquipmentReservations
                    .Include(er => er.Equipment)
                    .FirstOrDefaultAsync(er => er.Id == reservationId);

                if (reservation == null)
                    return NotFound(new { message = "الحجز غير موجود" });

                if (reservation.Status != EquipmentReservationStatus.Pending)
                    return BadRequest(new { message = "الحجز ليس في حالة انتظار الموافقة" });

                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

                reservation.Status = EquipmentReservationStatus.Approved;
                reservation.ApprovedByUserId = userId;
                reservation.ApprovedAt = DateTime.UtcNow;

                // Update equipment status
                reservation.Equipment.Status = EquipmentStatus.Reserved;

                await _context.SaveChangesAsync();

                return Ok(new { message = "تم الموافقة على الحجز بنجاح" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "حدث خطأ في الخادم", error = ex.Message });
            }
        }

        [HttpPut("reservations/{reservationId}/start")]
        
        public async Task<IActionResult> StartUsingEquipment(int reservationId)
        {
            try
            {
                var reservation = await _context.EquipmentReservations
                    .Include(er => er.Equipment)
                    .FirstOrDefaultAsync(er => er.Id == reservationId);

                if (reservation == null)
                    return NotFound(new { message = "الحجز غير موجود" });

                if (reservation.Status != EquipmentReservationStatus.Approved)
                    return BadRequest(new { message = "الحجز غير موافق عليه" });

                reservation.Status = EquipmentReservationStatus.InUse;
                reservation.ActualStartDateTime = DateTime.UtcNow;

                // Update equipment status
                reservation.Equipment.Status = EquipmentStatus.InUse;

                await _context.SaveChangesAsync();

                return Ok(new { message = "تم بدء استخدام المعدة بنجاح" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "حدث خطأ في الخادم", error = ex.Message });
            }
        }

        [HttpPut("reservations/{reservationId}/return")]
        
        public async Task<IActionResult> ReturnEquipment(int reservationId, [FromBody] ReturnEquipmentRequest request)
        {
            try
            {
                var reservation = await _context.EquipmentReservations
                    .Include(er => er.Equipment)
                    .FirstOrDefaultAsync(er => er.Id == reservationId);

                if (reservation == null)
                    return NotFound(new { message = "الحجز غير موجود" });

                if (reservation.Status != EquipmentReservationStatus.InUse)
                    return BadRequest(new { message = "المعدة ليست قيد الاستخدام" });

                reservation.Status = EquipmentReservationStatus.Returned;
                reservation.ActualEndDateTime = DateTime.UtcNow;
                reservation.ReturnNotes = request.ReturnNotes;
                reservation.DamageCost = request.DamageCost;

                // Update equipment status and condition
                reservation.Equipment.Status = EquipmentStatus.Available;
                if (request.NewCondition.HasValue)
                    reservation.Equipment.Condition = request.NewCondition.Value;

                await _context.SaveChangesAsync();

                return Ok(new { message = "تم إرجاع المعدة بنجاح" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "حدث خطأ في الخادم", error = ex.Message });
            }
        }

        [HttpGet("reservations")]
        public async Task<IActionResult> GetEquipmentReservations([FromQuery] EquipmentReservationStatus? status = null)
        {
            try
            {
                var userBranchId = int.Parse(User.FindFirst("BranchId")?.Value ?? "0");
                var query = _context.EquipmentReservations
                    .Include(er => er.Equipment)
                    .Include(er => er.ReservedByUser)
                    .Include(er => er.ApprovedByUser)
                    .Include(er => er.Course)
                    .Where(er => er.Equipment.BranchId == userBranchId);

                if (status.HasValue)
                    query = query.Where(er => er.Status == status.Value);

                var reservations = await query
                    .OrderByDescending(er => er.CreatedAt)
                    .Select(er => new
                    {
                        er.Id,
                        Equipment = er.Equipment.Name,
                        er.Purpose,
                        er.StartDateTime,
                        er.EndDateTime,
                        er.ActualStartDateTime,
                        er.ActualEndDateTime,
                        Status = er.Status.ToString(),
                        StatusArabic = GetEquipmentReservationStatusArabic(er.Status),
                        ReservedBy = er.ReservedByUser.FullName,
                        ApprovedBy = er.ApprovedByUser != null ? er.ApprovedByUser.FullName : null,
                        Course = er.Course != null ? er.Course.Name : null,
                        er.DamageCost,
                        er.ReturnNotes,
                        er.CreatedAt
                    })
                    .ToListAsync();

                return Ok(reservations);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "حدث خطأ في الخادم", error = ex.Message });
            }
        }

        private string GetEquipmentStatusArabic(EquipmentStatus status)
        {
            return status switch
            {
                EquipmentStatus.Available => "متاح",
                EquipmentStatus.InUse => "قيد الاستخدام",
                EquipmentStatus.Reserved => "محجوز",
                EquipmentStatus.UnderMaintenance => "تحت الصيانة",
                EquipmentStatus.OutOfOrder => "معطل",
                _ => status.ToString()
            };
        }

        private string GetEquipmentConditionArabic(EquipmentCondition condition)
        {
            return condition switch
            {
                EquipmentCondition.Excellent => "ممتاز",
                EquipmentCondition.Good => "جيد",
                EquipmentCondition.Fair => "مقبول",
                EquipmentCondition.Poor => "سيء",
                EquipmentCondition.Damaged => "تالف",
                _ => condition.ToString()
            };
        }

        private string GetEquipmentReservationStatusArabic(EquipmentReservationStatus status)
        {
            return status switch
            {
                EquipmentReservationStatus.Pending => "في الانتظار",
                EquipmentReservationStatus.Approved => "موافق عليه",
                EquipmentReservationStatus.InUse => "قيد الاستخدام",
                EquipmentReservationStatus.Returned => "تم الإرجاع",
                EquipmentReservationStatus.Cancelled => "ملغي",
                EquipmentReservationStatus.Overdue => "متأخر",
                _ => status.ToString()
            };
        }
    }

    public class CreateEquipmentRequest
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string SerialNumber { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Model { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Manufacturer { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public DateTime PurchaseDate { get; set; }

        [Range(0, double.MaxValue)]
        public decimal PurchasePrice { get; set; }

        public DateTime? WarrantyExpiry { get; set; }
        public int? RoomId { get; set; }
        public int? AssignedToUserId { get; set; }
    }

    public class ReserveEquipmentRequest
    {
        [Required]
        [MaxLength(500)]
        public string Purpose { get; set; } = string.Empty;

        [Required]
        public DateTime StartDateTime { get; set; }

        [Required]
        public DateTime EndDateTime { get; set; }

        public int? CourseId { get; set; }

        [MaxLength(1000)]
        public string Notes { get; set; } = string.Empty;
    }

    public class ReturnEquipmentRequest
    {
        [MaxLength(1000)]
        public string ReturnNotes { get; set; } = string.Empty;

        [Range(0, double.MaxValue)]
        public decimal DamageCost { get; set; } = 0;

        public EquipmentCondition? NewCondition { get; set; }
    }
}
