using LVTN_BE_COFFE.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace LVTN_BE_COFFE.Domain.VModel
{
    public class FlavorNoteCreateVModel
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }
        [Required]
        public bool? IsActive { get; set; }
    }
    // Cập nhật flavor note
    public class FlavorNoteUpdateVModel : FlavorNoteCreateVModel
    {
        [JsonIgnore]
        public int FlavorNoteId { get; set; }
    }

    // Kết quả trả về cho client
    public class FlavorNoteResponse
    {
        public long FlavorNoteId { get; set; }
        public string Name { get; set; }
        public bool? IsActive { get; set; } // Add IsActive property
        public DateTime? CreatedDate { get; set; } // Add audit fields if needed
        public DateTime? UpdatedDate { get; set; }
    }
    public class FlavorNoteFilterVModel
    {
        public string? Name { get; set; }
        public int PageNumber { get; set; } = Numbers.Pagination.DefaultPageNumber;
        public int PageSize { get; set; } = Numbers.Pagination.DefaultPageSize;
    }
}
