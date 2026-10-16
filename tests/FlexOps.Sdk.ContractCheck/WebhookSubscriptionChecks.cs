using System.Net;
using System.Text;
using FlexOps.Sdk;

/// <summary>
/// Webhook subscription requests and responses against the Gateway contract
/// (WebhookSubscriptionsController, WebhookDTOs.cs). The Gateway reads <c>eventTypes</c>, one
/// comma-separated string, and ignores an <c>events</c> field, which subscribes to every event. It matches
/// <c>("," + EventTypes + ",").Contains("," + name + ",")</c>, so names must be the exact PascalCase
/// <c>WebhookEventType</c> names with no spaces. Create answers 201 with the bare subscription and its secret;
/// list and update answer with the ApiResponse envelope.
/// </summary>
static class WebhookSubscriptionChecks
{
    const string Hook = "https://example.com/hook";

    const string CreatedBody = """{"secret":"whsec_test","id":"3f2c1a9e-0000-4000-8000-000000000001","url":"https://example.com/hook","description":"orders","eventTypes":"LabelCreated,BatchCompleted","status":"Active","consecutiveFailures":0,"createdAt":"2026-10-16T12:00:00Z","updatedAt":null}""";
    const string UpdatedBody = """{"success":true,"data":{"id":"3f2c1a9e-0000-4000-8000-000000000001","url":"https://example.com/hook","description":null,"eventTypes":"LabelCanceled","status":"Active","consecutiveFailures":0,"createdAt":"2026-10-16T12:00:00Z","updatedAt":"2026-10-16T12:05:00Z"}}""";
    const string ListBody = """{"success":true,"data":[{"id":"3f2c1a9e-0000-4000-8000-000000000001","url":"https://example.com/hook","description":null,"eventTypes":"*","status":"Paused","consecutiveFailures":10,"createdAt":"2026-10-16T12:00:00Z","updatedAt":null}]}""";

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

        var handler = new SubscriptionHandler();
        using var http = new HttpClient(handler) { BaseAddress = new Uri("https://example.test/") };
        using var client = new FlexOpsTypedClient("https://example.test", apiKey: "test-key", workspaceId: "ws_1", httpClient: http);

        // Create: events go out as eventTypes, comma-joined with no spaces, duplicates dropped, order kept.
        handler.Respond(HttpStatusCode.Created, CreatedBody);
        var created = await client.Webhooks.CreateAsync(new CreateWebhookRequest
        {
            Url = Hook,
            Description = "orders",
            Events = [WebhookEventType.LabelCreated, WebhookEventType.BatchCompleted, WebhookEventType.LabelCreated],
        });
        var call = handler.Calls[^1];
        Check("create method and route", call.Method == "POST" && call.Path == "/api/workspaces/ws_1/webhooks", $"{call.Method} {call.Path}");
        Check("create body", call.Body == """{"url":"https://example.com/hook","description":"orders","eventTypes":"LabelCreated,BatchCompleted"}""", call.Body);

        // Create answers with the bare subscription; the secret must survive.
        var subscription = created?.Data;
        Check("create response is read", created?.Success == true && subscription is not null, "Data was null");
        Check("create response fields",
            subscription?.Secret == "whsec_test" && subscription.Id == "3f2c1a9e-0000-4000-8000-000000000001" &&
            subscription.EventTypes == "LabelCreated,BatchCompleted" && subscription.Status == "Active" &&
            subscription.Description == "orders" && subscription.CreatedAt == "2026-10-16T12:00:00Z" && subscription.UpdatedAt is null,
            $"secret={subscription?.Secret}, eventTypes={subscription?.EventTypes}, status={subscription?.Status}");

        // No events: no eventTypes, so the Gateway subscribes to every event ("*").
        handler.Respond(HttpStatusCode.Created, CreatedBody);
        await client.Webhooks.CreateAsync(new CreateWebhookRequest { Url = Hook });
        Check("create without events", handler.Calls[^1].Body == """{"url":"https://example.com/hook"}""", handler.Calls[^1].Body);

        // An enveloped create response is read too.
        handler.Respond(HttpStatusCode.Created, UpdatedBody);
        var enveloped = await client.Webhooks.CreateAsync(new CreateWebhookRequest { Url = Hook, Events = [WebhookEventType.LabelCanceled] });
        Check("enveloped create response", enveloped?.Data?.EventTypes == "LabelCanceled", enveloped?.Data?.EventTypes ?? "null");

