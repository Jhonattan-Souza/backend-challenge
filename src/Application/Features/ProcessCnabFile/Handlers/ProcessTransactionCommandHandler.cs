using Application.Features.ProcessCnabFile.Commands;
using FastEndpoints;
using Microsoft.Extensions.Logging;

namespace Application.Features.ProcessCnabFile.Handlers;

public class ProcessTransactionCommandHandler(
    ILogger<ProcessTransactionCommandHandler> logger
) : ICommandHandler<ProcessTransactionCommand>
{
    public Task ExecuteAsync(ProcessTransactionCommand command, CancellationToken ct)
    {
        logger.LogInformation(
            "Processing transaction - Line: {LineNumber} | Type: {Type} | Amount: {Amount:C} | Store: {StoreName} | Owner: {StoreOwnerName}",
            command.LineNumber,
            command.Type,
            command.Amount,
            command.StoreName,
            command.StoreOwnerName
        );

        // TODO: Implement database persistence

        return Task.CompletedTask;
    }
}
