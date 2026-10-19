using System.Net;
using System.Text;
using FlexOps.Sdk;

/// <summary>
/// Routes, bodies, headers and response shapes of the calls fixed or added for the documented Gateway API
/// (FlexOpsGateway master 48d5dd4f). Enums go out as numbers. Bodies are compared as exact JSON, so a renamed or
/// extra field fails.
/// </summary>
static class ResourceRouteChecks
{
    const string Ws = "ws_1";

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

        var handler = new RouteHandler();
        using var http = new HttpClient(handler) { BaseAddress = new Uri("https://example.test/") };
        using var client = new FlexOpsTypedClient("https://example.test", apiKey: "test-key", workspaceId: Ws, httpClient: http);

        // Expect one call: method, path with query, and the exact body ("" for none).
        void Expect(string name, string method, string pathAndQuery, string body = "")
        {
            var call = handler.Calls[^1];
            Check($"{name} route", call.Method == method && call.PathAndQuery == pathAndQuery, $"{call.Method} {call.PathAndQuery}");
            Check($"{name} body", call.Body == body, call.Body);
        }

        // Wallet: GET /api/wallet/my-balance with X-Workspace-Id, bare response.
        handler.Respond("""{"hasWallet":true,"balanceCents":1250,"balance":12.50}""");
        var balance = await client.Wallet.GetBalanceAsync();
        Expect("wallet balance", "GET", "/api/wallet/my-balance");
        Check("wallet X-Workspace-Id", handler.Calls[^1].WorkspaceHeader == Ws, handler.Calls[^1].WorkspaceHeader ?? "missing");
        Check("wallet response", balance is { HasWallet: true, BalanceCents: 1250, Balance: 12.50m }, $"{balance?.HasWallet} {balance?.BalanceCents} {balance?.Balance}");
        using (var noWorkspace = new FlexOpsTypedClient("https://example.test", httpClient: http))
        {
            var before = handler.Calls.Count;
            try
            {
                await noWorkspace.Wallet.GetBalanceAsync();
                failures.Add("wallet without a workspace: did not throw");
            }
            catch (InvalidOperationException)
            {
            }

            Check("wallet without a workspace sends nothing", handler.Calls.Count == before);
        }

        // HS codes and landed cost: /api/shipping/..., no workspace in the path, bare responses.
        handler.Respond("""[{"hsCode":"6109.10","description":"T-shirts, cotton","chapter":"61","chapterDescription":"Apparel, knitted","dutyRatePercent":12}]""");
        var codes = await client.HsCodes.SearchAsync("cotton shirt", "GB", 5);
        Expect("hs search", "GET", "/api/shipping/hs-codes/search?query=cotton%20shirt&destinationCountry=GB&maxResults=5");
        Check("hs search response", codes is [{ HsCode: "6109.10", DutyRatePercent: 12m }], codes?.Count.ToString() ?? "null");
        handler.Respond("""{"hsCode":"6109.10","description":"T-shirts, cotton","chapter":"61","chapterDescription":"Apparel, knitted"}""");
        var code = await client.HsCodes.LookupAsync("6109.10");
        Expect("hs lookup", "GET", "/api/shipping/hs-codes/6109.10");
        Check("hs lookup response", code?.Chapter == "61", code?.Chapter ?? "null");
        handler.Respond("""{"shippingCost":20,"estimatedDuty":3,"estimatedTax":4,"totalLandedCost":27,"currency":"USD","isEstimate":true}""");
        var landed = await client.HsCodes.EstimateLandedCostAsync(new LandedCostRequest
        {
            DestinationCountry = "GB",
            Items = [new CustomsItem { Description = "T-shirt", Quantity = 2, Value = 12.5m, WeightOz = 6, HsCode = "6109.10", OriginCountry = "US", SkuOrProductId = "TS-1" }],
            ShippingCost = 20m,
        });
        Expect("landed cost", "POST", "/api/shipping/landed-cost",
            """{"destinationCountry":"GB","items":[{"description":"T-shirt","quantity":2,"value":12.5,"weightOz":6,"hsCode":"6109.10","originCountry":"US","skuOrProductId":"TS-1"}],"shippingCost":20}""");
        Check("landed cost response", landed?.TotalLandedCost == 27m, landed?.TotalLandedCost.ToString() ?? "null");

