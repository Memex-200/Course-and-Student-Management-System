using Api.Data;
using Api.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace Api.Scripts
{
    public static class UpdateTrainerName
    {
        public static async Task UpdateTrainerNames(ApplicationDbContext context)
        {
            Console.WriteLine("بدء تحديث أسماء المدربين...");

            // 1. البحث عن أي مدرب باسم "كريم سيد أحمد" وتحديثه إلى "إيمان حسن محمود"
            var karemSayedAhmed = await context.Employees
                .FirstOrDefaultAsync(e => e.FullName == "كريم سيد أحمد" && e.EmployeeRole == EmployeeRole.Instructor);
            
            if (karemSayedAhmed != null)
            {
                Console.WriteLine($"تم العثور على المدرب: {karemSayedAhmed.FullName}");
                karemSayedAhmed.FullName = "إيمان حسن محمود";
                await context.SaveChangesAsync();
                Console.WriteLine($"تم تحديث اسم المدرب إلى: {karemSayedAhmed.FullName}");
            }
            else
            {
                Console.WriteLine("لم يتم العثور على مدرب باسم 'كريم سيد أحمد'");
            }

            // 2. التأكد من وجود إيمان حسن محمود كمدرب
            var emanHassanMahmoud = await context.Employees
                .Include(e => e.User)
                .FirstOrDefaultAsync(e => e.FullName == "إيمان حسن محمود" && e.EmployeeRole == EmployeeRole.Instructor);
            
            if (emanHassanMahmoud == null)
            {
                // البحث عن مستخدم إيمان حسن محمود
                var emanUser = await context.Users.FirstOrDefaultAsync(u => u.Username == "memex");
                if (emanUser != null)
                {
                    var newEmanEmployee = new Employee
                    {
                        UserId = emanUser.Id,
                        FullName = "إيمان حسن محمود",
                        EmployeeRole = EmployeeRole.Instructor,
                        Position = "مدرب",
                        Salary = 0,
                        HireDate = DateTime.UtcNow,
                        BranchId = 1, // فرع أسيوط
                        IsActive = true,
                        Notes = "مدربة في مجال الروبوتيكس والذكاء الاصطناعي"
                    };

                    context.Employees.Add(newEmanEmployee);
                    await context.SaveChangesAsync();
                    Console.WriteLine($"تم إضافة المدربة: {newEmanEmployee.FullName}");
                }
                else
                {
                    Console.WriteLine("لم يتم العثور على مستخدم إيمان حسن محمود");
                }
            }
            else
            {
                Console.WriteLine($"المدربة {emanHassanMahmoud.FullName} موجودة بالفعل");
            }

            // 3. إزالة أي مدرب باسم "كريم العكاري" إذا كان موجود
            var karemElOkary = await context.Employees
                .FirstOrDefaultAsync(e => e.FullName == "كريم العكاري" && e.EmployeeRole == EmployeeRole.Instructor);
            
            if (karemElOkary != null)
            {
                Console.WriteLine($"إزالة المدرب: {karemElOkary.FullName}");
                context.Employees.Remove(karemElOkary);
                await context.SaveChangesAsync();
            }

            // 4. عرض قائمة المدربين الحالية
            var currentTrainers = await context.Employees
                .Include(e => e.User)
                .Where(e => e.EmployeeRole == EmployeeRole.Instructor && e.IsActive)
                .ToListAsync();

            Console.WriteLine("\nقائمة المدربين الحالية:");
            foreach (var trainer in currentTrainers)
            {
                Console.WriteLine($"- {trainer.FullName} (اسم المستخدم: {trainer.User?.Username})");
            }

            // 5. التحقق من الدورات التي قد تحتوي على المدرب القديم وتحديثها
            var coursesWithOldTrainer = await context.Courses
                .Include(c => c.Instructor)
                .Where(c => c.Instructor != null && c.Instructor.FullName == "كريم سيد أحمد")
                .ToListAsync();

            if (coursesWithOldTrainer.Any())
            {
                Console.WriteLine($"\nتم العثور على {coursesWithOldTrainer.Count} دورة تحتوي على المدرب القديم:");
                foreach (var course in coursesWithOldTrainer)
                {
                    Console.WriteLine($"- دورة: {course.Name}");
                }

                // تحديث الدورات لتستخدم المدرب الجديد
                var newTrainer = await context.Employees
                    .FirstOrDefaultAsync(e => e.FullName == "إيمان حسن محمود" && e.EmployeeRole == EmployeeRole.Instructor);
                
                if (newTrainer != null)
                {
                    foreach (var course in coursesWithOldTrainer)
                    {
                        course.InstructorId = newTrainer.Id;
                        Console.WriteLine($"تم تحديث دورة '{course.Name}' لتستخدم المدرب الجديد");
                    }
                    await context.SaveChangesAsync();
                }
            }

            Console.WriteLine("\nتم تحديث أسماء المدربين بنجاح!");
        }

        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password + "AIRoboticsAcademy2025"));
            return Convert.ToBase64String(hashedBytes);
        }
    }
}
