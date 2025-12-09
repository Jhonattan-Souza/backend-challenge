using Application.Features.ProcessCnabFile.Commands;
using FastEndpoints;

namespace Api.Endpoints;

public class CnabTextEndpoint(ILogger<CnabTextEndpoint> logger) : Endpoint<string>
{
    public override void Configure()
    {
        Post("api/v1/cnab-text");
        
        AllowAnonymous();
        
        Throttle(
            hitLimit: 1,
            durationSeconds: 5
        );
    }

    public override async Task HandleAsync(string req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req))
        {
            logger.LogWarning("No content received or content is empty");
            await Send.NoContentAsync(ct);
            return;
        }

        logger.LogInformation("CNAB content received ({Size} characters)", req.Length);
        
        using var reader = new StringReader(req);

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