        // Address validation: the documented POST /api/addresses/validate, bare AddressValidationResult.
        handler.Respond("""{"isValid":true,"suggestedAddress":{"line1":"1 MAIN ST","city":"ALBANY","state":"NY","postalCode":"12207-1234","country":"US"},"isResidential":false,"warnings":["Suite missing"],"validatedBy":"USPS","validationUnavailable":false,"checkedAt":"2026-10-19T12:00:00+00:00"}""");
        var validated = await client.Shipping.ValidateAddressAsync(new AddressValidationRequest { Name = "Acme", Line1 = "1 Main St", Line2 = "Ste 2", City = "Albany", State = "NY", PostalCode = "12207" });
        Expect("validate address", "POST", "/api/addresses/validate",
            """{"name":"Acme","line1":"1 Main St","line2":"Ste 2","city":"Albany","state":"NY","postalCode":"12207","country":"US"}""");
        Check("validate address response",
            validated is { IsValid: true, IsResidential: false, ValidatedBy: "USPS", SuggestedAddress.PostalCode: "12207-1234", Warnings: ["Suite missing"] } && validated.CheckedAt is not null,
            validated?.SuggestedAddress?.PostalCode ?? "null");

        // Insurance (envelope).
        handler.Respond("""{"success":true,"data":{"premium":1.5}}""");
        await client.Insurance.GetQuoteAsync(new InsuranceQuoteRequest { DeclaredValue = 150m, CarrierCode = "usps", TrackingNumber = "9400" });
        Expect("insurance quote", "POST", $"/api/workspaces/{Ws}/insurance/quote", """{"declaredValue":150,"carrierCode":"usps","trackingNumber":"9400"}""");
        await client.Insurance.PurchaseAsync(new PurchaseInsuranceRequest { TrackingNumber = "9400", CarrierCode = "usps", DeclaredValue = 150m, CoverageTier = "Standard" }, "policy-key-1");
        Expect("insurance purchase", "POST", $"/api/workspaces/{Ws}/insurance/policies", """{"trackingNumber":"9400","carrierCode":"usps","declaredValue":150,"coverageTier":"Standard"}""");
        Check("insurance purchase keeps the caller's key", handler.Calls[^1].IdempotencyKey == "policy-key-1", handler.Calls[^1].IdempotencyKey ?? "none");
        var policyId = Guid.Parse("11111111-2222-3333-4444-555555555555");
        await client.Insurance.FileClaimAsync(new FileInsuranceClaimRequest { PolicyId = policyId, ClaimReason = "Damaged", ClaimAmount = 75m });
        Expect("insurance claim", "POST", $"/api/workspaces/{Ws}/insurance/claims", """{"policyId":"11111111-2222-3333-4444-555555555555","claimReason":"Damaged","claimAmount":75}""");
        await client.Insurance.VoidAsync(policyId.ToString());
        Expect("insurance void", "DELETE", $"/api/workspaces/{Ws}/insurance/policies/{policyId}");
        await client.Insurance.GetProvidersAsync();
        Expect("insurance providers", "GET", $"/api/workspaces/{Ws}/insurance/providers");

        // Returns.
        await client.Returns.CreateAsync(new CreateRmaRequest
        {
            OrderId = 1042,
            OrderNumber = "SO1042",
            Reason = "Wrong size",
            Items = [new RmaItem { Sku = "TS-1", Quantity = 2, Reason = "Too small" }],
            CustomerEmail = "a@example.com",
        });
        Expect("return create", "POST", $"/api/workspaces/{Ws}/returns/rma",
            """{"orderId":1042,"orderNumber":"SO1042","reason":"Wrong size","items":[{"sku":"TS-1","quantity":2,"reason":"Too small"}],"customerEmail":"a@example.com"}""");
        await client.Returns.GenerateLabelAsync("RMA-7", new ReturnLabelRequest { ServiceType = "PRIORITY", WeightOz = 8, LengthIn = 6, WidthIn = 4, HeightIn = 2, RateIndicator = "FS" });
        Expect("return label", "POST", $"/api/workspaces/{Ws}/returns/RMA-7/label",
            """{"serviceType":"PRIORITY","weightOz":8,"lengthIn":6,"widthIn":4,"heightIn":2,"rateIndicator":"FS"}""");
        await client.Returns.MarkReceivedAsync(42, new ReceiveRmaRequest
        {
            RmaNumber = "RMA-7",
            WarehouseId = 3,
            InspectedItems = [new ReceivedRmaItem { PartNumber = "TS-1", Quantity = 2, Disposition = ReturnDisposition.Forward }],
        });
        Expect("return receive", "POST", $"/api/workspaces/{Ws}/returns/42/receive",
            """{"rmaNumber":"RMA-7","warehouseId":3,"inspectedItems":[{"partNumber":"TS-1","quantity":2,"disposition":2}]}""");

