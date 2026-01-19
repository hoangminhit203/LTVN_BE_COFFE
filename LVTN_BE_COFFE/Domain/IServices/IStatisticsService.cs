using LVTN_BE_COFFE.Domain.Model;
using LVTN_BE_COFFE.Domain.VModel;
using Microsoft.AspNetCore.Mvc;

namespace LVTN_BE_COFFE.Domain.IServices
{
    public interface IStatisticsService
    {
    
        /// Lấy thống kê tổng quan cho Dashboard
  
        Task<ActionResult<ResponseResult>> GetDashboardStatistics(StatisticsFilterVModel? filter = null);

         /// Lấy thống kê doanh thu chi tiết

        Task<ActionResult<ResponseResult>> GetRevenueStatistics(StatisticsFilterVModel filter);


        /// Lấy thống kê đơn hàng chi tiết

        Task<ActionResult<ResponseResult>> GetOrderStatistics(StatisticsFilterVModel filter);

       
        /// Lấy thống kê sản phẩm bán chạy
       
        Task<ActionResult<ResponseResult>> GetProductStatistics(StatisticsFilterVModel filter);

      
        /// Lấy thống kê khách hàng
       
        Task<ActionResult<ResponseResult>> GetCustomerStatistics(StatisticsFilterVModel filter);

     
        /// So sánh doanh thu giữa 2 khoảng thời gian
       
        Task<ActionResult<ResponseResult>> CompareRevenue(DateTime fromDate1, DateTime toDate1, DateTime fromDate2, DateTime toDate2);

        /// Xuất báo cáo Excel
      
        Task<byte[]> ExportRevenueReport(StatisticsFilterVModel filter);
    }
}