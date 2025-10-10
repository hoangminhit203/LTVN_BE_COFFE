using LVTN_BE_COFFE.Domain.Common;

namespace LVTN_BE_COFFE.Domain.VModel
{
    public class AspNetRolesCreateVModel
    {
        public required string Name { get; set; }
        public bool? IsActive { get; set; }

    }
    public class AspNetRolesUpdateVModel : AspNetRolesCreateVModel
    {
        public required string Id { get; set; }
    }
    public class AspNetRolesGetVModel : AspNetRolesUpdateVModel
    {
        public string? JsonRoleHasFunctions { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string? UpdatedBy { get; set; }
    }
    public class AspNetRolesFilterParams
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