        // Shipping rules.
        var ruleId = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");
        await client.Rules.CreateAsync(new CreateShippingRuleRequest
        {
            Name = "Heavy",
            Priority = 5,
            Conditions = [new RuleCondition { Field = "weightOz", Operator = "gt", Value = "80", Group = 1 }],
            Actions = [new RuleAction { Type = "setCarrier", Value = "UPS" }],
        });
        Expect("rule create", "POST", $"/api/workspaces/{Ws}/shipping-rules",
            """{"name":"Heavy","priority":5,"isEnabled":true,"conditions":[{"field":"weightOz","operator":"gt","value":"80","group":1}],"actions":[{"type":"setCarrier","value":"UPS"}]}""");
        await client.Rules.UpdateAsync(ruleId.ToString(), new UpdateShippingRuleRequest { IsEnabled = false });
        Expect("rule update", "PUT", $"/api/workspaces/{Ws}/shipping-rules/{ruleId}", """{"isEnabled":false}""");
        await client.Rules.ReorderAsync([new RulePriority { RuleId = ruleId, Priority = 1 }]);
        Expect("rule reorder", "POST", $"/api/workspaces/{Ws}/shipping-rules/reorder", """{"rules":[{"ruleId":"aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee","priority":1}]}""");
        await client.Rules.TestAsync(new TestShippingRuleRequest { WeightOz = 96, DestinationState = "CA", Tags = ["vip"] });
        Expect("rule test", "POST", $"/api/workspaces/{Ws}/shipping-rules/test",
            """{"weightOz":96,"declaredValue":0,"destinationCountry":"US","destinationState":"CA","itemCount":1,"tags":["vip"]}""");
        handler.RespondNoContent();
        var deletedRule = await client.Rules.DeleteAsync(ruleId.ToString());
        Expect("rule delete", "DELETE", $"/api/workspaces/{Ws}/shipping-rules/{ruleId}");
        Check("rule delete 204 is null", deletedRule is null);

        // Pickups.
        await client.Pickups.ScheduleAsync(new SchedulePickupRequest
        {
            CarrierCode = "usps",
            PickupDate = new DateTime(2026, 10, 21),
            Street = "1 Main St",
            City = "Albany",
            State = "NY",
            PostalCode = "12207",
            PackageCount = 3,
            TotalWeightOz = 48m,
            ContactPhone = "5185550100",
        });
        Expect("pickup schedule", "POST", $"/api/workspaces/{Ws}/pickups",
            """{"carrierCode":"usps","pickupDate":"2026-10-21T00:00:00","street":"1 Main St","city":"Albany","state":"NY","postalCode":"12207","country":"US","packageCount":3,"totalWeightOz":48,"contactPhone":"5185550100"}""");
        await client.Pickups.ListAsync();
        Expect("pickup list", "GET", $"/api/workspaces/{Ws}/pickups/scheduled");
        await client.Pickups.GetAsync("WTC123");
        Expect("pickup get", "GET", $"/api/workspaces/{Ws}/pickups/WTC123?carrierCode=usps");
        await client.Pickups.CancelAsync("WTC123", "ups");
        Expect("pickup cancel", "DELETE", $"/api/workspaces/{Ws}/pickups/WTC123?carrierCode=ups");

