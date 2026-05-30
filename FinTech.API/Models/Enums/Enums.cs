namespace FinTech.API.Models.Enums;

public enum LoanType
{
    Fixed,
    Decreasing
}

public enum LoanStatus
{
    Pending,
    Approved,
    Rejected,
    Active,
    Completed
}

public enum PaymentStatus
{
    Pending,
    Paid
}

public enum TransactionType
{
    Disbursement,
    Payment,
    Transfer
}

public enum TransactionStatus
{
    Pending,
    Completed,
    Failed
}
