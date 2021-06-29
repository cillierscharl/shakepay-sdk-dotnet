using ShakePay.Contracts;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ShakePay
{
    public interface IShakePayClient
    {
        Task<List<RecentContact>> GetRecentContactsAsync();
        Task<ShakingSatsResponse> ShakingSatsAsync();
        Task<List<Wallet>> GetWalletsAsync();
        Task<List<Transaction>> GetTransactionHistoryAsync(string currency, int limit);
        Task<List<Transaction>> GetTransactionsHistoryPagedAsync(int page, int limit, List<string> currencies = default);
        Task<WaitlistResponse> GetWaitListPositionAsync();
        Task<bool> PostTransactionAsync(string walletId, string username, decimal amount, string note);
    }
}