        // Workspace members: numeric roles, and the documented response containers.
        await client.Workspaces.InviteMemberAsync("new@example.com", WorkspaceRole.Admin);
        Expect("member invite", "POST", $"/api/workspaces/{Ws}/members/invite", """{"email":"new@example.com","role":1}""");
        await client.Workspaces.InviteMemberAsync("member@example.com");
        Expect("member invite default role", "POST", $"/api/workspaces/{Ws}/members/invite", """{"email":"member@example.com","role":2}""");
        await client.Workspaces.UpdateMemberRoleAsync("user-9", WorkspaceRole.Merchant);
        Expect("member role", "PUT", $"/api/workspaces/{Ws}/members/user-9/role", """{"newRole":3}""");
        handler.RespondNoContent();
        Check("member remove 204 is null", await client.Workspaces.RemoveMemberAsync("user-9") is null);
        Expect("member remove", "DELETE", $"/api/workspaces/{Ws}/members/user-9");
        handler.Respond("""{"success":true,"data":{"members":[{"membershipId":"m1","userId":"user-9","email":"a@example.com","role":3,"status":0,"joinedAt":"2026-01-01T00:00:00Z","isCreator":false,"isDeactivated":false,"storeScope":0,"storeConnectionIds":[]}],"pendingInvites":[{"inviteId":"i1","email":"b@example.com","role":1,"expiresAt":"2026-11-01T00:00:00Z","invitedByName":"Owner"}],"userLimit":5,"activeCount":1,"canInviteMore":true}}""");
        var members = await client.Workspaces.ListMembersAsync();
        Expect("member list", "GET", $"/api/workspaces/{Ws}/members");
        Check("member list response",
            members?.Data is { UserLimit: 5, CanInviteMore: true, Members: [{ UserId: "user-9", Role: WorkspaceRole.Merchant }], PendingInvites: [{ Role: WorkspaceRole.Admin }] },
            members?.Data?.Members.Count.ToString() ?? "null");
        handler.Respond("""{"success":true,"data":[{"id":"3f2c1a9e-0000-4000-8000-000000000001","name":"Acme","slug":"acme","myRole":0,"planId":"pro","memberCount":4,"isCreator":true,"entitlements":[]}]}""");
        var workspaces = await client.Workspaces.ListAsync();
        Expect("workspace list", "GET", "/api/workspaces");
        Check("workspace list response", workspaces?.Data is [{ Name: "Acme", MyRole: WorkspaceRole.Owner, MemberCount: 4, IsCreator: true }], workspaces?.Data?.Length.ToString() ?? "null");
        handler.Respond("""{"success":true,"data":{"id":"3f2c1a9e-0000-4000-8000-000000000001","name":"Acme","slug":"acme","planId":"pro","status":0,"subscriptionStatus":1,"userLimit":5,"currentUserCount":4,"monthlyLabelLimit":1000,"labelsUsedThisMonth":12,"isTrialing":false,"myRole":1,"canManageMembers":true,"canManageBilling":false,"createdAt":"2026-01-01T00:00:00Z","entitlements":[]}}""");
        var workspace = await client.Workspaces.GetAsync();
        Expect("workspace get", "GET", $"/api/workspaces/{Ws}");
        Check("workspace get response", workspace?.Data is { MyRole: WorkspaceRole.Admin, MonthlyLabelLimit: 1000, CanManageMembers: true }, workspace?.Data?.Name ?? "null");

        // Carbon offsets.
        await client.CarbonOffsets.QuoteAsync(new CarbonOffsetQuoteRequest { WeightLbs = 2.5m, DistanceMiles = 800m });
        Expect("carbon quote", "POST", $"/api/workspaces/{Ws}/carbon-offsets/quote", """{"weightLbs":2.5,"distanceMiles":800}""");
        await client.CarbonOffsets.PurchaseAsync(new CarbonOffsetPurchaseRequest { TrackingNumber = "9400", WeightLbs = 2.5m, DistanceMiles = 800m }, "offset-key-1");
        Expect("carbon purchase", "POST", $"/api/workspaces/{Ws}/carbon-offsets/purchase", """{"trackingNumber":"9400","weightLbs":2.5,"distanceMiles":800}""");
        Check("carbon purchase keeps the caller's key", handler.Calls[^1].IdempotencyKey == "offset-key-1", handler.Calls[^1].IdempotencyKey ?? "none");
        await client.CarbonOffsets.ListAsync(2, 10);
        Expect("carbon list", "GET", $"/api/workspaces/{Ws}/carbon-offsets?page=2&pageSize=10");
        await client.CarbonOffsets.GetAsync("off-1");
        Expect("carbon get", "GET", $"/api/workspaces/{Ws}/carbon-offsets/off-1");
        await client.CarbonOffsets.GetSummaryAsync();
        Expect("carbon summary", "GET", $"/api/workspaces/{Ws}/carbon-offsets/summary");
        handler.Respond("""{"success":true,"data":{"autoOffsetEnabled":true}}""");
        var settings = await client.CarbonOffsets.GetSettingsAsync();
        Expect("carbon settings", "GET", $"/api/workspaces/{Ws}/carbon-offsets/settings");
        Check("carbon settings response", settings?.Data?.AutoOffsetEnabled == true);
        await client.CarbonOffsets.UpdateSettingsAsync(new CarbonOffsetSettings { AutoOffsetEnabled = false });
        Expect("carbon settings update", "PUT", $"/api/workspaces/{Ws}/carbon-offsets/settings", """{"autoOffsetEnabled":false}""");

