using Domain.Entities;
using Shouldly;

namespace Tests.Unit.Domain.Entities;

public class StoreOwnerTests
{
    private const string ValidName = "João Silva";
    private const string ValidCpf = "12345678901";

    [Fact]
    public void Create_WithValidData_ReturnsSuccess()
    {
        // Act
        var result = StoreOwner.Create(ValidName, ValidCpf);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Name.ShouldBe(ValidName);
        result.Value.Cpf.ShouldBe(ValidCpf);
        result.Value.Id.ShouldNotBe(Guid.Empty);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithEmptyOrNullName_ReturnsFailed(string? name)
    {
        // Act
        var result = StoreOwner.Create(name!, ValidCpf);

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.ShouldContain(e => e.Message.Contains("name", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Create_WithNameTooLong_ReturnsFailed()
    {
        // Arrange 
        var longName = new string('A', 15);

        // Act
        var result = StoreOwner.Create(longName, ValidCpf);

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.ShouldContain(e => e.Message.Contains("14", StringComparison.OrdinalIgnoreCase));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithEmptyOrNullCpf_ReturnsFailed(string? cpf)
    {
        // Act
        var result = StoreOwner.Create(ValidName, cpf!);

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.ShouldContain(e => e.Message.Contains("CPF", StringComparison.OrdinalIgnoreCase));
    }

    [Theory]
    [InlineData("1234567890")]   
    [InlineData("123456789012")] 
    public void Create_WithCpfWrongLength_ReturnsFailed(string cpf)
    {
        // Act
        var result = StoreOwner.Create(ValidName, cpf);

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.ShouldContain(e => e.Message.Contains("11", StringComparison.OrdinalIgnoreCase));
    }

    [Theory]
    [InlineData("1234567890A")]    
    [InlineData("123.456.789")]    
    [InlineData("123-456-789")]    
    public void Create_WithNonNumericCpf_ReturnsFailed(string cpf)
    {
        // Act
        var result = StoreOwner.Create(ValidName, cpf);

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.ShouldContain(e => e.Message.Contains("numbers", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Create_InitializesStoresCollection()
    {
        // Act
        var result = StoreOwner.Create(ValidName, ValidCpf);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Stores.ShouldNotBeNull();
        result.Value.Stores.ShouldBeEmpty();
    }
}
