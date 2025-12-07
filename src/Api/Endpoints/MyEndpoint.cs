using FastEndpoints;

namespace Api.Endpoints;

public class MyEndpoint(ILogger<MyEndpoint> logger) : Endpoint<UploadFileRequest>
{
    public override void Configure()
    {
        Post("api/cnab/upload");
        AllowFileUploads(); 
        AllowAnonymous();
    }

    public override async Task HandleAsync(UploadFileRequest req, CancellationToken ct)
    {
        if (req.File is { Length: > 0 })
        {
            logger.LogInformation("Received file: {FileName} ({Size} bytes)", req.File.FileName, req.File.Length);
        }
        
        await Send.NoContentAsync(ct);
    }
}

public class UploadFileRequest
{
    [Microsoft.AspNetCore.Mvc.FromForm]
    public IFormFile File { get; set; } = null!;
}
