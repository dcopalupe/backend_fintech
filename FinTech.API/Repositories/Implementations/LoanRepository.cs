using FinTech.API.Data;
using FinTech.API.Models;
using FinTech.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinTech.API.Repositories.Implementations;

public class LoanRepository : ILoanRepository
{
    private readonly ApplicationDbContext _context;

    public LoanRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Loan?> GetByIdAsync(int id)
    {
        return await _context.Loans
            .Include(l => l.PaymentSchedules)
            .Include(l => l.Transactions)
            .FirstOrDefaultAsync(l => l.Id == id);
    }

    public async Task<IEnumerable<Loan>> GetAllAsync()
    {
        return await _context.Loans
            .Include(l => l.PaymentSchedules)
            .Include(l => l.Transactions)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Loan>> GetByUserIdAsync(string userId)
    {
        return await _context.Loans
            .Include(l => l.PaymentSchedules)
            .Include(l => l.Transactions)
            .Where(l => l.UserId == userId)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync();
    }

    public async Task<Loan> AddAsync(Loan loan)
    {
        await _context.Loans.AddAsync(loan);
        return loan;
    }

    public async Task UpdateAsync(Loan loan)
    {
        _context.Loans.Update(loan);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(int id)
    {
        var loan = await GetByIdAsync(id);
        if (loan != null)
        {
            _context.Loans.Remove(loan);
        }
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Loans.AnyAsync(l => l.Id == id);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
