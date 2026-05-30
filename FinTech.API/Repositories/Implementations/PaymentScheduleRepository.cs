using FinTech.API.Data;
using FinTech.API.Models;
using FinTech.API.Models.Enums;
using FinTech.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinTech.API.Repositories.Implementations;

public class PaymentScheduleRepository : IPaymentScheduleRepository
{
    private readonly ApplicationDbContext _context;

    public PaymentScheduleRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaymentSchedule?> GetByIdAsync(int id)
    {
        return await _context.PaymentSchedules
            .Include(ps => ps.Loan)
            .FirstOrDefaultAsync(ps => ps.Id == id);
    }

    public async Task<IEnumerable<PaymentSchedule>> GetByLoanIdAsync(int loanId)
    {
        return await _context.PaymentSchedules
            .Where(ps => ps.LoanId == loanId)
            .OrderBy(ps => ps.PaymentNumber)
            .ToListAsync();
    }

    public async Task<IEnumerable<PaymentSchedule>> GetPendingByLoanIdAsync(int loanId)
    {
        return await _context.PaymentSchedules
            .Where(ps => ps.LoanId == loanId && ps.Status == PaymentStatus.Pending)
            .OrderBy(ps => ps.PaymentNumber)
            .ToListAsync();
    }

    public async Task<PaymentSchedule> AddAsync(PaymentSchedule paymentSchedule)
    {
        await _context.PaymentSchedules.AddAsync(paymentSchedule);
        return paymentSchedule;
    }

    public async Task AddRangeAsync(IEnumerable<PaymentSchedule> paymentSchedules)
    {
        await _context.PaymentSchedules.AddRangeAsync(paymentSchedules);
    }

    public Task UpdateAsync(PaymentSchedule paymentSchedule)
    {
        _context.PaymentSchedules.Update(paymentSchedule);
        return Task.CompletedTask;
    }

    public Task UpdateRangeAsync(IEnumerable<PaymentSchedule> paymentSchedules)
    {
        _context.PaymentSchedules.UpdateRange(paymentSchedules);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(PaymentSchedule paymentSchedule)
    {
        _context.PaymentSchedules.Remove(paymentSchedule);
        return Task.CompletedTask;
    }

    public async Task<bool> AllPaidAsync(int loanId)
    {
        return await _context.PaymentSchedules
            .Where(ps => ps.LoanId == loanId)
            .AllAsync(ps => ps.Status == PaymentStatus.Paid);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
