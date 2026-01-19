using LVTN_BE_COFFE.Domain.IServices;
using LVTN_BE_COFFE.Domain.Model;
using LVTN_BE_COFFE.Domain.VModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LVTN_BE_COFFE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    // [Authorize(Roles = "Admin")] // Uncomment khi production
    public class StatisticsController : ControllerBase
    {
        private readonly IStatisticsService _statisticsService;

        public StatisticsController(IStatisticsService statisticsService)
        {
            _statisticsService = statisticsService;
        }

        /// <summary>
        /// Lấy thống kê tổng quan Dashboard
        /// GET: api/statistics/dashboard
        /// </summary>
        [HttpGet("dashboard")]
        public async Task<ActionResult<ResponseResult>> GetDashboardStatistics()
        {
            return await _statisticsService.GetDashboardStatistics();
        }

        /// <summary>
        /// Lấy thống kê doanh thu chi tiết
        /// GET: api/statistics/revenue?fromDate=2026-01-01&toDate=2026-01-31&groupBy=day
        /// </summary>
        [HttpGet("revenue")]
        public async Task<ActionResult<ResponseResult>> GetRevenueStatistics([FromQuery] StatisticsFilterVModel filter)
        {
            return await _statisticsService.GetRevenueStatistics(filter);
        }

        /// <summary>
        /// Lấy thống kê đơn hàng
        /// GET: api/statistics/orders?fromDate=2026-01-01&toDate=2026-01-31&groupBy=week
        /// </summary>
        [HttpGet("orders")]
        public async Task<ActionResult<ResponseResult>> GetOrderStatistics([FromQuery] StatisticsFilterVModel filter)
        {
            return await _statisticsService.GetOrderStatistics(filter);
        }

        /// <summary>
        /// Lấy thống kê sản phẩm bán chạy
        /// GET: api/statistics/products?fromDate=2026-01-01&toDate=2026-01-31
        /// </summary>
        [HttpGet("products")]
        public async Task<ActionResult<ResponseResult>> GetProductStatistics([FromQuery] StatisticsFilterVModel filter)
        {
            return await _statisticsService.GetProductStatistics(filter);
        }

        /// <summary>
        /// Lấy thống kê khách hàng
        /// GET: api/statistics/customers?fromDate=2026-01-01&toDate=2026-01-31
        /// </summary>
        [HttpGet("customers")]
        public async Task<ActionResult<ResponseResult>> GetCustomerStatistics([FromQuery] StatisticsFilterVModel filter)
        {
            return await _statisticsService.GetCustomerStatistics(filter);
        }

        /// <summary>
        /// So sánh doanh thu giữa 2 khoảng thời gian
        /// GET: api/statistics/compare?fromDate1=2026-01-01&toDate1=2026-01-31&fromDate2=2025-12-01&toDate2=2025-12-31
        /// </summary>
        [HttpGet("compare")]
        public async Task<ActionResult<ResponseResult>> CompareRevenue(
            [FromQuery] DateTime fromDate1,
            [FromQuery] DateTime toDate1,
            [FromQuery] DateTime fromDate2,
            [FromQuery] DateTime toDate2)
        {
            return await _statisticsService.CompareRevenue(fromDate1, toDate1, fromDate2, toDate2);
        }

        /// <summary>
        /// Xuất báo cáo Excel
        /// GET: api/statistics/export/revenue?fromDate=2026-01-01&toDate=2026-01-31
        /// </summary>
        [HttpGet("export/revenue")]
        public async Task<IActionResult> ExportRevenueReport([FromQuery] StatisticsFilterVModel filter)
        {
            var fileBytes = await _statisticsService.ExportRevenueReport(filter);
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "revenue_report.xlsx");
        }
    }
}