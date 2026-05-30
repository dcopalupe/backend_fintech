using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FinTech.API.Models.Enums;

namespace FinTech.API.Models;

public class Loan
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string UserId { get; set; } = string.Empty;

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    [Required]
    public int Term { get; set; } // Meses

    [Required]
    [Column(TypeName = "decimal(5,2)")]
    public decimal InterestRate { get; set; } // TEA (Tasa Efectiva Anual)

    [Required]
    public LoanType LoanType { get; set; }

    [Required]
    public LoanStatus Status { get; set; } = LoanStatus.Pending;

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal MonthlyPayment { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    public ICollection<PaymentSchedule> PaymentSchedules { get; set; } = new List<PaymentSchedule>();
}
