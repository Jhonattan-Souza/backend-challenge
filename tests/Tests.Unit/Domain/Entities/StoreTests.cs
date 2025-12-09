using Domain.Entities;
using Shouldly;

namespace Tests.Unit.Domain.Entities;

public class StoreTests
{
    private const string ValidStoreName = "BAR DO JOÃO";
    private const string ValidOwnerName = "João Silva";
    private const string ValidCpf = "12345678901";

    private static StoreOwner CreateValidOwner() => 
        StoreOwner.Create(ValidOwnerName, ValidCpf).Value;

    [Fact]
    public void Create_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var owner = CreateValidOwner();

        // Act
        var result = Store.Create(ValidStoreName, owner);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Name.ShouldBe(ValidStoreName);
        result.Value.Owner.ShouldBe(owner);
        result.Value.OwnerId.ShouldBe(owner.Id);
        result.Value.Id.ShouldNotBe(Guid.Empty);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithEmptyOrNullName_ReturnsFailed(string? name)
    {
        // Arrange
        var owner = CreateValidOwner();

        // Act
        var result = Store.Create(name!, owner);

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.ShouldContain(e => e.Message.Contains("name", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Create_WithNameTooLong_ReturnsFailed()
    {
        // Arrange 
        var owner = CreateValidOwner();
        var longName = new string('A', 20);

        // Act
        var result = Store.Create(longName, owner);

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.ShouldContain(e => e.Message.Contains("19", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Create_WithNullOwner_ThrowsNullReferenceException()
    {
        // Act & Assert
        Should.Throw<NullReferenceException>(() => Store.Create(ValidStoreName, null!));
    }

    [Fact]
    public void Create_InitializesTransactionsCollection()
    {
        // Arrange
        var owner = CreateValidOwner();

        // Act
        var result = Store.Create(ValidStoreName, owner);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Transactions.ShouldNotBeNull();
        result.Value.Transactions.ShouldBeEmpty();
    }

    [Fact]
    public void Create_WithMaxLengthName_ReturnsSuccess()
    {
        // Arrange
        var owner = CreateValidOwner();
        var exactLengthName = new string('A', 19);

        // Act
        var result = Store.Create(exactLengthName, owner);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Name.ShouldBe(exactLengthName);
    }
}
