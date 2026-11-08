# shakepay-sdk-dotnet

[![.NET](https://github.com/cillierscharl/shakepay-sdk-dotnet/actions/workflows/dotnet.yml/badge.svg)](https://github.com/cillierscharl/shakepay-sdk-dotnet/actions/workflows/dotnet.yml)
[![NuGet](https://img.shields.io/nuget/v/shakepay-sdk-dotnet)](https://www.nuget.org/packages/shakepay-sdk-dotnet/)

Unofficial ShakePay .NET SDK

#### Installation ####

```shell
Install-Package shakepay-sdk-dotnet
```


#### Quick start ####

```csharp
// Use an HTTP client factory. For demo purposes pass in a new client.
// The HttpClient isn't modified, so it can be shared.
var httpClient = new HttpClient();
var shakePayConfiguration = new ShakePayClientConfiguration()
{
    DeviceName = "",
    DeviceUniqueId = "",
    Jwt = "",
    PrivateIpAddress = "",
    // Renew the JWT every 5 minutes until the client is disposed
    AutoRefreshToken = true
};
using var client = new ShakePayClient(shakePayConfiguration, httpClient);

// Shake some sats! If ShakePay rejects the shake (e.g. already shaken today), Data is null and Message says why.
var shake = await client.ShakingSatsAsync();
if (shake?.Data != null) {
    Console.WriteLine($"Been shaking for {shake.Data.Streak} days!");
}

// Get the current ShakePay cryptocurrency prices (includes fees by default)
var prices = await client.GetCryptoCurrencyQuotesAsync(includeFees: true);

// Get all wallets associated with your account
var wallets = await client.GetWalletsAsync();
var cadWallet = wallets.Single(w => w.Currency == "CAD");

// Get up to 2000 CAD transactions made before now
var transactions = await client.GetTransactionHistoryAsync(DateTime.UtcNow, currency: "CAD", limit: 2000);
```

Methods return `null` when ShakePay responds with an error, so check results before using them.

#### Send a payment ####
```csharp
var sent = await client.PostTransactionAsync(
    walletId: cadWallet.Id,
    username: "charlc",
    amount: 5.00m,
    note: "🏓 Pong!");
```
