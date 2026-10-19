using System.Net;
using System.Text;
using FlexOps.Sdk;

/// <summary>
/// HTTP policy of <see cref="FlexOpsClient"/>. The SDK has no retry loop of its own, so a caller's
/// resilience handler (Polly, Microsoft.Extensions.Http.Resilience) resends the same request. The Gateway
/// <c>IdempotencyKeyMiddleware</c> replays a POST/PUT/PATCH that carries an <c>Idempotency-Key</c> it has
/// already finished, so every write must carry one, and every attempt of that call must carry the same one.
/// The checks also cover a 2xx with no body (204 or Content-Length 0), which is <c>null</c>, and Gateway
/// error codes, which survive on <see cref="FlexOpsException.ErrorCode"/> for every verb.
/// </summary>
static class HttpPolicyChecks
{
    public static async Task RunAsync()
    {
        var failures = new List<string>();
        void Check(string name, bool ok, string detail = "")
        {
            if (!ok)
            {
                failures.Add($"{name}{(detail.Length > 0 ? ": " + detail : "")}");
            }
        }

        // One failing call (an old client threw on an empty body) must not hide the checks after it.
        async Task Step(string name, Func<Task> body)
        {
            try
            {
                await body();
            }
            catch (Exception ex)
            {
                failures.Add($"{name}: threw {ex.GetType().Name}: {ex.Message}");
            }
        }

        var handler = new ScriptedHandler();
        using var http = new HttpClient(handler) { BaseAddress = new Uri("https://example.test/") };
        using var client = new FlexOpsClient("https://example.test", apiKey: "test-key", workspaceId: "ws_1", httpClient: http);

        // Every write carries a key, generated when the caller has none.
        handler.Reply(HttpStatusCode.OK, """{"ok":true}""");
        await client.PostAsync<object>("api/a", new { a = 1 });
        var postKey = handler.Calls[^1].Key;
        Check("POST without a key gets one", Guid.TryParse(postKey, out _), postKey ?? "no Idempotency-Key header");

        handler.Reply(HttpStatusCode.OK, """{"ok":true}""");
        await client.PostAsync<object>("api/a", new { a = 1 });
        Check("each call gets its own key", handler.Calls[^1].Key is { } second && second != postKey, $"{postKey} then {handler.Calls[^1].Key}");

        handler.Reply(HttpStatusCode.OK, """{"ok":true}""");
        await client.PostAsync<object>("api/a", new { a = 1 }, default, "caller-key");
        Check("POST keeps the caller's key", handler.Calls[^1].Key == "caller-key", handler.Calls[^1].Key ?? "none");

        handler.Reply(HttpStatusCode.OK, """{"ok":true}""");
        await client.PutAsync<object>("api/a", new { a = 1 });
        Check("PUT without a key gets one", Guid.TryParse(handler.Calls[^1].Key, out _), handler.Calls[^1].Key ?? "no Idempotency-Key header");

        handler.Reply(HttpStatusCode.OK, """{"ok":true}""");
        await client.GetAsync<object>("api/a");
        Check("GET carries no key", handler.Calls[^1].Key is null, handler.Calls[^1].Key ?? "");

        handler.Reply(HttpStatusCode.NoContent, null);
        await Step("DELETE", () => client.DeleteAsync<object>("api/a"));
        Check("DELETE carries no key", handler.Calls[^1].Key is null, handler.Calls[^1].Key ?? "");

        // A retrying handler in the caller's pipeline resends the same request: the key does not change.
        var flaky = new ScriptedHandler();
        flaky.Reply(HttpStatusCode.ServiceUnavailable, """{"errorCode":"ServiceUnavailable"}""");
        flaky.Reply(HttpStatusCode.Created, """{"trackingNumber":"t-1"}""");
        using (var retrying = new HttpClient(new RetryOnceHandler(flaky)) { BaseAddress = new Uri("https://example.test/") })
        using (var retried = new FlexOpsClient("https://example.test", httpClient: retrying))
        {
            await Step("resent write", () => retried.PostAsync<object>("api/shipping/labels", new { a = 1 }));
            Check("a resent write reuses its key",
                flaky.Calls.Count == 2 && flaky.Calls[0].Key is not null && flaky.Calls[0].Key == flaky.Calls[1].Key,
                string.Join(", ", flaky.Calls.Select(c => c.Key ?? "none")));
        }

        // A 2xx with no body is null, never an exception or a raw response.
        handler.Reply(HttpStatusCode.NoContent, null);
        await Step("DELETE 204", async () => Check("DELETE 204 is null", await client.DeleteAsync<ApiResponse<object>>("api/a") is null));

        handler.Reply(HttpStatusCode.NoContent, null);
        await Step("POST 204", async () => Check("POST 204 is null", await client.PostAsync<ApiResponse<object>>("api/a", new { }) is null));

        handler.Reply(HttpStatusCode.OK, "");
        await Step("PUT 200 with Content-Length 0", async () => Check("PUT 200 with Content-Length 0 is null", await client.PutAsync<ApiResponse<object>>("api/a", new { }) is null));

        handler.Reply(HttpStatusCode.NoContent, null);
        await Step("GET 204", async () => Check("GET 204 is null", await client.GetAsync<ApiResponse<object>>("api/a") is null));

        // Gateway error codes survive for every verb, with or without a caller key.
        async Task ExpectError<TException>(string name, Func<Task> call, int status, string? code) where TException : FlexOpsException
        {
            try
            {
                await call();
                failures.Add($"{name}: did not throw");
            }
            catch (TException ex)
            {
                Check(name, ex.StatusCode == status && ex.ErrorCode == code, $"status={ex.StatusCode}, code={ex.ErrorCode ?? "null"}");
            }
            catch (Exception ex)
            {
                failures.Add($"{name}: threw {ex.GetType().Name}, expected {typeof(TException).Name}");
            }
        }

        handler.Reply(HttpStatusCode.Conflict, """{"success":false,"message":"In flight","errorCode":"IDEMPOTENCY_CONFLICT"}""");
        await ExpectError<FlexOpsException>("POST without a key keeps errorCode", () => client.PostAsync<object>("api/a", new { }), 409, "IDEMPOTENCY_CONFLICT");

        // A key reused with a different request (Gateway #736): 422, sent once, surfaced with its code, never retried.
        handler.Reply((HttpStatusCode)422, """{"success":false,"message":"This Idempotency-Key was already used for a different request.","errorCode":"IDEMPOTENCY_KEY_REUSED"}""");
        var callsBefore422 = handler.Calls.Count;
        await ExpectError<FlexOpsException>("422 IDEMPOTENCY_KEY_REUSED keeps status and code",
            () => client.PostAsync<object>("api/shipping/labels", new { a = 2 }, default, "reused-key"), 422, "IDEMPOTENCY_KEY_REUSED");
        Check("422 IDEMPOTENCY_KEY_REUSED is sent exactly once", handler.Calls.Count == callsBefore422 + 1, $"{handler.Calls.Count - callsBefore422} calls");

        handler.Reply(HttpStatusCode.BadRequest, """{"code":"PackageMeasurementsRequired","message":"Weight is required"}""");
        await ExpectError<FlexOpsException>("PUT keeps code", () => client.PutAsync<object>("api/a", new { }), 400, "PackageMeasurementsRequired");

        handler.Reply(HttpStatusCode.NotFound, """{"success":false,"message":"Not found","errorCode":"NotFound"}""");
        await ExpectError<FlexOpsException>("GET keeps errorCode", () => client.GetAsync<object>("api/a"), 404, "NotFound");

        handler.Reply(HttpStatusCode.Conflict, """{"success":false,"errorCode":"LabelAlreadyVoided"}""");
        await ExpectError<FlexOpsException>("DELETE keeps errorCode", () => client.DeleteAsync<object>("api/a"), 409, "LabelAlreadyVoided");

        handler.Reply(HttpStatusCode.Forbidden, """{"errorCode":"ApiKeyScopeRequired","message":"This API key lacks the required permission."}""");
        await ExpectError<FlexOpsAuthException>("403 is an auth error", () => client.PostAsync<object>("api/a", new { }), 403, "ApiKeyScopeRequired");

        handler.Reply(HttpStatusCode.InternalServerError, "<html>oops</html>");
        await ExpectError<FlexOpsException>("non-JSON error body", () => client.GetBytesAsync("api/a"), 500, null);

        if (failures.Count > 0)
        {
            throw new InvalidOperationException("HTTP policy checks failed:" + Environment.NewLine + string.Join(Environment.NewLine, failures));
        }

        Console.WriteLine("HTTP policy checks passed.");
    }

