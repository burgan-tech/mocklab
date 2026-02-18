using Microsoft.AspNetCore.Http;

namespace Mocklab.App.Services;

/// <summary>
/// Processes template variables in response bodies
/// </summary>
public interface ITemplateProcessor
{
    /// <summary>
    /// Processes template variables in the given template string
    /// </summary>
    /// <param name="template">The template string containing variables like {{$randomUUID}}</param>
    /// <param name="request">The current HTTP request for request-based variables</param>
    /// <returns>The processed string with variables replaced</returns>
    string ProcessTemplate(string template, HttpRequest request);
}
