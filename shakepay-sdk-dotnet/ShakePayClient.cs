﻿using Newtonsoft.Json;
using ShakePay.Contracts;
using System;
using System.Net.Http;
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

        public async Task<TransactionHistoryResponse> GetTransactionsHistoryAsync(int limit = 10)
        {
            if (!_initialized)
            {
                throw new Exception("ShakePay client not initialized");
            }

            var beforeDateTime = DateTime.UtcNow.ToString("O");

            var transactionsHistoryResponse = await _httpClient.GetAsync($"{_baseUrl}/transactions/history?currency=CAD&before={beforeDateTime}&limit={limit}");
            if (transactionsHistoryResponse.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<TransactionHistoryResponse>(await transactionsHistoryResponse.Content.ReadAsStringAsync());
            }

            return null;
        }
    }

}
