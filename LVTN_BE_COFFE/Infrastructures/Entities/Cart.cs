using LVTN_BE_COFFE.Infrastructures.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

public class Cart
{
    [Key]
    public int Id { get; set; }

    public int? UserId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(UserId))]
    public AspNetUsers? User { get; set; }

    public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    [NotMapped]
    public decimal TotalPrice => CartItems?.Sum(i => i.Subtotal) ?? 0;

    public string Status { get; set; } = "Active";
}