        // Webhook test delivery.
        await client.Webhooks.TestAsync("3f2c1a9e-0000-4000-8000-000000000001");
        Check("webhook test route", handler.Calls[^1] is { Method: "POST", PathAndQuery: $"/api/workspaces/{Ws}/webhooks/3f2c1a9e-0000-4000-8000-000000000001/test" },
            $"{handler.Calls[^1].Method} {handler.Calls[^1].PathAndQuery}");

        // Recurring shipments: bare JSON responses, numeric interval, 204 for delete/pause/resume/trigger.
        const string Schedule = "5b0c1d2e-0000-4000-8000-0000000000aa";
        const string RecurringBody = """{"id":"5b0c1d2e-0000-4000-8000-0000000000aa","workspaceId":"3f2c1a9e-0000-4000-8000-000000000001","presetId":null,"name":"Weekly restock","interval":1,"intervalName":"Weekly","nextRunUtc":"2026-11-02T09:00:00Z","isActive":true,"failureCount":0,"maxFailures":3,"createdAt":"2026-10-19T00:00:00Z"}""";
        handler.Respond("[" + RecurringBody + "]");
        var recurring = await client.RecurringShipments.ListAsync();
        Expect("recurring list", "GET", $"/api/workspaces/{Ws}/recurring-shipments");
        Check("recurring list response", recurring is [{ Name: "Weekly restock", Interval: RecurrenceInterval.Weekly, IsActive: true, MaxFailures: 3 }], recurring?.Count.ToString() ?? "null");
        handler.Respond(RecurringBody);
        var oneRecurring = await client.RecurringShipments.GetAsync(Schedule);
        Expect("recurring get", "GET", $"/api/workspaces/{Ws}/recurring-shipments/{Schedule}");
        Check("recurring get response", oneRecurring?.Id == Schedule, oneRecurring?.Id ?? "null");
        var addressId = Guid.Parse("6c0d1e2f-0000-4000-8000-0000000000bb");
        handler.Respond(RecurringBody);
        var createdRecurring = await client.RecurringShipments.CreateAsync(new CreateRecurringShipmentRequest
        {
            Name = "Weekly restock",
            AddressId = addressId,
            Interval = RecurrenceInterval.Weekly,
            FirstRunUtc = new DateTime(2026, 11, 2, 9, 0, 0, DateTimeKind.Utc),
        });
        Expect("recurring create", "POST", $"/api/workspaces/{Ws}/recurring-shipments",
            """{"name":"Weekly restock","addressId":"6c0d1e2f-0000-4000-8000-0000000000bb","interval":1,"firstRunUtc":"2026-11-02T09:00:00Z"}""");
        Check("recurring create response", createdRecurring?.IntervalName == "Weekly", createdRecurring?.IntervalName ?? "null");
        handler.Respond(RecurringBody);
        await client.RecurringShipments.UpdateAsync(Schedule, new UpdateRecurringShipmentRequest { Interval = RecurrenceInterval.Custom, CronExpression = "0 9 * * 1" });
        Expect("recurring update", "PUT", $"/api/workspaces/{Ws}/recurring-shipments/{Schedule}", """{"interval":4,"cronExpression":"0 9 * * 1"}""");
        handler.RespondNoContent();
        await client.RecurringShipments.DeleteAsync(Schedule);
        Expect("recurring delete", "DELETE", $"/api/workspaces/{Ws}/recurring-shipments/{Schedule}");
        handler.RespondNoContent();
        await client.RecurringShipments.PauseAsync(Schedule);
        Expect("recurring pause", "POST", $"/api/workspaces/{Ws}/recurring-shipments/{Schedule}/pause", "null");
        handler.RespondNoContent();
        await client.RecurringShipments.ResumeAsync(Schedule);
        Expect("recurring resume", "POST", $"/api/workspaces/{Ws}/recurring-shipments/{Schedule}/resume", "null");
        handler.RespondNoContent();
        await client.RecurringShipments.TriggerAsync(Schedule, "trigger-key-1");
        Expect("recurring trigger", "POST", $"/api/workspaces/{Ws}/recurring-shipments/{Schedule}/trigger", "null");
        Check("recurring trigger keeps the caller's key", handler.Calls[^1].IdempotencyKey == "trigger-key-1", handler.Calls[^1].IdempotencyKey ?? "none");

