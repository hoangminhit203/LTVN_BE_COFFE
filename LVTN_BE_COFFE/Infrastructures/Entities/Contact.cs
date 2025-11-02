using System;
using System.ComponentModel.DataAnnotations;

public class Contact
{
    [Key]
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string Name { get; set; }

    [Required, StringLength(100)]
    public string Email { get; set; }

    [StringLength(255)]
    public string? Subject { get; set; }

    [Required]
    public string Message { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}