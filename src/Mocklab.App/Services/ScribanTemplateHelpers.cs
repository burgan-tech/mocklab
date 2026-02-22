using System.Globalization;
using System.Text;

namespace Mocklab.App.Services;

/// <summary>
/// Built-in helper functions exposed to Scriban templates (guid, random_*, timestamp, etc.).
/// All methods are stateless and safe (no file/network access).
/// </summary>
public static class ScribanTemplateHelpers
{
    private static readonly string[] FirstNames =
    [
        "Alice", "Bob", "Charlie", "Diana", "Edward", "Fiona", "George", "Hannah",
        "Ivan", "Julia", "Kevin", "Laura", "Michael", "Nancy", "Oscar", "Patricia",
        "Quentin", "Rachel", "Samuel", "Tina"
    ];

    private static readonly string[] LastNames =
    [
        "Johnson", "Smith", "Brown", "Ross", "Norton", "Apple", "Miller", "Montana",
        "Petrov", "Roberts", "Hart", "Palmer", "Scott", "Drew", "Wilde", "Arquette",
        "Blake", "Green", "Jackson", "Turner"
    ];

    private static readonly string[] SampleDomains =
    [
        "example.com", "test.org", "mock.io", "demo.dev", "sample.net"
    ];

    private const string AlphaNumericChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

    public static string Guid() => System.Guid.NewGuid().ToString();

    public static int RandomInt() => Random.Shared.Next(1, 1000000);
    public static int RandomInt(int min, int max) => Random.Shared.Next(min, max + 1);
    public static double RandomFloat() => Random.Shared.NextDouble() * 1000;
    public static double RandomDouble(double min, double max)
    {
        var u = Random.Shared.NextDouble();
        return min + (u * (max - min));
    }

    public static string RandomName() =>
        $"{FirstNames[Random.Shared.Next(FirstNames.Length)]} {LastNames[Random.Shared.Next(LastNames.Length)]}";

    public static string RandomFirstName() => FirstNames[Random.Shared.Next(FirstNames.Length)];
    public static string RandomLastName() => LastNames[Random.Shared.Next(LastNames.Length)];

    public static string RandomEmail()
    {
        var name = FirstNames[Random.Shared.Next(FirstNames.Length)].ToLowerInvariant() + "." +
                   LastNames[Random.Shared.Next(LastNames.Length)].ToLowerInvariant();
        var domain = SampleDomains[Random.Shared.Next(SampleDomains.Length)];
        return $"{name}@{domain}";
    }

    /// <summary>
    /// Random phone in E.164-like format +90 5XX XXX XX XX (Turkey) or simple 10-digit.
    /// </summary>
    public static string RandomPhone() =>
        $"+90 5{Random.Shared.Next(3, 6)} {Random.Shared.Next(100, 1000)} {Random.Shared.Next(10, 100)} {Random.Shared.Next(10, 100)}";

    public static string RandomAlphaNumeric(int length) => RandomString(length, null);
    public static string RandomString(int length, string? chars = null)
    {
        var set = chars ?? AlphaNumericChars;
        var sb = new StringBuilder(length);
        for (var i = 0; i < length; i++)
            sb.Append(set[Random.Shared.Next(set.Length)]);
        return sb.ToString();
    }
    /// <summary>Overload for Scriban: random_string(length) with no custom chars.</summary>
    public static string RandomStringLengthOnly(int length) => RandomString(length, null);
    /// <summary>Overload for Scriban: random_string(length, chars).</summary>
    public static string RandomStringWithChars(int length, string chars) => RandomString(length, chars);

    public static long Timestamp() => DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    public static string IsoTimestamp() => DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture);
    public static DateTime Now() => DateTime.UtcNow;

    public static bool RandomBool() => Random.Shared.Next(2) == 1;
}
