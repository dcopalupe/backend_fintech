using System.ComponentModel.DataAnnotations;
using FinTech.API.Models.Enums;

namespace FinTech.API.DTOs;

public class CreateTransactionDto
{
    [Required]
    [StringLength(200)]
    public string IdempotencyKey { get; set; } = string.Empty;

    [Required]
    public TransactionType Type { get; set; }

    [Required]
    [Range(0.01, 10000000)]
    public decimal Amount { get; set; }

    public int? LoanId { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }
}

public class TransactionDto
{
    public int Id { get; set; }
    public string IdempotencyKey { get; set; } = string.Empty;
    public TransactionType Type { get; set; }
    public decimal Amount { get; set; }
    public TransactionStatus Status { get; set; }
    public int? LoanId { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
}
