using Application.Features.ProcessCnabFile.Commands;
using FastEndpoints;

namespace Api.Endpoints;

public class CnabFilesEndpoint(ILogger<CnabFilesEndpoint> logger) : Endpoint<UploadFileRequest>
{
    public override void Configure()
    {
        Post("api/v1/cnab-files");
        
        AllowFileUploads(); 
        AllowAnonymous();
        
        Throttle(
            hitLimit: 1,
            durationSeconds: 5
        );
    }

    public override async Task HandleAsync(UploadFileRequest req, CancellationToken ct)
    {
        if (req.File is not { Length: > 0 })
        {
            logger.LogWarning("No file received or file is empty");
            await Send.NoContentAsync(ct);
            return;
        }

        logger.LogInformation("File received: {FileName} ({Size} bytes)", req.File.FileName, req.File.Length);
        
        await using var stream = req.File.OpenReadStream();
        using var reader = new StreamReader(stream);

        var lineNumber = 0;
        string? line;
        while ((line = await reader.ReadLineAsync(ct)) is not null && !ct.IsCancellationRequested)
        {
            lineNumber++;
            await new ProcessCnabLineCommand(line, lineNumber).ExecuteAsync(ct);
        }

        logger.LogInformation("Processing completed. Total lines: {TotalLines}", lineNumber);
        
        await Send.NoContentAsync(ct);
    }
}

public class UploadFileRequest
{
    [Microsoft.AspNetCore.Mvc.FromForm]
    public IFormFile File { get; set; } = null!;
}
