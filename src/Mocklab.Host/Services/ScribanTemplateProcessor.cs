using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Scriban;
using Scriban.Runtime;

namespace Mocklab.Host.Services;

/// <summary>
/// Processes template variables using Scriban. Supports full Scriban syntax (for/if/expressions),
/// request context (query, headers, cookies, route), built-in helpers, and data buckets.
/// Legacy {{$...}} placeholders are converted to Scriban syntax for backward compatibility.
/// </summary>
public partial class ScribanTemplateProcessor : ITemplateProcessor
{
    private readonly ILogger<ScribanTemplateProcessor> _logger;

    public ScribanTemplateProcessor(ILogger<ScribanTemplateProcessor> logger)
    {
        _logger = logger;
    }

    public string ProcessTemplate(string template, HttpRequest request, TemplateRequestContext? context = null)
    {
        if (string.IsNullOrEmpty(template))
            return template;

        var bodyForTemplate = context?.RequestBody;
        if (bodyForTemplate == null && request.Body != null && request.Body.CanSeek)
        {
            request.Body.Position = 0;
            using var reader = new StreamReader(request.Body, leaveOpen: true);
            bodyForTemplate = reader.ReadToEnd();
            request.Body.Position = 0;
        }

        var normalized = NormalizeLegacySyntax(template);
        var scriptObject = new ScriptObject();

        // Request object: method, path, body, query, headers, cookies, route
        var requestObj = new ScriptObject();
        requestObj.Add("method", request.Method);
        requestObj.Add("path", request.Path.Value ?? string.Empty);
        requestObj.Add("body", bodyForTemplate ?? string.Empty);

        var requestJson = JsonToScribanHelper.FromJson(bodyForTemplate);
        requestObj.Add("json", requestJson);

        var queryObj = new ScriptObject();
        foreach (var kv in request.Query)
            queryObj.Add(kv.Key, kv.Value.ToString());
        requestObj.Add("query", queryObj);

        var headersObj = new ScriptObject();
        foreach (var h in request.Headers)
            headersObj.Add(h.Key, h.Value.ToString());
        requestObj.Add("headers", headersObj);

        var cookiesObj = new ScriptObject();
        foreach (var c in request.Cookies)
            cookiesObj.Add(c.Key, c.Value);
        requestObj.Add("cookies", cookiesObj);

        var routeObj = new ScriptObject();
        if (context?.RouteParams != null)
        {
            foreach (var r in context.RouteParams)
                routeObj.Add(r.Key, r.Value);
        }
        requestObj.Add("route", routeObj);

        scriptObject.Add("request", requestObj);

        var headersDict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var h in request.Headers)
            headersDict[h.Key] = h.Value.ToString();
        scriptObject.Add("headers", headersDict);

        var helpersInstance = new TemplateHelpers();
        var helpersObj = new ScriptObject();
        helpersObj.Import("guid", (Func<string>)(() => helpersInstance.guid()));
        helpersObj.Import("rand_int", (Func<int, int, int>)((min, max) => helpersInstance.rand_int(min, max)));
        helpersObj.Import("alphanum", (Func<object?, string>)(o => helpersInstance.alphanum(o is int i ? i : 12)));
        helpersObj.Import("username", (Func<string>)(() => helpersInstance.username()));
        helpersObj.Import("email", (Func<object?, string>)(d => helpersInstance.email(d?.ToString())));
        scriptObject.Add("helpers", helpersObj);

        // Built-in helper functions (explicit register for static methods and overloads)
        scriptObject.Import("guid", (Func<string>)ScribanTemplateHelpers.Guid);
        scriptObject.Import("random_int", (Func<int>)ScribanTemplateHelpers.RandomInt);
        scriptObject.Import("random_int", (Func<int, int, int>)ScribanTemplateHelpers.RandomInt);
        scriptObject.Import("random_float", (Func<double>)ScribanTemplateHelpers.RandomFloat);
        scriptObject.Import("random_double", (Func<double, double, double>)ScribanTemplateHelpers.RandomDouble);
        scriptObject.Import("random_name", (Func<string>)ScribanTemplateHelpers.RandomName);
        scriptObject.Import("random_first_name", (Func<string>)ScribanTemplateHelpers.RandomFirstName);
        scriptObject.Import("random_last_name", (Func<string>)ScribanTemplateHelpers.RandomLastName);
        scriptObject.Import("random_email", (Func<string>)ScribanTemplateHelpers.RandomEmail);
        scriptObject.Import("random_phone", (Func<string>)ScribanTemplateHelpers.RandomPhone);
        scriptObject.Import("random_alpha_numeric", (Func<int, string>)ScribanTemplateHelpers.RandomAlphaNumeric);
        scriptObject.Import("random_string", (Func<int, string>)ScribanTemplateHelpers.RandomStringLengthOnly);
        scriptObject.Import("random_string", (Func<int, string, string>)ScribanTemplateHelpers.RandomStringWithChars);
        scriptObject.Import("timestamp", (Func<long>)ScribanTemplateHelpers.Timestamp);
        scriptObject.Import("iso_timestamp", (Func<string>)ScribanTemplateHelpers.IsoTimestamp);
        scriptObject.Import("now", (Func<DateTime>)ScribanTemplateHelpers.Now);
        scriptObject.Import("random_bool", (Func<bool>)ScribanTemplateHelpers.RandomBool);

