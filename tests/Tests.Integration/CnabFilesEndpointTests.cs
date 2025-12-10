using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using Application.Features.GetStores.Queries;
using Shouldly;

namespace Tests.Integration;

[Collection("Integration")]
public class CnabFilesEndpointTests(IntegrationTestFixture fixture)
{
    private readonly IntegrationTestFixture _fixture = fixture;
    private HttpClient CreateClient() => _fixture.CreateClientWithUniqueId();

    [Fact]
    public async Task Upload_ValidCnabFile_ReturnsNoContent()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();
        var cnabContent = "3201903010000014200096206760174753****3153153453JOÃO MACEDO   BAR DO JOÃO       ";
        var fileContent = new ByteArrayContent(Encoding.UTF8.GetBytes(cnabContent));
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("text/plain");
        
        using var formContent = new MultipartFormDataContent();
        formContent.Add(fileContent, "file", "sample.txt");

        // Act
        var response = await CreateClient().PostAsync("/api/v1/cnab-files", formContent);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Upload_EmptyFile_ReturnsNoContent()
    {
        // Arrange
        var fileContent = new ByteArrayContent([]);
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("text/plain");
        
        using var formContent = new MultipartFormDataContent();
        formContent.Add(fileContent, "file", "empty.txt");

        // Act
        var response = await CreateClient().PostAsync("/api/v1/cnab-files", formContent);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Upload_FileTooLarge_ReturnsPayloadTooLarge()
    {
        // Arrange
        var largeContent = new byte[5 * 1024 * 1024 + 1];
        var fileContent = new ByteArrayContent(largeContent);
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("text/plain");
        
        using var formContent = new MultipartFormDataContent();
        formContent.Add(fileContent, "file", "large.txt");

        // Act
        var response = await CreateClient().PostAsync("/api/v1/cnab-files", formContent);

        // Assert
        response.StatusCode.ShouldBe((HttpStatusCode)413);
    }

    [Fact]
    public async Task Upload_MultipleCnabLines_ProcessesAllLines()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();
        var cnabContent = "3201903010000014200096206760174753****3153153453JOÃO MACEDO   BAR DO JOÃO       \n5201903010000013200556418150633123****7687145607MARIA JOSEFINALOJA DO Ó - MATRIZ\n3201903010000012200845152540736777****1313172712MARCOS PEREIRAMERCADO DA AVENIDA";
        
        var fileContent = new ByteArrayContent(Encoding.UTF8.GetBytes(cnabContent));
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("text/plain");
        
        using var formContent = new MultipartFormDataContent();
        formContent.Add(fileContent, "file", "multiple.txt");

        // Act
        var response = await CreateClient().PostAsync("/api/v1/cnab-files", formContent);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        
        var storesResponse = await CreateClient().GetAsync("/api/v1/stores");
        storesResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        
        var result = await storesResponse.Content.ReadFromJsonAsync<GetStoresResult>();
        result.ShouldNotBeNull();
        result.Stores.Count.ShouldBe(3);
        result.Stores.ShouldContain(s => s.Name == "BAR DO JOÃO");
        result.Stores.ShouldContain(s => s.Name == "LOJA DO Ó - MATRIZ");
        result.Stores.ShouldContain(s => s.Name == "MERCADO DA AVENIDA");
    }
}
