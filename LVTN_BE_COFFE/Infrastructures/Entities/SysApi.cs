namespace LVTN_BE_COFFE.Infrastructures.Entities
{
    public class SysApi
    {
        public long Id { get; set; }

        public string ControllerName { get; set; } = null!;

        public string ActionName { get; set; } = null!;

        public string HttpMethod { get; set; } = null!;

        public DateTime? CreatedDate { get; set; }

        public string? CreatedBy { get; set; }

        public DateTime? UpdatedDate { get; set; }

        public string? UpdatedBy { get; set; }

        public bool? IsActive { get; set; }
    }
}
