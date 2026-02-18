using System.Text.Json;
using System.Text.RegularExpressions;
using Mocklab.App.Models;

namespace Mocklab.App.Services;

/// <summary>
/// Evaluates conditional response rules against incoming HTTP requests.
/// Rules are evaluated in priority order (ascending). The first matching rule wins.
/// </summary>
public class RuleEvaluator : IRuleEvaluator
{
    private readonly ILogger<RuleEvaluator> _logger;

    public RuleEvaluator(ILogger<RuleEvaluator> logger)
    {
        _logger = logger;
    }

    public MockResponseRule? Evaluate(IEnumerable<MockResponseRule> rules, HttpRequest request, string? requestBody)
    {
        var orderedRules = rules.OrderBy(r => r.Priority);

        foreach (var rule in orderedRules)
        {
            try
            {
                var fieldValue = ExtractFieldValue(rule.ConditionField, request, requestBody);
                var matches = EvaluateCondition(fieldValue, rule.ConditionOperator, rule.ConditionValue);

                if (matches)
                {
                    _logger.LogInformation(
                        "Rule matched: Id={RuleId}, Field={Field}, Operator={Operator}, Value={Value}",
                        rule.Id, rule.ConditionField, rule.ConditionOperator, rule.ConditionValue);
                    return rule;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Error evaluating rule Id={RuleId}, Field={Field}: {Message}",
                    rule.Id, rule.ConditionField, ex.Message);
            }
        }

        return null;
    }

    /// <summary>
    /// Extracts the value of the specified field from the HTTP request.
    /// Supported field formats:
    /// - "header.HeaderName" → Request header value
    /// - "query.paramName" → Query string parameter
    /// - "body.propertyPath" → JSON body property (supports nested: body.user.name)
    /// - "method" → HTTP method
    /// - "path" → Request path
    /// </summary>
    private static string? ExtractFieldValue(string conditionField, HttpRequest request, string? requestBody)
    {
        if (string.IsNullOrEmpty(conditionField))
            return null;

        // header.X-Api-Key
        if (conditionField.StartsWith("header.", StringComparison.OrdinalIgnoreCase))
        {
            var headerName = conditionField[7..];
            return request.Headers.TryGetValue(headerName, out var headerValue)
                ? headerValue.ToString()
                : null;
        }

        // query.page
        if (conditionField.StartsWith("query.", StringComparison.OrdinalIgnoreCase))
        {
            var paramName = conditionField[6..];
            return request.Query.TryGetValue(paramName, out var queryValue)
                ? queryValue.ToString()
                : null;
        }

        // body.amount or body.user.name (JSON path)
        if (conditionField.StartsWith("body.", StringComparison.OrdinalIgnoreCase))
        {
            if (string.IsNullOrEmpty(requestBody))
                return null;

            var jsonPath = conditionField[5..];
            return ExtractJsonValue(requestBody, jsonPath);
        }

        // method
        if (conditionField.Equals("method", StringComparison.OrdinalIgnoreCase))
        {
            return request.Method;
        }

        // path
        if (conditionField.Equals("path", StringComparison.OrdinalIgnoreCase))
        {
            return request.Path.Value;
        }

        return null;
    }

    /// <summary>
    /// Extracts a value from a JSON string using a dot-notation path.
    /// Supports nested properties: "user.address.city"
    /// </summary>
    private static string? ExtractJsonValue(string json, string dotPath)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            var current = doc.RootElement;
            var segments = dotPath.Split('.');

            foreach (var segment in segments)
            {
                if (current.ValueKind == JsonValueKind.Object && current.TryGetProperty(segment, out var next))
                {
                    current = next;
                }
                else
                {
                    return null;
                }
            }

            return current.ValueKind switch
            {
                JsonValueKind.String => current.GetString(),
                JsonValueKind.Number => current.GetRawText(),
                JsonValueKind.True => "true",
                JsonValueKind.False => "false",
                JsonValueKind.Null => null,
                _ => current.GetRawText()
            };
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Evaluates a condition against a field value using the specified operator.
    /// </summary>
    private static bool EvaluateCondition(string? fieldValue, string conditionOperator, string? conditionValue)
    {
        return conditionOperator.ToLowerInvariant() switch
        {
            "equals" => string.Equals(fieldValue, conditionValue, StringComparison.OrdinalIgnoreCase),
            "contains" => fieldValue != null && conditionValue != null &&
                          fieldValue.Contains(conditionValue, StringComparison.OrdinalIgnoreCase),
            "startswith" => fieldValue != null && conditionValue != null &&
                            fieldValue.StartsWith(conditionValue, StringComparison.OrdinalIgnoreCase),
            "endswith" => fieldValue != null && conditionValue != null &&
                          fieldValue.EndsWith(conditionValue, StringComparison.OrdinalIgnoreCase),
            "regex" => fieldValue != null && conditionValue != null &&
                       Regex.IsMatch(fieldValue, conditionValue, RegexOptions.IgnoreCase),
            "exists" => fieldValue != null,
            "notexists" => fieldValue == null,
            "greaterthan" => TryCompareNumeric(fieldValue, conditionValue, (a, b) => a > b),
            "lessthan" => TryCompareNumeric(fieldValue, conditionValue, (a, b) => a < b),
            _ => false
        };
    }

    /// <summary>
    /// Tries to compare two values numerically using the provided comparison function.
    /// </summary>
    private static bool TryCompareNumeric(string? fieldValue, string? conditionValue, Func<double, double, bool> comparison)
    {
        if (fieldValue == null || conditionValue == null)
            return false;

        if (double.TryParse(fieldValue, out var fieldNum) && double.TryParse(conditionValue, out var conditionNum))
        {
            return comparison(fieldNum, conditionNum);
        }

        return false;
    }
}
