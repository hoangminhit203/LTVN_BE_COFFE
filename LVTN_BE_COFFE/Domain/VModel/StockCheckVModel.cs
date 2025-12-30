namespace LVTN_BE_COFFE.Domain.VModel
{
    public class StockCheckResponse
    {
        public bool IsAvailable { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<StockCheckItemResponse> Items { get; set; } = new();
    }

    public class StockCheckItemResponse
    {
        public int CartItemId { get; set; }
        public int ProductVariantId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int RequestedQuantity { get; set; }
        public int AvailableStock { get; set; }
        public bool IsAvailable { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}