using Microsoft.AspNetCore.Http;

namespace Mocklab.Host.Services;

/// <summary>
/// Processes template variables in response bodies (and optionally header values) using Scriban.
/// </summary>
public interface ITemplateProcessor
{
    /// <summary>
    /// Processes template variables in the given template string.
    /// </summary>
    /// <param name="template">The template string (Scriban syntax; legacy {{$...}} is also supported via pre-pass)</param>
    /// <param name="request">The current HTTP request for request-based variables</param>
    /// <param name="context">Optional context (route params, pre-read body, data buckets)</param>
    /// <returns>The processed string with variables replaced</returns>
    string ProcessTemplate(string template, HttpRequest request, TemplateRequestContext? context = null);
}
