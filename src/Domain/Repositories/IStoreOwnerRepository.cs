using System.Threading;
using System.Threading.Tasks;
using Domain.Entities;

namespace Domain.Repositories;

public interface IStoreOwnerRepository
{
    Task<StoreOwner?> GetByCpfAsync(string cpf, CancellationToken ct = default);
    Task AddAsync(StoreOwner storeOwner, CancellationToken ct = default);
}
