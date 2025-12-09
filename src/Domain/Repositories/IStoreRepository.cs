using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Domain.Common;
using Domain.Entities;

namespace Domain.Repositories;

public interface IStoreRepository
{
    Task<Store?> GetByNameAsync(string name, CancellationToken ct = default);
    Task AddAsync(Store store, CancellationToken ct = default);
    Task<IReadOnlyList<Store>> GetAllWithTransactionsAsync(CancellationToken ct = default);
    Task<PagedResult<Store>> GetPagedAsync(int page, int pageSize, string? cpfFilter = null, CancellationToken ct = default);
}
