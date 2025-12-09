using Application.Features.GetStores.Handlers;
using Application.Features.GetStores.Queries;
using Domain.Common;
using Domain.Entities;
using Domain.Enums;
using Domain.Repositories;
using NSubstitute;
using Shouldly;

namespace Tests.Unit.Application.Features.GetStores.Handlers;

public class GetStoresQueryHandlerTests
{
    private readonly IStoreRepository _storeRepository;
    private readonly GetStoresQueryHandler _sut;

    public GetStoresQueryHandlerTests()
    {
        _storeRepository = Substitute.For<IStoreRepository>();
        _sut = new GetStoresQueryHandler(_storeRepository);
    }

    private static Store CreateStoreWithTransactions(
        string storeName,
        string ownerName,
        string ownerCpf,
        params (TransactionType type, decimal amount)[] transactions)
    {
        var owner = StoreOwner.Create(ownerName, ownerCpf).Value;
        var store = Store.Create(storeName, owner).Value;

        foreach (var (type, amount) in transactions)
        {
            var transaction = Transaction.Create(
                type,
                DateTimeOffset.UtcNow.AddDays(-1),
                amount,
                ownerCpf,
                "1234****5678",
                Guid.NewGuid().ToString(),
                store).Value;
            
            store.Transactions.Add(transaction);
        }

        return store;
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsCorrectPaginationInfo()
    {
        // Arrange
        var stores = new List<Store>();
        var pagedResult = PagedResult<Store>.Create(stores, 2, 10, 50);
        
        _storeRepository.GetPagedAsync(2, 10, null, Arg.Any<CancellationToken>())
            .Returns(pagedResult);

        var query = new GetStoresQuery(Page: 2, PageSize: 10);

        // Act
        var result = await _sut.ExecuteAsync(query, CancellationToken.None);

        // Assert
        result.Page.ShouldBe(2);
        result.PageSize.ShouldBe(10);
        result.TotalItems.ShouldBe(50);
        result.TotalPages.ShouldBe(5);
        result.HasPreviousPage.ShouldBeTrue();
        result.HasNextPage.ShouldBeTrue();
    }

    [Fact]
    public async Task ExecuteAsync_MapsStoreToDto()
    {
        // Arrange
        var store = CreateStoreWithTransactions(
            "BAR DO JOÃO",
            "João Silva",
            "12345678901");

        var pagedResult = PagedResult<Store>.Create(
            new List<Store> { store }, 1, 10, 1);

        _storeRepository.GetPagedAsync(1, 10, null, Arg.Any<CancellationToken>())
            .Returns(pagedResult);

        var query = new GetStoresQuery();

        // Act
        var result = await _sut.ExecuteAsync(query, CancellationToken.None);

        // Assert
        result.Stores.Count.ShouldBe(1);
        result.Stores[0].Id.ShouldBe(store.Id);
        result.Stores[0].Name.ShouldBe("BAR DO JOÃO");
        result.Stores[0].OwnerName.ShouldBe("João Silva");
    }

    [Fact]
    public async Task ExecuteAsync_CalculatesBalanceFromTransactions()
    {
        var store = CreateStoreWithTransactions(
            "BAR DO JOÃO",
            "João Silva",
            "12345678901",
            (TransactionType.Debit, 200m),
            (TransactionType.Credit, 100m),
            (TransactionType.BankSlip, -50m));

        var pagedResult = PagedResult<Store>.Create(
            new List<Store> { store }, 1, 10, 1);

        _storeRepository.GetPagedAsync(1, 10, null, Arg.Any<CancellationToken>())
            .Returns(pagedResult);

        var query = new GetStoresQuery();

        // Act
        var result = await _sut.ExecuteAsync(query, CancellationToken.None);

        // Assert
        result.Stores[0].Balance.ShouldBe(250m);
    }

    [Fact]
    public async Task ExecuteAsync_MapsTransactionsWithCorrectSign()
    {
        // Arrange
        var store = CreateStoreWithTransactions(
            "BAR DO JOÃO",
            "João Silva",
            "12345678901",
            (TransactionType.Debit, 100m));

        var pagedResult = PagedResult<Store>.Create(
            new List<Store> { store }, 1, 10, 1);

        _storeRepository.GetPagedAsync(1, 10, null, Arg.Any<CancellationToken>())
            .Returns(pagedResult);

        var query = new GetStoresQuery();

        // Act
        var result = await _sut.ExecuteAsync(query, CancellationToken.None);

        // Assert
        result.Stores[0].Transactions.Count.ShouldBe(1);
        result.Stores[0].Transactions[0].Type.ShouldBe("Debit");
        result.Stores[0].Transactions[0].Amount.ShouldBe(100m);
    }

    [Theory]
    [InlineData(TransactionType.BankSlip, "-")]
    [InlineData(TransactionType.Financing, "-")]
    [InlineData(TransactionType.Rent, "-")]
    public async Task ExecuteAsync_ExpenseTransaction_ReturnsMinusSign(
        TransactionType expenseType, 
        string expectedSign)
    {
        // Arrange
        var store = CreateStoreWithTransactions(
            "BAR DO JOÃO",
            "João Silva",
            "12345678901",
            (expenseType, -100m));

        var pagedResult = PagedResult<Store>.Create(
            new List<Store> { store }, 1, 10, 1);

        _storeRepository.GetPagedAsync(1, 10, null, Arg.Any<CancellationToken>())
            .Returns(pagedResult);

        var query = new GetStoresQuery();

        // Act
        var result = await _sut.ExecuteAsync(query, CancellationToken.None);

        // Assert
        result.Stores[0].Transactions[0].Sign.ShouldBe(expectedSign);
    }

    [Theory]
    [InlineData(TransactionType.Debit, "+")]
    [InlineData(TransactionType.Credit, "+")]
    [InlineData(TransactionType.LoanReceipt, "+")]
    [InlineData(TransactionType.Sales, "+")]
    [InlineData(TransactionType.TedReceipt, "+")]
    [InlineData(TransactionType.DocReceipt, "+")]
    public async Task ExecuteAsync_IncomeTransaction_ReturnsPlusSign(
        TransactionType incomeType, 
        string expectedSign)
    {
        // Arrange
        var store = CreateStoreWithTransactions(
            "BAR DO JOÃO",
            "João Silva",
            "12345678901",
            (incomeType, 100m));

        var pagedResult = PagedResult<Store>.Create(
            new List<Store> { store }, 1, 10, 1);

        _storeRepository.GetPagedAsync(1, 10, null, Arg.Any<CancellationToken>())
            .Returns(pagedResult);

        var query = new GetStoresQuery();

        // Act
        var result = await _sut.ExecuteAsync(query, CancellationToken.None);

        // Assert
        result.Stores[0].Transactions[0].Sign.ShouldBe(expectedSign);
    }

    [Fact]
    public async Task ExecuteAsync_WithCpfFilter_PassesFilterToRepository()
    {
        // Arrange
        var stores = new List<Store>();
        var pagedResult = PagedResult<Store>.Create(stores, 1, 10, 0);
        const string cpfFilter = "12345678901";

        _storeRepository.GetPagedAsync(1, 10, cpfFilter, Arg.Any<CancellationToken>())
            .Returns(pagedResult);

        var query = new GetStoresQuery(CpfFilter: cpfFilter);

        // Act
        await _sut.ExecuteAsync(query, CancellationToken.None);

        // Assert
        await _storeRepository.Received(1).GetPagedAsync(1, 10, cpfFilter, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_EmptyStores_ReturnsEmptyList()
    {
        // Arrange
        var stores = new List<Store>();
        var pagedResult = PagedResult<Store>.Create(stores, 1, 10, 0);

        _storeRepository.GetPagedAsync(1, 10, null, Arg.Any<CancellationToken>())
            .Returns(pagedResult);

        var query = new GetStoresQuery();

        // Act
        var result = await _sut.ExecuteAsync(query, CancellationToken.None);

        // Assert
        result.Stores.ShouldBeEmpty();
        result.TotalItems.ShouldBe(0);
    }

    [Fact]
    public async Task ExecuteAsync_MultipleStores_MapsAllCorrectly()
    {
        // Arrange
        var store1 = CreateStoreWithTransactions("STORE 1", "Owner 1", "11111111111", (TransactionType.Debit, 100m));
        var store2 = CreateStoreWithTransactions("STORE 2", "Owner 2", "22222222222", (TransactionType.Credit, 200m));

        var pagedResult = PagedResult<Store>.Create(
            new List<Store> { store1, store2 }, 1, 10, 2);

        _storeRepository.GetPagedAsync(1, 10, null, Arg.Any<CancellationToken>())
            .Returns(pagedResult);

        var query = new GetStoresQuery();

        // Act
        var result = await _sut.ExecuteAsync(query, CancellationToken.None);

        // Assert
        result.Stores.Count.ShouldBe(2);
        result.Stores[0].Name.ShouldBe("STORE 1");
        result.Stores[0].Balance.ShouldBe(100m);
        result.Stores[1].Name.ShouldBe("STORE 2");
        result.Stores[1].Balance.ShouldBe(200m);
    }
}
