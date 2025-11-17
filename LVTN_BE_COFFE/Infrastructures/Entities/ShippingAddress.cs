using LVTN_BE_COFFE.Infrastructures.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class ShippingAddress
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } // FK đến AspNetUsers

    [ForeignKey(nameof(UserId))]
    public AspNetUsers User { get; set; } // Navigation property

    [Required]
    public string FullAddress { get; set; }

    public string? ReceiverName { get; set; }
    public string? Phone { get; set; }

    public bool IsDefault { get; set; } = false;

    public ICollection<Order> Orders { get; set; }
}
