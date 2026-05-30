using FinTech.API.DTOs;

namespace FinTech.API.Services.Interfaces;

public interface ITransactionService
{
    Task<TransactionDto?> CreateTransactionAsync(CreateTransactionDto createTransactionDto);
    Task<TransactionDto?> GetTransactionByIdAsync(int id);
    Task<IEnumerable<TransactionDto>> GetTransactionsByLoanIdAsync(int loanId);
    Task<IEnumerable<TransactionDto>> GetAllTransactionsAsync();
}
