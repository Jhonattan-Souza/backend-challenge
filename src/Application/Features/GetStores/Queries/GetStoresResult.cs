using Application.Features.GetStores.DTOs;

namespace Application.Features.GetStores.Queries;

public sealed record GetStoresResult(
    List<StoreDto> Stores,
    int Page,
    int PageSize,
    int TotalItems,
    int TotalPages,
    bool HasPreviousPage,
    bool HasNextPage
);