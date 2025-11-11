using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Blog 
{
    [Key]
    public int Id { get; set; }

    [Required, StringLength(255)]
    public string Title { get; set; }

    [Required]
    public string Content { get; set; }

    public int? AuthorId { get; set; }

    public DateTime PublishedAt { get; set; } = DateTime.UtcNow;

    [StringLength(255)]
    public string? ImageUrl { get; set; }

    // Navigation property
    //[ForeignKey(nameof(AuthorId))]
    //public User? Author { get; set; }
}