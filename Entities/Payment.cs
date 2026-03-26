using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PixApiRest.Entities;

[Table("payments")]
public class Payment
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Required]
    [Column("amount", TypeName = "decimal(15,2)")]
    public decimal Amount { get; set; }

    [MaxLength(140)]
    [Column("description")]
    public string? Description { get; set; }

    [Required]
    [Column("status")]
    public PaymentStatus Status { get; set; } = PaymentStatus.PENDING;

    [Column("payload", TypeName = "TEXT")]
    public string? Payload { get; set; }

    [Required]
    [Column("pix_key")]
    public string PixKey { get; set; } = string.Empty;

    [Required]
    [MaxLength(25)]
    [Column("receiver_name")]
    public string ReceiverName { get; set; } = string.Empty;

    [Required]
    [MaxLength(15)]
    [Column("receiver_city")]
    public string ReceiverCity { get; set; } = string.Empty;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [Column("approved_at")]
    public DateTime? ApprovedAt { get; set; }
}
