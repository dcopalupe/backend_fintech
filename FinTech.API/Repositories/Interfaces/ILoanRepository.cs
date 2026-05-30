using FinTech.API.Models;

namespace FinTech.API.Repositories.Interfaces;

public interface ILoanRepository
{
    Task<Loan?> GetByIdAsync(int id);
    Task<IEnumerable<Loan>> GetAllAsync();
    Task<IEnumerable<Loan>> GetByUserIdAsync(string userId);
    Task<Loan> AddAsync(Loan loan);
    Task UpdateAsync(Loan loan);
    Task DeleteAsync(Loan loan);
    Task<bool> ExistsAsync(int id);
    Task SaveChangesAsync();
}
