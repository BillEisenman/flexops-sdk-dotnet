using System.Net;
using System.Reflection;
using System.Text.Json;
using FlexOps.Sdk;

/// <summary>
/// Contract test: every public SDK resource method calls an operation in the documented Gateway OpenAPI.
/// </summary>
/// <remarks>
/// <para>
/// Fixtures/gateway-operations.json is the sorted "METHOD /path" list of the Developer Docs snapshot
/// (FlexOpsDeveloperDocs static/openapi/gateway.json, commit in its <c>sourceCommit</c>). Refresh it by regenerating
/// that list from the current snapshot when the Gateway's documented surface changes.
/// </para>
/// <para>
/// Methods are found by reflection: every public instance method declared on every resource reachable from
/// <see cref="FlexOpsTypedClient"/>'s properties is invoked with dummy arguments, so a new method is exercised
/// without being listed. A new parameter type with no dummy, a resource type not reachable from the typed client,
/// or a method that sends no request fails the check.
/// </para>
/// <para>
/// The legacy <c>api/ApiProxy/{path}</c> catch-all: a GET may come from any method. A POST, PUT or DELETE may come
/// only from a method marked <see cref="ObsoleteAttribute"/> (the deprecated carrier writes, which need a user token),
/// never on a closed label-generation suffix (the Gateway answers 410 Gone), and a PATCH never (the proxy has none).
/// </para>
/// </remarks>
static class GatewayContractChecks
{
    const string ProxyPrefix = "/api/apiproxy/";

    /// <summary>
    /// Label-generation suffixes the Gateway closes with 410 Gone (<c>ApiProxyLegacyRouteSemantics</c>;
    /// <c>ApiProxy:CloseAllLabelSuffixes</c> defaults to true). Matched case-insensitively.
    /// </summary>
    static readonly string[] ClosedLabelSuffixes =
    [
        "generateNewUpsShipLabel", "postNewShipLabelOrder", "postUspsGenerateDomesticShippingLabel",
        "postUspsGenerateDomesticReturnsShippingLabel", "postUspsGenerateInternationalShippingLabel",
        "postUspsGenerateDomesticIndiciaLabel", "postFedExCreateNewShipment", "postFedExCreateNewReturnShipment",
        "postDhlCreateShipment", "postOnTracCreateShipment", "postLsoCreateShipment", "postAusPostCreateShipment",
        "postCanadaPostCreateShipment", "postRoyalMailCreateShipment",
    ];

    const string DeprecationMessage = "Legacy carrier proxy (sunset 2027-06-30). Needs a user token: API keys get 403 API_KEY_SCOPE_REQUIRED.";

