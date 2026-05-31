using FinTech.API.Data;
using FinTech.API.DTOs;
using FinTech.API.Models;
using FinTech.API.Models.Enums;
using FinTech.API.Repositories.Implementations;
using FinTech.API.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FinTech.API.Tests.IntegrationTests;

public class RequiredIntegrationTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly TransactionService _transactionService;
    private readonly LoanService _loanService;
    private readonly LoanRepository _loanRepository;
    private readonly TransactionRepository _transactionRepository;
    private readonly PaymentScheduleRepository _paymentScheduleRepository;

    public RequiredIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);

        _loanRepository = new LoanRepository(_context);
        _transactionRepository = new TransactionRepository(_context);
        _paymentScheduleRepository = new PaymentScheduleRepository(_context);

        _transactionService = new TransactionService(
            _transactionRepository,
            _loanRepository,
            _paymentScheduleRepository
        );

        _loanService = new LoanService(
            _loanRepository,
            _paymentScheduleRepository,
            _transactionRepository
        );
    }

    // Test 5: Deduplicación de transacciones con mismo IdempotencyKey
    [Fact]
    public async Task CreateTransaction_WithDuplicateIdempotencyKey_ShouldReturnExistingTransaction()
    {
        // Arrange - Crear un préstamo de prueba
        var loan = new Loan
        {
            UserId = "test-user",
            Amount = 10000m,
            Term = 12,
            InterestRate = 18.5m,
            LoanType = LoanType.Fixed,
            Status = LoanStatus.Active,
            MonthlyPayment = 888.49m,
            CreatedAt = DateTime.UtcNow
        };

        await _loanRepository.AddAsync(loan);
        await _loanRepository.SaveChangesAsync();

        var idempotencyKey = "test-payment-12345";

        var firstTransactionDto = new CreateTransactionDto
        {
            IdempotencyKey = idempotencyKey,
            Type = TransactionType.Payment,
            Amount = 888.49m,
            LoanId = loan.Id,
            Description = "Primer intento de pago"
        };

        // Act - Crear la primera transacción
        var firstResult = await _transactionService.CreateTransactionAsync(firstTransactionDto);

        // Intentar crear la misma transacción con el mismo IdempotencyKey
        var secondTransactionDto = new CreateTransactionDto
        {
            IdempotencyKey = idempotencyKey,
            Type = TransactionType.Payment,
            Amount = 888.49m,
            LoanId = loan.Id,
            Description = "Segundo intento de pago (duplicado)"
        };

        var secondResult = await _transactionService.CreateTransactionAsync(secondTransactionDto);

        // Assert
        Assert.NotNull(firstResult);
        Assert.NotNull(secondResult);

        // Debe devolver la misma transacción (mismo ID)
        Assert.Equal(firstResult!.Id, secondResult!.Id);
        Assert.Equal(firstResult.IdempotencyKey, secondResult.IdempotencyKey);

        // Solo debe existir UNA transacción en la base de datos
        var allTransactions = await _transactionRepository.GetAllAsync();
        Assert.Single(allTransactions);

        // Verificar que la descripción es de la primera transacción
        Assert.Equal("Primer intento de pago", firstResult.Description);
    }

    // Test adicional: Crear préstamo con aprobación automática
    [Fact]
    public async Task CreateLoan_WithAmountLessThan10k_AndLessThan2PreviousLoans_ShouldAutoApprove()
    {
        // Arrange
        var createLoanDto = new CreateLoanDto
        {
            UserId = "test-user-new",
            Amount = 5000m,
            Term = 12,
            InterestRate = 15.0m,
            LoanType = LoanType.Fixed
        };

        // Act
        var result = await _loanService.CreateLoanAsync(createLoanDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(LoanStatus.Approved, result.Status);
        Assert.True(result.MonthlyPayment > 0);
    }

    // Test adicional: Obtener todos los préstamos de un usuario
    [Fact]
    public async Task GetAllLoans_ByUserId_ShouldReturnUserLoans()
    {
        // Arrange
        var userId = "test-user-multiple";

        var loan1 = new Loan
        {
            UserId = userId,
            Amount = 5000m,
            Term = 12,
            InterestRate = 15.0m,
            LoanType = LoanType.Fixed,
            Status = LoanStatus.Active,
            MonthlyPayment = 450m,
            CreatedAt = DateTime.UtcNow
        };

        var loan2 = new Loan
        {
            UserId = userId,
            Amount = 8000m,
            Term = 24,
            InterestRate = 18.0m,
            LoanType = LoanType.Fixed,
            Status = LoanStatus.Active,
            MonthlyPayment = 390m,
            CreatedAt = DateTime.UtcNow.AddMonths(-1)
        };

        await _loanRepository.AddAsync(loan1);
        await _loanRepository.AddAsync(loan2);
        await _loanRepository.SaveChangesAsync();

        // Act
        var result = await _loanService.GetAllLoansAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.All(result, loan => Assert.Equal(userId, loan.UserId));
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
