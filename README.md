# shakepay-sdk-dotnet

[![.NET](https://github.com/cillierscharl/shakepay-sdk-dotnet/actions/workflows/dotnet.yml/badge.svg)](https://github.com/cillierscharl/shakepay-sdk-dotnet/actions/workflows/dotnet.yml)
[![NuGet](https://img.shields.io/nuget/v/shakepay-sdk-dotnet)](https://www.nuget.org/packages/shakepay-sdk-dotnet/)

Unofficial ShakePay .NET SDK

#### Installation ####

```shell
Install-Package shakepay-sdk-dotnet -Version 1.0.3
```


#### Quick start ####

```csharp
// JWT will automatically be refreshed every 5 minutes
// Use an HTTP client factory. For demo purposes pass in a new client
var httpClient = new HttpClient();
var shakePayConfiguration = new ShakePayClientConfiguration()
{
    DeviceName = "",
    DeviceUniqueId = "",
    Jwt = "",
    PrivateIpAddress = "",
    AutoRefreshToken = true
};
var client = new ShakePayClient(shakePayConfiguration, httpClient);

// Shake some sats!
await client.ShakingSatsAsync();

// Get the current ShakePay crypto currency prices
var prices = await client.GetCryptoCurrencyQuotes();

// Get all wallets associated with your account
var wallets = await client.GetWalletsAsync();
var cadWallet = wallets.Where(w => w.Currency == "CAD").Single();

// Get last 200 transactions from a specific wallet (Defaults to CAD)
var transactions1 = await client.GetTransactionHistoryAsync();

// Get last 2000 transactions
var transactions2 = await client.GetTransactionsHistoryPagedAsync(page: 1, limit: 2000, currencies: default);
```

#### Loading your entire transaction history ####
```csharp
var keepSearching = true;
var page = 1;
var transactionsList = new List<Transaction>();

while (keepSearching) {
    var transactions = await client.GetTransactionsHistoryPagedAsync(page, 2000);
    if (transactions.Count != 0) {
        transactionsList.AddRange(transactions);
    } else {
        keepSearching = false;
    }
    page++;
}
```
