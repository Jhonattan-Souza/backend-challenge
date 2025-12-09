using Domain.Entities;
using Domain.Repositories;

namespace Infrastructure.Persistence.Repositories;

public class TransactionRepository(AppDbContext context) : ITransactionRepository
{
    public async Task AddAsync(Transaction transaction, CancellationToken ct = default) => 
        await context.Transactions.AddAsync(transaction, ct);
}
