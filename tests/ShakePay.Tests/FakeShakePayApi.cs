using System.Collections.Concurrent;
using System.Net;
using System.Text;

namespace ShakePay.Tests;

public sealed record RecordedRequest(
    HttpMethod Method,
    Uri Uri,
    IReadOnlyDictionary<string, string> Headers,
    string? ContentType,
    string? Body)
{
    public string Path => Uri.AbsolutePath;
}

/// <summary>Records every request and answers with the supplied responder.</summary>
public sealed class FakeShakePayApi : HttpMessageHandler
{
    private readonly Func<RecordedRequest, HttpResponseMessage> _respond;

    public FakeShakePayApi(Func<RecordedRequest, HttpResponseMessage>? respond = null)
    {
        _respond = respond ?? (_ => Json(HttpStatusCode.OK, "{}"));
    }

    public ConcurrentQueue<RecordedRequest> Requests { get; } = new();

    public RecordedRequest Single() => Assert.Single(Requests);

    public static HttpResponseMessage Json(HttpStatusCode status, string json) =>
        new(status) { Content = new StringContent(json, Encoding.UTF8, "application/json") };

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var headers = request.Headers.NonValidated.ToDictionary(h => h.Key, h => h.Value.ToString(), StringComparer.OrdinalIgnoreCase);
        var body = request.Content is null ? null : await request.Content.ReadAsStringAsync(cancellationToken);
        var recorded = new RecordedRequest(request.Method, request.RequestUri!, headers, request.Content?.Headers.ContentType?.MediaType, body);
        Requests.Enqueue(recorded);
        return _respond(recorded);
    }
}