        // The untyped overload sends its payload as given, and its bare create response is read.
        handler.Respond(HttpStatusCode.Created, CreatedBody);
        var untyped = await client.Webhooks.CreateAsync(new { url = Hook, eventTypes = "LabelCreated" });
        Check("untyped create body", handler.Calls[^1].Body == """{"url":"https://example.com/hook","eventTypes":"LabelCreated"}""", handler.Calls[^1].Body);
        Check("untyped create response", untyped?.Data?.Secret == "whsec_test", untyped?.Data?.Secret ?? "null");

        // Update: only the set fields, events as eventTypes.
        handler.Respond(HttpStatusCode.OK, UpdatedBody);
        var updated = await client.Webhooks.UpdateAsync("wh-1", new UpdateWebhookRequest
        {
            Events = [WebhookEventType.LabelCanceled],
            Status = "Active",
        });
        call = handler.Calls[^1];
        Check("update method and route", call.Method == "PUT" && call.Path == "/api/workspaces/ws_1/webhooks/wh-1", $"{call.Method} {call.Path}");
        Check("update body", call.Body == """{"eventTypes":"LabelCanceled","status":"Active"}""", call.Body);
        Check("update response", updated?.Data?.EventTypes == "LabelCanceled" && updated.Data.UpdatedAt == "2026-10-16T12:05:00Z" && updated.Data.Secret is null,
            updated?.Data?.EventTypes ?? "null");

        // List: the Gateway's eventTypes/status/consecutiveFailures fields.
        handler.Respond(HttpStatusCode.OK, ListBody);
        var list = await client.Webhooks.ListAsync();
        var first = list?.Data?.FirstOrDefault();
        Check("list response", first?.EventTypes == "*" && first.Status == "Paused" && first.ConsecutiveFailures == 10,
            $"eventTypes={first?.EventTypes}, status={first?.Status}, failures={first?.ConsecutiveFailures}");

        // Refusals happen before any HTTP call.
        async Task Refused(string name, Func<Task> action)
        {
            var before = handler.Calls.Count;
            try
            {
                await action();
                failures.Add($"{name}: did not throw ArgumentException");
            }
            catch (ArgumentException ex) when (ex is not ArgumentNullException)
            {
                Check($"{name} names the parameter", ex.ParamName == "request", ex.ParamName ?? "null");
            }
            catch (Exception ex)
            {
                failures.Add($"{name}: threw {ex.GetType().Name}, expected ArgumentException");
            }

            Check($"{name} sends nothing", handler.Calls.Count == before, $"{handler.Calls.Count - before} call(s)");
        }

        await Refused("create with an empty list", () => client.Webhooks.CreateAsync(new CreateWebhookRequest { Url = Hook, Events = [] }));
        await Refused("create with an undefined event", () => client.Webhooks.CreateAsync(new CreateWebhookRequest
        {
            Url = Hook,
            Events = [WebhookEventType.LabelCreated, (WebhookEventType)99],
        }));
        await Refused("update with an empty list", () => client.Webhooks.UpdateAsync("wh-1", new UpdateWebhookRequest { Events = [] }));
        await Refused("update with an undefined event", () => client.Webhooks.UpdateAsync("wh-1", new UpdateWebhookRequest { Events = [(WebhookEventType)5] }));

        // The names FlexOps sends, in the X-FlexOps-Event header and in eventTypes.
        string[] expectedNames = ["LabelCreated", "LabelCanceled", "BatchCompleted", "TrackingUpdated", "TrackingDelivered"];
        Check("event names", Enum.GetNames<WebhookEventType>().SequenceEqual(expectedNames), string.Join(",", Enum.GetNames<WebhookEventType>()));

        if (failures.Count > 0)
        {
            throw new InvalidOperationException("Webhook subscription checks failed:" + Environment.NewLine + string.Join(Environment.NewLine, failures));
        }

        Console.WriteLine("Webhook subscription checks passed.");
    }

    sealed class SubscriptionHandler : HttpMessageHandler
    {
        HttpStatusCode _status = HttpStatusCode.OK;
        string _body = "{}";

        public List<(string Method, string Path, string Body)> Calls { get; } = [];

        public void Respond(HttpStatusCode status, string body) => (_status, _body) = (status, body);

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            Calls.Add((request.Method.Method, request.RequestUri!.AbsolutePath, request.Content is null ? "" : await request.Content.ReadAsStringAsync(ct)));
            return new HttpResponseMessage(_status) { Content = new StringContent(_body, Encoding.UTF8, "application/json") };
        }
    }
}
