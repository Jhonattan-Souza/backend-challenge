using Domain.Enums;
using FastEndpoints;
using FluentResults;

namespace Application.Features.ProcessCnabFile.Commands;

public sealed record ProcessTransactionCommand(
    TransactionType Type,
    DateTimeOffset Date,
    decimal Amount,
    string Cpf,
    string CardNumber,
    string StoreName,
    string StoreOwnerName,
    int LineNumber
) : ICommand<Result>;

