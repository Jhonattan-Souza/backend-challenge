namespace Application.Features.GetStores.DTOs;

public sealed record StoreDto(
    Guid Id,
    string Name,
    string OwnerName,
    decimal Balance,
    List<TransactionDto> Transactions
);