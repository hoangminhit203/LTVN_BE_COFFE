using LVTN_BE_COFFE.Domain.Common;

namespace LVTN_BE_COFFE.Domain.VModel
{
    public class AspNetUsersVModel
    {
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? AvatarPath { get; set; }
        public bool? Sex { get; set; }
        public DateOnly? Birthday { get; set; }
        public bool? IsActive { get; set; }
        public string RoleName { get; set; }
    }
    public class AspNetUsersCreateVModel : AspNetUsersVModel
    {
        public required string Password { get; set; }
        // Thêm dòng này
        public string? RoleName { get; set; } = "Customer";
    }
    public class AspNetUsersUpdateVModel : AspNetUsersVModel
    {
        public required string Id { get; set; }
    }
    public class AspNetUsersGetVModel : AspNetUsersUpdateVModel
    {
        public List<dynamic>? JsonUserHasFunctions { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string? UpdatedBy { get; set; }
   
    }
    public class AspNetUsersFilterParams
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
