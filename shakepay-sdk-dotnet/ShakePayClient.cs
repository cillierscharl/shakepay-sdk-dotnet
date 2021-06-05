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
        public ShakePayClient(ShakePayClientConfiguration config, HttpClient httpClient)
        {
            _httpClient = httpClient;
            _initialized = true;
            SetDefaultRequestHeaders(config);
            PeriodicallyRefreshToken(config);
        }

        public async Task<List<QuoteResponse>> GetCryptoCurrencyQuotes(bool includeFees = true)
        {
            if (!_initialized) {
                throw new Exception("ShakePay client not initialized");
            }

            var quotesHttpResponse = await _httpClient.GetAsync($"{_baseUrl}/quote?includeFees={includeFees}");
            if (quotesHttpResponse.IsSuccessStatusCode) {
                var quotesResponse = JsonConvert.DeserializeObject<List<QuoteResponse>>(await quotesHttpResponse.Content.ReadAsStringAsync());
                return quotesResponse;
            }

            return null;
        }

        public async Task<List<Wallet>> GetWalletsAsync()
        {
            if (!_initialized) {
                throw new Exception("ShakePay client not initialized");
            }

            var walletsHttpResponse = await _httpClient.GetAsync($"{_baseUrl}/wallets");
            if (walletsHttpResponse.IsSuccessStatusCode) {
                var walletsResponse = JsonConvert.DeserializeObject<WalletsResponse>(await walletsHttpResponse.Content.ReadAsStringAsync());
                return walletsResponse.Wallets;
            }

            return null;
        }

        public async Task<List<RecentContact>> GetRecentContactsAsync()
        {
            if (!_initialized) {
                throw new Exception("ShakePay client not initialized");
            }

            var recentContactsHttpResponse = await _httpClient.GetAsync($"{_baseUrl}/recent-contacts");
            if (recentContactsHttpResponse.IsSuccessStatusCode) {
                var recentContactsResponse = JsonConvert.DeserializeObject<List<RecentContact>>(await recentContactsHttpResponse.Content.ReadAsStringAsync());
                return recentContactsResponse;
            }

            return null;
        }

        public async Task<UserSearchResponse> GetUsersByNameAsync(string username)
        {
            if (!_initialized) {
                throw new Exception("ShakePay client not initialized");
            }

            var userSearchHttpResponse = await _httpClient.GetAsync($"{_baseUrl}/users?username={username}");

            if (userSearchHttpResponse.IsSuccessStatusCode) {
                var response = await userSearchHttpResponse.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<UserSearchResponse>(response);
            }

            return null;
        }

        public async Task<ShakingSatsResponse> ShakingSatsAsync()
        {
            if (!_initialized) {
                throw new Exception("ShakePay client not initialized");
            }

            var shakingSatsHttpsResponse = await _httpClient.GetAsync($"{_baseUrl}/shaking-sats");
            if (shakingSatsHttpsResponse.IsSuccessStatusCode) {
                var shakingSatsResponse = JsonConvert.DeserializeObject<ShakingSatsResponse>(await shakingSatsHttpsResponse.Content.ReadAsStringAsync());
                return shakingSatsResponse;
            }

            return null;
        }

        public async Task<List<Transaction>> GetTransactionsHistoryPagedAsync(int page = 0, int limit = 2000, List<string> currencies = default)
        {
            if (!_initialized) {
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
                FilterParams = new TransactionHistoryFilterParams()
                {
                    Currencies = currencies
                }
            };

            var transactionsHistoryHttpResponse = await _httpClient.PostAsync(
                $"{_baseUrl}/transactions/history",
                new StringContent(JsonConvert.SerializeObject(requestBody),
                Encoding.UTF8,
                "application/json"));

            if (transactionsHistoryHttpResponse.IsSuccessStatusCode) {
                var trasnactionHistoryResponseString = await transactionsHistoryHttpResponse.Content.ReadAsStringAsync();
                var transactionHistoryResponse = JsonConvert.DeserializeObject<PagedTransactionHistoryResponse>(trasnactionHistoryResponseString);
                return transactionHistoryResponse.Transactions;
            }

            return null;
        }

        public async Task<List<Transaction>> GetTransactionHistoryAsync(string currency = "CAD", int limit = 10)
        {
            if (!_initialized) {
                throw new Exception("ShakePay client not initialized");
            }

            var beforeDateTime = DateTime.UtcNow.ToString("O");
            var transactionsHistoryResponse = await _httpClient.GetAsync($"{_baseUrl}/transactions/history?currency={currency}&before={beforeDateTime}&limit={limit}");

            if (transactionsHistoryResponse.IsSuccessStatusCode) {
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

        private void SetDefaultRequestHeaders(ShakePayClientConfiguration config)
        {
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", config.Jwt);
            _httpClient.DefaultRequestHeaders.Add("x-device-total-memory", "6023036928");
            _httpClient.DefaultRequestHeaders.Add("x-device-name", config.DeviceName);
            _httpClient.DefaultRequestHeaders.Add("x-device-has-notch", "false");
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Shakepay App v1.6.101 (16101) on Apple iPhone (iOS 14.5.1)");
            _httpClient.DefaultRequestHeaders.Add("x-device-locale", "en - CA");
            _httpClient.DefaultRequestHeaders.Add("x-device-manufacturer", "Apple");
            _httpClient.DefaultRequestHeaders.Add("x-device-is-tablet", "false");
            _httpClient.DefaultRequestHeaders.Add("x-device-total-disk-capacity", "127881465856");
            _httpClient.DefaultRequestHeaders.Add("x-device-system-name", "iOS");
            _httpClient.DefaultRequestHeaders.Add("x-device-carrier", "Bell");
            _httpClient.DefaultRequestHeaders.Add("x-device-id", "iPhone13,3");
            _httpClient.DefaultRequestHeaders.Add("x-device-model", "iPhone");
            _httpClient.DefaultRequestHeaders.Add("x-device-serial-number", "");
            _httpClient.DefaultRequestHeaders.Add("x-device-country", "CA");
            _httpClient.DefaultRequestHeaders.Add("x-device-mac-address", "02:00:00:00:00:00");
            _httpClient.DefaultRequestHeaders.Add("x-device-tzoffset", "240");
            _httpClient.DefaultRequestHeaders.Add("x-device-ip-address", config.PrivateIpAddress);
            _httpClient.DefaultRequestHeaders.Add("x-device-unique-id", config.DeviceUniqueId);
            _httpClient.DefaultRequestHeaders.Add("x-notification-token", "");
            _httpClient.DefaultRequestHeaders.Add("x-device-brand", "Apple");
            _httpClient.DefaultRequestHeaders.Add("x-device-system-version", "14.5.1");
        }

        private void PeriodicallyRefreshToken(ShakePayClientConfiguration config)
        {
            if (!config.AutoRefreshToken) {
                return;
            }

            var t = new Task(async () =>
            {
                while (true) {
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
                AccessToken = _httpClient.DefaultRequestHeaders.Authorization.Scheme
            })));

            if (newTokenHttpResponse.IsSuccessStatusCode) {
                var newTokenString = await newTokenHttpResponse.Content.ReadAsStringAsync();
                var newTokenResponse = JsonConvert.DeserializeObject<RenewAuthenticationTokenResponse>(newTokenString);

                _httpClient.DefaultRequestHeaders.Remove("Authorization");
                _httpClient.DefaultRequestHeaders.Add("Authorization", newTokenResponse.AccessToken);
            }
        }
    }
}
