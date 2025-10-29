using Api.Data;
using Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using OfficeOpenXml;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    
    public class CafeteriaController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CafeteriaController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("items")]
        public async Task<IActionResult> GetCafeteriaItems([FromQuery] CafeteriaItemCategory? category = null, [FromQuery] bool? isAvailable = null)
        {
            try
            {
                var userBranchId = int.Parse(User.FindFirst("BranchId")?.Value ?? "0");
                var query = _context.CafeteriaItems
                    .Include(ci => ci.Branch)
                    .Where(ci => ci.BranchId == userBranchId && ci.IsActive);

                if (category.HasValue)
                    query = query.Where(ci => ci.Category == category.Value);

                if (isAvailable.HasValue && isAvailable.Value)
                    query = query.Where(ci => ci.StockQuantity > 0);

                var items = await query
                    .OrderBy(ci => ci.Category)
                    .ThenBy(ci => ci.Name)
                    .Select(ci => new
                    {
                        ci.Id,
                        ci.Name,
                        ci.Description,
                        ci.Category,
                        ci.Price,
                        ci.Cost,
                        ci.StockQuantity,
                        ci.MinimumStock,
                        ci.IsAvailable,
                        IsLowStock = ci.StockQuantity <= ci.MinimumStock,
                        ProfitMargin = ci.Price - ci.Cost,
                        ci.CreatedAt
                    })
                    .ToListAsync();

                return Ok(items);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "حدث خطأ في الخادم", error = ex.Message });
            }
        }

        [HttpPost("items")]
        
        public async Task<IActionResult> CreateCafeteriaItem([FromBody] CreateCafeteriaItemRequest request)
        {
            try
            {
                var userBranchId = int.Parse(User.FindFirst("BranchId")?.Value ?? "0");

                var item = new CafeteriaItem
                {
                    Name = request.Name,
                    Description = request.Description,
                    Category = request.Category,
                    Price = request.Price,
                    Cost = request.Cost,
                    StockQuantity = request.StockQuantity,
                    MinimumStock = request.MinimumStock,
                    IsAvailable = request.IsAvailable,
                    BranchId = userBranchId,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                };

                _context.CafeteriaItems.Add(item);
                await _context.SaveChangesAsync();

                return Ok(new { message = "تم إضافة المنتج بنجاح", itemId = item.Id });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "حدث خطأ في الخادم", error = ex.Message });
            }
        }

        [HttpPut("items/{id}")]
        
        public async Task<IActionResult> UpdateCafeteriaItem(int id, [FromBody] UpdateCafeteriaItemRequest request)
        {
            try
            {
                var item = await _context.CafeteriaItems.FindAsync(id);
                if (item == null)
                    return NotFound(new { message = "المنتج غير موجود" });

                item.Name = request.Name;
                item.Description = request.Description;
                item.Category = request.Category;
                item.Price = request.Price;
                item.Cost = request.Cost;
                item.StockQuantity = request.StockQuantity;
                item.MinimumStock = request.MinimumStock;
                item.IsAvailable = request.IsAvailable;
                item.IsActive = request.IsActive;

                await _context.SaveChangesAsync();

                return Ok(new { message = "تم تحديث المنتج بنجاح" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "حدث خطأ في الخادم", error = ex.Message });
            }
        }

        [HttpDelete("items/{id}")]
        
        public async Task<IActionResult> DeleteCafeteriaItem(int id)
        {
            var item = await _context.CafeteriaItems.FindAsync(id);
            if (item == null)
                return NotFound(new { message = "المنتج غير موجود" });

            _context.CafeteriaItems.Remove(item);
            await _context.SaveChangesAsync();

            return Ok(new { message = "تم حذف المنتج بنجاح" });
        }

        [HttpGet("orders")]
        public async Task<IActionResult> GetCafeteriaOrders([FromQuery] CafeteriaOrderStatus? status = null, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var userBranchId = int.Parse(User.FindFirst("BranchId")?.Value ?? "0");
                var query = _context.CafeteriaOrders
                    .Include(co => co.Student)
                    .Include(co => co.Employee)
                        .ThenInclude(e => e!.User)
                    .Include(co => co.CreatedByUser)
                    .Include(co => co.CafeteriaOrderItems)
                        .ThenInclude(coi => coi!.CafeteriaItem)
                    .Where(co => co.BranchId == userBranchId);

                if (status.HasValue)
                    query = query.Where(co => co.Status == status.Value);

                if (startDate.HasValue)
                    query = query.Where(co => co.OrderDate >= startDate.Value);

                if (endDate.HasValue)
                    query = query.Where(co => co.OrderDate <= endDate.Value);

                var ordersRaw = await query
                    .OrderByDescending(co => co.OrderDate)
                    .ToListAsync();

                var orders = ordersRaw.Select(co => new
                    {
                        co.Id,
                        co.OrderNumber,
                        co.OrderDate,
                        co.Status,
                    StatusArabic = GetStatusArabic(co.Status),
                        co.SubTotal,
                        co.TaxAmount,
                        co.DiscountAmount,
                        co.TotalAmount,
                        co.PaidAmount,
                        co.PaymentStatus,
                        co.PaymentMethod,
                        Customer = co.Student != null ? co.Student.FullName :
                                  ((co.Employee != null && co.Employee.User != null) ? co.Employee.User.FullName : "زبون"),
                        CustomerPhone = co.Student != null ? co.Student.Phone :
                                       ((co.Employee != null && co.Employee.User != null) ? co.Employee.User.Phone : ""),
                        CreatedBy = co.CreatedByUser != null ? co.CreatedByUser.FullName : string.Empty,
                    CreatedByUser = co.CreatedByUser != null ? new { co.CreatedByUser.Id, co.CreatedByUser.FullName } : null,
                        ItemsCount = co.CafeteriaOrderItems.Count,
                        RemainingAmount = co.RemainingAmount,
                        co.Notes
                }).ToList();

                return Ok(orders);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "حدث خطأ في الخادم", error = ex.Message });
            }
        }

        [HttpGet("orders/{id}")]
        public async Task<IActionResult> GetCafeteriaOrder(int id)
        {
            try
            {
                var order = await _context.CafeteriaOrders
                    .Include(co => co.Student)
                    .Include(co => co.Employee)
                        .ThenInclude(e => e!.User)
                    .Include(co => co.CreatedByUser)
                    .Include(co => co.CafeteriaOrderItems)
                        .ThenInclude(coi => coi!.CafeteriaItem)
                    .FirstOrDefaultAsync(co => co.Id == id);

                if (order == null)
                    return NotFound(new { message = "الطلب غير موجود" });

                var result = new
                {
                    order.Id,
                    order.OrderNumber,
                    order.OrderDate,
                    order.Status,
                    order.SubTotal,
                    order.TaxAmount,
                    order.DiscountAmount,
                    order.TotalAmount,
                    order.PaidAmount,
                    order.PaymentStatus,
                    PaymentStatusArabic = GetPaymentStatusArabic(order.PaymentStatus),
                    order.PaymentMethod,
                    PaymentMethodArabic = GetPaymentMethodArabic(order.PaymentMethod),
                    Customer = new
                    {
                        Name = order.CustomerName,
                        Phone = order.CustomerPhone,
                        Type = "Guest"
                    },
                    CreatedBy = order.CreatedByUser != null ? new { order.CreatedByUser.Id, order.CreatedByUser.FullName } : null,
                    Items = order.CafeteriaOrderItems.Select(coi => new
                    {
                        coi.Id,
                        Item = coi.CafeteriaItem.Name,
                        coi.Quantity,
                        coi.UnitPrice,
                        TotalPrice = coi.TotalPrice,
                        coi.Notes,
                        coi.Customization
                    }).ToList(),
                    order.Notes,
                    RemainingAmount = order.RemainingAmount
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "حدث خطأ في الخادم", error = ex.Message });
            }
        }

        [HttpPost("orders")]
        
        public async Task<IActionResult> CreateCafeteriaOrder([FromBody] CreateCafeteriaOrderRequest request)
        {
            try
            {
                var userBranchId = int.Parse(User.FindFirst("BranchId")?.Value ?? "0");
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

                // Generate order number
                var orderCount = await _context.CafeteriaOrders.CountAsync() + 1;
                var orderNumber = $"CAF-{DateTime.Now:yyyyMMdd}-{orderCount:D4}";

                // Calculate totals
                decimal subTotal = 0;
                var orderItems = new List<CafeteriaOrderItem>();

                foreach (var item in request.Items)
                {
                    var cafeteriaItem = await _context.CafeteriaItems.FindAsync(item.CafeteriaItemId);
                    if (cafeteriaItem == null)
                        return BadRequest(new { message = $"المنتج غير موجود: {item.CafeteriaItemId}" });

                    if (cafeteriaItem.StockQuantity < item.Quantity)
                        return BadRequest(new { message = $"الكمية المطلوبة غير متوفرة للمنتج: {cafeteriaItem.Name}" });

                    var orderItem = new CafeteriaOrderItem
                    {
                        CafeteriaItemId = item.CafeteriaItemId,
                        Quantity = item.Quantity,
                        UnitPrice = cafeteriaItem.Price,
                        Notes = item.Notes,
                        Customization = item.Customization
                    };

                    orderItems.Add(orderItem);
                    subTotal += orderItem.TotalPrice;

                    // Update stock
                    cafeteriaItem.StockQuantity -= item.Quantity;
                }

                var totalAmount = subTotal + request.TaxAmount - request.DiscountAmount;

                var order = new CafeteriaOrder
                {
                    OrderNumber = orderNumber,
                    StudentId = request.StudentId,
                    EmployeeId = request.EmployeeId,
                    CustomerName = request.CustomerName,
                    CustomerPhone = request.CustomerPhone,
                    OrderDate = DateTime.Now,
                    SubTotal = subTotal,
                    TaxAmount = request.TaxAmount,
                    DiscountAmount = request.DiscountAmount,
                    TotalAmount = totalAmount,
                    PaidAmount = request.PaidAmount,
                    PaymentMethod = request.PaymentMethod,
                    PaymentStatus = request.PaidAmount >= totalAmount 
                        ? PaymentStatus.FullyPaid 
                        : request.PaidAmount > 0 
                            ? PaymentStatus.PartiallyPaid 
                            : PaymentStatus.Unpaid,
                    BranchId = userBranchId,
                    CreatedByUserId = userId,
                    Status = CafeteriaOrderStatus.Pending,
                    Notes = request.Notes
                };

                _context.CafeteriaOrders.Add(order);
                await _context.SaveChangesAsync();

                // Add order items
                foreach (var item in orderItems)
                {
                    item.CafeteriaOrderId = order.Id;
                    _context.CafeteriaOrderItems.Add(item);
                }

                await _context.SaveChangesAsync();

                return Ok(new { message = "تم إنشاء الطلب بنجاح", orderId = order.Id, orderNumber = order.OrderNumber });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "حدث خطأ في الخادم", error = ex.Message });
            }
        }

        [HttpPut("orders/{id}")]
        
        public async Task<IActionResult> UpdateCafeteriaOrder(int id, [FromBody] UpdateCafeteriaOrderRequest request)
        {
            try
            {
                var order = await _context.CafeteriaOrders.FindAsync(id);
                if (order == null)
                    return NotFound(new { message = "الطلب غير موجود" });

                // Update order details
                if (!string.IsNullOrEmpty(request.CustomerName))
                    order.CustomerName = request.CustomerName;
                
                if (!string.IsNullOrEmpty(request.CustomerPhone))
                    order.CustomerPhone = request.CustomerPhone;
                
                order.DiscountAmount = request.DiscountAmount;
                order.PaidAmount = request.PaidAmount;
                order.PaymentMethod = request.PaymentMethod;
                
                if (!string.IsNullOrEmpty(request.Notes))
                    order.Notes = request.Notes;

                // Recalculate total amount
                order.TotalAmount = order.SubTotal + order.TaxAmount - order.DiscountAmount;
                
                // Update payment status
                order.PaymentStatus = order.PaidAmount >= order.TotalAmount 
                    ? PaymentStatus.FullyPaid 
                    : order.PaidAmount > 0 
                        ? PaymentStatus.PartiallyPaid 
                        : PaymentStatus.Unpaid;

                await _context.SaveChangesAsync();

                return Ok(new { message = "تم تحديث الطلب بنجاح" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "حدث خطأ في الخادم", error = ex.Message });
            }
        }

        [HttpDelete("orders/{id}")]
        
        public async Task<IActionResult> DeleteCafeteriaOrder(int id)
        {
            try
            {
                var order = await _context.CafeteriaOrders
                    .Include(co => co.CafeteriaOrderItems)
                        .ThenInclude(coi => coi!.CafeteriaItem)
                    .FirstOrDefaultAsync(co => co.Id == id);

                if (order == null)
                    return NotFound(new { message = "الطلب غير موجود" });

                // Return items to stock if order is not delivered
                if (order.Status != CafeteriaOrderStatus.Delivered)
                {
                    foreach (var item in order.CafeteriaOrderItems)
                    {
                        item.CafeteriaItem.StockQuantity += item.Quantity;
                    }
                }

                _context.CafeteriaOrders.Remove(order);
                await _context.SaveChangesAsync();

                return Ok(new { message = "تم حذف الطلب بنجاح" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "حدث خطأ في الخادم", error = ex.Message });
            }
        }

        [HttpPut("orders/{id}/status")]
        
        public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] UpdateOrderStatusRequest request)
        {
            try
            {
                var order = await _context.CafeteriaOrders.FindAsync(id);
                if (order == null)
                    return NotFound(new { message = "الطلب غير موجود" });

                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

                order.Status = request.Status;

                switch (request.Status)
                {
                    case CafeteriaOrderStatus.Preparing:
                        order.PreparedByUserId = userId;
                        break;
                    case CafeteriaOrderStatus.Ready:
                        order.PreparedAt = DateTime.Now;
                        break;
                    case CafeteriaOrderStatus.Delivered:
                        order.DeliveredByUserId = userId;
                        order.DeliveredAt = DateTime.Now;
                        break;
                    case CafeteriaOrderStatus.Cancelled:
                        order.CancellationReason = request.Notes;
                        // Return items to stock
                        var orderItems = await _context.CafeteriaOrderItems
                            .Include(coi => coi.CafeteriaItem)
                            .Where(coi => coi.CafeteriaOrderId == id)
                            .ToListAsync();

                        foreach (var item in orderItems)
                        {
                            item.CafeteriaItem.StockQuantity += item.Quantity;
                        }
                        break;
                }

                if (!string.IsNullOrEmpty(request.Notes))
                    order.Notes += $"\n{DateTime.Now:yyyy-MM-dd HH:mm}: {request.Notes}";

                await _context.SaveChangesAsync();

                return Ok(new { message = "تم تحديث حالة الطلب بنجاح" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "حدث خطأ في الخادم", error = ex.Message });
            }
        }

        [HttpGet("statistics")]
        public async Task<IActionResult> GetCafeteriaStatistics([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var userBranchId = int.Parse(User.FindFirst("BranchId")?.Value ?? "0");
                var query = _context.CafeteriaOrders
                    .Include(co => co.CafeteriaOrderItems)
                        .ThenInclude(coi => coi!.CafeteriaItem)
                    .Where(co => co.BranchId == userBranchId);

                if (startDate.HasValue)
                    query = query.Where(co => co.OrderDate >= startDate.Value);

                if (endDate.HasValue)
                    query = query.Where(co => co.OrderDate <= endDate.Value);

                var orders = await query.ToListAsync();

                // بيانات الرسم البياني للإيرادات (آخر 7 أيام)
                var last7Days = Enumerable.Range(0, 7)
                    .Select(i => DateTime.Today.AddDays(-i))
                    .OrderBy(d => d)
                    .ToList();

                var revenueData = last7Days.Select(date => new {
                    date = date.ToString("yyyy-MM-dd"),
                    revenue = orders.Where(o => o.OrderDate.Date == date).Sum(o => o.TotalAmount),
                    orders = orders.Count(o => o.OrderDate.Date == date),
                    profit = orders.Where(o => o.OrderDate.Date == date).Sum(o => o.TotalAmount) -
                             orders.Where(o => o.OrderDate.Date == date)
                                   .SelectMany(o => o.CafeteriaOrderItems)
                                   .Sum(oi => oi.Quantity * oi.CafeteriaItem.Cost)
                }).ToList();

                // توزيع المبيعات بالفئات
                var categoryData = orders
                    .SelectMany(o => o.CafeteriaOrderItems)
                    .GroupBy(oi => oi.CafeteriaItem.Category.ToString())
                    .Select(g => new {
                        name = g.Key,
                        value = g.Sum(oi => oi.TotalPrice),
                        orders = g.Sum(oi => oi.Quantity)
                    }).ToList();

                // اتجاه المبيعات الشهرية (كل أسبوع)
                var salesTrend = Enumerable.Range(0, 4)
                    .Select(i => {
                        var weekStart = DateTime.Today.AddDays(-7 * (3 - i));
                        var weekEnd = weekStart.AddDays(6);
                        return new {
                            week = $"الأسبوع {i + 1}",
                            sales = orders.Where(o => o.OrderDate.Date >= weekStart && o.OrderDate.Date <= weekEnd).Sum(o => o.TotalAmount),
                            orders = orders.Count(o => o.OrderDate.Date >= weekStart && o.OrderDate.Date <= weekEnd)
                        };
                    }).ToList();

                var statistics = new
                {
                    TotalOrders = orders.Count,
                    PendingOrders = orders.Count(o => o.Status == CafeteriaOrderStatus.Pending),
                    CompletedOrders = orders.Count(o => o.Status == CafeteriaOrderStatus.Delivered),
                    CancelledOrders = orders.Count(o => o.Status == CafeteriaOrderStatus.Cancelled),
                    TotalRevenue = orders.Sum(o => o.TotalAmount),
                    TotalPaid = orders.Sum(o => o.PaidAmount),
                    TotalCost = orders.SelectMany(o => o.CafeteriaOrderItems).Sum(oi => oi.Quantity * oi.CafeteriaItem.Cost),
                    TotalProfit = orders.Sum(o => o.TotalAmount) - orders.SelectMany(o => o.CafeteriaOrderItems).Sum(oi => oi.Quantity * oi.CafeteriaItem.Cost),
                    AverageOrderValue = orders.Any() ? orders.Average(o => o.TotalAmount) : 0,
                    TopSellingItems = orders
                        .SelectMany(o => o.CafeteriaOrderItems)
                        .GroupBy(oi => oi.CafeteriaItem.Name)
                        .Select(g => new { Item = g.Key, Quantity = g.Sum(oi => oi.Quantity), Revenue = g.Sum(oi => oi.TotalPrice) })
                        .OrderByDescending(x => x.Quantity)
                        .Take(5)
                        .ToList(),
                    revenueData,
                    categoryData,
                    salesTrend
                };

                return Ok(statistics);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "حدث خطأ في الخادم", error = ex.Message });
            }
        }

        [HttpGet("quick-stats")]
        public async Task<IActionResult> GetCafeteriaQuickStats()
        {
            try
            {
                var userBranchId = int.Parse(User.FindFirst("BranchId")?.Value ?? "0");
                var today = DateTime.Today;

                // إجمالي المنتجات
                var totalItems = await _context.CafeteriaItems.CountAsync(ci => ci.BranchId == userBranchId && ci.IsActive);

                // المنتجات منخفضة المخزون
                var lowStockItems = await _context.CafeteriaItems.CountAsync(ci => ci.BranchId == userBranchId && ci.IsActive && ci.StockQuantity <= ci.MinimumStock);

                // طلبات اليوم
                var todayOrders = await _context.CafeteriaOrders.CountAsync(co => co.BranchId == userBranchId && co.OrderDate.Date == today);

                // إيرادات اليوم
                var todayRevenue = await _context.CafeteriaOrders.Where(co => co.BranchId == userBranchId && co.OrderDate.Date == today)
                    .SumAsync(co => (decimal?)co.TotalAmount) ?? 0;

                // الطلبات المعلقة
                var pendingOrders = await _context.CafeteriaOrders.CountAsync(co => co.BranchId == userBranchId && co.Status == CafeteriaOrderStatus.Pending);

                return Ok(new
                {
                    totalItems,
                    lowStockItems,
                    todayOrders,
                    todayRevenue,
                    pendingOrders
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "حدث خطأ في الخادم", error = ex.Message });
            }
        }

        private string GetPaymentStatusArabic(PaymentStatus status)
        {
            return status switch
            {
                PaymentStatus.Pending => "في الانتظار",
                PaymentStatus.Unpaid => "غير مدفوع",
                PaymentStatus.PartiallyPaid => "مدفوع جزئياً",
                PaymentStatus.FullyPaid => "مدفوع بالكامل",
                PaymentStatus.Cancelled => "ملغي",
                _ => "غير محدد"
            };
        }

        private string GetPaymentMethodArabic(PaymentMethod method)
        {
            return method switch
            {
                PaymentMethod.Cash => "نقداً",
                PaymentMethod.InstaPay => "إنستا باي",
                PaymentMethod.Fawry => "فوري",
                _ => "غير محدد"
            };
        }

        private string GetStatusArabic(CafeteriaOrderStatus status)
        {
            return status switch
            {
                CafeteriaOrderStatus.Pending => "في الانتظار",
                CafeteriaOrderStatus.Preparing => "قيد التحضير",
                CafeteriaOrderStatus.Ready => "جاهز",
                CafeteriaOrderStatus.Delivered => "تم التسليم",
                CafeteriaOrderStatus.Cancelled => "ملغي",
                _ => "غير محدد"
            };
        }

        [HttpGet("export/excel")]
        
        public async Task<IActionResult> ExportToExcel([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var userBranchId = int.Parse(User.FindFirst("BranchId")?.Value ?? "0");
                
                // Get all orders with details
                var orders = await _context.CafeteriaOrders
                    .Include(co => co.Student)
                    .Include(co => co.Employee)
                        .ThenInclude(e => e!.User)
                    .Include(co => co.CafeteriaOrderItems)
                        .ThenInclude(coi => coi!.CafeteriaItem)
                    .Include(co => co.CreatedByUser)
                    .Where(co => co.BranchId == userBranchId)
                    .OrderByDescending(co => co.OrderDate)
                    .ToListAsync();

                // Filter by date if provided
                if (startDate.HasValue)
                    orders = orders.Where(o => o.OrderDate >= startDate.Value).ToList();
                if (endDate.HasValue)
                    orders = orders.Where(o => o.OrderDate <= endDate.Value).ToList();

                // Set EPPlus license context
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                using (var package = new ExcelPackage())
                {
                    // Summary Sheet
                    var summarySheet = package.Workbook.Worksheets.Add("ملخص عام");
                    
                    // Add title
                    summarySheet.Cells[1, 1].Value = "تقرير الكافيتيريا";
                    summarySheet.Cells[1, 1, 1, 8].Merge = true;
                    summarySheet.Cells[1, 1].Style.Font.Size = 16;
                    summarySheet.Cells[1, 1].Style.Font.Bold = true;
                    summarySheet.Cells[1, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                    // Summary statistics
                    summarySheet.Cells[3, 1].Value = "إجمالي الطلبات";
                    summarySheet.Cells[3, 2].Value = orders.Count;
                    
                    summarySheet.Cells[4, 1].Value = "إجمالي الإيرادات";
                    summarySheet.Cells[4, 2].Value = orders.Sum(o => o.TotalAmount);
                    
                    summarySheet.Cells[5, 1].Value = "إجمالي الأرباح";
                    summarySheet.Cells[5, 2].Value = orders.Sum(o => o.TotalAmount) - 
                        orders.SelectMany(o => o.CafeteriaOrderItems).Sum(oi => oi.Quantity * oi.CafeteriaItem.Cost);
                    
                    summarySheet.Cells[6, 1].Value = "متوسط قيمة الطلب";
                    summarySheet.Cells[6, 2].Value = orders.Any() ? orders.Average(o => o.TotalAmount) : 0;

                    // Orders Details Sheet
                    var ordersSheet = package.Workbook.Worksheets.Add("تفاصيل الطلبات");
                    
                    // Headers
                    ordersSheet.Cells[1, 1].Value = "رقم الطلب";
                    ordersSheet.Cells[1, 2].Value = "تاريخ الطلب";
                    ordersSheet.Cells[1, 3].Value = "وقت الطلب";
                    ordersSheet.Cells[1, 4].Value = "اسم العميل";
                    ordersSheet.Cells[1, 5].Value = "هاتف العميل";
                    ordersSheet.Cells[1, 6].Value = "حالة الطلب";
                    ordersSheet.Cells[1, 7].Value = "المجموع الفرعي";
                    ordersSheet.Cells[1, 8].Value = "الخصم";
                    ordersSheet.Cells[1, 9].Value = "الإجمالي";
                    ordersSheet.Cells[1, 10].Value = "المدفوع";
                    ordersSheet.Cells[1, 11].Value = "طريقة الدفع";
                    ordersSheet.Cells[1, 12].Value = "حالة الدفع";
                    ordersSheet.Cells[1, 13].Value = "أنشأ بواسطة";
                    ordersSheet.Cells[1, 14].Value = "الملاحظات";

                    // Style headers
                    using (var range = ordersSheet.Cells[1, 1, 1, 14])
                    {
                        range.Style.Font.Bold = true;
                        range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                    }

                    // Add data
                    int row = 2;
                    foreach (var order in orders)
                    {
                        ordersSheet.Cells[row, 1].Value = order.OrderNumber;
                        ordersSheet.Cells[row, 2].Value = order.OrderDate.ToString("yyyy-MM-dd");
                        ordersSheet.Cells[row, 3].Value = order.OrderDate.ToString("HH:mm:ss");
                        ordersSheet.Cells[row, 4].Value = order.Student?.FullName ?? order.Employee?.User.FullName ?? order.CustomerName;
                        ordersSheet.Cells[row, 5].Value = order.Student?.Phone ?? order.Employee?.User.Phone ?? order.CustomerPhone;
                        ordersSheet.Cells[row, 6].Value = GetStatusArabic(order.Status);
                        ordersSheet.Cells[row, 7].Value = order.SubTotal;
                        ordersSheet.Cells[row, 8].Value = order.DiscountAmount;
                        ordersSheet.Cells[row, 9].Value = order.TotalAmount;
                        ordersSheet.Cells[row, 10].Value = order.PaidAmount;
                        ordersSheet.Cells[row, 11].Value = GetPaymentMethodArabic(order.PaymentMethod);
                        ordersSheet.Cells[row, 12].Value = GetPaymentStatusArabic(order.PaymentStatus);
                        ordersSheet.Cells[row, 13].Value = order.CreatedByUser?.FullName;
                        ordersSheet.Cells[row, 14].Value = order.Notes;
                        row++;
                    }

                    // Auto-fit columns
                    ordersSheet.Cells.AutoFitColumns();

                    // Items Details Sheet
                    var itemsSheet = package.Workbook.Worksheets.Add("تفاصيل المنتجات");
                    
                    // Headers
                    itemsSheet.Cells[1, 1].Value = "رقم الطلب";
                    itemsSheet.Cells[1, 2].Value = "اسم المنتج";
                    itemsSheet.Cells[1, 3].Value = "الكمية";
                    itemsSheet.Cells[1, 4].Value = "سعر الوحدة";
                    itemsSheet.Cells[1, 5].Value = "الإجمالي";
                    itemsSheet.Cells[1, 6].Value = "التكلفة";
                    itemsSheet.Cells[1, 7].Value = "الربح";
                    itemsSheet.Cells[1, 8].Value = "الملاحظات";

                    // Style headers
                    using (var range = itemsSheet.Cells[1, 1, 1, 8])
                    {
                        range.Style.Font.Bold = true;
                        range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                    }

                    // Add items data
                    row = 2;
                    foreach (var order in orders)
                    {
                        foreach (var item in order.CafeteriaOrderItems)
                        {
                            itemsSheet.Cells[row, 1].Value = order.OrderNumber;
                            itemsSheet.Cells[row, 2].Value = item.CafeteriaItem.Name;
                            itemsSheet.Cells[row, 3].Value = item.Quantity;
                            itemsSheet.Cells[row, 4].Value = item.UnitPrice;
                            itemsSheet.Cells[row, 5].Value = item.TotalPrice;
                            itemsSheet.Cells[row, 6].Value = item.Quantity * item.CafeteriaItem.Cost;
                            itemsSheet.Cells[row, 7].Value = item.TotalPrice - (item.Quantity * item.CafeteriaItem.Cost);
                            itemsSheet.Cells[row, 8].Value = item.Notes;
                            row++;
                        }
                    }

                    // Auto-fit columns
                    itemsSheet.Cells.AutoFitColumns();

                    // Return the file
                    var content = package.GetAsByteArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
                        $"cafeteria-report-{DateTime.Now:yyyy-MM-dd}.xlsx");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "حدث خطأ في الخادم", error = ex.Message });
            }
        }

        [HttpGet("export/pdf")]
        
        public async Task<IActionResult> ExportToPDF([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var userBranchId = int.Parse(User.FindFirst("BranchId")?.Value ?? "0");
                
                // Get all orders with details
                var orders = await _context.CafeteriaOrders
                    .Include(co => co.Student)
                    .Include(co => co.Employee)
                        .ThenInclude(e => e!.User)
                    .Include(co => co.CafeteriaOrderItems)
                        .ThenInclude(coi => coi!.CafeteriaItem)
                    .Include(co => co.CreatedByUser)
                    .Where(co => co.BranchId == userBranchId)
                    .OrderByDescending(co => co.OrderDate)
                    .ToListAsync();

                // Filter by date if provided
                if (startDate.HasValue)
                    orders = orders.Where(o => o.OrderDate >= startDate.Value).ToList();
                if (endDate.HasValue)
                    orders = orders.Where(o => o.OrderDate <= endDate.Value).ToList();

                using (MemoryStream ms = new MemoryStream())
                {
                    Document document = new Document(PageSize.A4, 25, 25, 30, 30);
                    PdfWriter writer = PdfWriter.GetInstance(document, ms);

                    document.Open();

                    // Add title
                    Font titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
                    Paragraph title = new Paragraph("تقرير الكافيتيريا", titleFont);
                    title.Alignment = Element.ALIGN_CENTER;
                    document.Add(title);
                    document.Add(new Paragraph(" ")); // Spacing

                    // Add summary
                    Font headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
                    Font normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);

                    document.Add(new Paragraph("ملخص عام:", headerFont));
                    document.Add(new Paragraph($"إجمالي الطلبات: {orders.Count}", normalFont));
                    document.Add(new Paragraph($"إجمالي الإيرادات: {orders.Sum(o => o.TotalAmount):F2} جنيه", normalFont));
                    document.Add(new Paragraph($"إجمالي الأرباح: {orders.Sum(o => o.TotalAmount) - orders.SelectMany(o => o.CafeteriaOrderItems).Sum(oi => oi.Quantity * oi.CafeteriaItem.Cost):F2} جنيه", normalFont));
                    document.Add(new Paragraph($"متوسط قيمة الطلب: {(orders.Any() ? orders.Average(o => o.TotalAmount) : 0):F2} جنيه", normalFont));
                    document.Add(new Paragraph(" ")); // Spacing

                    // Add orders table
                    document.Add(new Paragraph("تفاصيل الطلبات:", headerFont));
                    PdfPTable table = new PdfPTable(8);
                    table.WidthPercentage = 100;

                    // Add headers
                    string[] headers = { "رقم الطلب", "التاريخ", "العميل", "الإجمالي", "المدفوع", "طريقة الدفع", "الحالة", "أنشأ بواسطة" };
                    foreach (string header in headers)
                    {
                        PdfPCell cell = new PdfPCell(new Phrase(header, headerFont));
                        cell.BackgroundColor = BaseColor.LIGHT_GRAY;
                        table.AddCell(cell);
                    }

                    // Add data
                    foreach (var order in orders.Take(50)) // Limit to first 50 orders for PDF
                    {
                        table.AddCell(new PdfPCell(new Phrase(order.OrderNumber ?? "", normalFont)));
                        table.AddCell(new PdfPCell(new Phrase(order.OrderDate.ToString("yyyy-MM-dd HH:mm"), normalFont)));
                        table.AddCell(new PdfPCell(new Phrase(order.Student?.FullName ?? order.Employee?.User.FullName ?? order.CustomerName ?? "", normalFont)));
                        table.AddCell(new PdfPCell(new Phrase(order.TotalAmount.ToString("F2"), normalFont)));
                        table.AddCell(new PdfPCell(new Phrase(order.PaidAmount.ToString("F2"), normalFont)));
                        table.AddCell(new PdfPCell(new Phrase(GetPaymentMethodArabic(order.PaymentMethod), normalFont)));
                        table.AddCell(new PdfPCell(new Phrase(GetStatusArabic(order.Status), normalFont)));
                        table.AddCell(new PdfPCell(new Phrase(order.CreatedByUser?.FullName ?? "", normalFont)));
                    }

                    document.Add(table);

                    document.Close();
                    writer.Close();

                    var content = ms.ToArray();
                    return File(content, "application/pdf", $"cafeteria-report-{DateTime.Now:yyyy-MM-dd}.pdf");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "حدث خطأ في الخادم", error = ex.Message });
            }
        }
    }

    public class CreateCafeteriaItemRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public CafeteriaItemCategory Category { get; set; }
        public decimal Price { get; set; }
        public decimal Cost { get; set; }
        public int StockQuantity { get; set; }
        public int MinimumStock { get; set; }
        public bool IsAvailable { get; set; } = true;
    }

    public class UpdateCafeteriaItemRequest : CreateCafeteriaItemRequest
    {
        public bool IsActive { get; set; } = true;
    }

    public class CreateCafeteriaOrderRequest
    {
        public int? StudentId { get; set; }
        public int? EmployeeId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public decimal TaxAmount { get; set; } = 0;
        public decimal DiscountAmount { get; set; } = 0;
        public decimal PaidAmount { get; set; } = 0;
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;
        public string Notes { get; set; } = string.Empty;
        public List<CreateOrderItemRequest> Items { get; set; } = new();
    }

    public class CreateOrderItemRequest
    {
        public int CafeteriaItemId { get; set; }
        public int Quantity { get; set; }
        public string Notes { get; set; } = string.Empty;
        public string Customization { get; set; } = string.Empty;
    }

    public class UpdateOrderStatusRequest
    {
        public CafeteriaOrderStatus Status { get; set; }
        public string Notes { get; set; } = string.Empty;
    }

    public class UpdateCafeteriaOrderRequest
    {
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public decimal DiscountAmount { get; set; } = 0;
        public decimal PaidAmount { get; set; } = 0;
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;
        public string Notes { get; set; } = string.Empty;
    }
}
