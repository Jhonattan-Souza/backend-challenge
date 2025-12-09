using FastEndpoints;

namespace Application.Features.GetStores.Queries;

public sealed record GetStoresQuery(
    int Page = 1,
    int PageSize = 10,
    string? CpfFilter = null
) : ICommand<GetStoresResult>;

public sealed record GetStoresResult(
    List<StoreDto> Stores,
    int Page,
    int PageSize,
    int TotalItems,
    int TotalPages,
    bool HasPreviousPage,
    bool HasNextPage
);

public sealed record StoreDto(
    Guid Id,
    string Name,
    string OwnerName,
    decimal Balance,
    List<TransactionDto> Transactions
);

public sealed record TransactionDto(
    string Type,
    DateTimeOffset Date,
    decimal Amount,
    string Sign,
    string Cpf,
    string CardNumber
);
