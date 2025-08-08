using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // DbSets
        public DbSet<Branch> Branches { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<CourseCategory> CourseCategories { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<CourseRegistration> CourseRegistrations { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<Lab> Labs { get; set; }
        public DbSet<LabReservation> LabReservations { get; set; }
        public DbSet<WorkspaceDesk> WorkspaceDesks { get; set; }
        public DbSet<WorkspaceSession> WorkspaceSessions { get; set; }
        public DbSet<WorkspaceInvoice> WorkspaceInvoices { get; set; }
        public DbSet<WorkspaceInvoiceItem> WorkspaceInvoiceItems { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<RoomReservation> RoomReservations { get; set; }
        public DbSet<Equipment> Equipment { get; set; }
        public DbSet<EquipmentReservation> EquipmentReservations { get; set; }
        public DbSet<Expense> Expenses { get; set; }
        public DbSet<Certificate> Certificates { get; set; }
        public DbSet<CafeteriaItem> CafeteriaItems { get; set; }
        public DbSet<CafeteriaOrder> CafeteriaOrders { get; set; }
        public DbSet<CafeteriaOrderItem> CafeteriaOrderItems { get; set; }
        public DbSet<WorkspaceBooking> WorkspaceBookings { get; set; }
        public DbSet<SharedWorkspace> SharedWorkspaces { get; set; }
        public DbSet<SharedWorkspaceBooking> SharedWorkspaceBookings { get; set; }
        public DbSet<WorkspaceOccupancy> WorkspaceOccupancies { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Branch Configuration
            modelBuilder.Entity<Branch>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.HasIndex(e => e.Name).IsUnique();
            });

            // User Configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasOne(e => e.Branch)
                      .WithMany()
                      .HasForeignKey(e => e.BranchId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Student Configuration
            modelBuilder.Entity<Student>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.User)
                      .WithOne()
                      .HasForeignKey<Student>(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade)
                      .IsRequired(false); // Make UserId optional
                entity.HasOne(e => e.Branch)
                      .WithMany(b => b.Students)
                      .HasForeignKey(e => e.BranchId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Employee Configuration
            modelBuilder.Entity<Employee>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Salary).HasColumnType("decimal(18,2)");
                entity.HasOne(e => e.User)
                      .WithOne()
                      .HasForeignKey<Employee>(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Branch)
                      .WithMany(b => b.Employees)
                      .HasForeignKey(e => e.BranchId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // CourseCategory Configuration
            modelBuilder.Entity<CourseCategory>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            });

            // Course Configuration
            modelBuilder.Entity<Course>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
                entity.Property(e => e.CourseDays).HasMaxLength(200);
                entity.Property(e => e.StartTime).HasMaxLength(50);
                entity.Property(e => e.EndTime).HasMaxLength(50);
                entity.HasOne(e => e.CourseCategory)
                      .WithMany(cc => cc.Courses)
                      .HasForeignKey(e => e.CourseCategoryId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Branch)
                      .WithMany(b => b.Courses)
                      .HasForeignKey(e => e.BranchId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Instructor)
                      .WithMany(i => i.CoursesAsInstructor)
                      .HasForeignKey(e => e.InstructorId)
                      .OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(e => e.Lab)
                      .WithMany(l => l.Courses)
                      .HasForeignKey(e => e.LabId)
                      .OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(e => e.Room)
                      .WithMany(r => r.CoursesInRoom)
                      .HasForeignKey(e => e.RoomId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // CourseRegistration Configuration
            modelBuilder.Entity<CourseRegistration>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.PaidAmount).HasColumnType("decimal(18,2)");
                entity.HasOne(e => e.Student)
                      .WithMany(s => s.CourseRegistrations)
                      .HasForeignKey(e => e.StudentId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Course)
                      .WithMany(c => c.CourseRegistrations)
                      .HasForeignKey(e => e.CourseId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Payment Configuration
            modelBuilder.Entity<Payment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
                entity.HasOne(e => e.CourseRegistration)
                      .WithMany(cr => cr.Payments)
                      .HasForeignKey(e => e.CourseRegistrationId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.WorkspaceBooking)
                      .WithMany()
                      .HasForeignKey(e => e.WorkspaceBookingId)
                      .OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(e => e.SharedWorkspaceBooking)
                      .WithMany(swb => swb.Payments)
                      .HasForeignKey(e => e.SharedWorkspaceBookingId)
                      .OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(e => e.ProcessedByUser)
                      .WithMany()
                      .HasForeignKey(e => e.ProcessedByUserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Attendance Configuration
            modelBuilder.Entity<Attendance>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Student)
                      .WithMany(s => s.Attendances)
                      .HasForeignKey(e => e.StudentId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Course)
                      .WithMany(c => c.Attendances)
                      .HasForeignKey(e => e.CourseId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.RecordedByUser)
                      .WithMany()
                      .HasForeignKey(e => e.RecordedByUserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Lab Configuration
            modelBuilder.Entity<Lab>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.HasOne(e => e.Branch)
                      .WithMany(b => b.Labs)
                      .HasForeignKey(e => e.BranchId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // LabReservation Configuration
            modelBuilder.Entity<LabReservation>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Lab)
                      .WithMany(l => l.LabReservations)
                      .HasForeignKey(e => e.LabId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Course)
                      .WithMany()
                      .HasForeignKey(e => e.CourseId)
                      .OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(e => e.ReservedByUser)
                      .WithMany()
                      .HasForeignKey(e => e.ReservedByUserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // WorkspaceDesk Configuration
            modelBuilder.Entity<WorkspaceDesk>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.DeskNumber).IsRequired().HasMaxLength(50);
                entity.Property(e => e.HourlyRate).HasColumnType("decimal(18,2)");
                entity.HasOne(e => e.Branch)
                      .WithMany()
                      .HasForeignKey(e => e.BranchId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasIndex(e => new { e.BranchId, e.DeskNumber }).IsUnique();
            });

            // WorkspaceSession Configuration
            modelBuilder.Entity<WorkspaceSession>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.InternetCost).HasColumnType("decimal(18,2)");
                entity.Property(e => e.LaptopCost).HasColumnType("decimal(18,2)");
                entity.Property(e => e.CafeteriaCost).HasColumnType("decimal(18,2)");
                entity.Property(e => e.TotalCost).HasColumnType("decimal(18,2)");
                entity.HasOne(e => e.WorkspaceDesk)
                      .WithMany(wd => wd.WorkspaceSessions)
                      .HasForeignKey(e => e.WorkspaceDeskId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Student)
                      .WithMany()
                      .HasForeignKey(e => e.StudentId)
                      .OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(e => e.CreatedByUser)
                      .WithMany()
                      .HasForeignKey(e => e.CreatedByUserId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Branch)
                      .WithMany(b => b.WorkspaceSessions)
                      .HasForeignKey(e => e.BranchId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // WorkspaceInvoice Configuration
            modelBuilder.Entity<WorkspaceInvoice>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.InvoiceNumber).IsRequired().HasMaxLength(50);
                entity.Property(e => e.SubTotal).HasColumnType("decimal(18,2)");
                entity.Property(e => e.TaxAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.DiscountAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");
                entity.HasOne(e => e.WorkspaceSession)
                      .WithMany()
                      .HasForeignKey(e => e.WorkspaceSessionId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Branch)
                      .WithMany()
                      .HasForeignKey(e => e.BranchId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.CreatedByUser)
                      .WithMany()
                      .HasForeignKey(e => e.CreatedByUserId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasIndex(e => e.InvoiceNumber).IsUnique();
            });

            // WorkspaceInvoiceItem Configuration
            modelBuilder.Entity<WorkspaceInvoiceItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Quantity).HasColumnType("decimal(18,2)");
                entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)");
                entity.HasOne(e => e.WorkspaceInvoice)
                      .WithMany(wi => wi.WorkspaceInvoiceItems)
                      .HasForeignKey(e => e.WorkspaceInvoiceId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.WorkspaceSession)
                      .WithMany(ws => ws.WorkspaceInvoiceItems)
                      .HasForeignKey(e => e.WorkspaceSessionId)
                      .OnDelete(DeleteBehavior.NoAction);
            });

            // Room Configuration
            modelBuilder.Entity<Room>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.RoomNumber).IsRequired().HasMaxLength(50);
                entity.HasOne(e => e.Branch)
                      .WithMany()
                      .HasForeignKey(e => e.BranchId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasIndex(e => new { e.BranchId, e.RoomNumber }).IsUnique();
            });

            // RoomReservation Configuration
            modelBuilder.Entity<RoomReservation>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.HasOne(e => e.Room)
                      .WithMany(r => r.RoomReservations)
                      .HasForeignKey(e => e.RoomId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Course)
                      .WithMany()
                      .HasForeignKey(e => e.CourseId)
                      .OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(e => e.ReservedByUser)
                      .WithMany()
                      .HasForeignKey(e => e.ReservedByUserId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Instructor)
                      .WithMany()
                      .HasForeignKey(e => e.InstructorId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // Equipment Configuration
            modelBuilder.Entity<Equipment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.SerialNumber).IsRequired().HasMaxLength(100);
                entity.Property(e => e.PurchasePrice).HasColumnType("decimal(18,2)");
                entity.HasOne(e => e.Branch)
                      .WithMany()
                      .HasForeignKey(e => e.BranchId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Room)
                      .WithMany()
                      .HasForeignKey(e => e.RoomId)
                      .OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(e => e.AssignedToUser)
                      .WithMany()
                      .HasForeignKey(e => e.AssignedToUserId)
                      .OnDelete(DeleteBehavior.SetNull);
                entity.HasIndex(e => e.SerialNumber).IsUnique();
            });

            // EquipmentReservation Configuration
            modelBuilder.Entity<EquipmentReservation>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Purpose).IsRequired().HasMaxLength(200);
                entity.Property(e => e.DamageCost).HasColumnType("decimal(18,2)");
                entity.HasOne(e => e.Equipment)
                      .WithMany(eq => eq.EquipmentReservations)
                      .HasForeignKey(e => e.EquipmentId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.ReservedByUser)
                      .WithMany()
                      .HasForeignKey(e => e.ReservedByUserId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.ApprovedByUser)
                      .WithMany()
                      .HasForeignKey(e => e.ApprovedByUserId)
                      .OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(e => e.Course)
                      .WithMany()
                      .HasForeignKey(e => e.CourseId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // Expense Configuration
            modelBuilder.Entity<Expense>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
                entity.HasOne(e => e.Branch)
                      .WithMany()
                      .HasForeignKey(e => e.BranchId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.RequestedByUser)
                      .WithMany()
                      .HasForeignKey(e => e.RequestedByUserId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.ApprovedByUser)
                      .WithMany()
                      .HasForeignKey(e => e.ApprovedByUserId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // CafeteriaItem Configuration
            modelBuilder.Entity<CafeteriaItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Cost).HasColumnType("decimal(18,2)");
                entity.HasOne(e => e.Branch)
                      .WithMany()
                      .HasForeignKey(e => e.BranchId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // CafeteriaOrder Configuration
            modelBuilder.Entity<CafeteriaOrder>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.OrderNumber).IsRequired().HasMaxLength(50);
                entity.Property(e => e.SubTotal).HasColumnType("decimal(18,2)");
                entity.Property(e => e.TaxAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.DiscountAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.PaidAmount).HasColumnType("decimal(18,2)");
                entity.HasOne(e => e.Student)
                      .WithMany()
                      .HasForeignKey(e => e.StudentId)
                      .OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(e => e.Employee)
                      .WithMany()
                      .HasForeignKey(e => e.EmployeeId)
                      .OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(e => e.Branch)
                      .WithMany()
                      .HasForeignKey(e => e.BranchId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.CreatedByUser)
                      .WithMany()
                      .HasForeignKey(e => e.CreatedByUserId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasIndex(e => e.OrderNumber).IsUnique();
            });

            // CafeteriaOrderItem Configuration
            modelBuilder.Entity<CafeteriaOrderItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)");
                entity.HasOne(e => e.CafeteriaOrder)
                      .WithMany(co => co.CafeteriaOrderItems)
                      .HasForeignKey(e => e.CafeteriaOrderId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.CafeteriaItem)
                      .WithMany(ci => ci.CafeteriaOrderItems)
                      .HasForeignKey(e => e.CafeteriaItemId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // SharedWorkspace Configuration
            modelBuilder.Entity<SharedWorkspace>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.HourlyRatePerPerson).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Equipment).HasMaxLength(500);
                entity.HasOne(e => e.Branch)
                      .WithMany()
                      .HasForeignKey(e => e.BranchId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // SharedWorkspaceBooking Configuration
            modelBuilder.Entity<SharedWorkspaceBooking>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CustomerName).HasMaxLength(100);
                entity.Property(e => e.CustomerPhone).HasMaxLength(20);
                entity.Property(e => e.CustomerEmail).HasMaxLength(100);
                entity.Property(e => e.HourlyRate).HasColumnType("decimal(18,2)");
                entity.Property(e => e.TotalHours).HasColumnType("decimal(18,2)");
                entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.PaidAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Purpose).HasMaxLength(500);
                entity.Property(e => e.Notes).HasMaxLength(1000);
                entity.Property(e => e.RequiredEquipment).HasMaxLength(500);
                entity.HasOne(e => e.SharedWorkspace)
                      .WithMany(sw => sw.Bookings)
                      .HasForeignKey(e => e.SharedWorkspaceId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Student)
                      .WithMany()
                      .HasForeignKey(e => e.StudentId)
                      .OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(e => e.Branch)
                      .WithMany()
                      .HasForeignKey(e => e.BranchId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.BookedByUser)
                      .WithMany()
                      .HasForeignKey(e => e.BookedByUserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // WorkspaceOccupancy Configuration
            modelBuilder.Entity<WorkspaceOccupancy>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Notes).HasMaxLength(500);
                entity.HasOne(e => e.SharedWorkspace)
                      .WithMany()
                      .HasForeignKey(e => e.SharedWorkspaceId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Booking)
                      .WithMany()
                      .HasForeignKey(e => e.SharedWorkspaceBookingId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // WorkspaceBooking Configuration
            modelBuilder.Entity<WorkspaceBooking>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.HourlyRate).HasColumnType("decimal(18,2)");
                entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.PaidAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.RemainingAmount).HasColumnType("decimal(18,2)");
                entity.HasOne(e => e.Student)
                      .WithMany(s => s.WorkspaceBookings)
                      .HasForeignKey(e => e.StudentId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Room)
                      .WithMany()
                      .HasForeignKey(e => e.RoomId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Branch)
                      .WithMany()
                      .HasForeignKey(e => e.BranchId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.BookedByUser)
                      .WithMany()
                      .HasForeignKey(e => e.BookedByUserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Seed Data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed Branches
            modelBuilder.Entity<Branch>().HasData(
                new Branch { Id = 1, Name = "فرع أسيوط", Address = "أسيوط، مصر", Phone = "0882123456", HasWorkspace = true, IsActive = true },
                new Branch { Id = 2, Name = "فرع أبو تيج", Address = "أبو تيج، أسيوط، مصر", Phone = "0882654321", HasWorkspace = false, IsActive = true }
            );

            // Seed Test User
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Username = "testuser",
                    FullName = "Test User",
                    Email = "test@example.com",
                    Phone = "0123456789",
                    PasswordHash = "9Dpj3YyGsMQPKJvPXhvtqhsGnXmyJLwx4m2TJgZx3jM=", // Test123!
                    UserRole = UserRole.Student,
                    BranchId = 1,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                }
            );

            // Seed Course Categories
            modelBuilder.Entity<CourseCategory>().HasData(
                new CourseCategory { Id = 1, Name = "أساسيات الروبوتيكس والتفكير الصحيح", Description = "للأطفال من 4-6 سنوات", MinAge = 4, MaxAge = 6, IsActive = true },
                new CourseCategory { Id = 2, Name = "المستوى الأول (7-12 سنة)", Description = "أساسيات الروبوتيكس والذكاء الاصطناعي، الكمبيوتر والبرمجة، الهندسة الكهربية والميكاترونكس", MinAge = 7, MaxAge = 12, IsActive = true },
                new CourseCategory { Id = 3, Name = "المستوى الأول (13-17 سنة)", Description = "أساسيات الروبوتيكس والذكاء الاصطناعي، البرمجة بـ Python و C++، الهندسة الكهربية والميكاترونكس", MinAge = 13, MaxAge = 17, IsActive = true },
                new CourseCategory { Id = 4, Name = "كورسات التخصص (18+ سنة)", Description = "تطوير المواقع، تطبيقات الهواتف، تطبيقات سطح المكتب، تحليل البيانات، الروبوتات، الميكاترونيكس", MinAge = 18, MaxAge = 100, IsActive = true }
            );

            // Seed Rooms for Assiut Branch (3 rooms)
            modelBuilder.Entity<Room>().HasData(
                new Room { Id = 1, Name = "قاعة الروبوتيكس الأولى", RoomNumber = "R101", Description = "قاعة مخصصة لكورسات الروبوتيكس والذكاء الاصطناعي", RoomType = RoomType.Lab, Capacity = 20, BranchId = 1, Equipment = "أجهزة كمبيوتر، مجموعات روبوتيكس، شاشة عرض", Location = "الطابق الأول", IsActive = true },
                new Room { Id = 2, Name = "قاعة البرمجة", RoomNumber = "R102", Description = "قاعة مخصصة لكورسات البرمجة وتطوير التطبيقات", RoomType = RoomType.Classroom, Capacity = 25, BranchId = 1, Equipment = "أجهزة كمبيوتر، شاشة عرض، سبورة ذكية", Location = "الطابق الأول", IsActive = true },
                new Room { Id = 3, Name = "ورشة الميكاترونيكس", RoomNumber = "R103", Description = "ورشة مخصصة للأعمال العملية والمشاريع", RoomType = RoomType.Workshop, Capacity = 15, BranchId = 1, Equipment = "طاولات عمل، أدوات هندسية، معدات إلكترونية", Location = "الطابق الأول", IsActive = true }
            );

            // Seed Shared Workspaces for Assiut Branch
            modelBuilder.Entity<SharedWorkspace>().HasData(
                new SharedWorkspace { Id = 1, Name = "مساحة العمل المشتركة الأولى", Description = "مساحة مفتوحة للدراسة والعمل الجماعي", MaxCapacity = 12, HourlyRatePerPerson = 5.0m, HasWifi = true, HasPrinter = true, HasProjector = false, HasWhiteboard = true, Equipment = "واي فاي، طابعة، سبورة، طاولات دراسة", BranchId = 1, IsActive = true },
                new SharedWorkspace { Id = 2, Name = "مساحة العمل المشتركة الثانية", Description = "مساحة هادئة للدراسة الفردية والجماعية", MaxCapacity = 8, HourlyRatePerPerson = 4.0m, HasWifi = true, HasPrinter = false, HasProjector = true, HasWhiteboard = true, Equipment = "واي فاي، بروجكتر، سبورة، طاولات دراسة", BranchId = 1, IsActive = true }
            );
        }
    }
}
