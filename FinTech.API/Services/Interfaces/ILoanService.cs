using FinTech.API.DTOs;

namespace FinTech.API.Services.Interfaces;

public interface ILoanService
{
    Task<LoanSimulationResultDto> SimulateLoanAsync(SimulateLoanDto simulateLoanDto);
    Task<LoanDto?> CreateLoanAsync(CreateLoanDto createLoanDto);
    Task<LoanDto?> GetLoanByIdAsync(int id);
    Task<IEnumerable<LoanDto>> GetAllLoansAsync(string? userId = null);
    Task<IEnumerable<PaymentScheduleDto>> GetPaymentScheduleAsync(int id);
    Task<LoanDto?> ApproveLoanAsync(int id);
    Task<LoanDto?> RejectLoanAsync(int id);
    Task<bool> DeleteLoanAsync(int id);
}
