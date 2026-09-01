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
