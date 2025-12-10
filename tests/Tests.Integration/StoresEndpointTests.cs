using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using Application.Features.GetStores.Queries;
using Shouldly;

namespace Tests.Integration;

[Collection("Integration")]
public class StoresEndpointTests(IntegrationTestFixture fixture)
{
    private readonly IntegrationTestFixture _fixture = fixture;
    private HttpClient CreateClient() => _fixture.CreateClientWithUniqueId();

    [Fact]
    public async Task Get_NoData_ReturnsEmptyList()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();

        // Act
        var response = await CreateClient().GetAsync("/api/v1/stores");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<GetStoresResult>();
        
        result.ShouldNotBeNull();
        result.Stores.ShouldBeEmpty();
        result.Page.ShouldBe(1);
        result.PageSize.ShouldBe(10);
        result.TotalItems.ShouldBe(0);
        result.TotalPages.ShouldBe(0);
        result.HasPreviousPage.ShouldBeFalse();
        result.HasNextPage.ShouldBeFalse();
    }

    [Fact]
    public async Task Get_WithData_ReturnsStoresWithAllProperties()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();
        await SeedDataAsync();

        // Act
        var response = await CreateClient().GetAsync("/api/v1/stores");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<GetStoresResult>();
        
        result.ShouldNotBeNull();
        result.Stores.ShouldNotBeEmpty();
        
        var firstStore = result.Stores.First();
        firstStore.Id.ShouldNotBe(Guid.Empty);
        firstStore.Name.ShouldNotBeNullOrEmpty();
        firstStore.OwnerName.ShouldNotBeNullOrEmpty();
        firstStore.Transactions.ShouldNotBeEmpty();
        
        var firstTransaction = firstStore.Transactions.First();
        firstTransaction.Type.ShouldNotBeNullOrEmpty();
        firstTransaction.Cpf.Length.ShouldBe(11);
        firstTransaction.CardNumber.ShouldNotBeNullOrEmpty();
        firstTransaction.Sign.ShouldBeOneOf("+", "-");
    }

    [Fact]
    public async Task Get_Pagination_ReturnsCorrectPage()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();
        await SeedDataAsync();

        // Act
        var response = await CreateClient().GetAsync("/api/v1/stores?page=1&pageSize=1");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<GetStoresResult>();
        
        result.ShouldNotBeNull();
        result.Page.ShouldBe(1);
        result.PageSize.ShouldBe(1);
        result.Stores.Count.ShouldBe(1);
        result.TotalItems.ShouldBeGreaterThan(0);
        result.TotalPages.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task Get_WithCpfFilter_ReturnsFilteredResults()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();
        await SeedDataAsync();
        var cpf = "09620676017";

        // Act
        var response = await CreateClient().GetAsync($"/api/v1/stores?cpf={cpf}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<GetStoresResult>();
        
        result.ShouldNotBeNull();
        result.Stores.ShouldNotBeEmpty();
        
        var store = result.Stores.First();
        store.OwnerName.ShouldBe("JOÃO MACEDO");
        store.Name.ShouldBe("BAR DO JOÃO");
        store.Transactions.ShouldAllBe(t => t.Cpf == cpf);
    }

    [Fact]
    public async Task Get_WithNonExistentCpfFilter_ReturnsEmptyList()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();
        await SeedDataAsync();

        // Act
        var response = await CreateClient().GetAsync("/api/v1/stores?cpf=00000000000");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<GetStoresResult>();
        
        result.ShouldNotBeNull();
        result.Stores.ShouldBeEmpty();
        result.TotalItems.ShouldBe(0);
    }

    [Fact]
    public async Task Get_PaginationMetadata_HasCorrectValues()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();
        await SeedDataAsync();

        // Act
        var response = await CreateClient().GetAsync("/api/v1/stores?page=1&pageSize=2");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<GetStoresResult>();
        
        result.ShouldNotBeNull();
        result.Page.ShouldBe(1);
        result.PageSize.ShouldBe(2);
        result.TotalItems.ShouldBeGreaterThan(0);
        result.TotalPages.ShouldBeGreaterThan(0);
        result.HasPreviousPage.ShouldBeFalse(); 
        result.HasNextPage.ShouldBeTrue();
    }

    private async Task SeedDataAsync()
    {
        var cnabContent = "3201903010000014200096206760174753****3153153453JOÃO MACEDO   BAR DO JOÃO       \n5201903010000013200556418150633123****7687145607MARIA JOSEFINALOJA DO Ó - MATRIZ\n3201903010000012200845152540736777****1313172712MARCOS PEREIRAMERCADO DA AVENIDA";
        
        var fileContent = new ByteArrayContent(Encoding.UTF8.GetBytes(cnabContent));
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("text/plain");
        
        using var formContent = new MultipartFormDataContent();
        formContent.Add(fileContent, "file", "seed.txt");
        
        await CreateClient().PostAsync("/api/v1/cnab-files", formContent);
    }
}
