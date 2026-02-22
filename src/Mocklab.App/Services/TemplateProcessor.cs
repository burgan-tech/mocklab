using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;

namespace Mocklab.App.Services;

/// <summary>
/// Processes template variables in response bodies.
/// Supports built-in random data generators and request-based variables.
/// </summary>
public partial class TemplateProcessor : ITemplateProcessor
{
    private static readonly string[] SampleNames =
    [
        "Alice Johnson", "Bob Smith", "Charlie Brown", "Diana Ross", "Edward Norton",
        "Fiona Apple", "George Miller", "Hannah Montana", "Ivan Petrov", "Julia Roberts",
        "Kevin Hart", "Laura Palmer", "Michael Scott", "Nancy Drew", "Oscar Wilde",
        "Patricia Arquette", "Quentin Blake", "Rachel Green", "Samuel Jackson", "Tina Turner"
    ];

    private static readonly string[] SampleDomains =
    [
        "example.com", "test.org", "mock.io", "demo.dev", "sample.net"
    ];

    public string ProcessTemplate(string template, HttpRequest request, TemplateRequestContext? context = null)
    {
        if (string.IsNullOrEmpty(template) || !template.Contains("{{"))
            return template;

        var result = template;

        // Process parameterized built-in variables first (e.g., {{$randomInt(1,100)}})
        result = RandomIntRangeRegex().Replace(result, match =>
        {
            if (int.TryParse(match.Groups[1].Value, out var min) &&
                int.TryParse(match.Groups[2].Value, out var max))
            {
                return Random.Shared.Next(min, max + 1).ToString();
            }
            return match.Value;
        });

        // Process simple built-in variables
        result = ReplaceBuiltInVariables(result);

        // Process request-based variables
        result = ReplaceRequestVariables(result, request);

        return result;
    }

    private static string ReplaceBuiltInVariables(string template)
    {
        var result = template;

        // Each replacement generates a new value per occurrence using regex
        result = RandomUuidRegex().Replace(result, _ => Guid.NewGuid().ToString());
        result = TimestampRegex().Replace(result, _ => DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString());
        result = IsoTimestampRegex().Replace(result, _ => DateTime.UtcNow.ToString("O"));
        result = RandomIntRegex().Replace(result, _ => Random.Shared.Next(1, 1000000).ToString());
        result = RandomFloatRegex().Replace(result, _ => (Random.Shared.NextDouble() * 1000).ToString("F2", System.Globalization.CultureInfo.InvariantCulture));
        result = RandomBoolRegex().Replace(result, _ => Random.Shared.Next(2) == 1 ? "true" : "false");
        result = RandomNameRegex().Replace(result, _ => SampleNames[Random.Shared.Next(SampleNames.Length)]);
        result = RandomEmailRegex().Replace(result, _ =>
        {
            var name = SampleNames[Random.Shared.Next(SampleNames.Length)]
                .ToLowerInvariant().Replace(" ", ".");
            var domain = SampleDomains[Random.Shared.Next(SampleDomains.Length)];
            return $"{name}@{domain}";
        });

        return result;
    }

    private static string ReplaceRequestVariables(string template, HttpRequest request)
    {
        var result = template;

        // Simple request variables
        result = result.Replace("{{$request.path}}", request.Path.Value ?? string.Empty);
        result = result.Replace("{{$request.method}}", request.Method);
        result = result.Replace("{{$request.body}}", ReadRequestBody(request));

        // Query parameter variables: {{$request.query.paramName}}
        result = RequestQueryRegex().Replace(result, match =>
        {
            var paramName = match.Groups[1].Value;
            return request.Query.TryGetValue(paramName, out var value) ? value.ToString() : string.Empty;
        });

        // Header variables: {{$request.header.headerName}}
        result = RequestHeaderRegex().Replace(result, match =>
        {
            var headerName = match.Groups[1].Value;
            return request.Headers.TryGetValue(headerName, out var value) ? value.ToString() : string.Empty;
        });

        return result;
    }

    private static string ReadRequestBody(HttpRequest request)
    {
        if (request.Body == null || !request.Body.CanSeek)
            return string.Empty;

        request.Body.Position = 0;
        using var reader = new StreamReader(request.Body, leaveOpen: true);
        var body = reader.ReadToEnd();
        request.Body.Position = 0;
        return body;
    }

    // Regex patterns for template variables
    [GeneratedRegex(@"\{\{\$randomInt\((\d+),(\d+)\)\}\}")]
    private static partial Regex RandomIntRangeRegex();

    [GeneratedRegex(@"\{\{\$randomUUID\}\}")]
    private static partial Regex RandomUuidRegex();

    [GeneratedRegex(@"\{\{\$timestamp\}\}")]
    private static partial Regex TimestampRegex();

    [GeneratedRegex(@"\{\{\$isoTimestamp\}\}")]
    private static partial Regex IsoTimestampRegex();

    [GeneratedRegex(@"\{\{\$randomInt\}\}")]
    private static partial Regex RandomIntRegex();

    [GeneratedRegex(@"\{\{\$randomFloat\}\}")]
    private static partial Regex RandomFloatRegex();

    [GeneratedRegex(@"\{\{\$randomBool\}\}")]
    private static partial Regex RandomBoolRegex();

    [GeneratedRegex(@"\{\{\$randomName\}\}")]
    private static partial Regex RandomNameRegex();

    [GeneratedRegex(@"\{\{\$randomEmail\}\}")]
    private static partial Regex RandomEmailRegex();

    [GeneratedRegex(@"\{\{\$request\.query\.(\w+)\}\}")]
    private static partial Regex RequestQueryRegex();

    [GeneratedRegex(@"\{\{\$request\.header\.([\w-]+)\}\}")]
    private static partial Regex RequestHeaderRegex();
}
