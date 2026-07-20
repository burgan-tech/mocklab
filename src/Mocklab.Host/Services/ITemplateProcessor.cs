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

    /// <summary>
    /// Same as <see cref="ProcessTemplate"/> but also reports parse/render errors instead of
    /// silently falling back to the raw template. On error, <see cref="TemplateRenderResult.Output"/>
    /// contains the unprocessed template and <see cref="TemplateRenderResult.Error"/> describes the failure.
    /// </summary>
    TemplateRenderResult ProcessTemplateDetailed(string template, HttpRequest request, TemplateRequestContext? context = null);
}

/// <summary>
/// Result of a template render: the output string plus an optional error description
/// when the template could not be (fully) processed.
/// </summary>
public sealed record TemplateRenderResult(string Output, string? Error = null);
