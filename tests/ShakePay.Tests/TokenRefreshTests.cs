using System.Net;
using Newtonsoft.Json.Linq;

namespace ShakePay.Tests;

public class TokenRefreshTests
{
    private static readonly TimeSpan Interval = TimeSpan.FromMilliseconds(20);

    private static ShakePayClientConfiguration Config() => new()
    {
        DeviceName = "test-phone",
        DeviceUniqueId = "device-123",
        Jwt = "jwt-1",
        AutoRefreshToken = true,
    };

    private static HttpResponseMessage RenewedToken() =>
        FakeShakePayApi.Json(HttpStatusCode.Created, """{"accessToken":"jwt-2"}""");

    private static HttpResponseMessage NoWallets() =>
        FakeShakePayApi.Json(HttpStatusCode.OK, """{"data":[]}""");

    private static int RenewCount(FakeShakePayApi api) => api.Requests.Count(r => r.Path == "/authentication");

    [Fact]
    public async Task RenewsTokenAndUsesItForLaterRequests()
    {
        var api = new FakeShakePayApi(r => r.Path == "/authentication" ? RenewedToken() : NoWallets());
        using var client = new ShakePayClient(Config(), new HttpClient(api), Interval);

        // The second renewal only starts once the first has been applied.
        await WaitUntil(() => RenewCount(api) >= 2);
        await client.GetWalletsAsync();

        var renew = api.Requests.First(r => r.Path == "/authentication");
        Assert.Equal(HttpMethod.Post, renew.Method);
        Assert.Equal("application/json", renew.ContentType);
        var body = JObject.Parse(renew.Body!);
        Assert.Equal("jwt", (string?)body["strategy"]);
        Assert.Equal("jwt-1", (string?)body["accessToken"]);
        Assert.Equal("jwt-2", api.Requests.Last(r => r.Path == "/wallets").Headers["Authorization"]);
    }

    [Fact]
    public async Task KeepsRefreshingAfterAFailure()
    {
        var attempts = 0;
        var api = new FakeShakePayApi(r =>
        {
            if (r.Path != "/authentication") {
                return NoWallets();
            }

            return Interlocked.Increment(ref attempts) == 1 ? throw new HttpRequestException("offline") : RenewedToken();
        });
        using var client = new ShakePayClient(Config(), new HttpClient(api), Interval);

        await WaitUntil(() => RenewCount(api) >= 3);
        await client.GetWalletsAsync();

        Assert.Equal("jwt-2", api.Requests.Last(r => r.Path == "/wallets").Headers["Authorization"]);
    }

    [Fact]
    public async Task KeepsTheCurrentTokenWhenRenewalIsRejected()
    {
        var api = new FakeShakePayApi(r => r.Path == "/authentication"
            ? FakeShakePayApi.Json(HttpStatusCode.Unauthorized, "{}")
            : NoWallets());
        using var client = new ShakePayClient(Config(), new HttpClient(api), Interval);

        await WaitUntil(() => RenewCount(api) >= 2);
        await client.GetWalletsAsync();

        Assert.Equal("jwt-1", api.Requests.Last(r => r.Path == "/wallets").Headers["Authorization"]);
    }

    [Fact]
    public async Task DisposeStopsRefreshing()
    {
        var api = new FakeShakePayApi(_ => RenewedToken());
        var client = new ShakePayClient(Config(), new HttpClient(api), Interval);
        await WaitUntil(() => RenewCount(api) >= 1);

        client.Dispose();
        await Task.Delay(100);
        var countAfterDispose = RenewCount(api);
        await Task.Delay(300);

        Assert.Equal(countAfterDispose, RenewCount(api));
    }

    [Fact]
    public async Task DoesNotRefreshWhenDisabled()
    {
        var api = new FakeShakePayApi(_ => RenewedToken());
        var config = Config();
        config.AutoRefreshToken = false;
        using var client = new ShakePayClient(config, new HttpClient(api), Interval);

        await Task.Delay(200);

        Assert.Empty(api.Requests);
    }

    private static async Task WaitUntil(Func<bool> condition)
    {
        var deadline = DateTime.UtcNow.AddSeconds(10);
        while (!condition()) {
            if (DateTime.UtcNow > deadline) {
                throw new TimeoutException("Condition not met within 10s.");
            }

            await Task.Delay(10);
        }
    }
}
