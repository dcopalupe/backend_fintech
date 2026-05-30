using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FinTech.API.Models.Enums;

namespace FinTech.API.Models;

public class Transaction
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    public string IdempotencyKey { get; set; } = string.Empty;

    [Required]
    public TransactionType Type { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    [Required]
    public TransactionStatus Status { get; set; } = TransactionStatus.Pending;

    public int? LoanId { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    [ForeignKey(nameof(LoanId))]
    public Loan? Loan { get; set; }
}
