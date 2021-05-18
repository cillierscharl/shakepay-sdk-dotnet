using Newtonsoft.Json;
using ShakePay.Contracts;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ShakePay
{
    public class ShakePayClient
    {
        private readonly bool _initialized;
        private readonly HttpClient _httpClient;
        private static readonly string _baseUrl = "https://api.shakepay.com";
        public ShakePayClient(string jwt, HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.DefaultRequestHeaders.Add("Authorization", jwt);
            _initialized = true;
        }

        public async Task<WalletsResponse> GetWalletsAsync()
        {
            if (!_initialized)
            {
                throw new Exception("ShakePay client not initialized");
            }

            var walletsResponse = await _httpClient.GetAsync($"{_baseUrl}/wallets");
            if (walletsResponse.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<WalletsResponse>(await walletsResponse.Content.ReadAsStringAsync());
            }

            return null;
        }

        public async Task<TransactionHistoryResponse> GetTransactionsHistoryAsync(int page = 0, int limit = 10)
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
                    Page = page,
                    RowsPerPage = limit
                },
                FilterParams = new object()
            };

            var transactionsHistoryResponse = await _httpClient.PostAsync(
                $"{_baseUrl}/transactions/history",
                new StringContent(JsonConvert.SerializeObject(requestBody),
                Encoding.UTF8,
                "application/json"));

            if (transactionsHistoryResponse.IsSuccessStatusCode)
            {
                var response = await transactionsHistoryResponse.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<TransactionHistoryResponse>(response);
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



            var request = await _httpClient.PostAsync($"{_baseUrl}/transactions", new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json"));

            return request.IsSuccessStatusCode;
        }
    }

}
