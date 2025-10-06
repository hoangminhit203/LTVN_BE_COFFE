namespace LVTN_BE_COFFE.Infrastructures.Entities
{
    public class BaseEntity
    {
        public long Id { get; set; }
        public bool? IsActive { get; set; } = false;
        public DateTime? CreatedDate { get; set; } = null!;
        public string? CreatedBy { get; set; } = null!;
        public DateTime? UpdatedDate { get; set; } = null!;
        public string? UpdatedBy { get; set; } = null!;
    }
}
