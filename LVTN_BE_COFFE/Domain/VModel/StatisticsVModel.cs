using System;
using System.Collections.Generic;

namespace LVTN_BE_COFFE.Domain.VModel
{
    // ==========================================
    // REQUEST MODELS
    // ==========================================

    public class StatisticsFilterVModel
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? GroupBy { get; set; } = "day"; // day, week, month, year
        public string? Status { get; set; } // Lọc theo trạng thái đơn hàng
    }

    // ==========================================
    // RESPONSE MODELS
    // ==========================================

    /// <summary>
    /// Thống kê tổng quan Dashboard
    /// </summary>
    public class DashboardStatisticsResponse
    {
        // Doanh thu
        public decimal TotalRevenue { get; set; }
        public decimal TodayRevenue { get; set; }
        public decimal MonthRevenue { get; set; }
        public decimal YearRevenue { get; set; }

        // Đơn hàng
        public int TotalOrders { get; set; }
        public int TodayOrders { get; set; }
        public int MonthOrders { get; set; }
        public int YearOrders { get; set; }

        // Trạng thái đơn hàng
        public int PendingOrders { get; set; }
        public int ProcessingOrders { get; set; }
        public int DeliveredOrders { get; set; }
        public int CancelledOrders { get; set; }
        public int ReturnRequestedOrders { get; set; }

        // Khách hàng
        public int TotalCustomers { get; set; }
        public int NewCustomersThisMonth { get; set; }
        public int ActiveCustomers { get; set; }

        // Chỉ số trung bình
        public decimal AverageOrderValue { get; set; }
        public double CompletionRate { get; set; }
        public double CancellationRate { get; set; }

        // Top data
        public List<TopProductResponse> TopSellingProducts { get; set; } = new();
        public List<TopCustomerResponse> TopCustomers { get; set; } = new();
        public List<RevenueByDateResponse> RevenueChart { get; set; } = new();
    }

    /// <summary>
    /// Thống kê doanh thu chi tiết
    /// </summary>
    public class RevenueStatisticsResponse
    {
        public decimal TotalRevenue { get; set; }
        public decimal TotalShippingFee { get; set; }
        public decimal TotalDiscount { get; set; }
        public decimal NetRevenue { get; set; }
        public int TotalOrders { get; set; }
        public decimal AverageOrderValue { get; set; }
        public decimal HighestOrderValue { get; set; }
        public decimal LowestOrderValue { get; set; }

        public List<RevenueByDateResponse> RevenueByDate { get; set; } = new();
        public List<RevenueByStatusResponse> RevenueByStatus { get; set; } = new();
        public List<RevenueByMethodResponse> RevenueByShippingMethod { get; set; } = new();
    }

    /// <summary>
    /// Thống kê đơn hàng chi tiết
    /// </summary>
    public class OrderStatisticsResponse
    {
        public int TotalOrders { get; set; }
        public int CompletedOrders { get; set; }
        public int CancelledOrders { get; set; }
        public int PendingOrders { get; set; }
        public int ProcessingOrders { get; set; }
        public int DeliveredOrders { get; set; }
        public int ReturnedOrders { get; set; }

        public double CompletionRate { get; set; }
        public double CancellationRate { get; set; }
        public double ReturnRate { get; set; }

        public List<OrderByDateResponse> OrdersByDate { get; set; } = new();
        public List<OrderByStatusResponse> OrdersByStatus { get; set; } = new();
        public List<OrderByHourResponse> OrdersByHour { get; set; } = new();
    }

    /// <summary>
    /// Thống kê sản phẩm
    /// </summary>
    public class ProductStatisticsResponse
    {
        public int TotalProductsSold { get; set; }
        public int TotalVariantsSold { get; set; }
        public int UniqueProductsSold { get; set; }

        public List<TopProductResponse> TopSellingProducts { get; set; } = new();
        public List<TopProductResponse> LeastSellingProducts { get; set; } = new();
        public List<ProductRevenueResponse> ProductRevenue { get; set; } = new();
        public List<ProductCategoryResponse> CategoryPerformance { get; set; } = new();
    }

    /// <summary>
    /// Thống kê khách hàng
    /// </summary>
    public class CustomerStatisticsResponse
    {
        public int TotalCustomers { get; set; }
        public int RegisteredUsers { get; set; }
        public int GuestOrders { get; set; }
        public int NewCustomersThisMonth { get; set; }
        public int ReturningCustomers { get; set; }
        public double ReturnCustomerRate { get; set; }

        public List<TopCustomerResponse> TopCustomers { get; set; } = new();
        public List<CustomerGrowthResponse> CustomerGrowth { get; set; } = new();
    }

    // ==========================================
    // SUB RESPONSE MODELS
    // ==========================================

    public class RevenueByDateResponse
    {
        public DateTime Date { get; set; }
        public decimal Revenue { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal Discount { get; set; }
        public decimal NetRevenue { get; set; }
        public int OrderCount { get; set; }
        public decimal AverageOrderValue { get; set; }
    }

    public class RevenueByStatusResponse
    {
        public string Status { get; set; } = null!;
        public decimal Revenue { get; set; }
        public int OrderCount { get; set; }
        public double Percentage { get; set; }
    }

    public class RevenueByMethodResponse
    {
        public string ShippingMethod { get; set; } = null!;
        public decimal Revenue { get; set; }
        public int OrderCount { get; set; }
        public double Percentage { get; set; }
    }

    public class OrderByDateResponse
    {
        public DateTime Date { get; set; }
        public int OrderCount { get; set; }
        public int CompletedCount { get; set; }
        public int CancelledCount { get; set; }
        public int PendingCount { get; set; }
    }

    public class OrderByStatusResponse
    {
        public string Status { get; set; } = null!;
        public int Count { get; set; }
        public double Percentage { get; set; }
    }

    public class OrderByHourResponse
    {
        public int Hour { get; set; }
        public int OrderCount { get; set; }
        public decimal Revenue { get; set; }
    }

    public class TopProductResponse
    {
        public int ProductId { get; set; }
        public int ProductVariantId { get; set; }
        public string ProductName { get; set; } = null!;
        public string? VariantName { get; set; }
        public int QuantitySold { get; set; }
        public decimal Revenue { get; set; }
        public int OrderCount { get; set; }
        public string? ImageUrl { get; set; }
        public decimal AveragePrice { get; set; }
    }

    public class ProductRevenueResponse
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public int QuantitySold { get; set; }
        public decimal Revenue { get; set; }
        public double Percentage { get; set; }
    }

    public class ProductCategoryResponse
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = null!;
        public int ProductsSold { get; set; }
        public decimal Revenue { get; set; }
        public double Percentage { get; set; }
    }

    public class TopCustomerResponse
    {
        public string? UserId { get; set; }
        public string CustomerName { get; set; } = null!;
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalSpent { get; set; }
        public decimal AverageOrderValue { get; set; }
        public DateTime? LastOrderDate { get; set; }
        public DateTime? FirstOrderDate { get; set; }
    }

    public class CustomerGrowthResponse
    {
        public DateTime Date { get; set; }
        public int NewCustomers { get; set; }
        public int TotalCustomers { get; set; }
    }
}