using Newtonsoft.Json;
using ShakePay.Contracts;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ShakePay
{
    /// <summary>
    /// Unofficial client for the ShakePay API. Methods return <c>null</c> when ShakePay
    /// responds with a non-success status code.
    /// </summary>
    /// <remarks>
    /// Headers are set per request, so the <see cref="HttpClient"/> passed in is never modified
    /// and can be shared. Dispose the client to stop the background token refresh.
    /// </remarks>
    public class ShakePayClient : IShakePayClient, IDisposable
    {
        private const string BaseUrl = "https://api.shakepay.com";
        private static readonly TimeSpan DefaultTokenRefreshInterval = TimeSpan.FromMinutes(5);

        private readonly HttpClient _httpClient;
        private readonly KeyValuePair<string, string>[] _deviceHeaders;
        private readonly CancellationTokenSource _disposed = new CancellationTokenSource();
        private volatile string _jwt;

        public ShakePayClient(ShakePayClientConfiguration config, HttpClient httpClient)
            : this(config, httpClient, DefaultTokenRefreshInterval)
        {
        }

        internal ShakePayClient(ShakePayClientConfiguration config, HttpClient httpClient, TimeSpan tokenRefreshInterval)
        {
            if (config == null) {
                throw new ArgumentNullException(nameof(config));
            }

            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _jwt = config.Jwt;
            _deviceHeaders = BuildDeviceHeaders(config);

            if (config.AutoRefreshToken) {
                _ = RefreshTokenPeriodicallyAsync(tokenRefreshInterval, _disposed.Token);
            }
        }

        public Task<List<QuoteResponse>> GetCryptoCurrencyQuotesAsync(bool includeFees = true)
        {
            return GetAsync<List<QuoteResponse>>($"/quote?includeFees={(includeFees ? "true" : "false")}");
        }

        [Obsolete("Use GetCryptoCurrencyQuotesAsync.")]
        public Task<List<QuoteResponse>> GetCryptoCurrencyQuotes(bool includeFees = true)
        {
            return GetCryptoCurrencyQuotesAsync(includeFees);
        }

        public async Task<List<Wallet>> GetWalletsAsync()
        {
            var response = await GetAsync<WalletsResponse>("/wallets").ConfigureAwait(false);
            return response?.Wallets;
        }

        public Task<List<RecentContact>> GetRecentContactsAsync()
        {
            return GetAsync<List<RecentContact>>("/recent-contacts");
        }

        public Task<UserSearchResponse> GetUsersByNameAsync(string username)
        {
            return GetAsync<UserSearchResponse>($"/users?username={Escape(username)}");
        }

        /// <summary>
        /// Shakes sats. When ShakePay rejects the shake (for example, already shaken today) the
        /// error body is returned with <see cref="ShakingSatsResponse.Message"/> set. Returns
        /// <c>null</c> if the request fails or the response can't be read.
        /// </summary>
        public async Task<ShakingSatsResponse> ShakingSatsAsync()
        {
            try {
                using (var response = await SendAsync(HttpMethod.Post, "/shaking-sats", new { }).ConfigureAwait(false)) {
                    var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    return JsonConvert.DeserializeObject<ShakingSatsResponse>(json);
                }
            } catch (Exception) {
                return null;
            }
        }

        [Obsolete("ShakePay deprecated this endpoint. Use GetTransactionHistoryAsync.")]
        public async Task<List<Transaction>> GetTransactionsHistoryPagedAsync(int page = 0, int limit = 2000, List<string> currencies = default)
        {
            var requestBody = new TransactionHistoryRequest()
            {
                Pagination = new TransactionHistoryPagination()
                {
                    Descending = true,
                    Page = page.ToString(CultureInfo.InvariantCulture),
                    RowsPerPage = limit
                },
                FilterParams = new TransactionHistoryFilterParams()
                {
                    Currencies = currencies
                }
            };

            var response = await SendAndReadAsync<PagedTransactionHistoryResponse>(HttpMethod.Post, "/transactions/history", requestBody).ConfigureAwait(false);
            return response?.Transactions;
        }

        /// <summary>Gets up to <paramref name="limit"/> transactions in <paramref name="currency"/> made before <paramref name="beforeDateTime"/>.</summary>
        public Task<List<Transaction>> GetTransactionHistoryAsync(DateTime beforeDateTime, string currency = "CAD", int limit = 2000)
        {
            var before = beforeDateTime.ToString("O", CultureInfo.InvariantCulture);
            return GetAsync<List<Transaction>>(
                $"/transactions/history?currency={Escape(currency)}&before={Escape(before)}&limit={limit.ToString(CultureInfo.InvariantCulture)}");
        }

        public Task<WaitlistResponse> GetWaitListPositionAsync()
        {
            return GetAsync<WaitlistResponse>("/card/waitlist");
        }

        /// <summary>Sends <paramref name="amount"/> from <paramref name="walletId"/> to a ShakePay user. Returns whether ShakePay accepted it.</summary>
        public async Task<bool> PostTransactionAsync(string walletId, string username, decimal amount, string note)
        {
            var requestBody = new TransactionRequest()
            {
                Amount = amount.ToString(CultureInfo.InvariantCulture),
                FromWallet = walletId,
                Note = note,
                To = username,
                ToType = "user"
            };

            using (var response = await SendAsync(HttpMethod.Post, "/transactions", requestBody).ConfigureAwait(false)) {
                return response.IsSuccessStatusCode;
            }
        }

        /// <summary>Stops the background token refresh. The <see cref="HttpClient"/> is not disposed.</summary>
        public void Dispose()
        {
            _disposed.Cancel();
        }

        private Task<T> GetAsync<T>(string pathAndQuery) where T : class
        {
            return SendAndReadAsync<T>(HttpMethod.Get, pathAndQuery, null);
        }

        private async Task<T> SendAndReadAsync<T>(HttpMethod method, string pathAndQuery, object body) where T : class
        {
            using (var response = await SendAsync(method, pathAndQuery, body).ConfigureAwait(false)) {
                if (!response.IsSuccessStatusCode) {
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return JsonConvert.DeserializeObject<T>(json);
            }
        }

        private async Task<HttpResponseMessage> SendAsync(HttpMethod method, string pathAndQuery, object body, CancellationToken cancellationToken = default)
        {
            using (var request = new HttpRequestMessage(method, BaseUrl + pathAndQuery)) {
                foreach (var header in _deviceHeaders) {
                    request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }

                request.Headers.TryAddWithoutValidation("Authorization", _jwt);

                if (body != null) {
                    request.Content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");
                }

                return await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
            }
        }

        private async Task RefreshTokenPeriodicallyAsync(TimeSpan interval, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested) {
                try {
                    await Task.Delay(interval, cancellationToken).ConfigureAwait(false);
                    await RenewTokenAsync(cancellationToken).ConfigureAwait(false);
                } catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) {
                    return;
                } catch (Exception) {
                    // Keep the current token and try again on the next interval.
                }
            }
        }

        private async Task RenewTokenAsync(CancellationToken cancellationToken)
        {
            var requestBody = new RenewAuthenticationTokenRequest()
            {
                Strategy = "jwt",
                AccessToken = _jwt
            };

            using (var response = await SendAsync(HttpMethod.Post, "/authentication", requestBody, cancellationToken).ConfigureAwait(false)) {
                if (!response.IsSuccessStatusCode) {
                    return;
                }

                var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                var newToken = JsonConvert.DeserializeObject<RenewAuthenticationTokenResponse>(json)?.AccessToken;
                if (!string.IsNullOrEmpty(newToken)) {
                    _jwt = newToken;
                }
            }
        }

        private static string Escape(string value)
        {
            return Uri.EscapeDataString(value ?? string.Empty);
        }

        private static KeyValuePair<string, string>[] BuildDeviceHeaders(ShakePayClientConfiguration config)
        {
            return new[]
            {
                Header("x-device-total-memory", "6023036928"),
                Header("x-device-name", config.DeviceName),
                Header("x-device-has-notch", "false"),
                Header("User-Agent", $"Shakepay App v1.6.100 (16100) on {config.DeviceName}"),
                Header("x-device-locale", "en - CA"),
                Header("x-device-manufacturer", "Apple"),
                Header("x-device-is-tablet", "false"),
                Header("x-device-total-disk-capacity", "127881465856"),
                Header("x-device-system-name", "iOS"),
                Header("x-device-carrier", "Bell"),
                Header("x-device-id", "iPhone13,3"),
                Header("x-device-model", "iPhone"),
                Header("x-device-serial-number", ""),
                Header("x-device-country", "CA"),
                Header("x-device-mac-address", "02:00:00:00:00:00"),
                Header("x-device-tzoffset", "240"),
                Header("x-device-ip-address", config.PrivateIpAddress),
                Header("x-device-unique-id", config.DeviceUniqueId),
                Header("x-notification-token", ""),
                Header("x-device-brand", "Apple"),
                Header("x-device-system-version", "14.5.1"),
            };
        }

        private static KeyValuePair<string, string> Header(string name, string value)
        {
            return new KeyValuePair<string, string>(name, value ?? string.Empty);
        }
    }
}
