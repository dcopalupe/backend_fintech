using FinTech.API.DTOs;
using FinTech.API.Models;
using FinTech.API.Models.Enums;
using FinTech.API.Repositories.Interfaces;
using FinTech.API.Services.Interfaces;
using FinTech.API.Utils;

namespace FinTech.API.Services;

public class LoanService : ILoanService
{
    private readonly ILoanRepository _loanRepository;
    private readonly IPaymentScheduleRepository _paymentScheduleRepository;
    private readonly ITransactionRepository _transactionRepository;

    public LoanService(
        ILoanRepository loanRepository,
        IPaymentScheduleRepository paymentScheduleRepository,
        ITransactionRepository transactionRepository)
    {
        _loanRepository = loanRepository;
        _paymentScheduleRepository = paymentScheduleRepository;
        _transactionRepository = transactionRepository;
    }

    public async Task<LoanSimulationResultDto> SimulateLoanAsync(SimulateLoanDto simulateLoanDto)
    {
        var monthlyRate = FinancialCalculator.ConvertAnnualToMonthlyRate(simulateLoanDto.InterestRate);

        decimal monthlyPayment;
        List<PaymentScheduleItemDto> paymentSchedule;

        if (simulateLoanDto.LoanType == LoanType.Fixed)
        {
            monthlyPayment = FinancialCalculator.CalculateFixedMonthlyPayment(
                simulateLoanDto.Amount,
                monthlyRate,
                simulateLoanDto.Term
            );

            paymentSchedule = GenerateFixedPaymentSchedule(
                simulateLoanDto.Amount,
                monthlyPayment,
                monthlyRate,
                simulateLoanDto.Term
            );
        }
        else
        {
            var constantPrincipal = FinancialCalculator.CalculateConstantPrincipal(
                simulateLoanDto.Amount,
                simulateLoanDto.Term
            );

            paymentSchedule = GenerateDecreasingPaymentSchedule(
                simulateLoanDto.Amount,
                constantPrincipal,
                monthlyRate,
                simulateLoanDto.Term
            );

            monthlyPayment = paymentSchedule.First().TotalPayment;
        }

        var totalToPay = paymentSchedule.Sum(p => p.TotalPayment);
        var totalInterest = FinancialCalculator.CalculateTotalInterest(totalToPay, simulateLoanDto.Amount);

        return new LoanSimulationResultDto
        {
            Amount = simulateLoanDto.Amount,
            Term = simulateLoanDto.Term,
            InterestRate = simulateLoanDto.InterestRate,
            LoanType = simulateLoanDto.LoanType,
            MonthlyPayment = monthlyPayment,
            TotalToPay = totalToPay,
            TotalInterest = totalInterest,
            PaymentSchedule = paymentSchedule
        };
    }

    public async Task<LoanDto?> CreateLoanAsync(CreateLoanDto createLoanDto)
    {

        var loansClient = await _loanRepository.GetByUserIdAsync(createLoanDto.UserId);

        var sumPaymentMonthly = loansClient.Sum(loan => loan.MonthlyPayment);

        if (sumPaymentMonthly > (createLoanDto.MonthlyPayment * 0.4m))
        {
            throw new Exception("La suma de las cuotas de sus prestamos no puede ser mayor al 40% de su ingreso mensual.");
        }

        int loansClientCount = loansClient.Count(loan => loan.Status == LoanStatus.Active);

        if (loansClientCount > 3)
        {
            throw new Exception("El cliente no puede tener mas de 3 prestamos activos simultaneamente.");
        }

        var monthlyRate = FinancialCalculator.ConvertAnnualToMonthlyRate(createLoanDto.InterestRate);

        decimal monthlyPayment;

        if (createLoanDto.LoanType == LoanType.Fixed)
        {
            monthlyPayment = FinancialCalculator.CalculateFixedMonthlyPayment(
                createLoanDto.Amount,
                monthlyRate,
                createLoanDto.Term
            );
        }
        else
        {
            var constantPrincipal = FinancialCalculator.CalculateConstantPrincipal(
                createLoanDto.Amount,
                createLoanDto.Term
            );

            monthlyPayment = FinancialCalculator.CalculateDecreasingPayment(
                constantPrincipal,
                createLoanDto.Amount,
                monthlyRate
            );
        }

        LoanStatus status = createLoanDto.Amount < 10000 && loansClientCount < 2 ? LoanStatus.Approved : LoanStatus.Pending;

        var loan = new Loan
        {
            UserId = createLoanDto.UserId,
            Amount = createLoanDto.Amount,
            Term = createLoanDto.Term,
            InterestRate = createLoanDto.InterestRate,
            LoanType = createLoanDto.LoanType,
            Status = status,
            MonthlyPayment = monthlyPayment,
            CreatedAt = DateTime.UtcNow
        };

        await _loanRepository.AddAsync(loan);
        await _loanRepository.SaveChangesAsync();

        await GeneratePaymentScheduleAsync(loan, monthlyRate);

        return MapToDto(loan);
    }

