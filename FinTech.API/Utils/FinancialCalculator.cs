namespace FinTech.API.Utils;

public static class FinancialCalculator
{
    // Convierte tasa efectiva anual a mensual
    public static decimal ConvertAnnualToMonthlyRate(decimal annualRate)
    {
        if (annualRate < 0)
            throw new ArgumentException("La tasa anual no puede ser negativa", nameof(annualRate));

        // TEM = (1 + TEA)^(1/12) - 1
        var tem = (decimal)(Math.Pow((double)(1 + annualRate / 100), 1.0 / 12.0) - 1);
        return tem;
    }

    // Calcula cuota mensual fija (método francés)
    public static decimal CalculateFixedMonthlyPayment(decimal principal, decimal monthlyRate, int numberOfPayments)
    {
        if (principal <= 0)
            throw new ArgumentException("El monto del préstamo debe ser mayor a cero", nameof(principal));

        if (numberOfPayments <= 0)
            throw new ArgumentException("El número de pagos debe ser mayor a cero", nameof(numberOfPayments));

        if (monthlyRate < 0)
            throw new ArgumentException("La tasa mensual no puede ser negativa", nameof(monthlyRate));

        if (monthlyRate == 0)
            return principal / numberOfPayments;

        var rate = (double)monthlyRate;
        var n = numberOfPayments;
        var p = (double)principal;

        var numerator = rate * Math.Pow(1 + rate, n);
        var denominator = Math.Pow(1 + rate, n) - 1;

        var monthlyPayment = (decimal)(p * (numerator / denominator));

        return Math.Round(monthlyPayment, 2);
    }

    public static decimal CalculateInterest(decimal remainingBalance, decimal monthlyRate)
    {
        if (remainingBalance < 0)
            throw new ArgumentException("El saldo no puede ser negativo", nameof(remainingBalance));

        if (monthlyRate < 0)
            throw new ArgumentException("La tasa mensual no puede ser negativa", nameof(monthlyRate));

        return Math.Round(remainingBalance * monthlyRate, 2);
    }

    public static decimal CalculatePrincipal(decimal monthlyPayment, decimal interest)
    {
        return Math.Round(monthlyPayment - interest, 2);
    }

    // Para el sistema alemán (cuota decreciente)
    public static decimal CalculateConstantPrincipal(decimal principal, int numberOfPayments)
    {
        if (principal <= 0)
            throw new ArgumentException("El monto del préstamo debe ser mayor a cero", nameof(principal));

        if (numberOfPayments <= 0)
            throw new ArgumentException("El número de pagos debe ser mayor a cero", nameof(numberOfPayments));

        return Math.Round(principal / numberOfPayments, 2);
    }

    public static decimal CalculateDecreasingPayment(decimal constantPrincipal, decimal remainingBalance, decimal monthlyRate)
    {
        var interest = CalculateInterest(remainingBalance, monthlyRate);
        return Math.Round(constantPrincipal + interest, 2);
    }

    public static decimal CalculateTotalInterest(decimal totalPayments, decimal principal)
    {
        return Math.Round(totalPayments - principal, 2);
    }

    public static decimal CalculateRemainingBalance(decimal currentBalance, decimal principalPaid)
    {
        var newBalance = currentBalance - principalPaid;
        return Math.Round(Math.Max(newBalance, 0), 2);
    }

    // Validaciones
    public static bool IsValidLoanAmount(decimal amount, decimal minAmount, decimal maxAmount)
    {
        return amount >= minAmount && amount <= maxAmount;
    }

    public static bool IsValidLoanTerm(int term, int minTerm, int maxTerm)
    {
        return term >= minTerm && term <= maxTerm;
    }

    public static bool IsValidInterestRate(decimal interestRate, decimal minRate, decimal maxRate)
    {
        return interestRate >= minRate && interestRate <= maxRate;
    }

    public static decimal CalculateTotalCost(decimal principal, decimal monthlyPayment, int numberOfPayments)
    {
        var totalPayments = monthlyPayment * numberOfPayments;
        return Math.Round(totalPayments, 2);
    }

    public static decimal ConvertMonthlyToAnnualRate(decimal monthlyRate)
    {
        if (monthlyRate < 0)
            throw new ArgumentException("La tasa mensual no puede ser negativa", nameof(monthlyRate));

        // TEA = [(1 + TEM)^12 - 1] * 100
        var tea = (decimal)(Math.Pow((double)(1 + monthlyRate), 12.0) - 1) * 100;
        return Math.Round(tea, 2);
    }

    public static decimal CalculateInterestPercentage(decimal totalInterest, decimal principal)
    {
        if (principal == 0)
            return 0;

        return Math.Round((totalInterest / principal) * 100, 2);
    }

    public static int CalculatePaymentsCompleted(decimal principal, decimal remainingBalance, decimal constantPrincipal)
    {
        if (constantPrincipal == 0)
            return 0;

        var amountPaid = principal - remainingBalance;
        return (int)(amountPaid / constantPrincipal);
    }

    public static decimal CalculateLoanProgress(decimal principal, decimal remainingBalance)
    {
        if (principal == 0)
            return 100;

        var amountPaid = principal - remainingBalance;
        return Math.Round((amountPaid / principal) * 100, 2);
    }
}
