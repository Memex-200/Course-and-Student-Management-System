using Microsoft.EntityFrameworkCore;
using Api.Data;
using Api.Models;
using System.Security.Cryptography;
using System.Text;
using BCrypt.Net;

namespace Api
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            using var context = new ApplicationDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>());

            // Check if admin users already exist
            var adminUsernames = new[] { "admin", "ibrahem", "karem", "ahmed", "mira", "memex" };
            var existingAdmins = await context.Users
                .Where(u => adminUsernames.Contains(u.Username))
                .CountAsync();

            if (existingAdmins >= 6)
            {
                return; // All admin users already exist
            }

            // Create Branches if they don't exist
            var assiutBranch = await context.Branches.FirstOrDefaultAsync(b => b.Name == "فرع أسيوط");
            var cairoBranch = await context.Branches.FirstOrDefaultAsync(b => b.Name == "فرع القاهرة");

            if (assiutBranch == null)
            {
                assiutBranch = new Branch
                {
                    Name = "فرع أسيوط",
                    Address = "أسيوط، مصر",
                    Phone = "01234567890",
                    HasWorkspace = true,
                    HasSharedWorkspace = true,
                    HasRooms = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                context.Branches.Add(assiutBranch);
            }

            if (cairoBranch == null)
            {
                cairoBranch = new Branch
                {
                    Name = "فرع القاهرة",
                    Address = "القاهرة، مصر",
                    Phone = "01234567891",
                    HasWorkspace = true,
                    HasSharedWorkspace = true,
                    HasRooms = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                context.Branches.Add(cairoBranch);
            }

            await context.SaveChangesAsync();

            // Create Course Categories based on real academy data if they don't exist
            if (!await context.CourseCategories.AnyAsync())
            {
                var categories = new List<CourseCategory>
                {
                    // للأعمار 4-6 سنوات
                    new CourseCategory
                    {
                        Name = "أساسيات الروبوتيكس والتفكير الصحيح",
                        Description = "للأطفال من سن 4-6 سنوات - أساسيات الروبوتيكس والتفكير الصحيح بأدوات مخصصة بمواصفات عالمية STEM Education",
                        MinAge = 4,
                        MaxAge = 6,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },

                    // للأعمار 7-12 سنوات - المستوى الأول
                    new CourseCategory
                    {
                        Name = "أساسيات الروبوتيكس والذكاء الاصطناعي (7-12)",
                        Description = "المستوى الأول للأعمار 7-12 سنة - أساسيات الروبوتيكس والذكاء الاصطناعي",
                        MinAge = 7,
                        MaxAge = 12,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new CourseCategory
                    {
                        Name = "أساسيات الكمبيوتر والبرمجة (7-12)",
                        Description = "المستوى الأول للأعمار 7-12 سنة - أساسيات الكمبيوتر والبرمجة",
                        MinAge = 7,
                        MaxAge = 12,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new CourseCategory
                    {
                        Name = "أساسيات الهندسة الكهربية والميكاترونكس (7-12)",
                        Description = "المستوى الأول للأعمار 7-12 سنة - أساسيات الهندسة الكهربية والميكاترونكس",
                        MinAge = 7,
                        MaxAge = 12,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },

                    // للأعمار 13-17 سنوات
                    new CourseCategory
                    {
                        Name = "أساسيات الروبوتيكس والذكاء الاصطناعي (13-17)",
                        Description = "للأعمار 13-17 سنة - أساسيات الروبوتيكس والذكاء الاصطناعي بأدوات مطابقة للسن",
                        MinAge = 13,
                        MaxAge = 17,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new CourseCategory
                    {
                        Name = "أساسيات الكمبيوتر والبرمجة (13-17)",
                        Description = "للأعمار 13-17 سنة - أساسيات الكمبيوتر والبرمجة بلغة Python و C++",
                        MinAge = 13,
                        MaxAge = 17,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new CourseCategory
                    {
                        Name = "أساسيات الهندسة الكهربية والميكاترونكس (13-17)",
                        Description = "للأعمار 13-17 سنة - أساسيات الهندسة الكهربية والميكاترونكس",
                        MinAge = 13,
                        MaxAge = 17,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },

                    // التخصصات للأعمار 18+
                    new CourseCategory
                    {
                        Name = "تطوير المواقع",
                        Description = "تطوير المواقع الإلكترونية - للأعمار 18+ سنة",
                        MinAge = 18,
                        MaxAge = 100,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new CourseCategory
                    {
                        Name = "تطبيقات الهواتف المحمولة",
                        Description = "تطوير تطبيقات الهواتف الذكية - للأعمار 18+ سنة",
                        MinAge = 18,
                        MaxAge = 100,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new CourseCategory
                    {
                        Name = "تطبيقات سطح المكتب",
                        Description = "تطوير البرمجيات لسطح المكتب - للأعمار 18+ سنة",
                        MinAge = 18,
                        MaxAge = 100,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new CourseCategory
                    {
                        Name = "تحليل البيانات",
                        Description = "تحليل البيانات واستخلاص الرؤى - للأعمار 18+ سنة",
                        MinAge = 18,
                        MaxAge = 100,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new CourseCategory
                    {
                        Name = "الروبوتات المتقدمة",
                        Description = "تصميم وتصنيع وتشغيل الروبوتات - للأعمار 18+ سنة",
                        MinAge = 18,
                        MaxAge = 100,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new CourseCategory
                    {
                        Name = "الميكاترونيكس المتقدمة",
                        Description = "الميكاترونيكس المتقدمة - للأعمار 18+ سنة",
                        MinAge = 18,
                        MaxAge = 100,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    }
                };

                context.CourseCategories.AddRange(categories);
                await context.SaveChangesAsync();
            }

            // Create Admin Users - Only these 5 users will be admins
            // Only add admin users that don't exist
            var adminUsers = new List<User>();
            
            foreach (var adminData in new[]
            {
                new { Username = "admin", FullName = "المدير العام", Email = "admin@airoboticsacademy.com", Phone = "01234567890", Password = "123456" },
                new { Username = "ibrahem", FullName = "إبراهيم سيد", Email = "ibrahemsayed433@gmail.com", Phone = "01234567891", Password = "Ibrahem@123!" },
                new { Username = "karem", FullName = "كريم العكاري", Email = "karemelokary8@gmail.com", Phone = "01234567892", Password = "Karem@123!" },
                new { Username = "ahmed", FullName = "أحمد خالد", Email = "franceckhaled@gmail.com", Phone = "01234567893", Password = "Ahmed@123!" },
                new { Username = "mira", FullName = "ميرا", Email = "miraairobotics@gmail.com", Phone = "01234567894", Password = "Mira@123!" },
                new { Username = "memex", FullName = "إيمان حسن محمود", Email = "emanhassanmahmoud1@gmail.com", Phone = "01234567895", Password = "Eman@123!" }
            })
            {
                var existingUser = await context.Users.FirstOrDefaultAsync(u => u.Username == adminData.Username);
                if (existingUser == null)
                {
                    adminUsers.Add(new User
                    {
                        FullName = adminData.FullName,
                        Username = adminData.Username,
                        Email = adminData.Email,
                        Phone = adminData.Phone,
                        Address = "",
                        PasswordHash = HashPassword(adminData.Password),
                        Role = UserRole.Admin,
                        UserRole = UserRole.Admin,
                        BranchId = assiutBranch.Id,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            if (adminUsers.Any())
            {
                context.Users.AddRange(adminUsers);
                await context.SaveChangesAsync();
            }

            // إعادة تعيين أو إضافة مستخدم memex كأدمن
            var memex = context.Users.FirstOrDefault(u => u.Username == "memex");
            var password = "Eman@123!";
            var passwordHash = HashPassword(password);
            if (memex == null)
            {
                // استخدم assiutBranch الموجود مسبقًا
                memex = new User
                {
                    FullName = "إيمان حسن محمود",
                    Username = "memex",
                    Email = "emanhassanmahmoud1@gmail.com",
                    Phone = "01234567894",
                    Address = "",
                    PasswordHash = passwordHash,
                    Role = UserRole.Admin,
                    UserRole = UserRole.Admin,
                    BranchId = assiutBranch.Id,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                context.Users.Add(memex);
            }
            else
            {
                memex.PasswordHash = passwordHash;
                memex.IsActive = true;
                memex.Email = "emanhassanmahmoud1@gmail.com";
                memex.Role = UserRole.Admin;
                memex.UserRole = UserRole.Admin;
            }
            context.SaveChanges();

            // Add course registrations
            await SeedCourseRegistrations(context);
        }

        private static string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        private static async Task SeedCourseRegistrations(ApplicationDbContext context)
        {
            // Create test course registrations
            if (!context.CourseRegistrations.Any())
            {
                var testCourse = context.Courses.FirstOrDefault();
                var testStudents = context.Students.Take(3).ToList();

                if (testCourse != null && testStudents.Any())
                {
                    foreach (var student in testStudents)
                    {
                        var registration = new CourseRegistration
                        {
                            CourseId = testCourse.Id,
                            StudentId = student.Id,
                            RegistrationDate = DateTime.UtcNow.AddDays(-10),
                            TotalAmount = testCourse.Price,
                            PaidAmount = testCourse.Price * 0.7m, // 70% paid
                            PaymentStatus = PaymentStatus.PartiallyPaid,
                            PaymentMethod = PaymentMethod.Cash,
                            Notes = "تسجيل تجريبي"
                        };
                        context.CourseRegistrations.Add(registration);
                    }
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}