    public static async Task RunAsync()
    {
        var failures = new List<string>();
        var operations = LoadOperations();

        var handler = new NoContentHandler();
        using var http = new HttpClient(handler) { BaseAddress = new Uri("https://example.test/") };
        using var client = new FlexOpsTypedClient("https://example.test", apiKey: "test-key", workspaceId: "ws_1", httpClient: http);

        var resources = FindResources(client);
        var exercised = 0;
        var proxyWrites = 0;
        foreach (var (resourcePath, resource) in resources)
        {
            var methods = resource.GetType()
                .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(m => !m.IsSpecialName)
                .OrderBy(m => m.ToString(), StringComparer.Ordinal);
            foreach (var method in methods)
            {
                var name = $"{resourcePath}.{method.Name}({string.Join(", ", method.GetParameters().Select(p => p.ParameterType.Name))})";
                var obsolete = method.GetCustomAttribute<ObsoleteAttribute>();
                object?[] args;
                try
                {
                    args = method.GetParameters().Select(p => Dummy(p.ParameterType)).ToArray();
                }
                catch (NotSupportedException ex)
                {
                    failures.Add($"{name}: {ex.Message}");
                    continue;
                }

                var before = handler.Calls.Count;
                try
                {
                    if (method.Invoke(resource, args) is Task task)
                    {
                        await task;
                    }
                }
                catch (Exception ex) when (handler.Calls.Count > before)
                {
                    // The request was sent; a 204 answer can be refused afterwards (for example an unresolved purchase).
                    _ = ex;
                }
                catch (Exception ex)
                {
                    var inner = ex is TargetInvocationException { InnerException: { } e } ? e : ex;
                    failures.Add($"{name}: threw {inner.GetType().Name} before sending a request: {inner.Message}");
                    continue;
                }

                if (handler.Calls.Count == before)
                {
                    failures.Add($"{name}: sent no request");
                    continue;
                }

                exercised++;
                foreach (var (verb, path) in handler.Calls.Skip(before))
                {
                    var problem = Classify(verb, path, operations, obsolete is not null);
                    if (problem is not null)
                    {
                        failures.Add($"{name}: {verb} {path} {problem}");
                    }

                    if (verb != "GET" && Normalize(path).StartsWith(ProxyPrefix, StringComparison.OrdinalIgnoreCase))
                    {
                        proxyWrites++;
                        if (obsolete?.Message?.StartsWith(DeprecationMessage, StringComparison.Ordinal) != true)
                        {
                            failures.Add($"{name}: a legacy proxy write must carry the deprecation message in its [Obsolete] attribute");
                        }
                    }
                }
            }
        }

        // Every public resource type must hang off the typed client, or its methods would never be checked.
        var reached = resources.Select(r => r.Resource.GetType()).ToHashSet();
        var unreached = typeof(FlexOpsTypedClient).Assembly.GetExportedTypes()
            .Where(t => t.Namespace?.StartsWith("FlexOps.Sdk.Resources", StringComparison.Ordinal) == true && t.IsClass && !t.IsAbstract)
            .Where(t => t.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Any(m => !m.IsSpecialName))
            .Where(t => !reached.Contains(t));
        failures.AddRange(unreached.Select(t => $"{t.Name}: not reachable from FlexOpsTypedClient, so its methods are not checked"));

        if (exercised < 100)
        {
            failures.Add($"only {exercised} methods were exercised; the reflection walk is broken");
        }

        if (failures.Count > 0)
        {
            throw new InvalidOperationException("Gateway contract checks failed:" + Environment.NewLine + string.Join(Environment.NewLine, failures));
        }

        Console.WriteLine($"Gateway contract checks passed: {exercised} methods, {handler.Calls.Count} calls, all documented; " +
            $"{proxyWrites} proxy writes, all [Obsolete], none on a closed label route.");
    }

    /// <summary>Returns why a call is not documented, or <c>null</c> when it matches an operation.</summary>
    static string? Classify(string verb, string path, IReadOnlyList<(string Method, string[] Segments)> operations, bool obsolete)
    {
        var normalized = Normalize(path);
        if (normalized.StartsWith(ProxyPrefix, StringComparison.OrdinalIgnoreCase))
        {
            // The legacy proxy is a catch-all. The Gateway refuses API-key writes through it, so only deprecated
            // (user-token) methods may write, and never to a label route it has closed.
            if (!operations.Any(o => o.Method == verb && string.Join('/', o.Segments).Equals("/api/apiproxy/{path}", StringComparison.OrdinalIgnoreCase)))
            {
                return "is not a documented ApiProxy verb";
            }

            if (verb == "GET")
            {
                return null;
            }

            if (!obsolete)
            {
                return "is a write through the legacy ApiProxy from a method that is not [Obsolete]; API keys get 403 there";
            }

            var suffix = normalized.TrimEnd('/').Split('/')[^1];
            return ClosedLabelSuffixes.Any(c => suffix.Equals(c, StringComparison.OrdinalIgnoreCase))
                ? "is a closed label-generation route (410 Gone)"
                : null;
        }

        var segments = normalized.Split('/');
        return operations.Any(o => o.Method == verb && Matches(o.Segments, segments)) ? null : "is not a documented Gateway operation";
    }

