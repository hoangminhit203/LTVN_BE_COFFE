using LVTN_BE_COFFE.Domain.Common;

namespace LVTN_BE_COFFE.Services.Services
{
    public class SysApiCreateVModel
    {
        public string ControllerName { get; set; } = null!;
        public string ActionName { get; set; } = null!;
        public string HttpMethod { get; set; } = null!;
        public bool? IsActive { get; set; }
    }
    public class SysApiUpdateVModel : SysApiCreateVModel
    {
        public long Id { get; set; }
    }
    public class SysApiGetVModel : SysApiUpdateVModel
    {
        public DateTime? CreatedDate { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string? UpdatedBy { get; set; }
    }
    public class SysApiFilterParams
    {
        public DateTime? CreatedDate { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string? UpdatedBy { get; set; }
        public bool? IsActive { get; set; }
        public int PageSize { get; set; } = Numbers.Pagination.DefaultPageSize;
        public int PageNumber { get; set; } = Numbers.Pagination.DefaultPageNumber;
    }
}
