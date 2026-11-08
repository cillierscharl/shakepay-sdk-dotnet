using ShakePay.Contracts;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ShakePay
{
    public interface IShakePayClient
    {
        Task<List<QuoteResponse>> GetCryptoCurrencyQuotesAsync(bool includeFees = true);
        Task<List<RecentContact>> GetRecentContactsAsync();
        Task<UserSearchResponse> GetUsersByNameAsync(string username);
        Task<ShakingSatsResponse> ShakingSatsAsync();
        Task<List<Wallet>> GetWalletsAsync();
        Task<List<Transaction>> GetTransactionHistoryAsync(DateTime beforeDateTime, string currency = "CAD", int limit = 2000);
        [Obsolete("ShakePay deprecated this endpoint. Use GetTransactionHistoryAsync.")]
        Task<List<Transaction>> GetTransactionsHistoryPagedAsync(int page = 0, int limit = 2000, List<string> currencies = default);
        Task<WaitlistResponse> GetWaitListPositionAsync();
        Task<bool> PostTransactionAsync(string walletId, string username, decimal amount, string note);
    }
}
