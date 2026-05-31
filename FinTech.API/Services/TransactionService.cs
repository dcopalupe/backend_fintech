using FinTech.API.DTOs;
using FinTech.API.Models;
using FinTech.API.Models.Enums;
using FinTech.API.Repositories.Interfaces;
using FinTech.API.Services.Interfaces;

namespace FinTech.API.Services;

public class TransactionService : ITransactionService
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly ILoanRepository _loanRepository;
    private readonly IPaymentScheduleRepository _paymentScheduleRepository;

    public TransactionService(
        ITransactionRepository transactionRepository,
        ILoanRepository loanRepository,
        IPaymentScheduleRepository paymentScheduleRepository)
    {
        _transactionRepository = transactionRepository;
        _loanRepository = loanRepository;
        _paymentScheduleRepository = paymentScheduleRepository;
    }

    public async Task<TransactionDto?> CreateTransactionAsync(CreateTransactionDto createTransactionDto)
    {
        var existingTransaction = await _transactionRepository.GetByIdempotencyKeyAsync(createTransactionDto.IdempotencyKey);

        if (existingTransaction != null)
        {
            return MapToDto(existingTransaction);
        }

        if (createTransactionDto.LoanId.HasValue)
        {
            var loan = await _loanRepository.GetByIdAsync(createTransactionDto.LoanId.Value);
            if (loan == null)
            {
                return null;
            }
        }

        var transaction = new Transaction
        {
            IdempotencyKey = createTransactionDto.IdempotencyKey,
            Type = createTransactionDto.Type,
            Amount = createTransactionDto.Amount,
            Status = TransactionStatus.Pending,
            LoanId = createTransactionDto.LoanId,
            Description = createTransactionDto.Description,
            CreatedAt = DateTime.UtcNow
        };

        await _transactionRepository.AddAsync(transaction);

        if (createTransactionDto.Type == TransactionType.Payment && createTransactionDto.LoanId.HasValue)
        {
            await ProcessPaymentAsync(createTransactionDto.LoanId.Value, createTransactionDto.Amount);
            transaction.Status = TransactionStatus.Completed;
        }
        else if (createTransactionDto.Type == TransactionType.Disbursement && createTransactionDto.LoanId.HasValue)
        {
            var loan = await _loanRepository.GetByIdAsync(createTransactionDto.LoanId.Value);
            if (loan != null && loan.Status == LoanStatus.Approved)
            {
                loan.Status = LoanStatus.Active;
                loan.UpdatedAt = DateTime.UtcNow;
                await _loanRepository.UpdateAsync(loan);
                transaction.Status = TransactionStatus.Completed;
            }
        }
        else
        {
            transaction.Status = TransactionStatus.Completed;
        }

        await _transactionRepository.SaveChangesAsync();

        return MapToDto(transaction);
    }

    public async Task<TransactionDto?> GetTransactionByIdAsync(int id)
    {
        var transaction = await _transactionRepository.GetByIdAsync(id);
        return transaction != null ? MapToDto(transaction) : null;
    }

    public async Task<IEnumerable<TransactionDto>> GetTransactionsByLoanIdAsync(int loanId)
    {
        var transactions = await _transactionRepository.GetByLoanIdAsync(loanId);
        return transactions.Select(MapToDto);
    }

    public async Task<IEnumerable<TransactionDto>> GetAllTransactionsAsync()
    {
        var transactions = await _transactionRepository.GetAllAsync();
        return transactions.Select(MapToDto);
    }

    private async Task ProcessPaymentAsync(int loanId, decimal paymentAmount)
    {
        var pendingSchedules = await _paymentScheduleRepository.GetPendingByLoanIdAsync(loanId);

        var remainingAmount = paymentAmount;

        foreach (var schedule in pendingSchedules)
        {
            if (remainingAmount <= 0) break;

            if (remainingAmount >= schedule.TotalPayment)
            {
                remainingAmount -= schedule.TotalPayment;
                schedule.Status = PaymentStatus.Paid;
                await _paymentScheduleRepository.UpdateAsync(schedule);
            }
            else
            {
                break;
            }
        }

        var allPaid = await _paymentScheduleRepository.AllPaidAsync(loanId);

        if (allPaid)
        {
            var loan = await _loanRepository.GetByIdAsync(loanId);
            if (loan != null)
            {
                loan.Status = LoanStatus.Completed;
                loan.UpdatedAt = DateTime.UtcNow;
                await _loanRepository.UpdateAsync(loan);
            }
        }

        await _paymentScheduleRepository.SaveChangesAsync();
    }

    private TransactionDto MapToDto(Transaction transaction)
    {
        return new TransactionDto
        {
            Id = transaction.Id,
            IdempotencyKey = transaction.IdempotencyKey,
            Type = transaction.Type,
            Amount = transaction.Amount,
            Status = transaction.Status,
            LoanId = transaction.LoanId,
            Description = transaction.Description,
            CreatedAt = transaction.CreatedAt
        };
    }
}
