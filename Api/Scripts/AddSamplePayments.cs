using Api.Data;
using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Scripts
{
    public class AddSamplePayments
    {
        private readonly ApplicationDbContext _context;

        public AddSamplePayments(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task ExecuteAsync()
        {
            try
            {
                Console.WriteLine("Adding sample payment data...");

                // Get existing students
                var students = await _context.Students.Take(5).ToListAsync();
                if (!students.Any())
                {
                    Console.WriteLine("No students found. Please add students first.");
                    return;
                }

                // Get existing courses
                var courses = await _context.Courses.Take(3).ToListAsync();
                if (!courses.Any())
                {
                    Console.WriteLine("No courses found. Please add courses first.");
                    return;
                }

                // Get admin user for processing payments
                var adminUser = await _context.Users.FirstOrDefaultAsync(u => u.UserRole == UserRole.Admin);
                if (adminUser == null)
                {
                    Console.WriteLine("No admin user found. Please create an admin user first.");
                    return;
                }

                var paymentMethods = new[] { PaymentMethod.Cash, PaymentMethod.InstaPay, PaymentMethod.Fawry };
                var random = new Random();

                foreach (var student in students)
                {
                    // Create course registration for each student
                    var course = courses[random.Next(courses.Count)];
                    var totalAmount = random.Next(500, 1500);
                    
                    var registration = new CourseRegistration
                    {
                        StudentId = student.Id,
                        CourseId = course.Id,
                        TotalAmount = totalAmount,
                        PaidAmount = 0,
                        PaymentStatus = PaymentStatus.Unpaid,
                        RegistrationDate = DateTime.UtcNow.AddDays(-random.Next(1, 30)),
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.CourseRegistrations.Add(registration);
                    await _context.SaveChangesAsync();

                    // Add multiple payments for each registration
                    var remainingAmount = totalAmount;
                    var paymentCount = random.Next(1, 4);

                    for (int i = 0; i < paymentCount && remainingAmount > 0; i++)
                    {
                        var paymentAmount = i == paymentCount - 1 ? remainingAmount : random.Next(100, Math.Min(remainingAmount, 500));
                        
                        var payment = new Payment
                        {
                            StudentId = student.Id,
                            CourseRegistrationId = registration.Id,
                            Amount = paymentAmount,
                            PaymentMethod = paymentMethods[random.Next(paymentMethods.Length)],
                            PaymentType = PaymentType.CourseFee,
                            PaymentDate = DateTime.UtcNow.AddDays(-random.Next(1, 15)),
                            ProcessedByUserId = adminUser.Id,
                            Notes = $"دفعة {i + 1} لكورس {course.Name}",
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        };

                        _context.Payments.Add(payment);
                        remainingAmount -= paymentAmount;
                    }

                    // Update registration with paid amount
                    var totalPaid = totalAmount - remainingAmount;
                    registration.PaidAmount = totalPaid;
                    registration.PaymentDate = DateTime.UtcNow.AddDays(-random.Next(1, 10));
                    registration.PaymentMethod = paymentMethods[random.Next(paymentMethods.Length)];

                    if (totalPaid >= totalAmount)
                        registration.PaymentStatus = PaymentStatus.FullyPaid;
                    else if (totalPaid > 0)
                        registration.PaymentStatus = PaymentStatus.PartiallyPaid;

                    Console.WriteLine($"Added payments for student {student.FullName}: {totalPaid} EGP out of {totalAmount} EGP");
                }

                // Add some expenses (negative amounts for expenses page)
                var expenseCategories = new[] { 
                    ExpenseCategory.Equipment, 
                    ExpenseCategory.Supplies, 
                    ExpenseCategory.Maintenance,
                    ExpenseCategory.Marketing
                };

                for (int i = 0; i < 5; i++)
                {
                    var expense = new Expense
                    {
                        Title = $"مصروف تجريبي {i + 1}",
                        Description = $"وصف المصروف التجريبي رقم {i + 1}",
                        Amount = -random.Next(100, 1000), // Negative for expenses
                        ExpenseDate = DateTime.UtcNow.AddDays(-random.Next(1, 30)),
                        Category = expenseCategories[random.Next(expenseCategories.Length)],
                        Status = ExpenseStatus.Paid,
                        Priority = ExpensePriority.Medium,
                        PaymentMethod = paymentMethods[random.Next(paymentMethods.Length)],
                        BranchId = 1, // Assiut branch
                        RequestedByUserId = adminUser.Id,
                        ApprovedByUserId = adminUser.Id,
                        ApprovedAt = DateTime.UtcNow,
                        Notes = $"مصروف تجريبي للاختبار",
                        CreatedAt = DateTime.UtcNow,
                        Vendor = $"مورد تجريبي {i + 1}"
                    };

                    _context.Expenses.Add(expense);
                }

                await _context.SaveChangesAsync();
                Console.WriteLine("Successfully added sample payment data!");

                // Print summary
                var totalPayments = await _context.Payments.CountAsync();
                var totalExpenses = await _context.Expenses.CountAsync();
                var totalRegistrations = await _context.CourseRegistrations.CountAsync();

                Console.WriteLine($"Summary:");
                Console.WriteLine($"Total payments: {totalPayments}");
                Console.WriteLine($"Total expenses: {totalExpenses}");
                Console.WriteLine($"Total registrations: {totalRegistrations}");

                // Show some sample data
                var recentPayments = await _context.Payments
                    .Include(p => p.Student)
                    .Include(p => p.CourseRegistration)
                        .ThenInclude(cr => cr.Course)
                    .OrderByDescending(p => p.PaymentDate)
                    .Take(3)
                    .ToListAsync();

                Console.WriteLine("\nRecent payments:");
                foreach (var payment in recentPayments)
                {
                    Console.WriteLine($"- {payment.Student?.FullName}: {payment.Amount} EGP for {payment.CourseRegistration?.Course?.Name}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }
    }
}
