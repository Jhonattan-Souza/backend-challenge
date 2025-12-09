using Application.Features.GetStores.Queries;
using FastEndpoints;

namespace Api.Endpoints;

public class StoresEndpoint : Endpoint<GetStoresRequest, GetStoresResult>
{
    public override void Configure()
    {
        Get("api/v1/stores");
        AllowAnonymous();
    }

    public override async Task HandleAsync(GetStoresRequest req, CancellationToken ct)
    {
        var query = new GetStoresQuery(req.Page, req.PageSize, req.Cpf);
        var result = await query.ExecuteAsync(ct);
        await Send.OkAsync(result, ct);
    }
}

public class GetStoresRequest
{
    [QueryParam]
    public int Page { get; set; } = 1;
    
    [QueryParam]
    public int PageSize { get; set; } = 10;
    
    [QueryParam]
    public string? Cpf { get; set; }
}