        // Report schedules: the envelope (delete is 204), numeric format/frequency, "HH:mm:ss" time of day.
        const string Report = "7d0e1f20-0000-4000-8000-0000000000cc";
        const string ReportBody = """{"id":"7d0e1f20-0000-4000-8000-0000000000cc","workspaceId":"3f2c1a9e-0000-4000-8000-000000000001","reportType":"shipments","format":0,"frequencyType":1,"frequencyName":"Weekly","dayOfWeek":1,"timeOfDayUtc":"06:30:00","nextRunUtc":"2026-10-26T06:30:00Z","isEnabled":true,"recipientEmails":"ops@example.com,cfo@example.com","failureCount":0,"maxFailures":3,"createdAt":"2026-10-19T00:00:00Z"}""";
        handler.Respond("""{"success":true,"data":[""" + ReportBody + "]}");
        var reports = await client.ReportSchedules.ListAsync();
        Expect("report list", "GET", $"/api/workspaces/{Ws}/report-schedules");
        Check("report list response",
            reports?.Data is [{ ReportType: "shipments", Format: ReportFormat.Csv, FrequencyType: ReportFrequency.Weekly, DayOfWeek: DayOfWeek.Monday, IsEnabled: true }] &&
            reports.Data[0].TimeOfDayUtc == new TimeSpan(6, 30, 0),
            reports?.Data?.Length.ToString() ?? "null");
        handler.Respond("""{"success":true,"data":""" + ReportBody + "}");
        var report = await client.ReportSchedules.GetAsync(Report);
        Expect("report get", "GET", $"/api/workspaces/{Ws}/report-schedules/{Report}");
        Check("report get response", report?.Data?.RecipientEmails == "ops@example.com,cfo@example.com", report?.Data?.RecipientEmails ?? "null");
        handler.Respond("""{"success":true,"data":""" + ReportBody + "}");
        await client.ReportSchedules.CreateAsync(new CreateReportScheduleRequest
        {
            ReportType = "shipments",
            Format = ReportFormat.Json,
            FrequencyType = ReportFrequency.Weekly,
            DayOfWeek = DayOfWeek.Monday,
            TimeOfDayUtc = new TimeSpan(6, 30, 0),
            RecipientEmails = "ops@example.com",
            MaxFailures = 5,
        });
        Expect("report create", "POST", $"/api/workspaces/{Ws}/report-schedules",
            """{"reportType":"shipments","format":1,"frequencyType":1,"dayOfWeek":1,"timeOfDayUtc":"06:30:00","recipientEmails":"ops@example.com","maxFailures":5}""");
        await client.ReportSchedules.UpdateAsync(Report, new UpdateReportScheduleRequest { FrequencyType = ReportFrequency.Monthly, DayOfMonth = 15 });
        Expect("report update", "PUT", $"/api/workspaces/{Ws}/report-schedules/{Report}", """{"frequencyType":2,"dayOfMonth":15}""");
        handler.RespondNoContent();
        await client.ReportSchedules.DeleteAsync(Report);
        Expect("report delete", "DELETE", $"/api/workspaces/{Ws}/report-schedules/{Report}");
        handler.Respond("""{"success":true,"data":null,"message":"Report schedule toggled"}""");
        var toggled = await client.ReportSchedules.ToggleAsync(Report);
        Expect("report toggle", "POST", $"/api/workspaces/{Ws}/report-schedules/{Report}/toggle", "null");
        Check("report toggle response", toggled is { Success: true, Data: null, Message: "Report schedule toggled" }, toggled?.Message ?? "null");

