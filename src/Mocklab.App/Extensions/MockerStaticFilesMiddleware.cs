using Microsoft.AspNetCore.Http;
using System.Reflection;

namespace Mocklab.App.Extensions;

/// <summary>
/// Middleware to serve embedded frontend static files
/// </summary>
public class MocklabStaticFilesMiddleware
{
    private readonly RequestDelegate _next;
    private static readonly Dictionary<string, string> ContentTypeMap = new()
    {
        { ".html", "text/html" },
        { ".css", "text/css" },
        { ".js", "application/javascript" },
        { ".json", "application/json" },
        { ".png", "image/png" },
        { ".jpg", "image/jpeg" },
        { ".jpeg", "image/jpeg" },
        { ".gif", "image/gif" },
        { ".svg", "image/svg+xml" },
        { ".ico", "image/x-icon" },
        { ".woff", "font/woff" },
        { ".woff2", "font/woff2" },
        { ".ttf", "font/ttf" },
        { ".eot", "application/vnd.ms-fontobject" }
    };

    public MocklabStaticFilesMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? "";

        // Only handle requests starting with /_admin (but not /_admin/mocks API endpoints)
        if (path.StartsWith("/_admin", StringComparison.OrdinalIgnoreCase))
        {
            // Skip API endpoints (/_admin/mocks/*)
            if (path.StartsWith("/_admin/mocks", StringComparison.OrdinalIgnoreCase))
            {
                await _next(context);
                return;
            }

            // For paths with a file extension (.js, .css, .svg, etc.), try embedded resources
            if (Path.HasExtension(path))
            {
                var served = await TryServeEmbeddedResource(context, path);
                if (served) return;
            }

            // SPA fallback - serve index.html for any non-file requests
            // This allows React Router to handle the routing
            // Handles: /_admin, /_admin/, /_admin/some-page, etc.
            await ServeIndexHtml(context);
            return;
        }

        await _next(context);
    }

    private async Task<bool> TryServeEmbeddedResource(HttpContext context, string requestPath)
    {
        try
        {
            // Convert URL path to embedded resource name
            // /_admin/assets/index.js -> wwwroot._mocklab.assets.index.js
            var resourcePath = requestPath
                .Replace("/_admin/", "wwwroot._mocklab.")
                .Replace("/_admin", "wwwroot._mocklab.index.html")
                .Replace("/", ".")
                .TrimStart('.');

            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = $"Mocklab.App.{resourcePath}";

            // Try to find the resource (case-insensitive)
            var actualResourceName = assembly.GetManifestResourceNames()
                .FirstOrDefault(r => r.Equals(resourceName, StringComparison.OrdinalIgnoreCase));

            if (actualResourceName == null)
                return false;

            using var stream = assembly.GetManifestResourceStream(actualResourceName);
            
            if (stream == null)
                return false;

            // Set content type
            var extension = Path.GetExtension(requestPath).ToLowerInvariant();
            if (ContentTypeMap.TryGetValue(extension, out var contentType))
            {
                context.Response.ContentType = contentType;
            }
            else
            {
                context.Response.ContentType = "application/octet-stream";
            }

            // Set cache headers for static assets
            if (extension != ".html")
            {
                context.Response.Headers["Cache-Control"] = "public, max-age=31536000";
            }

            await stream.CopyToAsync(context.Response.Body);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private async Task ServeIndexHtml(HttpContext context)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var indexResourceName = assembly.GetManifestResourceNames()
            .FirstOrDefault(r => r.EndsWith("wwwroot._mocklab.index.html", StringComparison.OrdinalIgnoreCase));

        if (indexResourceName != null)
        {
            using var stream = assembly.GetManifestResourceStream(indexResourceName);
            
            if (stream != null)
            {
                context.Response.ContentType = "text/html";
                context.Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
                await stream.CopyToAsync(context.Response.Body);
                return;
            }
        }

        // Fallback: if embedded resources not found, show a helpful message
        context.Response.ContentType = "text/html";
        await context.Response.WriteAsync(@"
<!DOCTYPE html>
<html>
<head>
    <title>Mocklab Admin UI</title>
    <style>
        body { font-family: system-ui, -apple-system, sans-serif; padding: 40px; max-width: 800px; margin: 0 auto; }
        .warning { background: #fff3cd; border: 1px solid #ffc107; padding: 20px; border-radius: 8px; }
        code { background: #f5f5f5; padding: 2px 6px; border-radius: 3px; }
    </style>
</head>
<body>
    <div class='warning'>
        <h1>⚠️ Mocklab Admin UI Not Available</h1>
        <p>The frontend resources are not embedded in the assembly.</p>
        <p>To build and embed the frontend:</p>
        <ol>
            <li>Navigate to the <code>frontend/</code> directory</li>
            <li>Run <code>npm install</code></li>
            <li>Run <code>npm run build</code></li>
            <li>Rebuild the project with <code>dotnet build</code></li>
        </ol>
        <p>The API endpoints are still available at <code>/_admin/mocks</code></p>
    </div>
</body>
</html>");
    }
}
