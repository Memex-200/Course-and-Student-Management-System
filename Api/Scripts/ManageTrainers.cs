using Api.Data;
using Api.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace Api.Scripts
{
    public static class ManageTrainers
    {
        public static async Task UpdateTrainers(ApplicationDbContext context)
        {
            Console.WriteLine("بدء تحديث قائمة المدربين...");

            // 1. إزالة كريم العكاري من جدول المدربين (إذا كان موجود)
            var karemUser = await context.Users.FirstOrDefaultAsync(u => u.Username == "karem");
            if (karemUser != null)
            {
                var karemEmployee = await context.Employees.FirstOrDefaultAsync(e => e.UserId == karemUser.Id);
                if (karemEmployee != null)
                {
                    Console.WriteLine($"إزالة المدرب: {karemEmployee.FullName}");
                    context.Employees.Remove(karemEmployee);
                    await context.SaveChangesAsync();
                }
            }

            // 2. إضافة إيمان حسن كمدرب
            var emanUser = await context.Users.FirstOrDefaultAsync(u => u.Username == "memex");
            if (emanUser != null)
            {
                // التحقق من عدم وجودها كمدرب بالفعل
                var existingEmanEmployee = await context.Employees.FirstOrDefaultAsync(e => e.UserId == emanUser.Id);
                if (existingEmanEmployee == null)
                {
                    var emanEmployee = new Employee
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

                    context.Employees.Add(emanEmployee);
                    await context.SaveChangesAsync();
                    Console.WriteLine($"تم إضافة المدربة: {emanEmployee.FullName}");
                }
                else
                {
                    Console.WriteLine($"المدربة {existingEmanEmployee.FullName} موجودة بالفعل");
                }
            }
            else
            {
                Console.WriteLine("لم يتم العثور على مستخدم إيمان حسن");
            }

            // 3. عرض قائمة المدربين الحالية
            var currentTrainers = await context.Employees
                .Include(e => e.User)
                .Where(e => e.EmployeeRole == EmployeeRole.Instructor && e.IsActive)
                .ToListAsync();

            Console.WriteLine("\nقائمة المدربين الحالية:");
            foreach (var trainer in currentTrainers)
            {
                Console.WriteLine($"- {trainer.FullName} (اسم المستخدم: {trainer.User.Username})");
            }

            Console.WriteLine("تم تحديث قائمة المدربين بنجاح!");
        }

        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password + "AIRoboticsAcademy2025"));
            return Convert.ToBase64String(hashedBytes);
        }
    }
}
