using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public enum PromotionType
{
    Percentage,
    Fixed
}

public class Promotion : IValidatableObject
{
    public int Id { get; set; }

    [Required, StringLength(50)]
    public string Code { get; set; } = string.Empty;

    public string? Description { get; set; }

    public PromotionType DiscountType { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal DiscountValue { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? MaxDiscountAmount { get; set; }

    public decimal? MinOrderValue { get; set; }

    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    public int? UsageLimit { get; set; }
    public int UsageCount { get; set; }

    public bool IsEnabled { get; set; } = true;

    // Navigation
    public ICollection<Order> Orders { get; set; } = new List<Order>();

    [NotMapped]
    public bool IsActive =>
        IsEnabled &&
        (!StartDate.HasValue || StartDate <= DateTime.UtcNow) &&
        (!EndDate.HasValue || EndDate >= DateTime.UtcNow);

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        throw new NotImplementedException();
    }
}
