namespace Application.Features.GetStores.DTOs;

public sealed record TransactionDto(
    string Type,
    DateTimeOffset Date,
    decimal Amount,
    string Sign,
    string Cpf,
    string CardNumber
);