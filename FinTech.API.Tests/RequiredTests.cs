using FinTech.API.DTOs;
using FinTech.API.Models;
using FinTech.API.Models.Enums;
using FinTech.API.Repositories.Interfaces;
using FinTech.API.Services;
using FinTech.API.Utils;
using Moq;
using Xunit;

namespace FinTech.API.Tests.UnitTests;

public class RequiredTests
{
    // Test 1: Cálculo de cuota fija (sistema francés)
    [Fact]
    public async Task SimulateLoan_WithFixedLoanType_ShouldCalculateCorrectMonthlyPayment()
    {
        // Arrange
        var mockLoanRepo = new Mock<ILoanRepository>();
        var mockPaymentScheduleRepo = new Mock<IPaymentScheduleRepository>();
        var mockTransactionRepo = new Mock<ITransactionRepository>();

        var service = new LoanService(mockLoanRepo.Object, mockPaymentScheduleRepo.Object, mockTransactionRepo.Object);

        var simulateDto = new SimulateLoanDto
        {
            Amount = 10000m,
            Term = 12,
            InterestRate = 18.5m,
            LoanType = LoanType.Fixed
        };

        // Act
        var result = await service.SimulateLoanAsync(simulateDto);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.MonthlyPayment > 0);
        Assert.True(result.MonthlyPayment > simulateDto.Amount / simulateDto.Term);
        Assert.InRange(result.MonthlyPayment, 800m, 1000m);
    }

    // Test 2: Generación de cronograma de pagos
    [Fact]
    public async Task SimulateLoan_ShouldGenerateCompletePaymentSchedule()
    {
        // Arrange
        var mockLoanRepo = new Mock<ILoanRepository>();
        var mockPaymentScheduleRepo = new Mock<IPaymentScheduleRepository>();
        var mockTransactionRepo = new Mock<ITransactionRepository>();

        var service = new LoanService(mockLoanRepo.Object, mockPaymentScheduleRepo.Object, mockTransactionRepo.Object);

        var simulateDto = new SimulateLoanDto
        {
            Amount = 5000m,
            Term = 6,
            InterestRate = 15.0m,
            LoanType = LoanType.Fixed
        };

        // Act
        var result = await service.SimulateLoanAsync(simulateDto);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.PaymentSchedule);
        Assert.Equal(simulateDto.Term, result.PaymentSchedule.Count);

        // Verificar que el balance final es cercano a 0
        var lastPayment = result.PaymentSchedule.Last();
        Assert.True(Math.Abs(lastPayment.RemainingBalance) < 1m);
    }

    // Test 3: Validación de monto mínimo/máximo
    [Theory]
    [InlineData(50)]        // Menor al mínimo (100)
    [InlineData(15000000)]  // Mayor al máximo (10000000)
    public void ValidateLoanAmount_WithInvalidAmount_ShouldReturnFalse(decimal amount)
    {
        // Arrange
        decimal minAmount = 100m;
        decimal maxAmount = 10000000m;

        // Act
        var isValid = FinancialCalculator.IsValidLoanAmount(amount, minAmount, maxAmount);

        // Assert
        Assert.False(isValid);
    }

    [Theory]
    [InlineData(100)]      // Mínimo válido
    [InlineData(5000)]     // Valor intermedio
    [InlineData(10000000)] // Máximo válido
    public void ValidateLoanAmount_WithValidAmount_ShouldReturnTrue(decimal amount)
    {
        // Arrange
        decimal minAmount = 100m;
        decimal maxAmount = 10000000m;

        // Act
        var isValid = FinancialCalculator.IsValidLoanAmount(amount, minAmount, maxAmount);

        // Assert
        Assert.True(isValid);
    }

    // Test 4: Validación de plazo
    [Theory]
    [InlineData(0)]    // Menor al mínimo
    [InlineData(500)]  // Mayor al máximo
    public void ValidateLoanTerm_WithInvalidTerm_ShouldReturnFalse(int term)
    {
        // Arrange
        int minTerm = 1;
        int maxTerm = 360;

        // Act
        var isValid = FinancialCalculator.IsValidLoanTerm(term, minTerm, maxTerm);

        // Assert
        Assert.False(isValid);
    }

    [Theory]
    [InlineData(1)]    // Mínimo válido
    [InlineData(12)]   // Valor común
    [InlineData(360)]  // Máximo válido
    public void ValidateLoanTerm_WithValidTerm_ShouldReturnTrue(int term)
    {
        // Arrange
        int minTerm = 1;
        int maxTerm = 360;

        // Act
        var isValid = FinancialCalculator.IsValidLoanTerm(term, minTerm, maxTerm);

        // Assert
        Assert.True(isValid);
    }
}
