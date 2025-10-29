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
    
    public class ExpensesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ExpensesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetExpenses([FromQuery] int? branchId = null, [FromQuery] ExpenseStatus? status = null, 
            [FromQuery] ExpenseCategory? category = null, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
                var userBranchId = int.Parse(User.FindFirst("BranchId")?.Value ?? "0");
                var query = _context.Expenses
                    .Include(e => e.Branch)
                    .Include(e => e.RequestedByUser)
                    .Include(e => e.ApprovedByUser)
                    .AsQueryable();

                // Filter by branch if a branchId is provided.
                // If no branchId is provided, transactions from all branches are returned for all roles.
                if (branchId.HasValue)
                {
                    query = query.Where(e => e.BranchId == branchId.Value);
                }

                // Filter by status
                if (status.HasValue)
                    query = query.Where(e => e.Status == status.Value);

                // Filter by category
                if (category.HasValue)
                    query = query.Where(e => e.Category == category.Value);

                // Filter by date range
                if (startDate.HasValue)
                    query = query.Where(e => e.ExpenseDate >= startDate.Value);

                if (endDate.HasValue)
                    query = query.Where(e => e.ExpenseDate <= endDate.Value);

                var expenses = await query
                    .OrderByDescending(e => e.CreatedAt)
                    .Select(e => new
                    {
                        e.Id,
                        e.Title,
                        e.Description,
                        e.Category,
                        CategoryArabic = GetExpenseCategoryArabic(e.Category),
                        e.Amount,
                        e.ExpenseDate,
                        e.Status,
                        StatusArabic = GetExpenseStatusArabic(e.Status),
                        e.Priority,
                        e.Vendor,
                        e.PaymentMethod,
                        BranchName = e.Branch.Name,
                        RequestedBy = e.RequestedByUser.FullName,
                        ApprovedBy = e.ApprovedByUser != null ? e.ApprovedByUser.FullName : null,
                        e.ApprovedAt,
                        e.CreatedAt,
                        e.Notes,
                        Type = e.Amount >= 0 ? "إيراد" : "مصروف", // تحديد نوع العملية
                        TransactionType = e.Amount >= 0 ? "income" : "expense" // نوع المعاملة للفرونت إند
                    })
                    .ToListAsync();

                return Ok(expenses);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "حدث خطأ في الخادم", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetExpense(int id)
        {
            try
            {
                var expense = await _context.Expenses
                    .Include(e => e.Branch)
                    .Include(e => e.RequestedByUser)
                    .Include(e => e.ApprovedByUser)
                    .FirstOrDefaultAsync(e => e.Id == id);

                if (expense == null)
                    return NotFound(new { message = "المصروف غير موجود" });

                var result = new
                {
                    expense.Id,
                    expense.Title,
                    expense.Description,
                    Category = expense.Category.ToString(),
                    CategoryArabic = GetExpenseCategoryArabic(expense.Category),
                    expense.Amount,
                    expense.ExpenseDate,
                    Status = expense.Status.ToString(),
                    StatusArabic = GetExpenseStatusArabic(expense.Status),
                    Priority = expense.Priority.ToString(),
                    PriorityArabic = GetExpensePriorityArabic(expense.Priority),
                    expense.Vendor,
                    PaymentMethod = expense.PaymentMethod.ToString(),
                    PaymentMethodArabic = GetPaymentMethodArabic(expense.PaymentMethod),
                    expense.ReceiptNumber,
                    expense.Notes,
                    expense.RejectionReason,
                    expense.IsRecurring,
                    expense.RecurrencePattern,
                    expense.NextRecurrenceDate,
                    Branch = new { expense.Branch.Id, expense.Branch.Name },
                    RequestedBy = new { expense.RequestedByUser.Id, expense.RequestedByUser.FullName },
                    ApprovedBy = expense.ApprovedByUser != null ?
                        new { expense.ApprovedByUser.Id, expense.ApprovedByUser.FullName } : null,
                    expense.ApprovedAt,
                    expense.CreatedAt
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "حدث خطأ في الخادم", error = ex.Message });
            }
        }

        [HttpPost]
        
        public async Task<IActionResult> CreateExpense([FromBody] CreateExpenseRequest request)
        {
            try
            {
                var userBranchId = int.Parse(User.FindFirst("BranchId")?.Value ?? "0");
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

                var expense = new Expense
                {
                    Title = request.Title,
                    Description = request.Description,
                    Category = request.Category,
                    Amount = request.Amount,
                    ExpenseDate = request.ExpenseDate,
                    Priority = request.Priority,
                    Vendor = request.Vendor,
                    PaymentMethod = request.PaymentMethod,
                    ReceiptNumber = request.ReceiptNumber,
                    Notes = request.Notes,
                    IsRecurring = request.IsRecurring,
                    RecurrencePattern = request.RecurrencePattern ?? string.Empty,
                    NextRecurrenceDate = request.NextRecurrenceDate,
                    BranchId = userBranchId,
                    RequestedByUserId = userId,
                    Status = ExpenseStatus.Pending,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Expenses.Add(expense);
                await _context.SaveChangesAsync();

                return Ok(new { message = "تم إنشاء طلب المصروف بنجاح", expenseId = expense.Id });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "حدث خطأ في الخادم", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        
        public async Task<IActionResult> UpdateExpense(int id, [FromBody] UpdateExpenseRequest request)
        {
            try
            {
                var expense = await _context.Expenses.FindAsync(id);
                if (expense == null)
                    return NotFound(new { message = "المصروف غير موجود" });

                // Only allow updates if status is Pending or Rejected
                if (expense.Status != ExpenseStatus.Pending && expense.Status != ExpenseStatus.Rejected)
                    return BadRequest(new { message = "لا يمكن تعديل المصروف في هذه الحالة" });

                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

                // Only the requester or admin can update
                if (expense.RequestedByUserId != userId && userRole != "Admin")
                    return Forbid("ليس لديك صلاحية لتعديل هذا المصروف");

                expense.Title = request.Title;
                expense.Description = request.Description;
                expense.Category = request.Category;
                expense.Amount = request.Amount;
                expense.ExpenseDate = request.ExpenseDate;
                expense.Priority = request.Priority;
                expense.Vendor = request.Vendor;
                expense.PaymentMethod = request.PaymentMethod;
                expense.ReceiptNumber = request.ReceiptNumber;
                expense.Notes = request.Notes;
                expense.IsRecurring = request.IsRecurring;
                expense.RecurrencePattern = request.RecurrencePattern ?? string.Empty;
                expense.NextRecurrenceDate = request.NextRecurrenceDate;

                // Reset status to Pending if it was Rejected
                if (expense.Status == ExpenseStatus.Rejected)
                {
                    expense.Status = ExpenseStatus.Pending;
                    expense.RejectionReason = string.Empty;
                }

                await _context.SaveChangesAsync();

                return Ok(new { message = "تم تحديث المصروف بنجاح" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "حدث خطأ في الخادم", error = ex.Message });
            }
        }

        [HttpPut("{id}/approve")]
        
        public async Task<IActionResult> ApproveExpense(int id, [FromBody] ApproveExpenseRequest request)
        {
            try
            {
                var expense = await _context.Expenses.FindAsync(id);
                if (expense == null)
                    return NotFound(new { message = "المصروف غير موجود" });

                if (expense.Status != ExpenseStatus.Pending)
                    return BadRequest(new { message = "المصروف ليس في حالة انتظار الموافقة" });

                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

                expense.Status = ExpenseStatus.Approved;
                expense.ApprovedByUserId = userId;
                expense.ApprovedAt = DateTime.UtcNow;
                expense.Notes += $"\nملاحظات الموافقة: {request.ApprovalNotes}";

                await _context.SaveChangesAsync();

                return Ok(new { message = "تم الموافقة على المصروف بنجاح" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "حدث خطأ في الخادم", error = ex.Message });
            }
        }

        [HttpPut("{id}/reject")]
        
        public async Task<IActionResult> RejectExpense(int id, [FromBody] RejectExpenseRequest request)
        {
            try
            {
                var expense = await _context.Expenses.FindAsync(id);
                if (expense == null)
                    return NotFound(new { message = "المصروف غير موجود" });

                if (expense.Status != ExpenseStatus.Pending)
                    return BadRequest(new { message = "المصروف ليس في حالة انتظار الموافقة" });

                expense.Status = ExpenseStatus.Rejected;
                expense.RejectionReason = request.RejectionReason;

                await _context.SaveChangesAsync();

                return Ok(new { message = "تم رفض المصروف" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "حدث خطأ في الخادم", error = ex.Message });
            }
        }

        [HttpPut("{id}/pay")]
        
        public async Task<IActionResult> MarkExpenseAsPaid(int id, [FromBody] PayExpenseRequest request)
        {
            try
            {
                var expense = await _context.Expenses.FindAsync(id);
                if (expense == null)
                    return NotFound(new { message = "المصروف غير موجود" });

                if (expense.Status != ExpenseStatus.Approved)
                    return BadRequest(new { message = "المصروف غير موافق عليه" });

                expense.Status = ExpenseStatus.Paid;
                expense.PaymentMethod = request.PaymentMethod;
                expense.ReceiptNumber = request.ReceiptNumber;
                expense.Notes += $"\nتم الدفع في: {DateTime.UtcNow:yyyy-MM-dd HH:mm}";
                expense.Notes += $"\nملاحظات الدفع: {request.PaymentNotes}";

                await _context.SaveChangesAsync();

                return Ok(new { message = "تم تسجيل دفع المصروف بنجاح" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "حدث خطأ في الخادم", error = ex.Message });
            }
        }

        [HttpGet("statistics")]
        public async Task<IActionResult> GetExpenseStatistics([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var userBranchId = int.Parse(User.FindFirst("BranchId")?.Value ?? "0");
                var query = _context.Expenses
                    .Where(e => e.BranchId == userBranchId);

                if (startDate.HasValue)
                    query = query.Where(e => e.ExpenseDate >= startDate.Value);

                if (endDate.HasValue)
                    query = query.Where(e => e.ExpenseDate <= endDate.Value);

                var expenses = await query.ToListAsync();

                var statistics = new
                {
                    TotalExpenses = expenses.Count,
                    PendingExpenses = expenses.Count(e => e.Status == ExpenseStatus.Pending),
                    ApprovedExpenses = expenses.Count(e => e.Status == ExpenseStatus.Approved),
                    PaidExpenses = expenses.Count(e => e.Status == ExpenseStatus.Paid),
                    RejectedExpenses = expenses.Count(e => e.Status == ExpenseStatus.Rejected),
                    TotalAmount = expenses.Sum(e => e.Amount),
                    PaidAmount = expenses.Where(e => e.Status == ExpenseStatus.Paid).Sum(e => e.Amount),
                    PendingAmount = expenses.Where(e => e.Status == ExpenseStatus.Pending || e.Status == ExpenseStatus.Approved).Sum(e => e.Amount),
                    ExpensesByCategory = expenses
                        .GroupBy(e => e.Category)
                        .Select(g => new { Category = g.Key.ToString(), Count = g.Count(), Amount = g.Sum(e => e.Amount) })
                        .ToList()
                };

                return Ok(statistics);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "حدث خطأ في الخادم", error = ex.Message });
            }
        }

        private string GetExpenseCategoryArabic(ExpenseCategory category)
        {
            return category switch
            {
                ExpenseCategory.Utilities => "المرافق",
                ExpenseCategory.Equipment => "المعدات",
                ExpenseCategory.Supplies => "المستلزمات",
                ExpenseCategory.Maintenance => "الصيانة",
                ExpenseCategory.Marketing => "التسويق",
                ExpenseCategory.Salaries => "الرواتب",
                ExpenseCategory.Rent => "الإيجار",
                ExpenseCategory.Insurance => "التأمين",
                ExpenseCategory.Transportation => "المواصلات",
                ExpenseCategory.Food => "الطعام",
                ExpenseCategory.Software => "البرمجيات",
                ExpenseCategory.Legal => "القانونية",
                ExpenseCategory.Training => "التدريب",
                ExpenseCategory.Other => "أخرى",
                _ => category.ToString()
            };
        }

        private string GetExpenseStatusArabic(ExpenseStatus status)
        {
            return status switch
            {
                ExpenseStatus.Pending => "في الانتظار",
                ExpenseStatus.Approved => "موافق عليه",
                ExpenseStatus.Rejected => "مرفوض",
                ExpenseStatus.Paid => "مدفوع",
                _ => status.ToString()
            };
        }

        private string GetExpensePriorityArabic(ExpensePriority priority)
        {
            return priority switch
            {
                ExpensePriority.Low => "منخفض",
                ExpensePriority.Medium => "متوسط",
                ExpensePriority.High => "عالي",
                ExpensePriority.Urgent => "عاجل",
                _ => priority.ToString()
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

    public class CreateExpenseRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public ExpenseCategory Category { get; set; }
        public decimal Amount { get; set; }
        public DateTime ExpenseDate { get; set; }
        public ExpensePriority Priority { get; set; }
        public string Vendor { get; set; } = string.Empty;
        public PaymentMethod PaymentMethod { get; set; }
        public string ReceiptNumber { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public bool IsRecurring { get; set; }
        public string? RecurrencePattern { get; set; }
        public DateTime? NextRecurrenceDate { get; set; }
    }

    public class UpdateExpenseRequest : CreateExpenseRequest { }

    public class ApproveExpenseRequest
    {
        public string ApprovalNotes { get; set; } = string.Empty;
    }

    public class RejectExpenseRequest
    {
        public string RejectionReason { get; set; } = string.Empty;
    }

    public class PayExpenseRequest
    {
        public PaymentMethod PaymentMethod { get; set; }
        public string ReceiptNumber { get; set; } = string.Empty;
        public string PaymentNotes { get; set; } = string.Empty;
    }
}
