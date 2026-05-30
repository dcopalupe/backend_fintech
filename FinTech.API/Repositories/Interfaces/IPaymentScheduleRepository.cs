using FinTech.API.Models;
using FinTech.API.Models.Enums;

namespace FinTech.API.Repositories.Interfaces;

public interface IPaymentScheduleRepository
{
    Task<PaymentSchedule?> GetByIdAsync(int id);
    Task<IEnumerable<PaymentSchedule>> GetByLoanIdAsync(int loanId);
    Task<IEnumerable<PaymentSchedule>> GetPendingByLoanIdAsync(int loanId);
    Task<PaymentSchedule> AddAsync(PaymentSchedule paymentSchedule);
    Task AddRangeAsync(IEnumerable<PaymentSchedule> paymentSchedules);
    Task UpdateAsync(PaymentSchedule paymentSchedule);
    Task UpdateRangeAsync(IEnumerable<PaymentSchedule> paymentSchedules);
    Task DeleteAsync(PaymentSchedule paymentSchedule);
    Task<bool> AllPaidAsync(int loanId);
    Task SaveChangesAsync();
}
