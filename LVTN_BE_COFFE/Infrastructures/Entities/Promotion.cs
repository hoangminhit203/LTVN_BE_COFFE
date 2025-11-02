using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public enum PromotionType
{
    Percentage,
    Fixed
}

public class Promotion
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string Code { get; set; }

    [StringLength(255)]
    public string? Description { get; set; }

    [Required]
    public PromotionType DiscountType { get; set; }

    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "Discount value must be greater than or equal to 0.")]
    public decimal DiscountValue { get; set; }

    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? MinOrderValue { get; set; }

    [StringLength(500)]
    public string? ApplicableProducts { get; set; } // Comma-separated IDs or JSON string

    // Navigation property
    public ICollection<Order> Orders { get; set; } = new List<Order>();

    [NotMapped]
    public bool IsActive =>
        (!StartDate.HasValue || StartDate <= DateTime.UtcNow) &&
        (!EndDate.HasValue || EndDate >= DateTime.UtcNow);
}
