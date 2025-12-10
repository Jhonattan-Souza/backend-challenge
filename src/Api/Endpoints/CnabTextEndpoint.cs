using Application.Features.ProcessCnabFile.Commands;
using FastEndpoints;

namespace Api.Endpoints;

public class CnabTextEndpoint(ILogger<CnabTextEndpoint> logger) : Endpoint<ProcessCnabTextRequest>
{
    public override void Configure()
    {
        Post("api/v1/cnab-text");
        
        AllowAnonymous();
        
        Throttle(
            hitLimit: 1,
            durationSeconds: 5,
            headerName: "X-Client-Id"
        );
        
        Description(b => b
            .WithTags("CNAB Processing")
            .Accepts<ProcessCnabTextRequest>("text/plain")
            .Produces(204)
            .Produces(429));
        
        Summary(s =>
        {
            s.Summary = "Process CNAB text content";
            s.Description = "Processes CNAB transaction data provided as plain text. Each line is parsed and saved to the database. Rate limited to 1 request per 5 seconds per client.";
            s.Responses[204] = "CNAB content processed successfully";
            s.Responses[429] = "Too many requests - rate limit exceeded";
        });
    }

    public override async Task HandleAsync(ProcessCnabTextRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.Content))
        {
            logger.LogWarning("No content received or content is empty");
            await Send.NoContentAsync(ct);
            return;
        }

        logger.LogInformation("CNAB content received ({Size} characters)", req.Content.Length);
        
        using var reader = new StringReader(req.Content);

        var lineNumber = 0;
        while (await reader.ReadLineAsync(ct) is { } line && !ct.IsCancellationRequested)
        {
            lineNumber++;
            await new ProcessCnabLineCommand(line, lineNumber).ExecuteAsync(ct);
        }

        logger.LogInformation("Processing completed. Total lines: {TotalLines}", lineNumber);
        
        await Send.NoContentAsync(ct);
    }
}

public class ProcessCnabTextRequest : IPlainTextRequest
{
    public string Content { get; set; } = string.Empty;
}