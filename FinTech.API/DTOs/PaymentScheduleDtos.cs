using FinTech.API.Models.Enums;

namespace FinTech.API.DTOs;

/// <summary>
/// DTO para representar un item del cronograma de pagos
/// </summary>
public class PaymentScheduleItemDto
{
    public int PaymentNumber { get; set; }
    public DateTime DueDate { get; set; }
    public decimal TotalPayment { get; set; }
    public decimal Principal { get; set; }
    public decimal Interest { get; set; }
    public decimal RemainingBalance { get; set; }
    public PaymentStatus Status { get; set; }
}

/// <summary>
/// DTO para respuesta de cronograma de pagos
/// </summary>
public class PaymentScheduleDto
{
    public int Id { get; set; }
    public int LoanId { get; set; }
    public int PaymentNumber { get; set; }
    public DateTime DueDate { get; set; }
    public decimal TotalPayment { get; set; }
    public decimal Principal { get; set; }
    public decimal Interest { get; set; }
    public decimal RemainingBalance { get; set; }
    public PaymentStatus Status { get; set; }
    public DateTime? PaidDate { get; set; }
}
