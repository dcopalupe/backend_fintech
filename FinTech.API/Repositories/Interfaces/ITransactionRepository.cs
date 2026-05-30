using FinTech.API.Models;

namespace FinTech.API.Repositories.Interfaces;

public interface ITransactionRepository
{
    Task<Transaction?> GetByIdAsync(int id);
    Task<IEnumerable<Transaction>> GetAllAsync();
    Task<IEnumerable<Transaction>> GetByLoanIdAsync(int loanId);
    Task<Transaction?> GetByIdempotencyKeyAsync(string idempotencyKey);
    Task<Transaction> AddAsync(Transaction transaction);
    Task UpdateAsync(Transaction transaction);
    Task DeleteAsync(Transaction transaction);
    Task SaveChangesAsync();
}
