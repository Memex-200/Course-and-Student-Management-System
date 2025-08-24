using Api.Data;
using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Scripts
{
    public static class PopulatePaymentData
    {
        public static async Task Execute(ApplicationDbContext context)
        {
            Console.WriteLine("بدء تحديث بيانات المدفوعات...");

            // 1. تحديث BranchId للمدفوعات المرتبطة بتسجيلات الكورسات
            var coursePayments = await context.Payments
                .Include(p => p.CourseRegistration)
                    .ThenInclude(cr => cr.Course)
                .Where(p => p.BranchId == null && p.CourseRegistrationId != null)
                .ToListAsync();

            Console.WriteLine($"تم العثور على {coursePayments.Count} دفعة مرتبطة بالكورسات تحتاج إلى تحديث BranchId");

            foreach (var payment in coursePayments)
            {
                if (payment.CourseRegistration?.Course?.BranchId != null)
                {
                    payment.BranchId = payment.CourseRegistration.Course.BranchId;
                    Console.WriteLine($"تم تحديث الدفعة {payment.Id} لترتبط بالفرع {payment.BranchId}");
                }
            }

            // 2. تحديث BranchId للمدفوعات المرتبطة بحجوزات مساحة العمل
            var workspacePayments = await context.Payments
                .Include(p => p.WorkspaceBooking)
                .Where(p => p.BranchId == null && p.WorkspaceBookingId != null)
                .ToListAsync();

            Console.WriteLine($"تم العثور على {workspacePayments.Count} دفعة مرتبطة بمساحة العمل تحتاج إلى تحديث BranchId");

            foreach (var payment in workspacePayments)
            {
                if (payment.WorkspaceBooking?.BranchId != null)
                {
                    payment.BranchId = payment.WorkspaceBooking.BranchId;
                    Console.WriteLine($"تم تحديث الدفعة {payment.Id} لترتبط بالفرع {payment.BranchId}");
                }
            }

            // 3. تحديث StudentId للمدفوعات التي لا تحتوي عليه
            var paymentsWithoutStudentId = await context.Payments
                .Include(p => p.CourseRegistration)
                    .ThenInclude(cr => cr.Student)
                .Include(p => p.WorkspaceBooking)
                    .ThenInclude(wb => wb.Student)
                .Include(p => p.SharedWorkspaceBooking)
                    .ThenInclude(swb => swb.Student)
                .Where(p => p.StudentId == null)
                .ToListAsync();

            Console.WriteLine($"تم العثور على {paymentsWithoutStudentId.Count} دفعة تحتاج إلى تحديث StudentId");

            foreach (var payment in paymentsWithoutStudentId)
            {
                if (payment.CourseRegistration?.StudentId != null)
                {
                    payment.StudentId = payment.CourseRegistration.StudentId;
                    Console.WriteLine($"تم تحديث الدفعة {payment.Id} لترتبط بالطالب {payment.CourseRegistration.Student.FullName}");
                }
                else if (payment.WorkspaceBooking?.StudentId != null)
                {
                    payment.StudentId = payment.WorkspaceBooking.StudentId;
                    Console.WriteLine($"تم تحديث الدفعة {payment.Id} لترتبط بالطالب {payment.WorkspaceBooking.Student.FullName}");
                }
                else if (payment.SharedWorkspaceBooking?.StudentId != null)
                {
                    payment.StudentId = payment.SharedWorkspaceBooking.StudentId;
                    Console.WriteLine($"تم تحديث الدفعة {payment.Id} لترتبط بالطالب {payment.SharedWorkspaceBooking.Student.FullName}");
                }
            }

            // 4. تحديث PaymentSource للمدفوعات التي لا تحتوي عليه
            var paymentsWithoutSource = await context.Payments
                .Where(p => p.PaymentSource == 0)
                .ToListAsync();

            Console.WriteLine($"تم العثور على {paymentsWithoutSource.Count} دفعة تحتاج إلى تحديث PaymentSource");

            foreach (var payment in paymentsWithoutSource)
            {
                if (payment.CourseRegistrationId != null)
                {
                    payment.PaymentSource = PaymentSource.CourseFee;
                }
                else if (payment.WorkspaceBookingId != null || payment.SharedWorkspaceBookingId != null)
                {
                    payment.PaymentSource = PaymentSource.Workspace;
                }
                else
                {
                    payment.PaymentSource = PaymentSource.Other;
                }
                Console.WriteLine($"تم تحديث الدفعة {payment.Id} لترتبط بمصدر الدفع {payment.PaymentSource}");
            }

            // 5. حفظ جميع التغييرات
            if (coursePayments.Any() || workspacePayments.Any() || paymentsWithoutStudentId.Any() || paymentsWithoutSource.Any())
            {
                await context.SaveChangesAsync();
                Console.WriteLine("تم حفظ جميع التحديثات بنجاح");
            }
            else
            {
                Console.WriteLine("لا توجد مدفوعات تحتاج إلى تحديث");
            }

            // 6. عرض إحصائيات نهائية
            var totalPayments = await context.Payments.CountAsync();
            var paymentsWithStudentId = await context.Payments.CountAsync(p => p.StudentId != null);
            var paymentsWithBranchId = await context.Payments.CountAsync(p => p.BranchId != null);
            var paymentsWithSource = await context.Payments.CountAsync(p => p.PaymentSource != 0);

            Console.WriteLine($"\nإحصائيات المدفوعات النهائية:");
            Console.WriteLine($"إجمالي المدفوعات: {totalPayments}");
            Console.WriteLine($"الدفوعات المرتبطة بالطلاب: {paymentsWithStudentId}");
            Console.WriteLine($"الدفوعات المرتبطة بالفروع: {paymentsWithBranchId}");
            Console.WriteLine($"الدفوعات المرتبطة بمصدر الدفع: {paymentsWithSource}");

            Console.WriteLine("\nتم الانتهاء من تحديث بيانات المدفوعات!");
        }
    }
}
