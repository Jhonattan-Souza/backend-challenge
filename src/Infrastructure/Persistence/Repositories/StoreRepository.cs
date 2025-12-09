using Domain.Common;
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

    public async Task<IReadOnlyList<Store>> GetAllWithTransactionsAsync(CancellationToken ct = default) =>
        await context.Stores
            .Include(s => s.Owner)
            .Include(s => s.Transactions)
            .AsNoTracking()
            .ToListAsync(ct);

    public async Task<PagedResult<Store>> GetPagedAsync(
        int page, 
        int pageSize, 
        string? cpfFilter = null, 
        CancellationToken ct = default)
    {
        var query = context.Stores
            .Include(s => s.Owner)
            .Include(s => s.Transactions)
            .AsNoTracking();
        
        if (!string.IsNullOrWhiteSpace(cpfFilter))
        {
            var normalizedCpf = cpfFilter.Replace(".", "").Replace("-", "");
            query = query.Where(s => s.Transactions.Any(t => t.Cpf == normalizedCpf));
        }

        var totalItems = await query.CountAsync(ct);

        var items = await query
            .OrderBy(s => s.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsSplitQuery()
            .ToListAsync(ct);

        return PagedResult<Store>.Create(items, page, pageSize, totalItems);
    }
}
