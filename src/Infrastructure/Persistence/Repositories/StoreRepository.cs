using Domain.Entities;
using Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class StoreRepository(AppDbContext context) : IStoreRepository
{
    public async Task<Store?> GetByNameAsync(string name, CancellationToken ct = default) =>
        await context.Stores
            .Include(s => s.Owner)
            .FirstOrDefaultAsync(s => s.Name == name, ct);

    public async Task AddAsync(Store store, CancellationToken ct = default) => 
        await context.Stores.AddAsync(store, ct);
}
