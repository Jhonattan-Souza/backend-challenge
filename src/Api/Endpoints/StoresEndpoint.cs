using Application.Features.GetStores.Queries;
using FastEndpoints;

namespace Api.Endpoints;

public class StoresEndpoint : Endpoint<GetStoresRequest, GetStoresResult>
{
    public override void Configure()
    {
        Get("api/v1/stores");
        AllowAnonymous();
        
        Description(b => b
            .WithTags("Stores")
            .Produces<GetStoresResult>(200, "application/json")
            .ProducesProblemDetails(400));
        
        Summary(s =>
        {
            s.Summary = "Get all stores with transactions";
            s.Description = "Returns a paginated list of stores with their transactions and calculated balance. Optionally filter by CPF.";
            s.ExampleRequest = new GetStoresRequest { Page = 1, PageSize = 10, Cpf = "12345678901" };
            s.Responses[200] = "Paginated list of stores with transactions and balance";
            s.Responses[400] = "Invalid request parameters";
        });
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
    /// <summary>Page number (1-indexed)</summary>
    [QueryParam]
    public int Page { get; set; } = 1;
    
    /// <summary>Number of items per page</summary>
    [QueryParam]
    public int PageSize { get; set; } = 10;
    
    /// <summary>Filter transactions by CPF (11 digits, no formatting)</summary>
    [QueryParam]
    public string? Cpf { get; set; }
}

