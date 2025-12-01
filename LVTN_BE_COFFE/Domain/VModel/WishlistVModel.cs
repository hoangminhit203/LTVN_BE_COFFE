using LVTN_BE_COFFE.Domain.Common;
using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace LVTN_BE_COFFE.Domain.VModel
{
    public class WishlistCreateVModel
    {
        [Required(ErrorMessage = "Mã sản phẩm là bắt buộc.")]
        public int ProductId { get; set; }
    }

    public class WishlistResponseVModel
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public decimal ProductPrice { get; set; }
        public string? ProductImageUrl { get; set; }
        public DateTime AddedAt { get; set; }
    }

    public class WishlistFilterVModel
    {
        public int PageNumber { get; set; } = Numbers.Pagination.DefaultPageNumber;
        public int PageSize { get; set; } = Numbers.Pagination.DefaultPageSize;
        public string? SortBy { get; set; }
    }
}
