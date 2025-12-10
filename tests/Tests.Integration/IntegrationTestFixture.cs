using Api;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.MsSql;

namespace Tests.Integration;

public class IntegrationTestFixture : IAsyncLifetime
{
    private MsSqlContainer _msSqlContainer = null!;
    public HttpClient Client { get; private set; } = null!;
    private WebApplicationFactory<IApiMarker> _factory = null!;

    public async Task InitializeAsync()
    {
        _msSqlContainer = new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
            .WithPassword("YourStrong@Passw0rd123!")
            .Build();

        await _msSqlContainer.StartAsync();

        _factory = new WebApplicationFactory<IApiMarker>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                    
                    if (descriptor != null) 
                        services.Remove(descriptor);
                    
                    services.AddDbContext<AppDbContext>(options =>
                    {
                        options.UseSqlServer(_msSqlContainer.GetConnectionString());
                    });
                    
                    var sp = services.BuildServiceProvider();
                    using var scope = sp.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    db.Database.EnsureCreated();
                });
                
                builder.UseEnvironment("Testing");
            });

        Client = _factory.CreateClient();
        Client.DefaultRequestHeaders.Add("X-Client-Id", Guid.NewGuid().ToString());
    }
    
    public HttpClient CreateClientWithUniqueId()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Client-Id", Guid.NewGuid().ToString());
        return client;
    }

    public async Task DisposeAsync()
    {
        Client.Dispose();
        await _factory.DisposeAsync();
        await _msSqlContainer.DisposeAsync();
    }

    public async Task ResetDatabaseAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        db.Transactions.RemoveRange(db.Transactions);
        db.Stores.RemoveRange(db.Stores);
        db.StoreOwners.RemoveRange(db.StoreOwners);
        await db.SaveChangesAsync();
    }
}

[CollectionDefinition("Integration")]
public class IntegrationTestCollection : ICollectionFixture<IntegrationTestFixture>;
