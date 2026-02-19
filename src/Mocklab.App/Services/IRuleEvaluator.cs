using Mocklab.App.Models;

namespace Mocklab.App.Services;

/// <summary>
/// Evaluates conditional response rules against incoming HTTP requests.
/// Rules are evaluated in priority order (ascending). The first matching rule wins.
/// </summary>
public interface IRuleEvaluator
{
    /// <summary>
    /// Evaluates the rules of a mock response against the current HTTP request.
    /// </summary>
    /// <param name="rules">The rules to evaluate, ordered by priority</param>
    /// <param name="request">The incoming HTTP request</param>
    /// <param name="requestBody">The request body (already read)</param>
    /// <returns>The matching rule, or null if no rule matches</returns>
    MockResponseRule? Evaluate(IEnumerable<MockResponseRule> rules, HttpRequest request, string? requestBody);
}