        // Email templates: bare JSON, PUT upserts by numeric eventType, preview renders.
        const string Template = "8e0f2031-0000-4000-8000-0000000000dd";
        const string TemplateBody = """{"id":"8e0f2031-0000-4000-8000-0000000000dd","workspaceId":"3f2c1a9e-0000-4000-8000-000000000001","eventType":3,"eventTypeName":"Delivered","subjectTemplate":"Delivered: {{tracking_number}}","htmlBodyTemplate":"<p>Delivered</p>","isEnabled":true,"createdAt":"2026-10-19T00:00:00Z","updatedAt":"2026-10-19T00:00:00Z"}""";
        handler.Respond("[" + TemplateBody + "]");
        var templates = await client.EmailTemplates.ListAsync();
        Expect("template list", "GET", $"/api/workspaces/{Ws}/email-templates");
        Check("template list response", templates is [{ EventType: TrackingEmailEvent.Delivered, EventTypeName: "Delivered", IsEnabled: true }], templates?.Count.ToString() ?? "null");
        handler.Respond(TemplateBody);
        var template = await client.EmailTemplates.GetAsync(Template);
        Expect("template get", "GET", $"/api/workspaces/{Ws}/email-templates/{Template}");
        Check("template get response", template?.SubjectTemplate == "Delivered: {{tracking_number}}", template?.SubjectTemplate ?? "null");
        handler.Respond(TemplateBody);
        var upserted = await client.EmailTemplates.UpsertAsync(new UpsertEmailTemplateRequest
        {
            EventType = TrackingEmailEvent.OutForDelivery,
            SubjectTemplate = "Out for delivery",
            HtmlBodyTemplate = "Arriving today: {{tracking_number}}",
        });
        Expect("template upsert", "PUT", $"/api/workspaces/{Ws}/email-templates",
            """{"eventType":2,"subjectTemplate":"Out for delivery","htmlBodyTemplate":"Arriving today: {{tracking_number}}"}""");
        Check("template upsert response", upserted?.Id == Template, upserted?.Id ?? "null");
        handler.RespondNoContent();
        await client.EmailTemplates.DeleteAsync(Template);
        Expect("template delete", "DELETE", $"/api/workspaces/{Ws}/email-templates/{Template}");
        handler.Respond("""{"renderedSubject":"Delivered: 9400111899223456789012","renderedHtmlBody":"<p>Delivered</p>"}""");
        var preview = await client.EmailTemplates.PreviewAsync(new EmailTemplatePreviewRequest { SubjectTemplate = "Delivered: {{tracking_number}}", HtmlBodyTemplate = "Delivered by {{carrier}}" });
        Expect("template preview", "POST", $"/api/workspaces/{Ws}/email-templates/preview",
            """{"subjectTemplate":"Delivered: {{tracking_number}}","htmlBodyTemplate":"Delivered by {{carrier}}"}""");
        Check("template preview response", preview?.RenderedSubject == "Delivered: 9400111899223456789012", preview?.RenderedSubject ?? "null");

        // Deprecated carrier proxy writes: master's route and verb, with an Idempotency-Key like any write.
#pragma warning disable CS0618 // The legacy proxy writes are obsolete by design; this checks they still work.
        await client.Carriers.FedEx.CancelPickupAsync(new { pickupConfirmationCode = "1" });
        Expect("fedex cancel pickup", "PUT", "/api/ApiProxy/api/v3/CarrierPickup/putFedExCancelCarrierPickupRequest", """{"pickupConfirmationCode":"1"}""");
        Check("proxy write carries a key", Guid.TryParse(handler.Calls[^1].IdempotencyKey, out _), handler.Calls[^1].IdempotencyKey ?? "none");
        handler.RespondNoContent();
        await client.Carriers.Usps.CancelPickupAsync();
        Expect("usps cancel pickup", "DELETE", "/api/ApiProxy/api/v3/CarrierPickup/cancelUspsCarrierPickupSchedule");
        Check("proxy DELETE carries no key", handler.Calls[^1].IdempotencyKey is null, handler.Calls[^1].IdempotencyKey ?? "");
#pragma warning restore CS0618

        // Read-only carrier lookups: GET through the proxy with the parameters in the query string.
        await client.Carriers.Usps.CityStateLookupAsync("12207");
        Expect("usps city/state", "GET", "/api/ApiProxy/api/v3/AddressValidation/getUspsCityStateLookupByZipCode?zipCode=12207");
        await client.Carriers.Dhl.TrackAsync(new Dictionary<string, string> { ["trackingNumber"] = "JD 0001" });
        Expect("dhl track", "GET", "/api/ApiProxy/api/v2/ShippingLabel/getDhlTrackSingleShipment?trackingNumber=JD%200001");

