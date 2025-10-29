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
                // Get the first branch
                var branch = await _context.Branches.FirstOrDefaultAsync();
                if (branch == null)
                {
                    // Create a sample branch if none exists
                    branch = new Branch
                    {
                        Name = "أسيوط",
                        Address = "شارع الجامعة، أسيوط",
                        Phone = "01234567890",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Branches.Add(branch);
                    await _context.SaveChangesAsync();
                }

                // Get or create a sample user
                var user = await _context.Users.FirstOrDefaultAsync();
                if (user == null)
                {
                    user = new User
                    {
                        Username = "admin",
                        Email = "admin@airobotics.com",
                        FullName = "مدير النظام",
                        Role = UserRole.Admin,
                        BranchId = branch.Id,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Users.Add(user);
                    await _context.SaveChangesAsync();
                }

                // Get or create a sample course
                var course = await _context.Courses.FirstOrDefaultAsync();
                if (course == null)
                {
                    course = new Course
                    {
                        Name = "دورة الذكاء الاصطناعي",
                        Description = "دورة شاملة في الذكاء الاصطناعي",
                        SessionsCount = 40,
                        Price = 2000,
                        BranchId = branch.Id,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Courses.Add(course);
                    await _context.SaveChangesAsync();
                }

                // Get or create a sample student
                var student = await _context.Students.FirstOrDefaultAsync();
                if (student == null)
                {
                    student = new Student
                    {
                        FullName = "أحمد محمد علي",
                        Phone = "01234567890",
                        Email = "ahmed@example.com",
                        BranchId = branch.Id,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Students.Add(student);
                    await _context.SaveChangesAsync();
                }

                // Create course registration
                var registration = new CourseRegistration
                {
                    StudentId = student.Id,
                    CourseId = course.Id,
                    TotalAmount = course.Price,
                    PaidAmount = 1000, // Partial payment
                    PaymentStatus = PaymentStatus.PartiallyPaid,
                    PaymentMethod = PaymentMethod.Cash,
                    PaymentDate = DateTime.UtcNow.AddDays(-5),
                    RegistrationDate = DateTime.UtcNow.AddDays(-10),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                _context.CourseRegistrations.Add(registration);
                await _context.SaveChangesAsync();

                // Create payment record
                var payment = new Payment
                {
                    StudentId = student.Id,
                    CourseRegistrationId = registration.Id,
                    BranchId = branch.Id,
                    Amount = 1000,
                    PaymentMethod = PaymentMethod.Cash,
                    PaymentType = PaymentType.CourseFee,
                    PaymentSource = PaymentSource.CourseFee,
                    PaymentDate = DateTime.UtcNow.AddDays(-5),
                    ProcessedByUserId = user.Id,
                    Notes = "دفعة جزئية للدورة",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Payments.Add(payment);

                // Create another payment record
                var payment2 = new Payment
                {
                    StudentId = student.Id,
                    CourseRegistrationId = registration.Id,
                    BranchId = branch.Id,
                    Amount = 500,
                    PaymentMethod = PaymentMethod.InstaPay,
                    PaymentType = PaymentType.CourseFee,
                    PaymentSource = PaymentSource.CourseFee,
                    PaymentDate = DateTime.UtcNow.AddDays(-2),
                    ProcessedByUserId = user.Id,
                    Notes = "دفعة إضافية",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Payments.Add(payment2);

                // Create another student and course registration
                var student2 = new Student
                {
                    FullName = "فاطمة أحمد",
                    Phone = "01234567891",
                    Email = "fatma@example.com",
                    BranchId = branch.Id,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Students.Add(student2);
                await _context.SaveChangesAsync();

                var registration2 = new CourseRegistration
                {
                    StudentId = student2.Id,
                    CourseId = course.Id,
                    TotalAmount = course.Price,
                    PaidAmount = course.Price, // Full payment
                    PaymentStatus = PaymentStatus.FullyPaid,
                    PaymentMethod = PaymentMethod.Fawry,
                    PaymentDate = DateTime.UtcNow.AddDays(-1),
                    RegistrationDate = DateTime.UtcNow.AddDays(-3),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                _context.CourseRegistrations.Add(registration2);
                await _context.SaveChangesAsync();

                var payment3 = new Payment
                {
                    StudentId = student2.Id,
                    CourseRegistrationId = registration2.Id,
                    BranchId = branch.Id,
                    Amount = course.Price,
                    PaymentMethod = PaymentMethod.Fawry,
                    PaymentType = PaymentType.CourseFee,
                    PaymentSource = PaymentSource.CourseFee,
                    PaymentDate = DateTime.UtcNow.AddDays(-1),
                    ProcessedByUserId = user.Id,
                    Notes = "دفعة كاملة للدورة",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Payments.Add(payment3);

                await _context.SaveChangesAsync();

                Console.WriteLine("Sample payment data created successfully!");
                Console.WriteLine($"Created {3} payment records");
                Console.WriteLine($"Created {2} course registrations");
                Console.WriteLine($"Created {2} students");
                Console.WriteLine($"Created {1} course");
                Console.WriteLine($"Created {1} branch");
                Console.WriteLine($"Created {1} user");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating sample data: {ex.Message}");
                throw;
            }
        }
    }
}