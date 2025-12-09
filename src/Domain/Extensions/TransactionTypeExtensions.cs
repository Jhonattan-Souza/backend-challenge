using Domain.Enums;

namespace Domain.Extensions;

public static class TransactionTypeExtensions
{
    public static bool IsExpense(this TransactionType type) =>
        type is TransactionType.BankSlip or TransactionType.Financing or TransactionType.Rent;
}
