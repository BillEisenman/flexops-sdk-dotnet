using System.Net;
using System.Text;
using FlexOps.Sdk;

var handler = new CaptureHandler();
using var http = new HttpClient(handler) { BaseAddress = new Uri("https://example.test/") };
using var client = new FlexOpsTypedClient("https://example.test", apiKey: "test-key", httpClient: http);

var response = await client.Shipping.GetRatesAsync(new RateShoppingRequest
{
    Origin = new() { AddressLine1 = "123 Main St", City = "New York", StateProvince = "NY", PostalCode = "10001" },
    Destination = new() { AddressLine1 = "456 Oak Ave", City = "Los Angeles", StateProvince = "CA", PostalCode = "90210" },
    Package = new() { Weight = 16, WeightUnit = "oz" }
});

if (handler.Path != "/api/shipping/rates" ||
    handler.Body is null ||
    !handler.Body.Contains("\"origin\"", StringComparison.Ordinal) ||
    !handler.Body.Contains("\"destination\"", StringComparison.Ordinal) ||
    !handler.Body.Contains("\"package\"", StringComparison.Ordinal) ||
    response?.Rates.Count != 1 ||
    response.Rates[0].CarrierCode != "ups")
{
    throw new InvalidOperationException("Rate-shopping contract check failed.");
}

// Bounded label approval: no purchase before approval, immutable snapshot and same-key retry.
var labelHandler = new ApprovalHandler();
using var labelHttp = new HttpClient(labelHandler) { BaseAddress = new Uri("https://example.test/") };
using var labelClient = new FlexOpsTypedClient("https://example.test", httpClient: labelHttp);
var input = new Dictionary<string, object> { ["carrierCode"] = "USPS", ["ConfirmationToken"] = "remove-this" };
var approval = await labelClient.Shipping.PrepareLabelAsync(input, 10m, "stable-label-key");
if (labelHandler.Calls.Count != 1 || labelHandler.Calls[0].Body.Contains("confirmationToken", StringComparison.OrdinalIgnoreCase)) throw new Exception("Preview dispatched approval.");
input["carrierCode"] = "UPS";
try { await labelClient.Shipping.PurchaseLabelAsync(approval); throw new Exception("Expected unknown outcome"); }
catch (FlexOpsException e) when (e.ErrorCode == "OutcomeUnknown") { }
var purchased = await labelClient.Shipping.PurchaseLabelAsync(approval);
if (purchased?.TrackingNumber != "tracking-1" || labelHandler.Calls[1] != labelHandler.Calls[2] ||
    labelHandler.Calls.Any(c => c.Key != "stable-label-key") || labelHandler.Calls[1].Body.Contains("UPS")) throw new Exception("Purchase snapshot/replay failed.");
Console.WriteLine("Rate and bounded-label contract checks passed.");

sealed class ApprovalHandler : HttpMessageHandler
{
    public List<(string Key, string Body)> Calls { get; } = [];
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        Calls.Add((request.Headers.GetValues("Idempotency-Key").Single(), await request.Content!.ReadAsStringAsync(ct)));
        string body = Calls.Count == 1 ? """{"status":"Preview","quotedPostageAmount":8.5,"maximumPostageAmount":10,"currency":"USD","expiresAt":"2099-01-01T00:00:00Z","confirmationToken":"signed-preview"}"""
            : Calls.Count == 2 ? """{"errorCode":"OutcomeUnknown","message":"Carrier outcome requires reconciliation"}"""
            : """{"trackingNumber":"tracking-1","rate":8.5}""";
        return new HttpResponseMessage(Calls.Count == 2 ? HttpStatusCode.Conflict : HttpStatusCode.OK) { Content = new StringContent(body, Encoding.UTF8, "application/json") };
    }
}

sealed class CaptureHandler : HttpMessageHandler
{
    public string? Path { get; private set; }
    public string? Body { get; private set; }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Path = request.RequestUri?.AbsolutePath;
        Body = request.Content is null ? null : await request.Content.ReadAsStringAsync(cancellationToken);
        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                "{\"currency\":\"USD\",\"rates\":[{\"carrierCode\":\"ups\",\"carrierName\":\"UPS\",\"serviceCode\":\"ground\",\"serviceName\":\"Ground\",\"rate\":8.42,\"currency\":\"USD\"}]}",
                Encoding.UTF8,
                "application/json")
        };
    }
}
