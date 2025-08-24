using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Api.Models;
using Api.Data;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TransactionsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TransactionsController> _logger;

        public TransactionsController(ApplicationDbContext context, ILogger<TransactionsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        [Authorize(Policy = "AdminOrEmployee")]
        public async Task<IActionResult> GetTransactions(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] string? transactionType = null,
            [FromQuery] int? courseId = null,
            [FromQuery] int? studentId = null,
            [FromQuery] int? branchId = null)
        {
            try
            {
                var userBranchId = int.Parse(User.FindFirst("BranchId")?.Value ?? "0");
                var effectiveBranchId = branchId ?? userBranchId;

                var transactions = new List<TransactionDTO>();

                // 1. Get Income Transactions (from Payments)
                if (transactionType == null || transactionType == "income" || transactionType == "all")
                {
                                    var incomeQuery = _context.Payments
                    .Include(p => p.CourseRegistration)
                        .ThenInclude(cr => cr.Student)
                    .Include(p => p.CourseRegistration)
                        .ThenInclude(cr => cr.Course)
                            .ThenInclude(c => c.Branch)
                    .Include(p => p.ProcessedByUser)
                    .Where(p => p.IsActive && p.CourseRegistration != null && p.CourseRegistration.Course != null && p.CourseRegistration.Course.BranchId == effectiveBranchId);

                                    if (startDate.HasValue)
                    incomeQuery = incomeQuery.Where(p => p.PaymentDate >= startDate.Value);
                if (endDate.HasValue)
                    incomeQuery = incomeQuery.Where(p => p.PaymentDate <= endDate.Value);
                if (courseId.HasValue)
                    incomeQuery = incomeQuery.Where(p => p.CourseRegistration!.CourseId == courseId.Value);
                if (studentId.HasValue)
                    incomeQuery = incomeQuery.Where(p => p.CourseRegistration!.StudentId == studentId.Value);

                    var incomeTransactions = await incomeQuery
                        .OrderByDescending(p => p.PaymentDate)
                        .Select(p => new TransactionDTO
                        {
                            Id = p.Id,
                                                    StudentName = p.CourseRegistration!.Student != null ? p.CourseRegistration.Student.FullName : string.Empty,
                        CourseName = p.CourseRegistration.Course != null ? p.CourseRegistration.Course.Name : string.Empty,
                            Amount = p.Amount,
                            TransactionType = "income",
                            PaymentStatus = "paid",
                            PaymentDate = p.PaymentDate,
                            InvoiceUrl = $"/api/invoices/payment/{p.Id}",
                            Category = "التدريب",
                            Description = p.Notes ?? "دفعة كورس",
                            BranchName = p.CourseRegistration!.Course != null && p.CourseRegistration.Course.Branch != null ? p.CourseRegistration.Course.Branch.Name : string.Empty,
                            ProcessedBy = p.ProcessedByUser != null ? p.ProcessedByUser.FullName : string.Empty
                        })
                        .ToListAsync();

                    transactions.AddRange(incomeTransactions);
                }

                // 2. Get Expense Transactions (from Expenses)
                if (transactionType == null || transactionType == "expense" || transactionType == "all")
                {
                    var expenseQuery = _context.Expenses
                        .Include(e => e.Branch)
                        .Include(e => e.RequestedByUser)
                        .Where(e => e.BranchId == effectiveBranchId);

                    if (startDate.HasValue)
                        expenseQuery = expenseQuery.Where(e => e.ExpenseDate >= startDate.Value);
                    if (endDate.HasValue)
                        expenseQuery = expenseQuery.Where(e => e.ExpenseDate <= endDate.Value);

                    var expenseTransactions = await expenseQuery
                        .OrderByDescending(e => e.ExpenseDate)
                        .Select(e => new TransactionDTO
                        {
                            Id = e.Id,
                            StudentName = string.Empty, // Expenses don't have students
                            CourseName = string.Empty, // Expenses don't have courses
                            Amount = Math.Abs(e.Amount), // Always positive for display
                            TransactionType = "expense",
                            PaymentStatus = e.Status == ExpenseStatus.Paid ? "paid" : "pending",
                            PaymentDate = e.ExpenseDate,
                            InvoiceUrl = $"/api/invoices/expense/{e.Id}",
                            Category = GetExpenseCategoryArabic(e.Category),
                            Description = e.Description,
                            BranchName = e.Branch != null ? e.Branch.Name : string.Empty,
                            ProcessedBy = e.RequestedByUser != null ? e.RequestedByUser.FullName : string.Empty
                        })
                        .ToListAsync();

                    transactions.AddRange(expenseTransactions);
                }

                // Sort all transactions by date (newest first)
                var sortedTransactions = transactions
                    .OrderByDescending(t => t.PaymentDate)
                    .ToList();

                return Ok(new { 
                    success = true, 
                    data = sortedTransactions,
                    totalIncome = sortedTransactions.Where(t => t.TransactionType == "income").Sum(t => t.Amount),
                    totalExpenses = sortedTransactions.Where(t => t.TransactionType == "expense").Sum(t => t.Amount),
                    netBalance = sortedTransactions.Where(t => t.TransactionType == "income").Sum(t => t.Amount) - 
                                sortedTransactions.Where(t => t.TransactionType == "expense").Sum(t => t.Amount)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching transactions");
                return StatusCode(500, new { message = "حدث خطأ في الخادم", error = ex.Message });
            }
        }

        private string GetExpenseCategoryArabic(ExpenseCategory category)
        {
            return category switch
            {
                ExpenseCategory.Training => "التدريب",
                ExpenseCategory.Marketing => "التسويق",
                ExpenseCategory.Equipment => "المعدات",
                ExpenseCategory.Rent => "الإيجار",
                ExpenseCategory.Utilities => "المرافق",
                ExpenseCategory.Salaries => "الرواتب",
                ExpenseCategory.Other => "أخرى",
                _ => category.ToString()
            };
        }
    }

    public class TransactionDTO
    {
        public int Id { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string TransactionType { get; set; } = string.Empty; // "income" or "expense"
        public string PaymentStatus { get; set; } = string.Empty; // "paid" or "pending"
        public DateTime PaymentDate { get; set; }
        public string InvoiceUrl { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string BranchName { get; set; } = string.Empty;
        public string ProcessedBy { get; set; } = string.Empty;
    }
}
