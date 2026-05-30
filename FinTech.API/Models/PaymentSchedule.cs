using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FinTech.API.Models.Enums;

namespace FinTech.API.Models;

public class PaymentSchedule
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int LoanId { get; set; }

    [Required]
    public int PaymentNumber { get; set; }

    [Required]
    public DateTime DueDate { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalPayment { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Principal { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Interest { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal RemainingBalance { get; set; }

    [Required]
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

    // Navigation property
    [ForeignKey(nameof(LoanId))]
    public Loan? Loan { get; set; }
}
