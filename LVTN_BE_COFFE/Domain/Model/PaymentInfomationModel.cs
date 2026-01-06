namespace LVTN_BE_COFFE.Domain.Model
{
    public class PaymentInfomationModel
    {
        public string OrderId { get; set; } // Thêm dòng này
        public string OrderType { get; set; } = string.Empty;
        public double Amount { get; set; }
        public string OrderDescription { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }
}
