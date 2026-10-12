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


// Typed international customs survives serialization and the immutable approval snapshot.
var internationalJson = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "international-label.json"));
var serializerOptions = new System.Text.Json.JsonSerializerOptions(System.Text.Json.JsonSerializerDefaults.Web);
var internationalRequest = System.Text.Json.JsonSerializer.Deserialize<LabelRequest>(internationalJson, serializerOptions)!;
var internationalHandler = new ApprovalHandler(50);
using var internationalHttp = new HttpClient(internationalHandler) { BaseAddress = new Uri("https://example.test/") };
using var internationalClient = new FlexOpsTypedClient("https://example.test", httpClient: internationalHttp);
var internationalApproval = await internationalClient.Shipping.PrepareLabelAsync(internationalRequest, 50, "international-1");
using var sentInternational = System.Text.Json.JsonDocument.Parse(internationalHandler.Calls[0].Body);
var customs = sentInternational.RootElement.GetProperty("customsDeclaration");
if (internationalHandler.Calls.Count != 1 || customs.GetProperty("aesItn").GetString() != "NOEEI 30.37(a)"
    || customs.GetProperty("items")[0].GetProperty("value").GetDecimal() != 12.5m
    || sentInternational.RootElement.GetProperty("orderId").GetInt64() != 42)
    throw new Exception("International customs contract failed.");
Console.WriteLine("International typed approval contract passed.");

// HAZMAT declaration: sent only when the caller sets it (0 = None is an answer), never defaulted.
var hazmatHandler = new RecordingHandler();
using var hazmatHttp = new HttpClient(hazmatHandler) { BaseAddress = new Uri("https://example.test/") };
using var hazmatClient = new FlexOpsTypedClient("https://example.test", apiKey: "test-key", workspaceId: "ws_1", httpClient: hazmatHttp);
ShippingAddress Address(string line) => new() { AddressLine1 = line, City = "New York", StateProvince = "NY", PostalCode = "10001" };
LabelRequest HazmatLabel(HazmatClassification? hazmat) => new()
{
    Origin = Address("1 Main St"), Destination = Address("2 Main St"), Package = new() { Weight = 16 },
    CarrierCode = "USPS", ServiceCode = "PRIORITY", HazmatClassification = hazmat
};
RateShoppingRequest HazmatRate(HazmatClassification? hazmat) => new()
{
    Origin = Address("1 Main St"), Destination = Address("2 Main St"), Package = new() { Weight = 16 }, HazmatClassification = hazmat
};
await hazmatClient.Shipping.PrepareLabelAsync(HazmatLabel(null), 10m, "hazmat-unset");
await hazmatClient.Shipping.PrepareLabelAsync(HazmatLabel(HazmatClassification.None), 10m, "hazmat-none");
await hazmatClient.Shipping.CreateLabelAsync(HazmatLabel(HazmatClassification.LimitedQuantity));
await hazmatClient.Shipping.CreateLabelAsync(HazmatLabel(null));
await hazmatClient.Shipping.GetRatesAsync(HazmatRate(HazmatClassification.None));
await hazmatClient.Shipping.GetRatesAsync(HazmatRate(null));
await hazmatClient.Returns.GenerateLabelAsync("RMA-1", HazmatClassification.None);
await hazmatClient.Returns.GenerateLabelAsync("RMA-2");
int? SentHazmat(int call)
{
    using var sent = System.Text.Json.JsonDocument.Parse(hazmatHandler.Calls[call].Body);
    return sent.RootElement.ValueKind == System.Text.Json.JsonValueKind.Object && sent.RootElement.TryGetProperty("hazmatClassification", out var value)
        ? value.GetInt32() : null;
}
int?[] expectedHazmat = [null, 0, 1, null, 0, null, 0, null];
for (var call = 0; call < expectedHazmat.Length; call++)
{
    if (SentHazmat(call) != expectedHazmat[call])
        throw new Exception($"HAZMAT contract failed on call {call} ({hazmatHandler.Calls[call].Path}): sent {hazmatHandler.Calls[call].Body}");
}
if (hazmatHandler.Calls[6].Path != "/api/workspaces/ws_1/returns/RMA-1/label")
    throw new Exception("Return-label HAZMAT overload posted to the wrong route.");
Console.WriteLine("HAZMAT declaration contract passed.");

// Address validation sends the Gateway AddressValidationRequest: { "address": { ... } }.
await hazmatClient.Shipping.ValidateAddressAsync(Address("123 Main St"));
using (var validation = System.Text.Json.JsonDocument.Parse(hazmatHandler.Calls[^1].Body))
{
    if (hazmatHandler.Calls[^1].Path != "/api/shipping/addresses/validate" ||
        !validation.RootElement.TryGetProperty("address", out var wrapped) ||
        wrapped.GetProperty("addressLine1").GetString() != "123 Main St" ||
        wrapped.GetProperty("stateProvince").GetString() != "NY")
        throw new Exception($"Address validation contract failed: sent {hazmatHandler.Calls[^1].Body}");
}
Console.WriteLine("Address validation contract passed.");

sealed class RecordingHandler : HttpMessageHandler
{
    public List<(string Path, string Body)> Calls { get; } = [];
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        Calls.Add((request.RequestUri!.AbsolutePath, request.Content is null ? "" : await request.Content.ReadAsStringAsync(ct)));
        const string body = """{"success":true,"isSandbox":true,"labelId":"sandbox-1","trackingNumber":"sandbox-1","rates":[],"currency":"USD","isValid":true}""";
        return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(body, Encoding.UTF8, "application/json") };
    }
}

sealed class ApprovalHandler(decimal maximum = 10) : HttpMessageHandler
{
    public List<(string Key, string Body)> Calls { get; } = [];
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        Calls.Add((request.Headers.GetValues("Idempotency-Key").Single(), await request.Content!.ReadAsStringAsync(ct)));
        string body = Calls.Count == 1 ? """{"status":"Preview","quotedPostageAmount":8.5,"maximumPostageAmount":10,"currency":"USD","expiresAt":"2099-01-01T00:00:00Z","confirmationToken":"signed-preview"}"""
            : Calls.Count == 2 ? """{"errorCode":"OutcomeUnknown","message":"Carrier outcome requires reconciliation"}"""
            : """{"trackingNumber":"tracking-1","rate":8.5}""";
        body = body.Replace("\"maximumPostageAmount\":10", "\"maximumPostageAmount\":" + maximum.ToString(System.Globalization.CultureInfo.InvariantCulture), StringComparison.Ordinal);
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
