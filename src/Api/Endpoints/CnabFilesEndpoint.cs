using Application.Features.ProcessCnabFile.Commands;
using FastEndpoints;

namespace Api.Endpoints;

public class CnabFilesEndpoint(
    ILogger<CnabFilesEndpoint> logger,
    IConfiguration configuration) : Endpoint<UploadFileRequest>
{
    private const int DefaultMaxFileSizeMb = 5;

    public override void Configure()
    {
        Post("api/v1/cnab-files");
        
        AllowFileUploads(); 
        AllowAnonymous();
        
        Throttle(
            hitLimit: 1,
            durationSeconds: 5,
            headerName: "X-Client-Id"
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

        var maxFileSizeMb = configuration.GetValue("FileUpload:MaxFileSizeMB", DefaultMaxFileSizeMb);
        var maxFileSizeBytes = maxFileSizeMb * 1024 * 1024;

        if (req.File.Length > maxFileSizeBytes)
        {
            logger.LogWarning("File too large: {FileName} ({Size} bytes). Max allowed: {MaxSize} bytes",
                req.File.FileName, req.File.Length, maxFileSizeBytes);
            
            await Send.StringAsync(
                $"{{\"error\":\"File size exceeds the maximum allowed size of {maxFileSizeMb}MB\"}}",
                statusCode: StatusCodes.Status413PayloadTooLarge,
                contentType: "application/json",
                cancellation: ct);
            return;
        }

        logger.LogInformation("File received: {FileName} ({Size} bytes)", req.File.FileName, req.File.Length);
        
        await using var stream = req.File.OpenReadStream();
        using var reader = new StreamReader(stream);

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

public class UploadFileRequest
{
    [Microsoft.AspNetCore.Mvc.FromForm]
    public IFormFile File { get; set; } = null!;
}

