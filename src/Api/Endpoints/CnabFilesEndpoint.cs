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
            logger.LogWarning("Nenhum arquivo recebido ou arquivo vazio");
            await Send.NoContentAsync(ct);
            return;
        }

        logger.LogInformation("Arquivo recebido: {FileName} ({Size} bytes)", req.File.FileName, req.File.Length);
        await using var stream = req.File.OpenReadStream();
        using var reader = new StreamReader(stream);

        var lineNumber = 0;
        while (!reader.EndOfStream && !ct.IsCancellationRequested)
        {
            var line = await reader.ReadLineAsync(ct);
            lineNumber++;
            
            logger.LogInformation("Linha {LineNumber}: {Content}", lineNumber, line);
        }

        logger.LogInformation("Processamento concluído. Total de linhas: {TotalLines}", lineNumber);
        
        await Send.NoContentAsync(ct);
    }
}

public class UploadFileRequest
{
    [Microsoft.AspNetCore.Mvc.FromForm]
    public IFormFile File { get; set; } = null!;
}
