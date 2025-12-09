using System.Threading;
using System.Threading.Tasks;
using Domain.Entities;

namespace Domain.Repositories;

public interface ITransactionRepository
{
    Task AddAsync(Transaction transaction, CancellationToken ct = default);
}
