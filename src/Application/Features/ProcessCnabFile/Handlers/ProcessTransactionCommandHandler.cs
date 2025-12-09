using Application.Features.ProcessCnabFile.Commands;
using Domain.Entities;
using Domain.Extensions;
using Domain.Repositories;
using FastEndpoints;
using FluentResults;
using Microsoft.Extensions.Logging;

namespace Application.Features.ProcessCnabFile.Handlers;

public class ProcessTransactionCommandHandler(
    IUnitOfWork unitOfWork,
    IStoreOwnerRepository storeOwnerRepository,
    IStoreRepository storeRepository,
    ITransactionRepository transactionRepository,
    ILogger<ProcessTransactionCommandHandler> logger
) : ICommandHandler<ProcessTransactionCommand, Result>
{
    public async Task<Result> ExecuteAsync(ProcessTransactionCommand command, CancellationToken ct)
    {
        if (await transactionRepository.ExistsByHashAsync(command.LineHash, ct))
        {
            logger.LogWarning(
                "Duplicate transaction detected - Line: {LineNumber} | Hash: {Hash}",
                command.LineNumber,
                command.LineHash);
            return Result.Ok();
        }

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
            var storeOwnerResult = StoreOwner.Create(command.StoreOwnerName, command.Cpf);
            if (storeOwnerResult.IsFailed)
            {
                logger.LogWarning("Failed to create StoreOwner: {Errors}", 
                    string.Join(", ", storeOwnerResult.Errors.Select(e => e.Message)));
                return storeOwnerResult.ToResult();
            }
            storeOwner = storeOwnerResult.Value;
            await storeOwnerRepository.AddAsync(storeOwner, ct);
            logger.LogDebug("Created new StoreOwner: {Name}", command.StoreOwnerName);
        }
        
        var store = await storeRepository.GetByNameAsync(command.StoreName, ct);
        if (store is null)
        {
            var storeResult = Store.Create(command.StoreName, storeOwner);
            if (storeResult.IsFailed)
            {
                logger.LogWarning("Failed to create Store: {Errors}", 
                    string.Join(", ", storeResult.Errors.Select(e => e.Message)));
                return storeResult.ToResult();
            }
            store = storeResult.Value;
            await storeRepository.AddAsync(store, ct);
            logger.LogDebug("Created new Store: {Name}", command.StoreName);
        }
        
        var signedAmount = command.Type.IsExpense() 
            ? -Math.Abs(command.Amount) 
            : Math.Abs(command.Amount);

        var transactionResult = Transaction.Create(
            command.Type,
            command.Date,
            signedAmount,
            command.Cpf,
            command.CardNumber,
            command.LineHash,
            store
        );

        if (transactionResult.IsFailed)
        {
            logger.LogWarning("Failed to create Transaction: {Errors}", 
                string.Join(", ", transactionResult.Errors.Select(e => e.Message)));
            return transactionResult.ToResult();
        }

        var transaction = transactionResult.Value;
        await transactionRepository.AddAsync(transaction, ct);
        await unitOfWork.SaveChangesAsync(ct);

        logger.LogInformation(
            "Transaction persisted successfully - Id: {TransactionId} | Store: {StoreName} | Amount: {Amount:C}",
            transaction.Id,
            command.StoreName,
            signedAmount
        );

        return Result.Ok();
    }
}
