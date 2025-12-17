using LVTN_BE_COFFE.Infrastructures.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

public class Cart : BaseEntity
{
    [Key]
    public int Id { get; set; }

    public string UserId { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(UserId))]
    public AspNetUsers? User { get; set; } = null!;

    public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    [NotMapped]
    public decimal TotalPrice => CartItems.Sum(item => item.CalculatedSubtotal);

    [StringLength(50)]
    public string Status { get; set; } = "Active";
}