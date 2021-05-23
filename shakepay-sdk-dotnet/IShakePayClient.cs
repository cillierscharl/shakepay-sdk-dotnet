using ShakePay.Contracts;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ShakePay
{
    public interface IShakePayClient
    {
        Task<List<Wallet>> GetWalletsAsync();
        Task<List<Transaction>> GetTransactionHistory(string currency, int limit);

        Task<List<Transaction>> GetTransactionsHistoryPagedAsync(int page, int limit);
        Task<bool> PostTransactionAsync(string walletId, string username, decimal amount, string note);
    }
}
