using Application.Features.ProcessCnabFile.Commands;
using Application.Services;
using FastEndpoints;
using Microsoft.Extensions.Logging;

namespace Application.Features.ProcessCnabFile.Handlers;

public class ProcessCnabLineCommandHandler(
    ICnabParser cnabParser,
    ILogger<ProcessCnabLineCommandHandler> logger
) : ICommandHandler<ProcessCnabLineCommand>
{
    public async Task ExecuteAsync(ProcessCnabLineCommand command, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(command.Line))
        {
            logger.LogWarning("Line {LineNumber} is empty, skipping...", command.LineNumber);
            return;
        }

        try
        {
            var result = cnabParser.ParseLine(command.Line);

            var transactionCommand = new ProcessTransactionCommand(
                Type: result.Type,
                Date: result.Date,
                Amount: result.Amount,
                Cpf: result.Cpf,
                CardNumber: result.CardNumber,
                StoreName: result.StoreName,
                StoreOwnerName: result.StoreOwnerName,
                LineNumber: command.LineNumber
            );

            await transactionCommand.ExecuteAsync(ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing line {LineNumber}: {Content}", command.LineNumber, command.Line);
        }
    }
}