    /// <summary>Answers with queued responses and records each request's method, path and Idempotency-Key.</summary>
    sealed class ScriptedHandler : HttpMessageHandler
    {
        readonly Queue<(HttpStatusCode Status, string? Body)> _replies = new();

        public List<(string Method, string Path, string? Key)> Calls { get; } = [];

        public void Reply(HttpStatusCode status, string? body) => _replies.Enqueue((status, body));

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            var key = request.Headers.TryGetValues("Idempotency-Key", out var values) ? values.Single() : null;
            Calls.Add((request.Method.Method, request.RequestUri!.AbsolutePath, key));
            var (status, body) = _replies.Count > 0 ? _replies.Dequeue() : (HttpStatusCode.OK, "{}");
            var response = new HttpResponseMessage(status);
            if (body is not null)
            {
                response.Content = new StringContent(body, Encoding.UTF8, "application/json");
            }

            return Task.FromResult(response);
        }
    }

    /// <summary>What a caller's resilience handler does: resend the same request once after a 503.</summary>
    sealed class RetryOnceHandler(HttpMessageHandler inner) : DelegatingHandler(inner)
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            var response = await base.SendAsync(request, ct);
            if (response.StatusCode != HttpStatusCode.ServiceUnavailable)
            {
                return response;
            }

            response.Dispose();
            return await base.SendAsync(request, ct);
        }
    }
}
