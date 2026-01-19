using LVTN_BE_COFFE.Domain.IServices;
using LVTN_BE_COFFE.Domain.Model;
using LVTN_BE_COFFE.Domain.VModel;
using LVTN_BE_COFFE.Infrastructures.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LVTN_BE_COFFE.Services.Services
{
    public class StatisticsService : IStatisticsService
    {
        private readonly AppDbContext _context;

        public StatisticsService(AppDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // 1. THỐNG KÊ DASHBOARD
        // ==========================================
        public async Task<ActionResult<ResponseResult>> GetDashboardStatistics(StatisticsFilterVModel? filter = null)
        {
            try
            {
                var now = DateTime.Now;
                var today = now.Date;
                var firstDayOfMonth = new DateTime(now.Year, now.Month, 1);
                var firstDayOfYear = new DateTime(now.Year, 1, 1);

                // Lấy tất cả đơn hàng (không bị hủy) để tính doanh thu
                var allOrders = await _context.Orders
                    .Include(o => o.OrderItems)
                    .Where(o => o.Status != "cancelled")
                    .ToListAsync();

                var todayOrders = allOrders.Where(o => o.CreatedAt.Date == today).ToList();
                var monthOrders = allOrders.Where(o => o.CreatedAt >= firstDayOfMonth).ToList();
                var yearOrders = allOrders.Where(o => o.CreatedAt >= firstDayOfYear).ToList();

                // Doanh thu
                var totalRevenue = allOrders.Sum(o => o.FinalAmount);
                var todayRevenue = todayOrders.Sum(o => o.FinalAmount);
                var monthRevenue = monthOrders.Sum(o => o.FinalAmount);
                var yearRevenue = yearOrders.Sum(o => o.FinalAmount);

                // Đơn hàng (bao gồm cả cancelled để đếm tổng số)
                var allOrdersWithCancelled = await _context.Orders.ToListAsync();
                var ordersByStatus = allOrdersWithCancelled.GroupBy(o => o.Status).ToDictionary(g => g.Key, g => g.Count());

                var totalOrders = allOrdersWithCancelled.Count;
                var completedOrders = ordersByStatus.GetValueOrDefault("delivered", 0);

                // Khách hàng
                var totalCustomers = await _context.AspNetUsers.CountAsync();
                var newCustomersThisMonth = await _context.AspNetUsers
                    .Where(u => u.CreatedDate.HasValue && u.CreatedDate >= firstDayOfMonth)
                    .CountAsync();

                var activeCustomers = await _context.Orders
                    .Where(o => !string.IsNullOrEmpty(o.UserId) && o.CreatedAt >= firstDayOfMonth)
                    .Select(o => o.UserId)
                    .Distinct()
                    .CountAsync();

                // Tỷ lệ
                var completionRate = totalOrders > 0 ? (double)completedOrders / totalOrders * 100 : 0;
                var cancellationRate = totalOrders > 0
                    ? (double)ordersByStatus.GetValueOrDefault("cancelled", 0) / totalOrders * 100 : 0;

                // Top 5 sản phẩm bán chạy
                var topProducts = await GetTopProductsInternal(5);

                // Top 5 khách hàng
                var topCustomers = await GetTopCustomersInternal(5);

                // Biểu đồ doanh thu 7 ngày gần nhất
                var last7Days = Enumerable.Range(0, 7).Select(i => today.AddDays(-i)).Reverse().ToList();
                var revenueChart = last7Days.Select(date =>
                {
                    var dayOrders = allOrders.Where(o => o.CreatedAt.Date == date).ToList();
                    return new RevenueByDateResponse
                    {
                        Date = date,
                        Revenue = dayOrders.Sum(o => o.TotalAmount),
                        ShippingFee = dayOrders.Sum(o => o.ShippingFee),
                        Discount = dayOrders.Sum(o => o.DiscountAmount),
                        NetRevenue = dayOrders.Sum(o => o.FinalAmount),
                        OrderCount = dayOrders.Count,
                        AverageOrderValue = dayOrders.Any() ? dayOrders.Average(o => o.FinalAmount) : 0
                    };
                }).ToList();

                var response = new DashboardStatisticsResponse
                {
                    TotalRevenue = totalRevenue,
                    TodayRevenue = todayRevenue,
                    MonthRevenue = monthRevenue,
                    YearRevenue = yearRevenue,

                    TotalOrders = totalOrders,
                    TodayOrders = todayOrders.Count,
                    MonthOrders = monthOrders.Count,
                    YearOrders = yearOrders.Count,

                    PendingOrders = ordersByStatus.GetValueOrDefault("pending", 0),
                    ProcessingOrders = ordersByStatus.GetValueOrDefault("processing", 0),
                    DeliveredOrders = ordersByStatus.GetValueOrDefault("delivered", 0),
                    CancelledOrders = ordersByStatus.GetValueOrDefault("cancelled", 0),
                    ReturnRequestedOrders = ordersByStatus.GetValueOrDefault("return_requested", 0),

                    TotalCustomers = totalCustomers,
                    NewCustomersThisMonth = newCustomersThisMonth,
                    ActiveCustomers = activeCustomers,

                    AverageOrderValue = allOrders.Any() ? allOrders.Average(o => o.FinalAmount) : 0,
                    CompletionRate = completionRate,
                    CancellationRate = cancellationRate,

                    TopSellingProducts = topProducts,
                    TopCustomers = topCustomers,
                    RevenueChart = revenueChart
                };

                return new OkObjectResult(new ResponseResult { IsSuccess = true, Data = response });
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(new ResponseResult
                {
                    IsSuccess = false,
                    Message = $"Lỗi khi lấy thống kê dashboard: {ex.Message}"
                });
            }
        }

        // ==========================================
        // 2. THỐNG KÊ DOANH THU
        // ==========================================
        public async Task<ActionResult<ResponseResult>> GetRevenueStatistics(StatisticsFilterVModel filter)
        {
            try
            {
                var query = _context.Orders.AsQueryable();

                // Lọc theo ngày
                if (filter.FromDate.HasValue)
                    query = query.Where(o => o.CreatedAt >= filter.FromDate.Value);

                if (filter.ToDate.HasValue)
                    query = query.Where(o => o.CreatedAt <= filter.ToDate.Value.AddDays(1).AddSeconds(-1));

                // Lọc theo trạng thái
                if (!string.IsNullOrEmpty(filter.Status))
                    query = query.Where(o => o.Status == filter.Status);
                else
                    query = query.Where(o => o.Status != "cancelled");

                var orders = await query.Include(o => o.OrderItems).ToListAsync();

                // Tính toán
                var totalRevenue = orders.Sum(o => o.TotalAmount);
                var totalShippingFee = orders.Sum(o => o.ShippingFee);
                var totalDiscount = orders.Sum(o => o.DiscountAmount);
                var netRevenue = orders.Sum(o => o.FinalAmount);

                // Nhóm theo ngày/tuần/tháng
                var revenueByDate = GroupOrdersByDate(orders, filter.GroupBy ?? "day")
                    .Select(g => new RevenueByDateResponse
                    {
                        Date = g.Key,
                        Revenue = g.Sum(o => o.TotalAmount),
                        ShippingFee = g.Sum(o => o.ShippingFee),
                        Discount = g.Sum(o => o.DiscountAmount),
                        NetRevenue = g.Sum(o => o.FinalAmount),
                        OrderCount = g.Count(),
                        AverageOrderValue = g.Average(o => o.FinalAmount)
                    })
                    .OrderBy(r => r.Date)
                    .ToList();

                // Thống kê theo trạng thái
                var revenueByStatus = orders
                    .GroupBy(o => o.Status)
                    .Select(g => new RevenueByStatusResponse
                    {
                        Status = g.Key,
                        Revenue = g.Sum(o => o.FinalAmount),
                        OrderCount = g.Count(),
                        Percentage = netRevenue > 0 ? (double)(g.Sum(o => o.FinalAmount) / netRevenue * 100) : 0
                    })
                    .ToList();

                // Thống kê theo phương thức vận chuyển
                var revenueByMethod = orders
                    .Where(o => !string.IsNullOrEmpty(o.ShippingMethod))
                    .GroupBy(o => o.ShippingMethod!)
                    .Select(g => new RevenueByMethodResponse
                    {
                        ShippingMethod = g.Key,
                        Revenue = g.Sum(o => o.FinalAmount),
                        OrderCount = g.Count(),
                        Percentage = netRevenue > 0 ? (double)(g.Sum(o => o.FinalAmount) / netRevenue * 100) : 0
                    })
                    .ToList();

                var response = new RevenueStatisticsResponse
                {
                    TotalRevenue = totalRevenue,
                    TotalShippingFee = totalShippingFee,
                    TotalDiscount = totalDiscount,
                    NetRevenue = netRevenue,
                    TotalOrders = orders.Count,
                    AverageOrderValue = orders.Any() ? orders.Average(o => o.FinalAmount) : 0,
                    HighestOrderValue = orders.Any() ? orders.Max(o => o.FinalAmount) : 0,
                    LowestOrderValue = orders.Any() ? orders.Min(o => o.FinalAmount) : 0,
                    RevenueByDate = revenueByDate,
                    RevenueByStatus = revenueByStatus,
                    RevenueByShippingMethod = revenueByMethod
                };

                return new OkObjectResult(new ResponseResult { IsSuccess = true, Data = response });
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(new ResponseResult
                {
                    IsSuccess = false,
                    Message = $"Lỗi khi lấy thống kê doanh thu: {ex.Message}"
                });
            }
        }

        // ==========================================
        // 3. THỐNG KÊ ĐƠN HÀNG
        // ==========================================
        public async Task<ActionResult<ResponseResult>> GetOrderStatistics(StatisticsFilterVModel filter)
        {
            try
            {
                var query = _context.Orders.AsQueryable();

                if (filter.FromDate.HasValue)
                    query = query.Where(o => o.CreatedAt >= filter.FromDate.Value);

                if (filter.ToDate.HasValue)
                    query = query.Where(o => o.CreatedAt <= filter.ToDate.Value.AddDays(1).AddSeconds(-1));

                var orders = await query.ToListAsync();
                var totalOrders = orders.Count;

                // Thống kê theo trạng thái
                var ordersByStatus = orders.GroupBy(o => o.Status).ToDictionary(g => g.Key, g => g.Count());

                var completedOrders = ordersByStatus.GetValueOrDefault("delivered", 0);
                var cancelledOrders = ordersByStatus.GetValueOrDefault("cancelled", 0);
                var returnedOrders = ordersByStatus.GetValueOrDefault("returned", 0);

                // Tỷ lệ
                var completionRate = totalOrders > 0 ? (double)completedOrders / totalOrders * 100 : 0;
                var cancellationRate = totalOrders > 0 ? (double)cancelledOrders / totalOrders * 100 : 0;
                var returnRate = totalOrders > 0 ? (double)returnedOrders / totalOrders * 100 : 0;

                // Nhóm theo ngày
                var ordersByDate = GroupOrdersByDate(orders, filter.GroupBy ?? "day")
                    .Select(g => new OrderByDateResponse
                    {
                        Date = g.Key,
                        OrderCount = g.Count(),
                        CompletedCount = g.Count(o => o.Status == "delivered"),
                        CancelledCount = g.Count(o => o.Status == "cancelled"),
                        PendingCount = g.Count(o => o.Status == "pending")
                    })
                    .OrderBy(o => o.Date)
                    .ToList();

                // Phân bố theo trạng thái
                var ordersByStatusList = ordersByStatus
                    .Select(kvp => new OrderByStatusResponse
                    {
                        Status = kvp.Key,
                        Count = kvp.Value,
                        Percentage = totalOrders > 0 ? (double)kvp.Value / totalOrders * 100 : 0
                    })
                    .ToList();

                // Thống kê theo giờ trong ngày
                var ordersByHour = orders
                    .GroupBy(o => o.CreatedAt.Hour)
                    .Select(g => new OrderByHourResponse
                    {
                        Hour = g.Key,
                        OrderCount = g.Count(),
                        Revenue = g.Where(o => o.Status != "cancelled").Sum(o => o.FinalAmount)
                    })
                    .OrderBy(o => o.Hour)
                    .ToList();

                var response = new OrderStatisticsResponse
                {
                    TotalOrders = totalOrders,
                    CompletedOrders = completedOrders,
                    CancelledOrders = cancelledOrders,
                    PendingOrders = ordersByStatus.GetValueOrDefault("pending", 0),
                    ProcessingOrders = ordersByStatus.GetValueOrDefault("processing", 0),
                    DeliveredOrders = completedOrders,
                    ReturnedOrders = returnedOrders,
                    CompletionRate = completionRate,
                    CancellationRate = cancellationRate,
                    ReturnRate = returnRate,
                    OrdersByDate = ordersByDate,
                    OrdersByStatus = ordersByStatusList,
                    OrdersByHour = ordersByHour
                };

                return new OkObjectResult(new ResponseResult { IsSuccess = true, Data = response });
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(new ResponseResult
                {
                    IsSuccess = false,
                    Message = $"Lỗi khi lấy thống kê đơn hàng: {ex.Message}"
                });
            }
        }

        // ==========================================
        // 4. THỐNG KÊ SẢN PHẨM
        // ==========================================
        // ==========================================
        // 4. THỐNG KÊ SẢN PHẨM
        // ==========================================
        // ==========================================
        // 4. THỐNG KÊ SẢN PHẨM
        // ==========================================
        public async Task<ActionResult<ResponseResult>> GetProductStatistics(StatisticsFilterVModel filter)
        {
            try
            {
                var query = _context.OrderItems
                    .Include(oi => oi.Order)
                    .Include(oi => oi.ProductVariant)
                        .ThenInclude(pv => pv.Product)
                            .ThenInclude(p => p.Categories) // ✅ Include Categories collection
                    .Include(oi => oi.ProductVariant)
                        .ThenInclude(pv => pv.Images)
                    .AsQueryable();

                // Lọc theo ngày
                if (filter.FromDate.HasValue)
                    query = query.Where(oi => oi.Order.CreatedAt >= filter.FromDate.Value);

                if (filter.ToDate.HasValue)
                    query = query.Where(oi => oi.Order.CreatedAt <= filter.ToDate.Value.AddDays(1).AddSeconds(-1));

                // Chỉ tính đơn hàng thành công
                query = query.Where(oi => oi.Order.Status != "cancelled");

                var orderItems = await query.ToListAsync();

                // Top 10 sản phẩm bán chạy
                var topProducts = orderItems
                    .GroupBy(oi => new { oi.ProductVariantId, oi.ProductVariant })
                    .Select(g => new TopProductResponse
                    {
                        ProductVariantId = g.Key.ProductVariantId,
                        ProductId = g.Key.ProductVariant.ProductId,
                        ProductName = g.Key.ProductVariant.Product.Name,
                        VariantName = g.Key.ProductVariant.Sku,
                        QuantitySold = g.Sum(oi => oi.Quantity),
                        Revenue = g.Sum(oi => oi.PriceAtPurchase * oi.Quantity),
                        OrderCount = g.Select(oi => oi.OrderId).Distinct().Count(),
                        ImageUrl = g.Key.ProductVariant.Images.FirstOrDefault()?.ImageUrl,
                        AveragePrice = g.Average(oi => oi.PriceAtPurchase)
                    })
                    .OrderByDescending(p => p.QuantitySold)
                    .Take(10)
                    .ToList();

                // Top 5 sản phẩm bán ít nhất
                var leastProducts = orderItems
                    .GroupBy(oi => new { oi.ProductVariantId, oi.ProductVariant })
                    .Select(g => new TopProductResponse
                    {
                        ProductVariantId = g.Key.ProductVariantId,
                        ProductId = g.Key.ProductVariant.ProductId,
                        ProductName = g.Key.ProductVariant.Product.Name,
                        VariantName = g.Key.ProductVariant.Sku,
                        QuantitySold = g.Sum(oi => oi.Quantity),
                        Revenue = g.Sum(oi => oi.PriceAtPurchase * oi.Quantity),
                        OrderCount = g.Select(oi => oi.OrderId).Distinct().Count(),
                        ImageUrl = g.Key.ProductVariant.Images.FirstOrDefault()?.ImageUrl,
                        AveragePrice = g.Average(oi => oi.PriceAtPurchase)
                    })
                    .OrderBy(p => p.QuantitySold)
                    .Take(5)
                    .ToList();

                // Doanh thu theo sản phẩm
                var totalRevenue = orderItems.Sum(oi => oi.PriceAtPurchase * oi.Quantity);
                var productRevenue = orderItems
                    .GroupBy(oi => new { oi.ProductVariant.ProductId, oi.ProductVariant.Product.Name })
                    .Select(g => new ProductRevenueResponse
                    {
                        ProductId = g.Key.ProductId,
                        ProductName = g.Key.Name,
                        QuantitySold = g.Sum(oi => oi.Quantity),
                        Revenue = g.Sum(oi => oi.PriceAtPurchase * oi.Quantity),
                        Percentage = totalRevenue > 0 ? (double)(g.Sum(oi => oi.PriceAtPurchase * oi.Quantity) / totalRevenue * 100) : 0
                    })
                    .OrderByDescending(p => p.Revenue)
                    .ToList();

                // ✅ THỐNG KÊ THEO DANH MỤC - XỬ LÝ MANY-TO-MANY
                // Vì Product có nhiều Categories, cần xử lý flatten ra
                var categoryPerformance = orderItems
                    .SelectMany(oi => oi.ProductVariant.Product.Categories.Select(c => new
                    {
                        Category = c,
                        OrderItem = oi
                    }))
                    .GroupBy(x => x.Category)
                    .Select(g => new ProductCategoryResponse
                    {
                        CategoryId = g.Key.Id,
                        CategoryName = g.Key.Name,
                        ProductsSold = g.Sum(x => x.OrderItem.Quantity),
                        Revenue = g.Sum(x => x.OrderItem.PriceAtPurchase * x.OrderItem.Quantity),
                        Percentage = totalRevenue > 0 ? (double)(g.Sum(x => x.OrderItem.PriceAtPurchase * x.OrderItem.Quantity) / totalRevenue * 100) : 0
                    })
                    .OrderByDescending(c => c.Revenue)
                    .ToList();

                var response = new ProductStatisticsResponse
                {
                    TotalProductsSold = orderItems.Sum(oi => oi.Quantity),
                    TotalVariantsSold = orderItems.Select(oi => oi.ProductVariantId).Distinct().Count(),
                    UniqueProductsSold = orderItems.Select(oi => oi.ProductVariant.ProductId).Distinct().Count(),
                    TopSellingProducts = topProducts,
                    LeastSellingProducts = leastProducts,
                    ProductRevenue = productRevenue,
                    CategoryPerformance = categoryPerformance
                };

                return new OkObjectResult(new ResponseResult { IsSuccess = true, Data = response });
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(new ResponseResult
                {
                    IsSuccess = false,
                    Message = $"Lỗi khi lấy thống kê sản phẩm: {ex.Message}"
                });
            }
        }

        // 5. THỐNG KÊ KHÁCH HÀNG
        public async Task<ActionResult<ResponseResult>> GetCustomerStatistics(StatisticsFilterVModel filter)
        {
            try
            {
                var allUsers = await _context.AspNetUsers.ToListAsync();

                var query = _context.Orders.AsQueryable();

                if (filter.FromDate.HasValue)
                    query = query.Where(o => o.CreatedAt >= filter.FromDate.Value);

                if (filter.ToDate.HasValue)
                    query = query.Where(o => o.CreatedAt <= filter.ToDate.Value.AddDays(1).AddSeconds(-1));

                var orders = await query.Where(o => o.Status != "cancelled").ToListAsync();

                // Khách hàng đã đăng ký
                var registeredUsers = orders.Where(o => !string.IsNullOrEmpty(o.UserId))
                    .Select(o => o.UserId)
                    .Distinct()
                    .Count();

                // Đơn hàng của khách
                var guestOrders = orders.Count(o => string.IsNullOrEmpty(o.UserId));

                // Khách hàng mới trong tháng
                var firstDayOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                var newCustomersThisMonth = allUsers.Count(u => u.CreatedDate.HasValue && u.CreatedDate >= firstDayOfMonth);

                // Khách hàng quay lại (đặt >= 2 đơn)
                var returningCustomers = orders
                    .Where(o => !string.IsNullOrEmpty(o.UserId))
                    .GroupBy(o => o.UserId)
                    .Count(g => g.Count() >= 2);

                var returnCustomerRate = registeredUsers > 0 ? (double)returningCustomers / registeredUsers * 100 : 0;

                // Top 10 khách hàng
                var topCustomers = await GetTopCustomersInternal(10, filter);

                // Tăng trưởng khách hàng theo thời gian
                var customerGrowth = allUsers
                    .Where(u => u.CreatedDate.HasValue)
                    .GroupBy(u => u.CreatedDate!.Value.Date)
                    .OrderBy(g => g.Key)
                    .Select(g => new CustomerGrowthResponse
                    {
                        Date = g.Key,
                        NewCustomers = g.Count(),
                        TotalCustomers = allUsers.Count(u => u.CreatedDate.HasValue && u.CreatedDate.Value.Date <= g.Key)
                    })
                    .ToList();

                var response = new CustomerStatisticsResponse
                {
                    TotalCustomers = allUsers.Count,
                    RegisteredUsers = registeredUsers,
                    GuestOrders = guestOrders,
                    NewCustomersThisMonth = newCustomersThisMonth,
                    ReturningCustomers = returningCustomers,
                    ReturnCustomerRate = returnCustomerRate,
                    TopCustomers = topCustomers,
                    CustomerGrowth = customerGrowth
                };

                return new OkObjectResult(new ResponseResult { IsSuccess = true, Data = response });
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(new ResponseResult
                {
                    IsSuccess = false,
                    Message = $"Lỗi khi lấy thống kê khách hàng: {ex.Message}"
                });
            }
        }

        // ==========================================
        // 6. SO SÁNH DOANH THU
        // ==========================================
        public async Task<ActionResult<ResponseResult>> CompareRevenue(DateTime fromDate1, DateTime toDate1, DateTime fromDate2, DateTime toDate2)
        {
            try
            {
                var period1Orders = await _context.Orders
                    .Where(o => o.CreatedAt >= fromDate1 && o.CreatedAt <= toDate1.AddDays(1).AddSeconds(-1) && o.Status != "cancelled")
                    .ToListAsync();

                var period2Orders = await _context.Orders
                    .Where(o => o.CreatedAt >= fromDate2 && o.CreatedAt <= toDate2.AddDays(1).AddSeconds(-1) && o.Status != "cancelled")
                    .ToListAsync();

                var result = new
                {
                    Period1 = new
                    {
                        FromDate = fromDate1,
                        ToDate = toDate1,
                        TotalRevenue = period1Orders.Sum(o => o.FinalAmount),
                        TotalOrders = period1Orders.Count,
                        AverageOrderValue = period1Orders.Any() ? period1Orders.Average(o => o.FinalAmount) : 0
                    },
                    Period2 = new
                    {
                        FromDate = fromDate2,
                        ToDate = toDate2,
                        TotalRevenue = period2Orders.Sum(o => o.FinalAmount),
                        TotalOrders = period2Orders.Count,
                        AverageOrderValue = period2Orders.Any() ? period2Orders.Average(o => o.FinalAmount) : 0
                    },
                    Comparison = new
                    {
                        RevenueGrowth = period1Orders.Sum(o => o.FinalAmount) - period2Orders.Sum(o => o.FinalAmount),
                        OrderGrowth = period1Orders.Count - period2Orders.Count,
                        RevenueGrowthPercentage = period2Orders.Sum(o => o.FinalAmount) > 0
                            ? (double)((period1Orders.Sum(o => o.FinalAmount) - period2Orders.Sum(o => o.FinalAmount)) / period2Orders.Sum(o => o.FinalAmount) * 100)
                            : 0
                    }
                };

                return new OkObjectResult(new ResponseResult { IsSuccess = true, Data = result });
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(new ResponseResult
                {
                    IsSuccess = false,
                    Message = $"Lỗi khi so sánh doanh thu: {ex.Message}"
                });
            }
        }

        // ==========================================
        // 7. XUẤT BÁO CÁO EXCEL (Placeholder)
        // ==========================================
        public async Task<byte[]> ExportRevenueReport(StatisticsFilterVModel filter)
        {
            // TODO: Implement Excel export using EPPlus or ClosedXML
            await Task.CompletedTask;
            return Array.Empty<byte>();
        }

        // ==========================================
        // HELPER METHODS
        // ==========================================

        private IEnumerable<IGrouping<DateTime, Order>> GroupOrdersByDate(List<Order> orders, string groupBy)
        {
            return groupBy.ToLower() switch
            {
                "day" => orders.GroupBy(o => o.CreatedAt.Date),
                "week" => orders.GroupBy(o => GetWeekStart(o.CreatedAt)),
                "month" => orders.GroupBy(o => new DateTime(o.CreatedAt.Year, o.CreatedAt.Month, 1)),
                "year" => orders.GroupBy(o => new DateTime(o.CreatedAt.Year, 1, 1)),
                _ => orders.GroupBy(o => o.CreatedAt.Date)
            };
        }

        private DateTime GetWeekStart(DateTime date)
        {
            var diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
            return date.AddDays(-1 * diff).Date;
        }

        private async Task<List<TopProductResponse>> GetTopProductsInternal(int count)
        {
            var orderItems = await _context.OrderItems
                .Include(oi => oi.ProductVariant)
                    .ThenInclude(pv => pv.Product)
                .Include(oi => oi.ProductVariant)
                    .ThenInclude(pv => pv.Images)
                .Where(oi => oi.Order.Status != "cancelled")
                .ToListAsync();

            return orderItems
                .GroupBy(oi => oi.ProductVariantId)
                .Select(g => new TopProductResponse
                {
                    ProductVariantId = g.Key,
                    ProductId = g.First().ProductVariant.ProductId,
                    ProductName = g.First().ProductVariant.Product.Name,
                    VariantName = g.First().ProductVariant.Sku,
                    QuantitySold = g.Sum(oi => oi.Quantity),
                    Revenue = g.Sum(oi => oi.PriceAtPurchase * oi.Quantity),
                    OrderCount = g.Count(),
                    ImageUrl = g.First().ProductVariant.Images.FirstOrDefault()?.ImageUrl,
                    AveragePrice = g.Average(oi => oi.PriceAtPurchase)
                })
                .OrderByDescending(p => p.QuantitySold)
                .Take(count)
                .ToList();
        }

        private async Task<List<TopCustomerResponse>> GetTopCustomersInternal(int count, StatisticsFilterVModel? filter = null)
        {
            var query = _context.Orders.Where(o => !string.IsNullOrEmpty(o.UserId) && o.Status != "cancelled");

            if (filter != null)
            {
                if (filter.FromDate.HasValue)
                    query = query.Where(o => o.CreatedAt >= filter.FromDate.Value);

                if (filter.ToDate.HasValue)
                    query = query.Where(o => o.CreatedAt <= filter.ToDate.Value.AddDays(1).AddSeconds(-1));
            }

            // Load data into memory first, then perform grouping and calculations
            var orders = await query.ToListAsync();

            return orders
                .GroupBy(o => new { o.UserId, o.ReceiverName, o.ReceiverEmail, o.ReceiverPhone })
                .Select(g => new TopCustomerResponse
                {
                    UserId = g.Key.UserId,
                    CustomerName = g.Key.ReceiverName,
                    Email = g.Key.ReceiverEmail,
                    Phone = g.Key.ReceiverPhone,
                    TotalOrders = g.Count(),
                    TotalSpent = g.Sum(o => o.FinalAmount),
                    AverageOrderValue = g.Average(o => o.FinalAmount),
                    LastOrderDate = g.Max(o => o.CreatedAt),
                    FirstOrderDate = g.Min(o => o.CreatedAt)
                })
                .OrderByDescending(c => c.TotalSpent)
                .Take(count)
                .ToList();
        }
        
    }
}