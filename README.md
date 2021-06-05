# shakepay-sdk-dotnet

[![.NET](https://github.com/cillierscharl/shakepay-sdk-dotnet/actions/workflows/dotnet.yml/badge.svg)](https://github.com/cillierscharl/shakepay-sdk-dotnet/actions/workflows/dotnet.yml)
[![NuGet](https://img.shields.io/nuget/v/shakepay-sdk-dotnet)](https://www.nuget.org/packages/shakepay-sdk-dotnet/)

Unofficial ShakePay .NET SDK

#### Installation ####

```shell
Install-Package shakepay-sdk-dotnet -Version 1.0.0
```


#### QuickStart ####

```csharp
// JWT will automatically be refreshed every 5 minutes
var jwt = "";
// Use an HTTP client factory. For demo purposes pass in a new client
var httpClient = new HttpClient();
var client = new ShakePayClient(jwt, httpClient);

// Shake some sats!
await client.ShakingSatsAsync();

// Get all wallets associated with your account
var wallets = await client.GetWalletsAsync();
var cadWallet = wallets.Where(w => w.Currency == "CAD").Single();

// Get last 200 transactions from a specific wallet (Defaults to CAD)
var transactions1 = await client.GetTransactionHistory();

// Get last 2000 transactions
var transactions2 = await client.GetTransactionsHistoryPagedAsync(page: 1, limit: 2000);
```