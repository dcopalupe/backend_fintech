using System.ComponentModel.DataAnnotations;
using FinTech.API.Models.Enums;

namespace FinTech.API.DTOs;

public class SimulateLoanDto
{
    [Required(ErrorMessage = "El monto es requerido")]
    [Range(100, 10000000, ErrorMessage = "El monto debe estar entre 100 y 10,000,000")]
    public decimal Amount { get; set; }

    [Required(ErrorMessage = "El plazo es requerido")]
    [Range(1, 360, ErrorMessage = "El plazo debe estar entre 1 y 360 meses")]
    public int Term { get; set; }

    [Required(ErrorMessage = "La tasa de interés es requerida")]
    [Range(0.01, 100, ErrorMessage = "La tasa de interés debe estar entre 0.01 y 100")]
    public decimal InterestRate { get; set; }

    [Required(ErrorMessage = "El tipo de préstamo es requerido")]
    public LoanType LoanType { get; set; }
}

public class CreateLoanDto
{
    [Required(ErrorMessage = "El ID de usuario es requerido")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "El ID de usuario debe tener entre 1 y 100 caracteres")]
    public string UserId { get; set; } = string.Empty;

    [Required(ErrorMessage = "El monto es requerido")]
    [Range(100, 10000000, ErrorMessage = "El monto debe estar entre 100 y 10,000,000")]
    public decimal Amount { get; set; }

    [Required(ErrorMessage = "El plazo es requerido")]
    [Range(1, 360, ErrorMessage = "El plazo debe estar entre 1 y 360 meses")]
    public int Term { get; set; }

    [Required(ErrorMessage = "La tasa de interés es requerida")]
    [Range(0.01, 100, ErrorMessage = "La tasa de interés debe estar entre 0.01 y 100")]
    public decimal InterestRate { get; set; }

    [Required(ErrorMessage = "El tipo de préstamo es requerido")]
    public LoanType LoanType { get; set; }
}

public class LoanSimulationResultDto
{
    public decimal Amount { get; set; }
    public int Term { get; set; }
    public decimal InterestRate { get; set; }
    public LoanType LoanType { get; set; }
    public decimal MonthlyPayment { get; set; }
    public decimal TotalToPay { get; set; }
    public decimal TotalInterest { get; set; }
    public List<PaymentScheduleItemDto> PaymentSchedule { get; set; } = new();
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
