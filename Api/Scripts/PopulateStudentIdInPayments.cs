using Api.Data;
using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Scripts
{
    public static class PopulateStudentIdInPayments
    {
        public static async Task PopulateStudentIds(ApplicationDbContext context)
        {
            Console.WriteLine("بدء تحديث StudentId في جدول المدفوعات...");

            // 1. تحديث المدفوعات المرتبطة بتسجيلات الكورسات
            var courseRegistrationPayments = await context.Payments
                .Include(p => p.CourseRegistration)
                    .ThenInclude(cr => cr.Student)
                .Where(p => p.StudentId == null && p.CourseRegistrationId != null)
                .ToListAsync();

            Console.WriteLine($"تم العثور على {courseRegistrationPayments.Count} دفعة مرتبطة بتسجيلات الكورسات");

            foreach (var payment in courseRegistrationPayments)
            {
                if (payment.CourseRegistration?.StudentId != null)
                {
                    payment.StudentId = payment.CourseRegistration.StudentId;
                    Console.WriteLine($"تم تحديث الدفعة {payment.Id} لترتبط بالطالب {payment.CourseRegistration.Student.FullName}");
                }
            }

            // 2. تحديث المدفوعات المرتبطة بحجوزات مساحة العمل
            var workspacePayments = await context.Payments
                .Include(p => p.WorkspaceBooking)
                    .ThenInclude(wb => wb.Student)
                .Where(p => p.StudentId == null && p.WorkspaceBookingId != null)
                .ToListAsync();

            Console.WriteLine($"تم العثور على {workspacePayments.Count} دفعة مرتبطة بحجوزات مساحة العمل");

            foreach (var payment in workspacePayments)
            {
                if (payment.WorkspaceBooking?.StudentId != null)
                {
                    payment.StudentId = payment.WorkspaceBooking.StudentId;
                    Console.WriteLine($"تم تحديث الدفعة {payment.Id} لترتبط بالطالب {payment.WorkspaceBooking.Student.FullName}");
                }
            }

            // 3. تحديث المدفوعات المرتبطة بحجوزات المساحة المشتركة
            var sharedWorkspacePayments = await context.Payments
                .Include(p => p.SharedWorkspaceBooking)
                    .ThenInclude(swb => swb.Student)
                .Where(p => p.StudentId == null && p.SharedWorkspaceBookingId != null)
                .ToListAsync();

            Console.WriteLine($"تم العثور على {sharedWorkspacePayments.Count} دفعة مرتبطة بحجوزات المساحة المشتركة");

            foreach (var payment in sharedWorkspacePayments)
            {
                if (payment.SharedWorkspaceBooking?.StudentId != null)
                {
                    payment.StudentId = payment.SharedWorkspaceBooking.StudentId;
                    Console.WriteLine($"تم تحديث الدفعة {payment.Id} لترتبط بالطالب {payment.SharedWorkspaceBooking.Student.FullName}");
                }
            }

            // 4. حفظ التغييرات
            if (courseRegistrationPayments.Any() || workspacePayments.Any() || sharedWorkspacePayments.Any())
            {
                await context.SaveChangesAsync();
                Console.WriteLine("تم حفظ جميع التحديثات بنجاح");
            }
            else
            {
                Console.WriteLine("لا توجد مدفوعات تحتاج إلى تحديث");
            }

            // 5. عرض إحصائيات
            var totalPayments = await context.Payments.CountAsync();
            var paymentsWithStudentId = await context.Payments.CountAsync(p => p.StudentId != null);
            var paymentsWithoutStudentId = await context.Payments.CountAsync(p => p.StudentId == null);

            Console.WriteLine($"\nإحصائيات المدفوعات:");
            Console.WriteLine($"إجمالي المدفوعات: {totalPayments}");
            Console.WriteLine($"الدفوعات المرتبطة بالطلاب: {paymentsWithStudentId}");
            Console.WriteLine($"الدفوعات غير المرتبطة بالطلاب: {paymentsWithoutStudentId}");

            // 6. عرض المدفوعات التي لا تزال غير مرتبطة
            if (paymentsWithoutStudentId > 0)
            {
                Console.WriteLine("\nالدفوعات التي لا تزال غير مرتبطة بالطلاب:");
                var unlinkedPayments = await context.Payments
                    .Where(p => p.StudentId == null)
                    .Take(10)
                    .ToListAsync();

                foreach (var payment in unlinkedPayments)
                {
                    Console.WriteLine($"- الدفعة {payment.Id}: المبلغ {payment.Amount}, التاريخ {payment.PaymentDate:yyyy-MM-dd}");
                }

                if (paymentsWithoutStudentId > 10)
                {
                    Console.WriteLine($"... و {paymentsWithoutStudentId - 10} دفعة أخرى");
                }
            }

            Console.WriteLine("\nتم الانتهاء من تحديث StudentId في جدول المدفوعات!");
        }
    }
}
