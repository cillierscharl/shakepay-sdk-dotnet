using System.Globalization;
using System.Net;
using System.Web;
using Newtonsoft.Json.Linq;
using ShakePay.Contracts;

namespace ShakePay.Tests;

public class ShakePayClientTests
{
    private static ShakePayClientConfiguration Config(string jwt = "jwt-1") => new()
    {
        DeviceName = "test-phone",
        DeviceUniqueId = "device-123",
        Jwt = jwt,
        PrivateIpAddress = "10.0.0.5",
    };

    private static (ShakePayClient Client, FakeShakePayApi Api) Create(Func<RecordedRequest, HttpResponseMessage>? respond = null)
    {
        var api = new FakeShakePayApi(respond);
        return (new ShakePayClient(Config(), new HttpClient(api)), api);
    }

    private static Func<RecordedRequest, HttpResponseMessage> Respond(string json, HttpStatusCode status = HttpStatusCode.OK) =>
        _ => FakeShakePayApi.Json(status, json);

    [Fact]
    public async Task PostTransaction_FormatsAmountWithInvariantCulture()
    {
        var (client, api) = Create();
        var original = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = new CultureInfo("fr-CA");
        try {
            Assert.True(await client.PostTransactionAsync("wallet-1", "friend", 5.25m, "🏓 Pong!"));
        } finally {
            CultureInfo.CurrentCulture = original;
        }

        var request = api.Single();
        Assert.Equal(HttpMethod.Post, request.Method);
        Assert.Equal("/transactions", request.Path);
        Assert.Equal("application/json", request.ContentType);
        var body = JObject.Parse(request.Body!);
        Assert.Equal("5.25", (string?)body["amount"]);
        Assert.Equal("wallet-1", (string?)body["fromWallet"]);
        Assert.Equal("friend", (string?)body["to"]);
        Assert.Equal("user", (string?)body["toType"]);
        Assert.Equal("🏓 Pong!", (string?)body["note"]);
    }

    [Fact]
    public async Task PostTransaction_ReturnsFalseWhenRejected()
    {
        var (client, _) = Create(Respond("{}", HttpStatusCode.BadRequest));

        Assert.False(await client.PostTransactionAsync("wallet-1", "friend", 1m, ""));
    }

    [Fact]
    public async Task GetUsersByName_EscapesUsername()
    {
        var (client, api) = Create(Respond("""{"data":[{"username":"a&b c"}]}"""));

        var result = await client.GetUsersByNameAsync("a&b c#");

        Assert.Equal("?username=a%26b%20c%23", api.Single().Uri.Query);
        Assert.Equal("a&b c", Assert.Single(result.Users).Username);
    }

    [Fact]
    public async Task GetTransactionHistory_EncodesQuery()
    {
        var (client, api) = Create(Respond("""[{"transactionId":"t1","amount":12.34,"currency":"CAD","direction":"credit"}]"""));
        // A local time formats with its UTC offset, which contains '+' east of UTC (and in UTC).
        var before = new DateTime(2021, 7, 14, 16, 48, 55, DateTimeKind.Local);

        var transactions = await client.GetTransactionHistoryAsync(before, "CAD", 50);

        var request = api.Single();
        Assert.Equal("/transactions/history", request.Path);
        var query = HttpUtility.ParseQueryString(request.Uri.Query);
        Assert.Equal("CAD", query["currency"]);
        Assert.Equal(before.ToString("O"), query["before"]);
        Assert.Equal("50", query["limit"]);
        var transaction = Assert.Single(transactions);
        Assert.Equal(12.34m, transaction.Amount);
    }

    [Fact]
    public async Task GetCryptoCurrencyQuotes_SendsLowercaseFlagAndKeepsPrecision()
    {
        var (client, api) = Create(Respond("""[{"symbol":"BTC_CAD","baseRate":123456.78,"rate":123987.65}]"""));

        var quotes = await client.GetCryptoCurrencyQuotesAsync(includeFees: false);

        Assert.Equal("/quote", api.Single().Path);
        Assert.Equal("?includeFees=false", api.Single().Uri.Query);
        var quote = Assert.Single(quotes);
        Assert.Equal(123456.78m, quote.BaseRate);
        Assert.Equal(123987.65m, quote.Rate);
    }

