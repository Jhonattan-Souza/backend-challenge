using Domain.Entities;
using Domain.Enums;
using Shouldly;

namespace Tests.Unit.Domain.Entities;

public class TransactionTests
{
    private const string ValidCpf = "12345678901";
    private const string ValidCardNumber = "1234****5678";
    private const string ValidLineHash = "ABCDEF123456";

    private static Store CreateValidStore()
    {
        var owner = StoreOwner.Create("João Silva", ValidCpf).Value;
        return Store.Create("BAR DO JOÃO", owner).Value;
    }

    [Fact]
    public void Create_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var store = CreateValidStore();
        var date = DateTimeOffset.UtcNow.AddDays(-1);
        const decimal amount = 100.50m;

        // Act
        var result = Transaction.Create(
            TransactionType.Debit,
            date,
            amount,
            ValidCpf,
            ValidCardNumber,
            ValidLineHash,
            store);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Type.ShouldBe(TransactionType.Debit);
        result.Value.Date.ShouldBe(date);
        result.Value.Amount.ShouldBe(amount);
        result.Value.Cpf.ShouldBe(ValidCpf);
        result.Value.CardNumber.ShouldBe(ValidCardNumber);
        result.Value.LineHash.ShouldBe(ValidLineHash);
        result.Value.Store.ShouldBe(store);
        result.Value.StoreId.ShouldBe(store.Id);
        result.Value.Id.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public void Create_WithZeroAmount_ReturnsFailed()
    {
        // Arrange
        var store = CreateValidStore();

        // Act
        var result = Transaction.Create(
            TransactionType.Debit,
            DateTimeOffset.UtcNow.AddDays(-1),
            0m,
            ValidCpf,
            ValidCardNumber,
            ValidLineHash,
            store);

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.ShouldContain(e => e.Message.Contains("zero", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Create_WithFutureDate_ReturnsFailed()
    {
        // Arrange
        var store = CreateValidStore();
        var futureDate = DateTimeOffset.UtcNow.AddDays(10);

        // Act
        var result = Transaction.Create(
            TransactionType.Debit,
            futureDate,
            100m,
            ValidCpf,
            ValidCardNumber,
            ValidLineHash,
            store);

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.ShouldContain(e => e.Message.Contains("future", StringComparison.OrdinalIgnoreCase));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithEmptyOrNullCpf_ReturnsFailed(string? cpf)
    {
        // Arrange
        var store = CreateValidStore();

        // Act
        var result = Transaction.Create(
            TransactionType.Debit,
            DateTimeOffset.UtcNow.AddDays(-1),
            100m,
            cpf!,
            ValidCardNumber,
            ValidLineHash,
            store);

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.ShouldContain(e => e.Message.Contains("CPF", StringComparison.OrdinalIgnoreCase));
    }

    [Theory]
    [InlineData("1234567890")]     
    [InlineData("123456789012")]   
    public void Create_WithCpfWrongLength_ReturnsFailed(string cpf)
    {
        // Arrange
        var store = CreateValidStore();

        // Act
        var result = Transaction.Create(
            TransactionType.Debit,
            DateTimeOffset.UtcNow.AddDays(-1),
            100m,
            cpf,
            ValidCardNumber,
            ValidLineHash,
            store);

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.ShouldContain(e => e.Message.Contains("11", StringComparison.OrdinalIgnoreCase));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithEmptyOrNullCardNumber_ReturnsFailed(string? cardNumber)
    {
        // Arrange
        var store = CreateValidStore();

        // Act
        var result = Transaction.Create(
            TransactionType.Debit,
            DateTimeOffset.UtcNow.AddDays(-1),
            100m,
            ValidCpf,
            cardNumber!,
            ValidLineHash,
            store);

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.ShouldContain(e => e.Message.Contains("card", StringComparison.OrdinalIgnoreCase));
    }

    [Theory]
    [InlineData("12345678901")]   
    [InlineData("1234567890123")] 
    public void Create_WithCardNumberWrongLength_ReturnsFailed(string cardNumber)
    {
        // Arrange
        var store = CreateValidStore();

        // Act
        var result = Transaction.Create(
            TransactionType.Debit,
            DateTimeOffset.UtcNow.AddDays(-1),
            100m,
            ValidCpf,
            cardNumber,
            ValidLineHash,
            store);

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.ShouldContain(e => e.Message.Contains("12", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Create_WithNullStore_ThrowsNullReferenceException()
    {
        // Act & Assert
        Should.Throw<NullReferenceException>(() => Transaction.Create(
            TransactionType.Debit,
            DateTimeOffset.UtcNow.AddDays(-1),
            100m,
            ValidCpf,
            ValidCardNumber,
            ValidLineHash,
            null!));
    }

    [Theory]
    [InlineData(TransactionType.Debit)]
    [InlineData(TransactionType.BankSlip)]
    [InlineData(TransactionType.Financing)]
    [InlineData(TransactionType.Credit)]
    [InlineData(TransactionType.LoanReceipt)]
    [InlineData(TransactionType.Sales)]
    [InlineData(TransactionType.TedReceipt)]
    [InlineData(TransactionType.DocReceipt)]
    [InlineData(TransactionType.Rent)]
    public void Create_WithAllValidTransactionTypes_ReturnsSuccess(TransactionType type)
    {
        // Arrange
        var store = CreateValidStore();

        // Act
        var result = Transaction.Create(
            type,
            DateTimeOffset.UtcNow.AddDays(-1),
            100m,
            ValidCpf,
            ValidCardNumber,
            ValidLineHash,
            store);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Type.ShouldBe(type);
    }

    [Fact]
    public void Create_WithNegativeAmount_ReturnsSuccess()
    {
        var store = CreateValidStore();

        // Act
        var result = Transaction.Create(
            TransactionType.BankSlip,
            DateTimeOffset.UtcNow.AddDays(-1),
            -100m,
            ValidCpf,
            ValidCardNumber,
            ValidLineHash,
            store);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Amount.ShouldBe(-100m);
    }
}
