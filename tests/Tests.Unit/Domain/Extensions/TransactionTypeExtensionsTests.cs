using Domain.Enums;
using Domain.Extensions;
using Shouldly;

namespace Tests.Unit.Domain.Extensions;

public class TransactionTypeExtensionsTests
{
    [Theory]
    [InlineData(TransactionType.BankSlip)]
    [InlineData(TransactionType.Financing)]
    [InlineData(TransactionType.Rent)]
    public void IsExpense_ExpenseTypes_ReturnsTrue(TransactionType type)
    {
        // Act
        var result = type.IsExpense();

        // Assert
        result.ShouldBeTrue($"{type} should be classified as expense");
    }

    [Theory]
    [InlineData(TransactionType.Debit)]
    [InlineData(TransactionType.Credit)]
    [InlineData(TransactionType.LoanReceipt)]
    [InlineData(TransactionType.Sales)]
    [InlineData(TransactionType.TedReceipt)]
    [InlineData(TransactionType.DocReceipt)]
    public void IsExpense_IncomeTypes_ReturnsFalse(TransactionType type)
    {
        // Act
        var result = type.IsExpense();

        // Assert
        result.ShouldBeFalse($"{type} should NOT be classified as expense");
    }
}
