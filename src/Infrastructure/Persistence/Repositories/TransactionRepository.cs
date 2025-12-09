using Domain.Entities;
using Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class TransactionRepository(AppDbContext context) : ITransactionRepository
{
    public async Task AddAsync(Transaction transaction, CancellationToken ct = default) => 
        await context.Transactions.AddAsync(transaction, ct);

    public async Task<bool> ExistsByHashAsync(string lineHash, CancellationToken ct = default) =>
        await context.Transactions.AnyAsync(t => t.LineHash == lineHash, ct);
}