    public async Task<IEnumerable<LoanDto>> GetAllLoansAsync(string? userId = null)
    {
        IEnumerable<Loan> loans;

        if (!string.IsNullOrEmpty(userId))
        {
            loans = await _loanRepository.GetByUserIdAsync(userId);
        }
        else
        {
            loans = await _loanRepository.GetAllAsync();
        }

        return loans.Select(MapToDto);
    }

    public async Task<LoanDto?> GetLoanByIdAsync(int id)
    {
        var loan = await _loanRepository.GetByIdAsync(id);
        return loan != null ? MapToDto(loan) : null;
    }

    public async Task<IEnumerable<PaymentScheduleDto>> GetPaymentScheduleAsync(int loanId)
    {
        var schedules = await _paymentScheduleRepository.GetByLoanIdAsync(loanId);
        return schedules.Select(s => new PaymentScheduleDto
        {
            Id = s.Id,
            LoanId = s.LoanId,
            PaymentNumber = s.PaymentNumber,
            DueDate = s.DueDate,
            TotalPayment = s.TotalPayment,
            Principal = s.Principal,
            Interest = s.Interest,
            RemainingBalance = s.RemainingBalance,
            Status = s.Status,
        });
    }

    public async Task<LoanDto?> ApproveLoanAsync(int loanId)
    {
        var loan = await _loanRepository.GetByIdAsync(loanId);

        if (loan == null || loan.Status != LoanStatus.Pending)
        {
            return null;
        }

        loan.Status = LoanStatus.Active;
        loan.UpdatedAt = DateTime.UtcNow;

        await _loanRepository.UpdateAsync(loan);

        // Crear transacción de desembolso
        var disbursementTransaction = new Transaction
        {
            IdempotencyKey = $"disbursement-{loanId}-{Guid.NewGuid()}",
            Type = TransactionType.Disbursement,
            Amount = loan.Amount,
            LoanId = loanId,
            Status = TransactionStatus.Completed,
            Description = $"Desembolso del préstamo #{loanId}",
            CreatedAt = DateTime.UtcNow
        };

        await _transactionRepository.AddAsync(disbursementTransaction);
        await _loanRepository.SaveChangesAsync();

        return MapToDto(loan);
    }

    public async Task<LoanDto?> RejectLoanAsync(int loanId)
    {
        var loan = await _loanRepository.GetByIdAsync(loanId);

        if (loan == null || loan.Status != LoanStatus.Pending)
        {
            return null;
        }

        loan.Status = LoanStatus.Rejected;
        loan.UpdatedAt = DateTime.UtcNow;

        await _loanRepository.UpdateAsync(loan);
        await _loanRepository.SaveChangesAsync();

        return MapToDto(loan);
    }

    private List<PaymentScheduleItemDto> GenerateFixedPaymentSchedule(
        decimal amount,
        decimal monthlyPayment,
        decimal monthlyRate,
        int term)
    {
        var schedule = new List<PaymentScheduleItemDto>();
        var remainingBalance = amount;

        for (int i = 1; i <= term; i++)
        {
            var interest = FinancialCalculator.CalculateInterest(remainingBalance, monthlyRate);
            var principal = FinancialCalculator.CalculatePrincipal(monthlyPayment, interest);
            remainingBalance = FinancialCalculator.CalculateRemainingBalance(remainingBalance, principal);

            schedule.Add(new PaymentScheduleItemDto
            {
                PaymentNumber = i,
                DueDate = DateTime.UtcNow.AddMonths(i),
                TotalPayment = monthlyPayment,
                Principal = principal,
                Interest = interest,
                RemainingBalance = remainingBalance,
                Status = PaymentStatus.Pending
            });
        }

        return schedule;
    }

