using LVTN_BE_COFFE.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace LVTN_BE_COFFE.Domain.VModel
{
    // ✅ Create ViewModel
    public class BrewingMethodsCreateVModel
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [StringLength(500)]
        public string Description { get; set; }

        [Required]
        public bool? IsActive { get; set; }
    }

    // ✅ Update ViewModel
    public class BrewingMethodsUpdateVModel : BrewingMethodsCreateVModel
    {
        [Required]
        [JsonIgnore] // Ẩn field này khỏi JSON serialization
        public int BrewingMethodId { get; set; }
    }

    // ✅ Response ViewModel
    public class BrewingMethodsResponse
    {
        public long BrewingMethodId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }

    // ✅ Filter ViewModel (cho pagination)
    public class BrewingMethodsFilterVModel
    {
        public string? Name { get; set; }
        public int PageNumber { get; set; } = Numbers.Pagination.DefaultPageNumber;
        public int PageSize { get; set; } = Numbers.Pagination.DefaultPageSize;
    }

}
