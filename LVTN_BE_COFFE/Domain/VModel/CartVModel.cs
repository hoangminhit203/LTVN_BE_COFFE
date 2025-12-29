using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LVTN_BE_COFFE.Domain.VModel
{
    public class CartResponse
    {
        public int CartId { get; set; }
        public string? UserId { get; set; }
        public string? GuestKey { get; set; }
        public string? UserName { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal TotalPrice { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<CartItemResponse> Items { get; set; } = new();
    }
}
