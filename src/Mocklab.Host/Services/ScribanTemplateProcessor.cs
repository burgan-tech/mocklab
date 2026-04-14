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

        var requestJson = JsonToScribanHelper.FromJson(bodyForTemplate);
        // body: parsed JSON object when valid JSON (enables request.body.accountName navigation),
        // falls back to raw string when body is not valid JSON
        requestObj.Add("body", requestJson ?? (object)(bodyForTemplate ?? string.Empty));
        requestObj.Add("body_raw", bodyForTemplate ?? string.Empty);
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

        // All ScribanTemplateHelpers also available under helpers.*
        helpersObj.Import("random_int", (Func<int>)ScribanTemplateHelpers.RandomInt);
        helpersObj.Import("random_float", (Func<double>)ScribanTemplateHelpers.RandomFloat);
        helpersObj.Import("random_name", (Func<string>)ScribanTemplateHelpers.RandomName);
        helpersObj.Import("random_first_name", (Func<string>)ScribanTemplateHelpers.RandomFirstName);
        helpersObj.Import("random_last_name", (Func<string>)ScribanTemplateHelpers.RandomLastName);
        helpersObj.Import("random_email", (Func<string>)ScribanTemplateHelpers.RandomEmail);
        helpersObj.Import("random_phone", (Func<string>)ScribanTemplateHelpers.RandomPhone);
        helpersObj.Import("random_alpha_numeric", (Func<int, string>)ScribanTemplateHelpers.RandomAlphaNumeric);
        helpersObj.Import("random_string", (Func<int, string>)ScribanTemplateHelpers.RandomStringLengthOnly);
        helpersObj.Import("random_bool", (Func<bool>)ScribanTemplateHelpers.RandomBool);
        helpersObj.Import("upper", (Func<string?, string>)ScribanTemplateHelpers.Upper);
        helpersObj.Import("lower", (Func<string?, string>)ScribanTemplateHelpers.Lower);
        helpersObj.Import("random_number_string", (Func<int, string>)ScribanTemplateHelpers.RandomNumberString);
        helpersObj.Import("random_company_name", (Func<string>)ScribanTemplateHelpers.RandomCompanyName);
        helpersObj.Import("random_city", (Func<string>)ScribanTemplateHelpers.RandomCity);
        helpersObj.Import("random_country", (Func<string>)ScribanTemplateHelpers.RandomCountry);
        helpersObj.Import("random_currency_code", (Func<string>)ScribanTemplateHelpers.RandomCurrencyCode);
        helpersObj.Import("random_product_name", (Func<string>)ScribanTemplateHelpers.RandomProductName);
        helpersObj.Import("random_job_title", (Func<string>)ScribanTemplateHelpers.RandomJobTitle);
        helpersObj.Import("random_address", (Func<string>)ScribanTemplateHelpers.RandomAddress);
        helpersObj.Import("random_iban", (Func<string>)ScribanTemplateHelpers.RandomIban);
        helpersObj.Import("random_username", (Func<string>)ScribanTemplateHelpers.RandomUsername);
        helpersObj.Import("random_password", (Func<string>)ScribanTemplateHelpers.RandomPassword);
        helpersObj.Import("random_age", (Func<int>)ScribanTemplateHelpers.RandomAge);
        helpersObj.Import("random_birthdate", (Func<string>)ScribanTemplateHelpers.RandomBirthdate);
        helpersObj.Import("random_zip_code", (Func<string>)ScribanTemplateHelpers.RandomZipCode);
        helpersObj.Import("random_latitude", (Func<double>)ScribanTemplateHelpers.RandomLatitude);
        helpersObj.Import("random_longitude", (Func<double>)ScribanTemplateHelpers.RandomLongitude);
        helpersObj.Import("random_account_number", (Func<string>)ScribanTemplateHelpers.RandomAccountNumber);
        helpersObj.Import("random_swift_code", (Func<string>)ScribanTemplateHelpers.RandomSwiftCode);
        helpersObj.Import("random_credit_card_number", (Func<string>)ScribanTemplateHelpers.RandomCreditCardNumber);
        helpersObj.Import("random_price", (Func<string>)ScribanTemplateHelpers.RandomPrice);
        helpersObj.Import("random_stock_symbol", (Func<string>)ScribanTemplateHelpers.RandomStockSymbol);
        helpersObj.Import("random_transaction_type", (Func<string>)ScribanTemplateHelpers.RandomTransactionType);
        helpersObj.Import("random_ip", (Func<string>)ScribanTemplateHelpers.RandomIp);
        helpersObj.Import("random_mac_address", (Func<string>)ScribanTemplateHelpers.RandomMacAddress);
        helpersObj.Import("random_url", (Func<string>)ScribanTemplateHelpers.RandomUrl);
        helpersObj.Import("random_status", (Func<string>)ScribanTemplateHelpers.RandomStatus);
        helpersObj.Import("random_http_status_code", (Func<int>)ScribanTemplateHelpers.RandomHttpStatusCode);
        helpersObj.Import("random_color", (Func<string>)ScribanTemplateHelpers.RandomColor);
        helpersObj.Import("random_hex_color", (Func<string>)ScribanTemplateHelpers.RandomHexColor);
        helpersObj.Import("random_department", (Func<string>)ScribanTemplateHelpers.RandomDepartment);
        helpersObj.Import("random_category", (Func<string>)ScribanTemplateHelpers.RandomCategory);
        helpersObj.Import("random_role", (Func<string>)ScribanTemplateHelpers.RandomRole);
        helpersObj.Import("random_priority", (Func<string>)ScribanTemplateHelpers.RandomPriority);
        helpersObj.Import("random_ticket_status", (Func<string>)ScribanTemplateHelpers.RandomTicketStatus);
        helpersObj.Import("random_order_status", (Func<string>)ScribanTemplateHelpers.RandomOrderStatus);
        helpersObj.Import("random_language_code", (Func<string>)ScribanTemplateHelpers.RandomLanguageCode);
        helpersObj.Import("random_continent", (Func<string>)ScribanTemplateHelpers.RandomContinent);
        helpersObj.Import("random_timezone", (Func<string>)ScribanTemplateHelpers.RandomTimezone);
        helpersObj.Import("random_file_extension", (Func<string>)ScribanTemplateHelpers.RandomFileExtension);
        helpersObj.Import("random_mime_type", (Func<string>)ScribanTemplateHelpers.RandomMimeType);
        helpersObj.Import("add",      (Func<double, double, double>)ScribanTemplateHelpers.Add);
        helpersObj.Import("subtract", (Func<double, double, double>)ScribanTemplateHelpers.Subtract);
        helpersObj.Import("multiply", (Func<double, double, double>)ScribanTemplateHelpers.Multiply);
        helpersObj.Import("divide",   (Func<double, double, double>)ScribanTemplateHelpers.Divide);
        helpersObj.Import("date_time_add",     (Func<DateTime, double, string, string>)ScribanTemplateHelpers.DateTimeAdd);
        helpersObj.Import("date_time_add_fmt", (Func<DateTime, double, string, object?, string>)((dt, amt, unit, fmt) => ScribanTemplateHelpers.DateTimeAddFmt(dt, amt, unit, fmt?.ToString())));
        helpersObj.Import("date_format",       (Func<DateTime, object?, string>)((dt, fmt) => ScribanTemplateHelpers.DateFormat(dt, fmt?.ToString())));
        helpersObj.Import("faker", (Func<string, object[], object>)ScribanTemplateHelpers.Faker);

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
        // now() → current UTC DateTime (pass to date_time_add / date_time_add_fmt)
        // now_fmt('yyyy-MM-dd') → formatted string shorthand
        scriptObject.Import("now",     (Func<DateTime>)ScribanTemplateHelpers.Now);
        scriptObject.Import("now_fmt", (Func<string?, string>)(fmt => ScribanTemplateHelpers.NowFormatted(fmt)));
        // date_format works on DateTime objects returned by now()
        scriptObject.Import("date_format", (Func<DateTime, object?, string>)((dt, fmt) => ScribanTemplateHelpers.DateFormat(dt, fmt?.ToString())));
        scriptObject.Import("random_bool", (Func<bool>)ScribanTemplateHelpers.RandomBool);
        scriptObject.Import("upper", (Func<string?, string>)ScribanTemplateHelpers.Upper);
        scriptObject.Import("lower", (Func<string?, string>)ScribanTemplateHelpers.Lower);
        scriptObject.Import("random_number_string", (Func<int, string>)ScribanTemplateHelpers.RandomNumberString);
        scriptObject.Import("random_company_name", (Func<string>)ScribanTemplateHelpers.RandomCompanyName);
        scriptObject.Import("random_city", (Func<string>)ScribanTemplateHelpers.RandomCity);
        scriptObject.Import("random_country", (Func<string>)ScribanTemplateHelpers.RandomCountry);
        scriptObject.Import("random_currency_code", (Func<string>)ScribanTemplateHelpers.RandomCurrencyCode);
        scriptObject.Import("random_product_name", (Func<string>)ScribanTemplateHelpers.RandomProductName);
        scriptObject.Import("random_job_title", (Func<string>)ScribanTemplateHelpers.RandomJobTitle);
        scriptObject.Import("random_address", (Func<string>)ScribanTemplateHelpers.RandomAddress);
        scriptObject.Import("random_iban", (Func<string>)ScribanTemplateHelpers.RandomIban);

        // Personal & Contact
        scriptObject.Import("random_username", (Func<string>)ScribanTemplateHelpers.RandomUsername);
        scriptObject.Import("random_password", (Func<string>)ScribanTemplateHelpers.RandomPassword);
        scriptObject.Import("random_age", (Func<int>)ScribanTemplateHelpers.RandomAge);
        scriptObject.Import("random_birthdate", (Func<string>)ScribanTemplateHelpers.RandomBirthdate);
        scriptObject.Import("random_zip_code", (Func<string>)ScribanTemplateHelpers.RandomZipCode);
        scriptObject.Import("random_latitude", (Func<double>)ScribanTemplateHelpers.RandomLatitude);
        scriptObject.Import("random_longitude", (Func<double>)ScribanTemplateHelpers.RandomLongitude);

        // Finance & Commerce
        scriptObject.Import("random_account_number", (Func<string>)ScribanTemplateHelpers.RandomAccountNumber);
        scriptObject.Import("random_swift_code", (Func<string>)ScribanTemplateHelpers.RandomSwiftCode);
        scriptObject.Import("random_credit_card_number", (Func<string>)ScribanTemplateHelpers.RandomCreditCardNumber);
        scriptObject.Import("random_price", (Func<string>)ScribanTemplateHelpers.RandomPrice);
        scriptObject.Import("random_stock_symbol", (Func<string>)ScribanTemplateHelpers.RandomStockSymbol);
        scriptObject.Import("random_transaction_type", (Func<string>)ScribanTemplateHelpers.RandomTransactionType);

        // System & Technical
        scriptObject.Import("random_ip", (Func<string>)ScribanTemplateHelpers.RandomIp);
        scriptObject.Import("random_mac_address", (Func<string>)ScribanTemplateHelpers.RandomMacAddress);
        scriptObject.Import("random_url", (Func<string>)ScribanTemplateHelpers.RandomUrl);
        scriptObject.Import("random_status", (Func<string>)ScribanTemplateHelpers.RandomStatus);
        scriptObject.Import("random_http_status_code", (Func<int>)ScribanTemplateHelpers.RandomHttpStatusCode);
        scriptObject.Import("random_color", (Func<string>)ScribanTemplateHelpers.RandomColor);
        scriptObject.Import("random_hex_color", (Func<string>)ScribanTemplateHelpers.RandomHexColor);

        // Domain & Business
        scriptObject.Import("random_department", (Func<string>)ScribanTemplateHelpers.RandomDepartment);
        scriptObject.Import("random_category", (Func<string>)ScribanTemplateHelpers.RandomCategory);
        scriptObject.Import("random_role", (Func<string>)ScribanTemplateHelpers.RandomRole);
        scriptObject.Import("random_priority", (Func<string>)ScribanTemplateHelpers.RandomPriority);
        scriptObject.Import("random_ticket_status", (Func<string>)ScribanTemplateHelpers.RandomTicketStatus);
        scriptObject.Import("random_order_status", (Func<string>)ScribanTemplateHelpers.RandomOrderStatus);
        scriptObject.Import("random_language_code", (Func<string>)ScribanTemplateHelpers.RandomLanguageCode);
        scriptObject.Import("random_continent", (Func<string>)ScribanTemplateHelpers.RandomContinent);
        scriptObject.Import("random_timezone", (Func<string>)ScribanTemplateHelpers.RandomTimezone);
        scriptObject.Import("random_file_extension", (Func<string>)ScribanTemplateHelpers.RandomFileExtension);
        scriptObject.Import("random_mime_type", (Func<string>)ScribanTemplateHelpers.RandomMimeType);

        // Arithmetic helpers (Scriban-compatible)
        scriptObject.Import("add",      (Func<double, double, double>)ScribanTemplateHelpers.Add);
        scriptObject.Import("subtract", (Func<double, double, double>)ScribanTemplateHelpers.Subtract);
        scriptObject.Import("multiply", (Func<double, double, double>)ScribanTemplateHelpers.Multiply);
        scriptObject.Import("divide",   (Func<double, double, double>)ScribanTemplateHelpers.Divide);

        // Date/time helpers (Scriban-compatible)
        scriptObject.Import("date_time_add",     (Func<DateTime, double, string, string>)ScribanTemplateHelpers.DateTimeAdd);
        scriptObject.Import("date_time_add_fmt", (Func<DateTime, double, string, object?, string>)((dt, amt, unit, fmt) => ScribanTemplateHelpers.DateTimeAddFmt(dt, amt, unit, fmt?.ToString())));

        // Faker-style helper (Scriban-compatible)
        scriptObject.Import("faker", (Func<string, object[], object>)ScribanTemplateHelpers.Faker);

        // body('fieldName') shorthand — Scriban-compatible convenience for request.body.field
        var capturedBody = bodyForTemplate;
        scriptObject.Import("body", (Func<string, object?>)(field => ScribanTemplateHelpers.BodyField(capturedBody, field)));

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
