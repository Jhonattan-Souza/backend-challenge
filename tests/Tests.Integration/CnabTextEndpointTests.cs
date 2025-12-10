using System.Net;
using System.Text;
using Shouldly;

namespace Tests.Integration;

[Collection("Integration")]
public class CnabTextEndpointTests(IntegrationTestFixture fixture)
{
    private readonly IntegrationTestFixture _fixture = fixture;
    private HttpClient CreateClient() => _fixture.CreateClientWithUniqueId();

    [Fact]
    public async Task Post_ValidCnabText_ReturnsNoContent()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();
        var cnabContent = "3201903010000014200096206760174753****3153153453JOÃO MACEDO   BAR DO JOÃO       ";
        var content = new StringContent(cnabContent, Encoding.UTF8, "text/plain");

        // Act
        var response = await CreateClient().PostAsync("/api/v1/cnab-text", content);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Post_EmptyText_ReturnsNoContent()
    {
        // Arrange
        var content = new StringContent("", Encoding.UTF8, "text/plain");

        // Act
        var response = await CreateClient().PostAsync("/api/v1/cnab-text", content);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Post_MultipleCnabLines_ProcessesAllLines()
    {
        // Arrange
        var cnabContent = "3201903010000014200096206760174753****3153153453JOÃO MACEDO   BAR DO JOÃO       \n5201903010000013200556418150633123****7687145607MARIA JOSEFINALOJA DO Ó - MATRIZ";
        var content = new StringContent(cnabContent, Encoding.UTF8, "text/plain");

        // Act
        var response = await CreateClient().PostAsync("/api/v1/cnab-text", content);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }
}
