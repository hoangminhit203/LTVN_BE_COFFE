using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Payment
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string OrderId { get; set; }

    [Required]
    public string PaymentMethod { get; set; } // ENUM: cod, bank_transfer, momo, zalopay, vnpay, credit_card

    [Required]
    public decimal Amount { get; set; }

    [Required]
    public string Status { get; set; } = "pending"; // ENUM: pending, completed, failed, refunded

    [StringLength(100)]
    public string? TransactionId { get; set; }

    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    [ForeignKey(nameof(OrderId))]
    public Order Order { get; set; }
}