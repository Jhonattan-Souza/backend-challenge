using Domain.Enums;

namespace Application.Models;

public sealed record CnabLineResult(
    TransactionType Type,
    DateTimeOffset Date,
    decimal Amount,
    string Cpf,
    string CardNumber,
    string StoreName,
    string StoreOwnerName
);