    static bool Matches(string[] template, string[] segments)
        => template.Length == segments.Length && template.Zip(segments).All(pair =>
            (pair.First.StartsWith('{') && pair.First.EndsWith('}') && pair.Second.Length > 0) ||
            pair.First.Equals(pair.Second, StringComparison.OrdinalIgnoreCase));

    /// <summary>Strips the query and maps a leading <c>/api/v1</c> to <c>/api</c>.</summary>
    static string Normalize(string path)
    {
        var withoutQuery = path.Split('?')[0];
        return withoutQuery.StartsWith("/api/v1/", StringComparison.OrdinalIgnoreCase) ? "/api/" + withoutQuery["/api/v1/".Length..] : withoutQuery;
    }

    static List<(string Method, string[] Segments)> LoadOperations()
    {
        using var document = JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", "gateway-operations.json")));
        return document.RootElement.GetProperty("operations").EnumerateArray()
            .Select(o => o.GetString()!.Split(' ', 2))
            .Select(parts => (parts[0], Normalize(parts[1]).Split('/')))
            .ToList();
    }

    /// <summary>The typed client's resources, and their nested resources (for example <c>Carriers.Usps</c>).</summary>
    static List<(string Path, object Resource)> FindResources(FlexOpsTypedClient client)
    {
        var found = new List<(string, object)>();
        void Walk(string prefix, object owner)
        {
            foreach (var property in owner.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (property.PropertyType.Namespace?.StartsWith("FlexOps.Sdk.Resources", StringComparison.Ordinal) != true ||
                    property.GetValue(owner) is not { } resource)
                {
                    continue;
                }

                var path = prefix.Length == 0 ? property.Name : $"{prefix}.{property.Name}";
                found.Add((path, resource));
                Walk(path, resource);
            }
        }

        Walk("", client);
        return found;
    }

    static readonly ShippingAddress Address = new() { AddressLine1 = "1 Main St", City = "New York", StateProvince = "NY", PostalCode = "10001" };

