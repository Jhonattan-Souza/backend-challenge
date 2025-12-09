using Application.Features.GetStores.Queries;
using Domain.Enums;
using Domain.Extensions;
using Domain.Repositories;
using FastEndpoints;

namespace Application.Features.GetStores.Handlers;

public class GetStoresQueryHandler(IStoreRepository storeRepository) 
    : ICommandHandler<GetStoresQuery, GetStoresResult>
{
    public async Task<GetStoresResult> ExecuteAsync(GetStoresQuery command, CancellationToken ct)
    {
        var pagedResult = await storeRepository.GetPagedAsync(
            command.Page, 
            command.PageSize, 
            command.CpfFilter, 
            ct);
        
        var storeItems = pagedResult.Items.Select(store =>
        {
            var transactions = store.Transactions.Select(t => new TransactionDto(
                t.Type.ToString(),
                t.Date,
                t.Amount,
                GetTransactionSign(t.Type),
                t.Cpf,
                t.CardNumber
            )).ToList();

            var balance = store.Transactions.Sum(t => 
                t.Type.IsExpense() ? -t.Amount : t.Amount);

            return new StoreDto(
                store.Id,
                store.Name,
                store.Owner.Name,
                balance,
                transactions
            );
        }).ToList();

        return new GetStoresResult(
            storeItems,
            pagedResult.Page,
            pagedResult.PageSize,
            pagedResult.TotalItems,
            pagedResult.TotalPages,
            pagedResult.HasPreviousPage,
            pagedResult.HasNextPage
        );
    }
    
    private static string GetTransactionSign(TransactionType type) => type.IsExpense() ? "-" : "+";
}