        // Returns list filters by creation date and pages.
        await client.Returns.ListAsync(new DateTime(2026, 10, 1, 0, 0, 0, DateTimeKind.Utc), null, 2, 50);
        Expect("returns list", "GET", $"/api/workspaces/{Ws}/returns?startDate=2026-10-01T00%3A00%3A00.0000000Z&page=2&pageSize=50");

        // Login: the envelope's data.token becomes the Bearer token.
        handler.Respond("""{"success":true,"data":{"token":"access-1","refreshToken":"refresh-1","expiresAt":"2026-10-19T13:00:00Z","userId":"u1","email":"a@example.com","firstName":"A","lastName":"B","roles":["User"],"requiresTwoFactor":false}}""");
        var login = await client.Auth.LoginAsync(new { email = "a@example.com", password = "secret" });
        Expect("auth login", "POST", "/api/Account/login", """{"email":"a@example.com","password":"secret"}""");
        Check("auth login response", login?.Data is { Token: "access-1", RefreshToken: "refresh-1", Roles: ["User"] }, login?.Data?.Token ?? "null");
        await client.Auth.LogoutAsync();
        Check("login sets the Bearer token", handler.Calls[^1].Authorization == "Bearer access-1", handler.Calls[^1].Authorization ?? "none");

        // Token refresh: a JSON body with both tokens; the access token defaults to the client's own.
        handler.Respond("""{"success":true,"data":{"token":"access-2","refreshToken":"refresh-2"}}""");
        await client.Auth.RefreshTokenAsync("refresh-1");
        Expect("auth refresh", "POST", "/api/Account/refresh-token", """{"token":"access-1","refreshToken":"refresh-1"}""");
        await client.Auth.LogoutAsync();
        Check("refresh updates the Bearer token", handler.Calls[^1].Authorization == "Bearer access-2", handler.Calls[^1].Authorization ?? "none");
        await client.Auth.RefreshTokenAsync("refresh-2", "explicit-access");
        Expect("auth refresh with explicit token", "POST", "/api/Account/refresh-token", """{"token":"explicit-access","refreshToken":"refresh-2"}""");
        using (var anonymous = new FlexOpsTypedClient("https://example.test", httpClient: new HttpClient(handler) { BaseAddress = new Uri("https://example.test/") }))
        {
            var before = handler.Calls.Count;
            try
            {
                await anonymous.Auth.RefreshTokenAsync("refresh-1");
                failures.Add("refresh without an access token: did not throw");
            }
            catch (InvalidOperationException)
            {
            }

            Check("refresh without an access token sends nothing", handler.Calls.Count == before);
        }

        if (failures.Count > 0)
        {
            throw new InvalidOperationException("Resource route checks failed:" + Environment.NewLine + string.Join(Environment.NewLine, failures));
        }

        Console.WriteLine("Resource route checks passed.");
    }

    /// <summary>Records each request and answers with the next queued body, or an empty success envelope.</summary>
    sealed class RouteHandler : HttpMessageHandler
    {
        readonly Queue<(HttpStatusCode Status, string? Body)> _replies = new();

        public List<(string Method, string PathAndQuery, string Body, string? WorkspaceHeader, string? IdempotencyKey, string? Authorization)> Calls { get; } = [];

        public void Respond(string body) => _replies.Enqueue((HttpStatusCode.OK, body));

        public void RespondNoContent() => _replies.Enqueue((HttpStatusCode.NoContent, null));

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            string? Header(string name) => request.Headers.TryGetValues(name, out var values) ? values.Single() : null;
            Calls.Add((
                request.Method.Method,
                request.RequestUri!.PathAndQuery,
                request.Content is null ? "" : await request.Content.ReadAsStringAsync(ct),
                Header("X-Workspace-Id"),
                Header("Idempotency-Key"),
                request.Headers.Authorization?.ToString()));
            var (status, body) = _replies.Count > 0 ? _replies.Dequeue() : (HttpStatusCode.OK, """{"success":true,"data":null}""");
            var response = new HttpResponseMessage(status);
            if (body is not null)
            {
                response.Content = new StringContent(body, Encoding.UTF8, "application/json");
            }

            return response;
        }
    }
}