    private List<PaymentScheduleItemDto> GenerateDecreasingPaymentSchedule(
        decimal amount,
        decimal constantPrincipal,
        decimal monthlyRate,
        int term)
    {
        var schedule = new List<PaymentScheduleItemDto>();
        var remainingBalance = amount;

        for (int i = 1; i <= term; i++)
        {
            var interest = FinancialCalculator.CalculateInterest(remainingBalance, monthlyRate);
            var totalPayment = FinancialCalculator.CalculateDecreasingPayment(
                constantPrincipal,
                remainingBalance,
                monthlyRate
            );
            remainingBalance = FinancialCalculator.CalculateRemainingBalance(remainingBalance, constantPrincipal);

            schedule.Add(new PaymentScheduleItemDto
            {
                PaymentNumber = i,
                DueDate = DateTime.UtcNow.AddMonths(i),
                TotalPayment = totalPayment,
                Principal = constantPrincipal,
                Interest = interest,
                RemainingBalance = remainingBalance,
                Status = PaymentStatus.Pending
            });
        }

        return schedule;
    }

    private async Task GeneratePaymentScheduleAsync(Loan loan, decimal monthlyRate)
    {
        var paymentSchedules = new List<PaymentSchedule>();
        var remainingBalance = loan.Amount;

        if (loan.LoanType == LoanType.Fixed)
        {
            for (int i = 1; i <= loan.Term; i++)
            {
                var interest = FinancialCalculator.CalculateInterest(remainingBalance, monthlyRate);
                var principal = FinancialCalculator.CalculatePrincipal(loan.MonthlyPayment, interest);
                remainingBalance = FinancialCalculator.CalculateRemainingBalance(remainingBalance, principal);

                paymentSchedules.Add(new PaymentSchedule
                {
                    LoanId = loan.Id,
                    PaymentNumber = i,
                    DueDate = DateTime.UtcNow.AddMonths(i),
                    TotalPayment = loan.MonthlyPayment,
                    Principal = principal,
                    Interest = interest,
                    RemainingBalance = remainingBalance,
                    Status = PaymentStatus.Pending
                });
            }
        }
        else
        {
            var constantPrincipal = FinancialCalculator.CalculateConstantPrincipal(loan.Amount, loan.Term);

            for (int i = 1; i <= loan.Term; i++)
            {
                var interest = FinancialCalculator.CalculateInterest(remainingBalance, monthlyRate);
                var totalPayment = FinancialCalculator.CalculateDecreasingPayment(
                    constantPrincipal,
                    remainingBalance,
                    monthlyRate
                );
                remainingBalance = FinancialCalculator.CalculateRemainingBalance(remainingBalance, constantPrincipal);

                paymentSchedules.Add(new PaymentSchedule
                {
                    LoanId = loan.Id,
                    PaymentNumber = i,
                    DueDate = DateTime.UtcNow.AddMonths(i),
                    TotalPayment = totalPayment,
                    Principal = constantPrincipal,
                    Interest = interest,
                    RemainingBalance = remainingBalance,
                    Status = PaymentStatus.Pending
                });
            }
        }

        await _paymentScheduleRepository.AddRangeAsync(paymentSchedules);
        await _paymentScheduleRepository.SaveChangesAsync();
    }

    private LoanDto MapToDto(Loan loan)
    {
        return new LoanDto
        {
            Id = loan.Id,
            UserId = loan.UserId,
            Amount = loan.Amount,
            Term = loan.Term,
            InterestRate = loan.InterestRate,
            LoanType = loan.LoanType,
            Status = loan.Status,
            MonthlyPayment = loan.MonthlyPayment,
            CreatedAt = loan.CreatedAt,
            UpdatedAt = loan.UpdatedAt
        };
    }
}
