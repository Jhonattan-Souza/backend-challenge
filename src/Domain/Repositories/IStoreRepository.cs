using System.Threading;
using System.Threading.Tasks;
using Domain.Entities;

namespace Domain.Repositories;

public interface IStoreRepository
{
    Task<Store?> GetByNameAsync(string name, CancellationToken ct = default);
    Task AddAsync(Store store, CancellationToken ct = default);
}
