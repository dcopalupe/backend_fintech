using FinTech.API.Data;
using FinTech.API.Models;
using FinTech.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinTech.API.Repositories.Implementations;

public class TransactionRepository : ITransactionRepository
{
    private readonly ApplicationDbContext _context;

    public TransactionRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Transaction?> GetByIdAsync(int id)
    {
        return await _context.Transactions
            .Include(t => t.Loan)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<IEnumerable<Transaction>> GetAllAsync()
    {
        return await _context.Transactions
            .Include(t => t.Loan)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Transaction>> GetByLoanIdAsync(int loanId)
    {
        return await _context.Transactions
            .Where(t => t.LoanId == loanId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<Transaction?> GetByIdempotencyKeyAsync(string idempotencyKey)
    {
        return await _context.Transactions
            .FirstOrDefaultAsync(t => t.IdempotencyKey == idempotencyKey);
    }

    public async Task<Transaction> AddAsync(Transaction transaction)
    {
        await _context.Transactions.AddAsync(transaction);
        return transaction;
    }

    public Task UpdateAsync(Transaction transaction)
    {
        _context.Transactions.Update(transaction);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Transaction transaction)
    {
        _context.Transactions.Remove(transaction);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
