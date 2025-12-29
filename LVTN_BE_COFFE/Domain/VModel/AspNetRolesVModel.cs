using LVTN_BE_COFFE.Domain.Common;

namespace LVTN_BE_COFFE.Domain.VModel
{
    public class AspNetRolesVModel
    {
        public required string Name { get; set; }
        public List<dynamic>? JsonRoleHasFunctions { get; set; }
        public bool? IsActive { get; set; }
    }

    public class AspNetRolesCreateVModel : AspNetRolesVModel
    {

    }

    public class AspNetRolesUpdateVModel : AspNetRolesVModel
    {
        public required string Id { get; set; }
    }

    public class AspNetRolesGetVModel : AspNetRolesUpdateVModel
    {
        public DateTime? CreatedDate { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string? UpdatedBy { get; set; }
    }

    public class AspNetRolesFilterParams
    {
        public string? Name { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string? UpdatedBy { get; set; }
        public bool? IsActive { get; set; }
        public int PageSize { get; set; } = Numbers.Pagination.DefaultPageSize;
        public int PageNumber { get; set; } = Numbers.Pagination.DefaultPageNumber;
    }
}
