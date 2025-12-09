using Domain.Entities;
using Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class StoreOwnerRepository(AppDbContext context) : IStoreOwnerRepository
{
    public async Task<StoreOwner?> GetByCpfAsync(string cpf, CancellationToken ct = default) =>
        await context.StoreOwners
            .FirstOrDefaultAsync(o => o.Cpf == cpf, ct);

    public async Task AddAsync(StoreOwner storeOwner, CancellationToken ct = default) => 
        await context.StoreOwners.AddAsync(storeOwner, ct);
}
