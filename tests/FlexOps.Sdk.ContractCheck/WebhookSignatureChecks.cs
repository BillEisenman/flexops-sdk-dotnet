using FlexOps.Sdk.Resources;

/// <summary>
/// Webhook signature verification against a fixed vector made with the Gateway algorithm
/// (WebhookDeliveryService): lowercase hex HMAC-SHA256 over the UTF-8 string
/// "{timestamp}.{raw body}", keyed with the UTF-8 bytes of the whole "whsec_..." secret.
/// The vector was computed with Python's hmac module and cross-checked with
/// `openssl dgst -sha256 -hmac`; it is hard-coded so the check never reuses the code under test.
/// </summary>
static class WebhookSignatureChecks
{
    const string Secret = "whsec_AAECAwQFBgcICQoLDA0ODxAREhMUFRYXGBkaGxwdHh8=";
    const string Timestamp = "1760000000";
    const string Body = """{"labelId":"lbl_123","trackingNumber":"9400100000000000000000","carrierCode":"USPS"}""";
    const string Signature = "sha256=02a9afec40dedbbd4e698f0ff995ba9f267651f836f94e571d7f6f28f6b8d1e9";

    // What the old helper produced: HMAC of the body alone. A real delivery never carries it.
    const string BodyOnlyHex = "7eaa376c69823a5a584fa676486bee36ed90dba3f13090b1fcfe5500c611689a";

    static readonly DateTimeOffset Now = DateTimeOffset.FromUnixTimeSeconds(1760000000);

    static bool Verify(
        string payload = Body,
        string? signature = Signature,
        string secret = Secret,
        string? timestamp = Timestamp,
        DateTimeOffset? now = null,
        TimeSpan? tolerance = null)
        => WebhooksResource.VerifySignature(payload, signature, secret, timestamp, tolerance, now ?? Now);

    public static void Run()
    {
        var failures = new List<string>();
        void Expect(string name, bool expected, Func<bool> check)
        {
            try
            {
                if (check() != expected)
                {
                    failures.Add($"{name}: expected {expected}");
                }
            }
            catch (Exception ex)
            {
                failures.Add($"{name}: threw {ex.GetType().Name}");
            }
        }

        void ExpectThrows<TException>(string name, Action action) where TException : Exception
        {
            try
            {
                action();
                failures.Add($"{name}: did not throw {typeof(TException).Name}");
            }
            catch (TException)
            {
            }
            catch (Exception ex)
            {
                failures.Add($"{name}: threw {ex.GetType().Name}, expected {typeof(TException).Name}");
            }
        }

        Expect("valid server-signed delivery", true, () => Verify());
        Expect("signature without sha256= prefix", true, () => Verify(signature: Signature["sha256=".Length..]));
        Expect("uppercase hex", true, () => Verify(signature: "sha256=" + Signature["sha256=".Length..].ToUpperInvariant()));
        Expect("other scheme prefix", false, () => Verify(signature: Signature.Replace("sha256=", "sha1=", StringComparison.Ordinal)));
        Expect("wrong secret", false, () => Verify(secret: "whsec_not-the-secret"));
        Expect("tampered body", false, () => Verify(payload: Body.Replace("lbl_123", "lbl_124", StringComparison.Ordinal)));
        Expect("timestamp that was not signed", false, () => Verify(timestamp: "1760000001"));
        Expect("old body-only signature", false, () => Verify(signature: "sha256=" + BodyOnlyHex));
        Expect("old body-only signature, bare", false, () => Verify(signature: BodyOnlyHex));
        Expect("timestamp 300 s old", true, () => Verify(now: Now.AddSeconds(300)));
        Expect("timestamp 300 s ahead", true, () => Verify(now: Now.AddSeconds(-300)));
        Expect("stale timestamp", false, () => Verify(now: Now.AddSeconds(301)));
        Expect("future timestamp beyond tolerance", false, () => Verify(now: Now.AddSeconds(-301)));
        Expect("custom tolerance rejects", false, () => Verify(now: Now.AddSeconds(30), tolerance: TimeSpan.FromSeconds(10)));
        Expect("custom tolerance accepts", true, () => Verify(now: Now.AddHours(1), tolerance: TimeSpan.FromHours(2)));
        Expect("real clock by default", false, () => WebhooksResource.VerifySignature(Body, Signature, Secret, Timestamp));

        foreach (var timestamp in new string?[] { null, "", " ", "abc", "-1760000000", "+1760000000", "1.76e9", "1760000000.0", "0x68e0b800", "١٧", "99999999999999999999" })
        {
            Expect($"timestamp {timestamp ?? "null"}", false, () => Verify(timestamp: timestamp));
        }

        foreach (var (name, signature) in new (string, string?)[]
        {
            ("missing", null),
            ("empty", ""),
            ("prefix only", "sha256="),
            ("one byte short", Signature[..^2]),
            ("one byte long", Signature + "00"),
            ("non-hex", "sha256=" + new string('z', 64)),
            ("non-ASCII", "sha256=" + new string('é', 64)),
        })
        {
            Expect($"{name} signature", false, () => Verify(signature: signature));
        }

        ExpectThrows<ArgumentException>("empty secret", () => Verify(secret: ""));
        ExpectThrows<ArgumentOutOfRangeException>("negative tolerance", () => Verify(tolerance: TimeSpan.FromSeconds(-1)));

        if (failures.Count > 0)
        {
            throw new InvalidOperationException("Webhook signature checks failed:" + Environment.NewLine + string.Join(Environment.NewLine, failures));
        }

        Console.WriteLine("Webhook signature checks passed.");
    }
}
