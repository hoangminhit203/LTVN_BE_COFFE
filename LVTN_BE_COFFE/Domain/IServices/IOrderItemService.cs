public interface IOrderItemService
{
    Task<IEnumerable<OrderItem>> GetAllAsync();
    Task<OrderItem?> GetByIdAsync(int id);
    Task<OrderItem> CreateAsync(OrderItem orderItem);
    Task<OrderItem?> UpdateAsync(int id, OrderItem orderItem);
    Task<bool> DeleteAsync(int id);
}
