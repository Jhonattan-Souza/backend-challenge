using FastEndpoints;

namespace Application.Features.GetStores.Queries;

public sealed record GetStoresQuery(
    int Page = 1,
    int PageSize = 10,
    string? CpfFilter = null
) : ICommand<GetStoresResult>;