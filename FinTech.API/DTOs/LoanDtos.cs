using System.ComponentModel.DataAnnotations;
using FinTech.API.Models.Enums;

namespace FinTech.API.DTOs;

public class CreateLoanDto
{
    [Required]
    [StringLength(100)]
    public string UserId { get; set; } = string.Empty;

    [Required]
    [Range(100, 10000000)]
    public decimal Amount { get; set; }

    [Required]
    [Range(1, 360)]
    public int Term { get; set; } // Meses

    [Required]
    [Range(0.01, 100)]
    public decimal InterestRate { get; set; } // TEA

    [Required]
    public LoanType LoanType { get; set; }
}

public class LoanDto
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int Term { get; set; }
    public decimal InterestRate { get; set; }
    public LoanType LoanType { get; set; }
    public LoanStatus Status { get; set; }
    public decimal MonthlyPayment { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class UpdateLoanStatusDto
{
    [Required]
    public LoanStatus Status { get; set; }
}
