using Api.Data;
using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Scripts
{
    public static class TestTrainers
    {
        public static async Task TestCurrentTrainers(ApplicationDbContext context)
        {
            Console.WriteLine("=== اختبار المدربين الحاليين ===");

            // 1. عرض جميع المدربين
            var allTrainers = await context.Employees
                .Include(e => e.User)
                .Where(e => e.EmployeeRole == EmployeeRole.Instructor)
                .ToListAsync();

            Console.WriteLine($"\nإجمالي المدربين: {allTrainers.Count}");
            foreach (var trainer in allTrainers)
            {
                Console.WriteLine($"- {trainer.FullName} (اسم المستخدم: {trainer.User?.Username}, نشط: {trainer.IsActive})");
            }

            // 2. التحقق من وجود إيمان حسن محمود
            var emanTrainer = allTrainers.FirstOrDefault(t => t.FullName == "إيمان حسن محمود");
            if (emanTrainer != null)
            {
                Console.WriteLine($"\n✅ إيمان حسن محمود موجودة كمدربة (ID: {emanTrainer.Id})");
            }
            else
            {
                Console.WriteLine("\n❌ إيمان حسن محمود غير موجودة كمدربة");
            }

            // 3. التحقق من عدم وجود كريم سيد أحمد
            var karemTrainer = allTrainers.FirstOrDefault(t => t.FullName == "كريم سيد أحمد");
            if (karemTrainer == null)
            {
                Console.WriteLine("\n✅ كريم سيد أحمد غير موجود (كما هو مطلوب)");
            }
            else
            {
                Console.WriteLine($"\n❌ كريم سيد أحمد لا يزال موجوداً (ID: {karemTrainer.Id})");
            }

            // 4. التحقق من الدورات التي تستخدم المدربين
            var coursesWithTrainers = await context.Courses
                .Include(c => c.Instructor)
                .Where(c => c.Instructor != null)
                .ToListAsync();

            Console.WriteLine($"\nالدورات التي لها مدربين: {coursesWithTrainers.Count}");
            foreach (var course in coursesWithTrainers)
            {
                Console.WriteLine($"- دورة: {course.Name} | المدرب: {course.Instructor?.FullName}");
            }

            // 5. اختبار API endpoint
            var testData = new
            {
                Branches = await context.Branches.Select(b => new { b.Id, b.Name }).ToListAsync(),
                Categories = await context.CourseCategories.Select(c => new { c.Id, c.Name }).ToListAsync(),
                Rooms = await context.Rooms.Select(r => new { r.Id, r.Name, r.BranchId }).ToListAsync(),
                Labs = await context.Labs.Select(l => new { l.Id, l.Name, l.BranchId }).ToListAsync(),
                Employees = await context.Employees
                    .Where(e => e.EmployeeRole == Models.EmployeeRole.Instructor && e.IsActive)
                    .Select(e => new { e.Id, e.FullName, e.BranchId })
                    .ToListAsync()
            };

            Console.WriteLine($"\n=== بيانات API ===");
            Console.WriteLine($"المدربين النشطين: {testData.Employees.Count}");
            foreach (var employee in testData.Employees)
            {
                Console.WriteLine($"- {employee.FullName} (ID: {employee.Id})");
            }

            Console.WriteLine("\n=== انتهى الاختبار ===");
        }
    }
}


