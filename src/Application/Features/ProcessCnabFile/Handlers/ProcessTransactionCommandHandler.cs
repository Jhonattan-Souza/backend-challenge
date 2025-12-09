using Application.Features.ProcessCnabFile.Commands;
using Domain.Entities;
using Domain.Repositories;
using FastEndpoints;
using Microsoft.Extensions.Logging;

namespace Application.Features.ProcessCnabFile.Handlers;

public class ProcessTransactionCommandHandler(
    IUnitOfWork unitOfWork,
    IStoreOwnerRepository storeOwnerRepository,
    IStoreRepository storeRepository,
    ITransactionRepository transactionRepository,
    ILogger<ProcessTransactionCommandHandler> logger
) : ICommandHandler<ProcessTransactionCommand>
{
    public async Task ExecuteAsync(ProcessTransactionCommand command, CancellationToken ct)
    {
        logger.LogInformation(
            "Processing transaction - Line: {LineNumber} | Type: {Type} | Amount: {Amount:C} | Store: {StoreName} | Owner: {StoreOwnerName}",
            command.LineNumber,
            command.Type,
            command.Amount,
            command.StoreName,
            command.StoreOwnerName
        );
        
        var storeOwner = await storeOwnerRepository.GetByCpfAsync(command.Cpf, ct);
        if (storeOwner is null)
        {
            storeOwner = StoreOwner.Create(command.StoreOwnerName, command.Cpf);
            await storeOwnerRepository.AddAsync(storeOwner, ct);
            logger.LogDebug("Created new StoreOwner: {Name}", command.StoreOwnerName);
        }
        
        var store = await storeRepository.GetByNameAsync(command.StoreName, ct);
        if (store is null)
        {
            store = Store.Create(command.StoreName, storeOwner);
            await storeRepository.AddAsync(store, ct);
            logger.LogDebug("Created new Store: {Name}", command.StoreName);
        }
        
        var transaction = Transaction.Create(
            command.Type,
            command.Date,
            command.Amount,
            command.Cpf,
            command.CardNumber,
            store
        );

        await transactionRepository.AddAsync(transaction, ct);
        await unitOfWork.SaveChangesAsync(ct);

        logger.LogInformation(
            "Transaction persisted successfully - Id: {TransactionId} | Store: {StoreName}",
            transaction.Id,
            command.StoreName
        );
    }
}