    /// <summary>A dummy argument for each parameter type the resources take. Add one when a new type appears.</summary>
    static object? Dummy(Type type)
    {
        if (Nullable.GetUnderlyingType(type) is { } underlying) return Dummy(underlying);
        if (type == typeof(string)) return "id-1";
        if (type == typeof(DateTime)) return new DateTime(2026, 10, 1, 0, 0, 0, DateTimeKind.Utc);
        if (type == typeof(CancellationToken)) return CancellationToken.None;
        if (type == typeof(int)) return 1;
        if (type == typeof(long)) return 1L;
        if (type == typeof(decimal)) return 10m;
        if (type == typeof(object)) return new { carrierCode = "USPS" };
        if (type.IsEnum) return Enum.GetValues(type).GetValue(0);
        if (type == typeof(IReadOnlyDictionary<string, string>)) return new Dictionary<string, string> { ["key"] = "value" };
        if (type == typeof(IEnumerable<RulePriority>)) return new[] { new RulePriority { RuleId = Guid.NewGuid(), Priority = 1 } };
        if (type == typeof(ShippingAddress)) return Address;
        if (type == typeof(RateShoppingRequest)) return new RateShoppingRequest { Origin = Address, Destination = Address, Package = new() { Weight = 16 } };
        if (type == typeof(LabelPurchaseApproval)) return new LabelPurchaseApproval("key-1", """{"carrierCode":"USPS"}""", default, null);
        if (type == typeof(AddressValidationRequest)) return new AddressValidationRequest { Line1 = "1 Main St", City = "New York", PostalCode = "10001" };
        if (type == typeof(CreateWebhookRequest)) return new CreateWebhookRequest { Url = "https://example.com/hook" };
        if (type == typeof(UpdateWebhookRequest)) return new UpdateWebhookRequest { Status = "Active" };
        if (type == typeof(InsuranceQuoteRequest)) return new InsuranceQuoteRequest { DeclaredValue = 10m, CarrierCode = "usps" };
        if (type == typeof(PurchaseInsuranceRequest)) return new PurchaseInsuranceRequest { TrackingNumber = "9400", CarrierCode = "usps", DeclaredValue = 10m };
        if (type == typeof(FileInsuranceClaimRequest)) return new FileInsuranceClaimRequest { PolicyId = Guid.NewGuid(), ClaimReason = "Lost", ClaimAmount = 10m };
        if (type == typeof(CreateRmaRequest)) return new CreateRmaRequest { OrderId = 1, Reason = "Damaged", Items = [new RmaItem { Sku = "SKU-1" }] };
        if (type == typeof(ReturnLabelRequest)) return new ReturnLabelRequest { WeightOz = 8, LengthIn = 6, WidthIn = 4, HeightIn = 2 };
        if (type == typeof(ReceiveRmaRequest)) return new ReceiveRmaRequest { RmaNumber = "RMA-1", InspectedItems = [new ReceivedRmaItem { PartNumber = "SKU-1", Quantity = 1 }] };
        if (type == typeof(CreateShippingRuleRequest)) return new CreateShippingRuleRequest { Name = "Rule", Conditions = [new RuleCondition { Field = "weightOz", Operator = "gt", Value = "1" }], Actions = [new RuleAction { Type = "setCarrier", Value = "USPS" }] };
        if (type == typeof(UpdateShippingRuleRequest)) return new UpdateShippingRuleRequest { IsEnabled = true };
        if (type == typeof(TestShippingRuleRequest)) return new TestShippingRuleRequest { WeightOz = 16 };
        if (type == typeof(SchedulePickupRequest)) return new SchedulePickupRequest { CarrierCode = "usps", PickupDate = DateTime.UtcNow.Date, Street = "1 Main St", City = "New York", State = "NY", PostalCode = "10001" };
        if (type == typeof(LandedCostRequest)) return new LandedCostRequest { DestinationCountry = "GB", Items = [new CustomsItem { Description = "Shirt", Quantity = 1, OriginCountry = "US" }] };
        if (type == typeof(CarbonOffsetQuoteRequest)) return new CarbonOffsetQuoteRequest { WeightLbs = 1m, DistanceMiles = 100m };
        if (type == typeof(CarbonOffsetPurchaseRequest)) return new CarbonOffsetPurchaseRequest { TrackingNumber = "9400", WeightLbs = 1m, DistanceMiles = 100m };
        if (type == typeof(CarbonOffsetSettings)) return new CarbonOffsetSettings { AutoOffsetEnabled = true };
        if (type == typeof(CreateRecurringShipmentRequest)) return new CreateRecurringShipmentRequest { Name = "Weekly", FirstRunUtc = new DateTime(2026, 11, 1, 0, 0, 0, DateTimeKind.Utc) };
        if (type == typeof(UpdateRecurringShipmentRequest)) return new UpdateRecurringShipmentRequest { Name = "Weekly" };
        if (type == typeof(CreateReportScheduleRequest)) return new CreateReportScheduleRequest { ReportType = "shipments", TimeOfDayUtc = TimeSpan.FromHours(6), RecipientEmails = "ops@example.com" };
        if (type == typeof(UpdateReportScheduleRequest)) return new UpdateReportScheduleRequest { RecipientEmails = "ops@example.com" };
        if (type == typeof(UpsertEmailTemplateRequest)) return new UpsertEmailTemplateRequest { SubjectTemplate = "Shipped", HtmlBodyTemplate = "<p>Shipped</p>" };
        if (type == typeof(EmailTemplatePreviewRequest)) return new EmailTemplatePreviewRequest { SubjectTemplate = "Shipped", HtmlBodyTemplate = "<p>Shipped</p>" };
        throw new NotSupportedException($"no dummy argument for {type.Name}; add one to GatewayContractChecks.Dummy");
    }

    /// <summary>Records each request's method and path, and answers 204 No Content.</summary>
    sealed class NoContentHandler : HttpMessageHandler
    {
        public List<(string Method, string Path)> Calls { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            Calls.Add((request.Method.Method, request.RequestUri!.AbsolutePath));
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NoContent));
        }
    }
}
