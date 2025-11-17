using LVTN_BE_COFFE.Domain.VModel;

public interface IPromotionService
{
    // Quản lý (Admin functions)
    Task<Promotion?> GetPromotionByIdAsync(int id);
    Task<IEnumerable<Promotion>> GetAllPromotionsAsync();
    Task<Promotion> CreatePromotionAsync(PromotionCreateVModel model);
    Task<Promotion?> UpdatePromotionAsync(int id, PromotionUpdateVModel model);
    Task<bool> DeletePromotionAsync(int id);

    // Chức năng áp dụng (User function)
    /// <summary>
    /// Áp dụng mã khuyến mãi cho đơn hàng, trả về số tiền giảm giá được tính toán.
    /// </summary>
    /// <param name="code">Mã khuyến mãi.</param>
    /// <param name="orderTotal">Tổng tiền đơn hàng hiện tại (trước giảm giá).</param>
    /// <param name="productIds">Danh sách IDs sản phẩm trong đơn hàng.</param>
    /// <returns>Số tiền giảm giá hoặc null nếu không hợp lệ.</returns>
    Task<decimal?> ApplyPromotionAsync(string code, decimal orderTotal, List<int> productIds);
}