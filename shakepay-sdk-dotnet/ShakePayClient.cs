using Newtonsoft.Json;
using ShakePay.Contracts;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ShakePay
{
    public class ShakePayClient : IShakePayClient
    {
        private readonly bool _initialized;
        private readonly HttpClient _httpClient;
        private static readonly string _baseUrl = "https://api.shakepay.com";
        private string _jwt;
        public ShakePayClient(string jwt, HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.DefaultRequestHeaders.Add("Authorization", jwt);
            _initialized = true;
            _jwt = jwt;
            PeriodicallyRefreshToken();
        }

        public async Task<List<Wallet>> GetWalletsAsync()
        {
            if (!_initialized)
            {
                throw new Exception("ShakePay client not initialized");
            }

            var walletsHttpResponse = await _httpClient.GetAsync($"{_baseUrl}/wallets");
            if (walletsHttpResponse.IsSuccessStatusCode)
            {
                var walletsResponse = JsonConvert.DeserializeObject<WalletsResponse>(await walletsHttpResponse.Content.ReadAsStringAsync());
                return walletsResponse.Wallets;
            }

            return null;
        }

        public async Task<List<Transaction>> GetTransactionsHistoryPagedAsync(int page = 0, int limit = 20)
        {
            if (!_initialized)
            {
                throw new Exception("ShakePay client not initialized");
            }

            var requestBody = new TransactionHistoryRequest()
            {
                Pagination = new TransactionHistoryPagination()
                {
                    Descending = true,
                    Page = page.ToString(),
                    RowsPerPage = limit
                },
                FilterParams = new object()
            };

            var transactionsHistoryHttpResponse = await _httpClient.PostAsync(
                $"{_baseUrl}/transactions/history",
                new StringContent(JsonConvert.SerializeObject(requestBody),
                Encoding.UTF8,
                "application/json"));

            if (transactionsHistoryHttpResponse.IsSuccessStatusCode)
            {
                var trasnactionHistoryResponseString = await transactionsHistoryHttpResponse.Content.ReadAsStringAsync();
                var transactionHistoryResponse = JsonConvert.DeserializeObject<PagedTransactionHistoryResponse>(trasnactionHistoryResponseString);
                return transactionHistoryResponse.Transactions;
            }

            return null;
        }

        public async Task<List<Transaction>> GetTransactionHistory(string currency = "CAD", int limit = 10)
        {
            if (!_initialized)
            {
                throw new Exception("ShakePay client not initialized");
            }

            var beforeDateTime = DateTime.UtcNow.ToString("O");
            var transactionsHistoryResponse = await _httpClient.GetAsync($"{_baseUrl}/transactions/history?currency={currency}&before={beforeDateTime}&limit={limit}");

            if (transactionsHistoryResponse.IsSuccessStatusCode)
            {
                var response = await transactionsHistoryResponse.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<Transaction>>(response);
            }

            return null;
        }

        public async Task<bool> PostTransactionAsync(string walletId, string username, decimal amount, string note)
        {
            var requestBody = new TransactionRequest()
            {
                Amount = amount.ToString(),
                FromWallet = walletId,
                Note = note,
                To = username,
                ToType = "user"
            };

            var postTransactionResponse = await _httpClient.PostAsync(
                $"{_baseUrl}/transactions",
                new StringContent(JsonConvert.SerializeObject(requestBody),
                Encoding.UTF8,
                "application/json"));

            return postTransactionResponse.IsSuccessStatusCode;
        }

        private void PeriodicallyRefreshToken()
        {
            var t = new Task(async () =>
            {
                while (true)
                {
                    await Task.Delay((int)TimeSpan.FromMinutes(1).TotalMilliseconds);
                    await RenewTokenAsync();
                }
            });

            t.Start();
        }

        private async Task RenewTokenAsync()
        {
            var newTokenHttpResponse = await _httpClient.PostAsync($"{_baseUrl}/authentication", new StringContent(JsonConvert.SerializeObject(new RenewAuthenticationTokenRequest()
            {
                Strategy = "jwt",
                AccessToken = _jwt
            })));

            if (newTokenHttpResponse.IsSuccessStatusCode)
            {
                var newTokenString = await newTokenHttpResponse.Content.ReadAsStringAsync();
                var newTokenResponse = JsonConvert.DeserializeObject<RenewAuthenticationTokenResponse>(newTokenString);

                _jwt = newTokenResponse.AccessToken;
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", _jwt);
            }
        }
    }
}