        // Data buckets (from context, populated when collection has buckets)
        if (context?.Buckets != null)
        {
            foreach (var b in context.Buckets)
                scriptObject.Add(b.Key, b.Value);
            scriptObject.Import("random_item", (Func<string, object?>)(bucketName => RandomItemFromBuckets(context.Buckets, bucketName)));
        }

        var templateContext = new TemplateContext
        {
            StrictVariables = false,
            LoopLimit = 10_000
        };
        templateContext.PushGlobal(scriptObject);

        try
        {
            var parsed = Template.Parse(normalized);
            if (parsed.HasErrors)
            {
                _logger.LogWarning("Scriban parse errors: {Errors}", string.Join("; ", parsed.Messages));
                return template;
            }
            var result = parsed.Render(templateContext);
            return result ?? string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Scriban render failed for template (first 200 chars): {Preview}", template.Length > 200 ? template[..200] + "..." : template);
            return template;
        }
    }

    private static object? RandomItemFromBuckets(IReadOnlyDictionary<string, object> buckets, string bucketName)
    {
        if (string.IsNullOrEmpty(bucketName) || !buckets.TryGetValue(bucketName, out var data))
            return null;
        if (data is System.Collections.IList list && list.Count > 0)
            return list[Random.Shared.Next(list.Count)];
        return data;
    }

    /// <summary>
    /// Converts legacy {{$variable}} syntax to Scriban {{ variable }} or {{ function }} so existing templates keep working.
    /// </summary>
    private static string NormalizeLegacySyntax(string template)
    {
        if (!template.Contains("{{"))
            return template;

        var result = template;

        // {{$randomInt(1,100)}} -> {{ random_int 1 100 }}
        result = LegacyRandomIntRangeRegex().Replace(result, m =>
            "{{ " + (int.TryParse(m.Groups[1].Value, out var min) && int.TryParse(m.Groups[2].Value, out var max)
                ? $"random_int {min} {max}"
                : "random_int 1 100") + " }}");

        // Simple built-ins: {{$variable}} -> {{ scriban_equivalent }}
        result = LegacyRandomUuidRegex().Replace(result, "{{ guid }}");
        result = LegacyTimestampRegex().Replace(result, "{{ timestamp }}");
        result = LegacyIsoTimestampRegex().Replace(result, "{{ iso_timestamp }}");
        result = LegacyRandomIntRegex().Replace(result, "{{ random_int }}");
        result = LegacyRandomFloatRegex().Replace(result, "{{ random_float }}");
        result = LegacyRandomBoolRegex().Replace(result, "{{ random_bool }}");
        result = LegacyRandomNameRegex().Replace(result, "{{ random_name }}");
        result = LegacyRandomEmailRegex().Replace(result, "{{ random_email }}");

        result = result.Replace("{{$request.path}}", "{{ request.path }}");
        result = result.Replace("{{$request.method}}", "{{ request.method }}");
        result = result.Replace("{{$request.body}}", "{{ request.body }}");

        // {{$request.query.paramName}} -> {{ request.query.paramName }}
        result = LegacyRequestQueryRegex().Replace(result, m => "{{ request.query." + m.Groups[1].Value + " }}");
        // {{$request.header.HeaderName}} -> {{ request.headers[\"Header-Name\"] }} (Scriban indexer)
        result = LegacyRequestHeaderRegex().Replace(result, m => "{{ request.headers[\"" + m.Groups[1].Value.Replace("\"", "\\\"") + "\"] }}");

        return result;
    }

    [GeneratedRegex(@"\{\{\$randomInt\((\d+),(\d+)\)\}\}")]
    private static partial Regex LegacyRandomIntRangeRegex();
    [GeneratedRegex(@"\{\{\$randomUUID\}\}")]
    private static partial Regex LegacyRandomUuidRegex();
    [GeneratedRegex(@"\{\{\$timestamp\}\}")]
    private static partial Regex LegacyTimestampRegex();
    [GeneratedRegex(@"\{\{\$isoTimestamp\}\}")]
    private static partial Regex LegacyIsoTimestampRegex();
    [GeneratedRegex(@"\{\{\$randomInt\}\}")]
    private static partial Regex LegacyRandomIntRegex();
    [GeneratedRegex(@"\{\{\$randomFloat\}\}")]
    private static partial Regex LegacyRandomFloatRegex();
    [GeneratedRegex(@"\{\{\$randomBool\}\}")]
    private static partial Regex LegacyRandomBoolRegex();
    [GeneratedRegex(@"\{\{\$randomName\}\}")]
    private static partial Regex LegacyRandomNameRegex();
    [GeneratedRegex(@"\{\{\$randomEmail\}\}")]
    private static partial Regex LegacyRandomEmailRegex();
    [GeneratedRegex(@"\{\{\$request\.query\.(\w+)\}\}")]
    private static partial Regex LegacyRequestQueryRegex();
    [GeneratedRegex(@"\{\{\$request\.header\.([\w-]+)\}\}")]
    private static partial Regex LegacyRequestHeaderRegex();
}
