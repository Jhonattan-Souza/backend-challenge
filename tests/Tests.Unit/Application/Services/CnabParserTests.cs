using Application.Services;
using Domain.Enums;
using Shouldly;

namespace Tests.Unit.Application.Services;

public class CnabParserTests
{
    private readonly CnabParser _sut = new();
    
    private const string SampleLine = "3201903010000014200096206760174753****3153153453JOÃO MACEDO   BAR DO JOÃO       ";

    [Fact]
    public void ParseLine_SampleFileLine_ParsesCorrectly()
    {
        // Act
        var result = _sut.ParseLine(SampleLine);

        // Assert
        result.ShouldNotBeNull();
        result.Type.ShouldBe(TransactionType.Financing);
        result.Amount.ShouldBe(142.00m);
        result.Cpf.ShouldBe("09620676017");
        result.CardNumber.ShouldBe("4753****3153");
        result.StoreOwnerName.ShouldBe("JOÃO MACEDO");
        result.StoreName.ShouldBe("BAR DO JOÃO");
    }

    [Fact]
    public void ParseLine_ValidLine_ReturnsCorrectTransactionType()
    {
        const string debitLine = "1201903010000015200096206760171234****7890233000JOÃO MACEDO   BAR DO JOÃO       ";

        // Act
        var result = _sut.ParseLine(debitLine);

        // Assert
        result.Type.ShouldBe(TransactionType.Debit);
    }

    [Theory]
    [InlineData("1201903010000015200096206760171234****7890233000JOÃO MACEDO   BAR DO JOÃO       ", TransactionType.Debit)]
    [InlineData("2201903010000011200096206760173648****0099234234JOÃO MACEDO   BAR DO JOÃO       ", TransactionType.BankSlip)]
    [InlineData("3201903010000014200096206760174753****3153153453JOÃO MACEDO   BAR DO JOÃO       ", TransactionType.Financing)]
    [InlineData("4201903010000015232556418150631234****6678100000MARIA JOSEFINALOJA DO Ó - FILIAL", TransactionType.Credit)]
    [InlineData("5201903010000013200556418150633123****7687145607MARIA JOSEFINALOJA DO Ó - MATRIZ", TransactionType.LoanReceipt)]
    [InlineData("6201903010000013200556418150633123****7687145607MARIA JOSEFINALOJA DO Ó - MATRIZ", TransactionType.Sales)]
    [InlineData("7201903010000013200556418150633123****7687145607MARIA JOSEFINALOJA DO Ó - MATRIZ", TransactionType.TedReceipt)]
    [InlineData("8201903010000010203845152540732344****1222123222MARCOS PEREIRAMERCADO DA AVENIDA", TransactionType.DocReceipt)]
    [InlineData("9201903010000010200556418150636228****9090000000MARIA JOSEFINALOJA DO Ó - MATRIZ", TransactionType.Rent)]
    public void ParseLine_AllTransactionTypes_ParsesCorrectType(string line, TransactionType expectedType)
    {
        // Act
        var result = _sut.ParseLine(line);

        // Assert
        result.Type.ShouldBe(expectedType);
    }

    [Fact]
    public void ParseLine_ValidLine_ReturnsCorrectDate()
    { 
        // Act
        var result = _sut.ParseLine(SampleLine);

        // Assert
        result.Date.Year.ShouldBe(2019);
        result.Date.Month.ShouldBe(3);
        result.Date.Day.ShouldBe(1);
        result.Date.Hour.ShouldBe(15);
        result.Date.Minute.ShouldBe(34);
        result.Date.Second.ShouldBe(53);
    }

    [Fact]
    public void ParseLine_ValidLine_ReturnsCorrectAmount()
    {
        // Act
        var result = _sut.ParseLine(SampleLine);

        // Assert
        result.Amount.ShouldBe(142.00m);
    }

    [Fact]
    public void ParseLine_AmountWithCents_ParsesCorrectly()
    {
        const string lineWithCents = "1201903010000001050096206760171234****7890233000JOÃO MACEDO   BAR DO JOÃO       ";

        // Act
        var result = _sut.ParseLine(lineWithCents);

        // Assert
        result.Amount.ShouldBe(10.50m);
    }

    [Fact]
    public void ParseLine_ValidLine_ReturnsCorrectCpf()
    {
        // Act
        var result = _sut.ParseLine(SampleLine);

        // Assert
        result.Cpf.ShouldBe("09620676017");
        result.Cpf.Length.ShouldBe(11);
    }

    [Fact]
    public void ParseLine_ValidLine_ReturnsCorrectCardNumber()
    {
        // Act
        var result = _sut.ParseLine(SampleLine);

        // Assert
        result.CardNumber.ShouldBe("4753****3153");
    }

    [Fact]
    public void ParseLine_ValidLine_ReturnsCorrectStoreName()
    {
        // Act
        var result = _sut.ParseLine(SampleLine);

        // Assert
        result.StoreName.ShouldBe("BAR DO JOÃO");
    }

    [Fact]
    public void ParseLine_ValidLine_ReturnsCorrectStoreOwnerName()
    {
        // Act
        var result = _sut.ParseLine(SampleLine);

        // Assert
        result.StoreOwnerName.ShouldBe("JOÃO MACEDO");
    }

    [Fact]
    public void ParseLine_DifferentStore_ParsesCorrectly()
    {
        // Arrange
        const string line = "5201903010000013200556418150633123****7687145607MARIA JOSEFINALOJA DO Ó - MATRIZ";

        // Act
        var result = _sut.ParseLine(line);

        // Assert
        result.Type.ShouldBe(TransactionType.LoanReceipt);
        result.Amount.ShouldBe(132.00m);
        result.Cpf.ShouldBe("55641815063");
        result.CardNumber.ShouldBe("3123****7687");
        result.StoreOwnerName.ShouldBe("MARIA JOSEFINA");
        result.StoreName.ShouldBe("LOJA DO Ó - MATRIZ");
    }

    [Fact]
    public void ParseLine_LargeAmount_ParsesCorrectly()
    {
        const string line = "3201903010000610200232702980566777****1313172712JOSÉ COSTA    MERCEARIA 3 IRMÃOS";

        // Act
        var result = _sut.ParseLine(line);

        // Assert
        result.Amount.ShouldBe(6102.00m);
    }

    [Fact]
    public void ParseLine_SmallAmount_ParsesCorrectly()
    {
        const string line = "2201903010000000500232702980567677****8778141808JOSÉ COSTA    MERCEARIA 3 IRMÃOS";

        // Act
        var result = _sut.ParseLine(line);

        // Assert
        result.Amount.ShouldBe(5.00m);
    }
}
