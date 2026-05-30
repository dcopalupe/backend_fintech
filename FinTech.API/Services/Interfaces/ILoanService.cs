using FinTech.API.DTOs;

namespace FinTech.API.Services.Interfaces;

public interface ILoanService
{
    Task<LoanSimulationResultDto> SimulateLoanAsync(SimulateLoanDto simulateLoanDto);
    Task<LoanDto?> CreateLoanAsync(CreateLoanDto createLoanDto);
    Task<IEnumerable<LoanDto>> GetAllLoansAsync(string? userId = null);
    Task<LoanDto?> GetLoanByIdAsync(int id);
    Task<IEnumerable<PaymentScheduleDto>> GetPaymentScheduleAsync(int loanId);
    Task<LoanDto?> ApproveLoanAsync(int loanId);
    Task<LoanDto?> RejectLoanAsync(int loanId);
}