    [Fact]
    public async Task GetWallets_ReturnsWallets()
    {
        var (client, api) = Create(Respond("""{"data":[{"id":"w1","currency":"CAD","balance":10.5,"fiatBalance":10.5}]}"""));

        var wallets = await client.GetWalletsAsync();

        Assert.Equal("/wallets", api.Single().Path);
        var wallet = Assert.Single(wallets);
        Assert.Equal("w1", wallet.Id);
        Assert.Equal(10.5m, wallet.Balance);
    }

    [Fact]
    public async Task Get_ReturnsNullOnFailure()
    {
        var (client, _) = Create(Respond("""{"message":"nope"}""", HttpStatusCode.Unauthorized));

        Assert.Null(await client.GetWalletsAsync());
        Assert.Null(await client.GetCryptoCurrencyQuotesAsync());
        Assert.Null(await client.GetWaitListPositionAsync());
    }

    [Fact]
    public async Task ShakingSats_ReturnsErrorBodyWhenRejected()
    {
        var (client, api) = Create(Respond(
            """{"name":"BadRequest","message":"Already shook today","code":400,"className":"bad-request"}""",
            HttpStatusCode.BadRequest));

        var response = await client.ShakingSatsAsync();

        Assert.Equal(400, response.Code);
        Assert.Equal("Already shook today", response.Message);
        var request = api.Single();
        Assert.Equal("/shaking-sats", request.Path);
        Assert.Equal("{}", request.Body);
    }

    [Fact]
    public async Task ShakingSats_ReturnsNullWhenRequestFails()
    {
        var (client, _) = Create(_ => throw new HttpRequestException("offline"));

        Assert.Null(await client.ShakingSatsAsync());
    }

    [Fact]
    public async Task Requests_CarryAuthAndDeviceHeaders_WithoutModifyingTheHttpClient()
    {
        var api = new FakeShakePayApi(Respond("""{"data":[]}"""));
        var httpClient = new HttpClient(api);
        httpClient.DefaultRequestHeaders.Add("x-caller", "kept");
        var client = new ShakePayClient(Config(), httpClient);

        await client.GetWalletsAsync();

        var headers = api.Single().Headers;
        Assert.Equal("jwt-1", headers["Authorization"]);
        Assert.Equal("test-phone", headers["x-device-name"]);
        Assert.Equal("device-123", headers["x-device-unique-id"]);
        Assert.Equal("10.0.0.5", headers["x-device-ip-address"]);
        Assert.Equal("Shakepay App v1.6.100 (16100) on test-phone", headers["User-Agent"]);
        Assert.Equal("kept", headers["x-caller"]);
        Assert.Equal(["x-caller"], httpClient.DefaultRequestHeaders.Select(h => h.Key));
    }

    [Fact]
    public async Task ClientsSharingAnHttpClient_EachSendTheirOwnToken()
    {
        var api = new FakeShakePayApi(Respond("""{"data":[]}"""));
        var httpClient = new HttpClient(api);
        var first = new ShakePayClient(Config("first"), httpClient);
        var second = new ShakePayClient(Config("second"), httpClient);

        await first.GetWalletsAsync();
        await second.GetWalletsAsync();

        Assert.Equal(["first", "second"], api.Requests.Select(r => r.Headers["Authorization"]));
    }

    [Fact]
    public void Transaction_EqualityUsesId()
    {
        Assert.Equal(new Transaction { TransactionId = "a" }, new Transaction { TransactionId = "a" });
        Assert.NotEqual(new Transaction { TransactionId = "a" }, new Transaction { TransactionId = "b" });
        Assert.Single(new HashSet<Transaction> { new() });
    }
}
