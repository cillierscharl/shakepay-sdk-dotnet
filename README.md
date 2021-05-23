# shakepay-sdk-dotnet

[![.NET](https://github.com/cillierscharl/shakepay-sdk-dotnet/actions/workflows/dotnet.yml/badge.svg)](https://github.com/cillierscharl/shakepay-sdk-dotnet/actions/workflows/dotnet.yml)

Unofficial ShakePay .NET SDK


#### QuickStart ####

```csharp
// JWT will automatically be refreshed every minute
var jwt = "";
// Use an HTTP client factory. For demo purposes pass in a new client.
var httpClient = new HttpClient();
var client = new ShakePayClient(jwt, httpClient);

// Get all wallets associated with your account
var wallets = await client.GetWalletsAsync();
var cadWallet = wallets.Where(w => w.Currency == "CAD").Single();

// Get last 200 transactions from a specific wallet (Defaults to CAD)
var transactions1 = await client.GetTransactionHistory();

// Get last 2000 transactions
var transactions2 = await client.GetTransactionsHistoryPagedAsync(page: 1, limit: 2000);
```

