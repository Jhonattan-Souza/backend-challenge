using Application.Features.ProcessCnabFile.Commands;
using Application.Features.ProcessCnabFile.Handlers;
using Domain.Entities;
using Domain.Enums;
using Domain.Repositories;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shouldly;

namespace Tests.Unit.Application.Features.ProcessCnabFile.Handlers;

public class ProcessTransactionCommandHandlerTests
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IStoreOwnerRepository _storeOwnerRepository;
    private readonly IStoreRepository _storeRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly ILogger<ProcessTransactionCommandHandler> _logger;
    private readonly ProcessTransactionCommandHandler _sut;

    public ProcessTransactionCommandHandlerTests()
    {
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _storeOwnerRepository = Substitute.For<IStoreOwnerRepository>();
        _storeRepository = Substitute.For<IStoreRepository>();
        _transactionRepository = Substitute.For<ITransactionRepository>();
        _logger = Substitute.For<ILogger<ProcessTransactionCommandHandler>>();

        _sut = new ProcessTransactionCommandHandler(
            _unitOfWork,
            _storeOwnerRepository,
            _storeRepository,
            _transactionRepository,
            _logger);
    }

    private static ProcessTransactionCommand CreateValidCommand(
        TransactionType type = TransactionType.Debit,
        decimal amount = 100m,
        string cpf = "12345678901",
        string ownerName = "João Silva",
        string storeName = "BAR DO JOÃO") =>
        new(
            Type: type,
            Date: DateTimeOffset.UtcNow.AddDays(-1),
            Amount: amount,
            Cpf: cpf,
            CardNumber: "1234****5678",
            StoreName: storeName,
            StoreOwnerName: ownerName,
            LineHash: "UNIQUE_HASH_" + Guid.NewGuid(),
            LineNumber: 1);

    [Fact]
    public async Task ExecuteAsync_DuplicateHash_SkipsAndReturnsSuccess()
    {
        // Arrange
        var command = CreateValidCommand();
        _transactionRepository.ExistsByHashAsync(command.LineHash, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var result = await _sut.ExecuteAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        await _transactionRepository.DidNotReceive().AddAsync(Arg.Any<Transaction>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_NewOwnerAndStore_CreatesEntities()
    {
        // Arrange
        var command = CreateValidCommand();
        _transactionRepository.ExistsByHashAsync(command.LineHash, Arg.Any<CancellationToken>())
            .Returns(false);
        _storeOwnerRepository.GetByCpfAsync(command.Cpf, Arg.Any<CancellationToken>())
            .Returns((StoreOwner?)null);
        _storeRepository.GetByNameAsync(command.StoreName, Arg.Any<CancellationToken>())
            .Returns((Store?)null);

        // Act
        var result = await _sut.ExecuteAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        await _storeOwnerRepository.Received(1).AddAsync(Arg.Any<StoreOwner>(), Arg.Any<CancellationToken>());
        await _storeRepository.Received(1).AddAsync(Arg.Any<Store>(), Arg.Any<CancellationToken>());
        await _transactionRepository.Received(1).AddAsync(Arg.Any<Transaction>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ExistingOwnerAndStore_ReusesEntities()
    {
        // Arrange
        var command = CreateValidCommand();
        var existingOwner = StoreOwner.Create("João Silva", command.Cpf).Value;
        var existingStore = Store.Create(command.StoreName, existingOwner).Value;

        _transactionRepository.ExistsByHashAsync(command.LineHash, Arg.Any<CancellationToken>())
            .Returns(false);
        _storeOwnerRepository.GetByCpfAsync(command.Cpf, Arg.Any<CancellationToken>())
            .Returns(existingOwner);
        _storeRepository.GetByNameAsync(command.StoreName, Arg.Any<CancellationToken>())
            .Returns(existingStore);

        // Act
        var result = await _sut.ExecuteAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        await _storeOwnerRepository.DidNotReceive().AddAsync(Arg.Any<StoreOwner>(), Arg.Any<CancellationToken>());
        await _storeRepository.DidNotReceive().AddAsync(Arg.Any<Store>(), Arg.Any<CancellationToken>());
        await _transactionRepository.Received(1).AddAsync(Arg.Any<Transaction>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(TransactionType.BankSlip)]
    [InlineData(TransactionType.Financing)]
    [InlineData(TransactionType.Rent)]
    public async Task ExecuteAsync_ExpenseType_SetsNegativeAmount(TransactionType expenseType)
    {
        // Arrange
        var command = CreateValidCommand(type: expenseType, amount: 100m);
        var existingOwner = StoreOwner.Create("João Silva", command.Cpf).Value;
        var existingStore = Store.Create(command.StoreName, existingOwner).Value;

        _transactionRepository.ExistsByHashAsync(command.LineHash, Arg.Any<CancellationToken>())
            .Returns(false);
        _storeOwnerRepository.GetByCpfAsync(command.Cpf, Arg.Any<CancellationToken>())
            .Returns(existingOwner);
        _storeRepository.GetByNameAsync(command.StoreName, Arg.Any<CancellationToken>())
            .Returns(existingStore);

        Transaction? capturedTransaction = null;
        await _transactionRepository.AddAsync(
            Arg.Do<Transaction>(t => capturedTransaction = t),
            Arg.Any<CancellationToken>());

        // Act
        var result = await _sut.ExecuteAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        capturedTransaction.ShouldNotBeNull();
        capturedTransaction.Amount.ShouldBe(-100m);
    }

    [Theory]
    [InlineData(TransactionType.Debit)]
    [InlineData(TransactionType.Credit)]
    [InlineData(TransactionType.LoanReceipt)]
    [InlineData(TransactionType.Sales)]
    [InlineData(TransactionType.TedReceipt)]
    [InlineData(TransactionType.DocReceipt)]
    public async Task ExecuteAsync_IncomeType_SetsPositiveAmount(TransactionType incomeType)
    {
        // Arrange
        var command = CreateValidCommand(type: incomeType, amount: 100m);
        var existingOwner = StoreOwner.Create("João Silva", command.Cpf).Value;
        var existingStore = Store.Create(command.StoreName, existingOwner).Value;

        _transactionRepository.ExistsByHashAsync(command.LineHash, Arg.Any<CancellationToken>())
            .Returns(false);
        _storeOwnerRepository.GetByCpfAsync(command.Cpf, Arg.Any<CancellationToken>())
            .Returns(existingOwner);
        _storeRepository.GetByNameAsync(command.StoreName, Arg.Any<CancellationToken>())
            .Returns(existingStore);

        Transaction? capturedTransaction = null;
        await _transactionRepository.AddAsync(
            Arg.Do<Transaction>(t => capturedTransaction = t),
            Arg.Any<CancellationToken>());

        // Act
        var result = await _sut.ExecuteAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        capturedTransaction.ShouldNotBeNull();
        capturedTransaction.Amount.ShouldBe(100m);
    }

    [Fact]
    public async Task ExecuteAsync_InvalidOwnerData_ReturnsFailure()
    {
        // Arrange
        var command = CreateValidCommand(ownerName: new string('A', 20));
        
        _transactionRepository.ExistsByHashAsync(command.LineHash, Arg.Any<CancellationToken>())
            .Returns(false);
        _storeOwnerRepository.GetByCpfAsync(command.Cpf, Arg.Any<CancellationToken>())
            .Returns((StoreOwner?)null);

        // Act
        var result = await _sut.ExecuteAsync(command, CancellationToken.None);

        // Assert
        result.IsFailed.ShouldBeTrue();
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_InvalidStoreData_ReturnsFailure()
    {
        // Arrange 
        var command = CreateValidCommand(storeName: new string('A', 25));
        var existingOwner = StoreOwner.Create("João Silva", command.Cpf).Value;
        
        _transactionRepository.ExistsByHashAsync(command.LineHash, Arg.Any<CancellationToken>())
            .Returns(false);
        _storeOwnerRepository.GetByCpfAsync(command.Cpf, Arg.Any<CancellationToken>())
            .Returns(existingOwner);
        _storeRepository.GetByNameAsync(command.StoreName, Arg.Any<CancellationToken>())
            .Returns((Store?)null);

        // Act
        var result = await _sut.ExecuteAsync(command, CancellationToken.None);

        // Assert
        result.IsFailed.ShouldBeTrue();
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_SuccessfulTransaction_CallsSaveChanges()
    {
        // Arrange
        var command = CreateValidCommand();
        var existingOwner = StoreOwner.Create("João Silva", command.Cpf).Value;
        var existingStore = Store.Create(command.StoreName, existingOwner).Value;

        _transactionRepository.ExistsByHashAsync(command.LineHash, Arg.Any<CancellationToken>())
            .Returns(false);
        _storeOwnerRepository.GetByCpfAsync(command.Cpf, Arg.Any<CancellationToken>())
            .Returns(existingOwner);
        _storeRepository.GetByNameAsync(command.StoreName, Arg.Any<CancellationToken>())
            .Returns(existingStore);

        // Act
        var result = await _sut.ExecuteAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_AlreadyNegativeExpenseAmount_EnsuresNegative()
    {
        // Arrange 
        var command = CreateValidCommand(type: TransactionType.BankSlip, amount: -50m);
        var existingOwner = StoreOwner.Create("João Silva", command.Cpf).Value;
        var existingStore = Store.Create(command.StoreName, existingOwner).Value;

        _transactionRepository.ExistsByHashAsync(command.LineHash, Arg.Any<CancellationToken>())
            .Returns(false);
        _storeOwnerRepository.GetByCpfAsync(command.Cpf, Arg.Any<CancellationToken>())
            .Returns(existingOwner);
        _storeRepository.GetByNameAsync(command.StoreName, Arg.Any<CancellationToken>())
            .Returns(existingStore);

        Transaction? capturedTransaction = null;
        await _transactionRepository.AddAsync(
            Arg.Do<Transaction>(t => capturedTransaction = t),
            Arg.Any<CancellationToken>());

        // Act
        var result = await _sut.ExecuteAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        capturedTransaction.ShouldNotBeNull();
        capturedTransaction.Amount.ShouldBe(-50m);
    }
}
