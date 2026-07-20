using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Mocklab.Host.Services;

namespace Mocklab.Host.Tests;

public class TemplateRenderResultTests
{
    private static TemplateRenderResult RenderDetailed(string template)
    {
        var processor = new ScribanTemplateProcessor(NullLogger<ScribanTemplateProcessor>.Instance);
        var request = new DefaultHttpContext().Request;
        return processor.ProcessTemplateDetailed(template, request);
    }

    [Fact]
    public void ValidTemplate_RendersWithoutError()
    {
        var result = RenderDetailed("{{ random_int 18 65 }}");

        Assert.Null(result.Error);
        Assert.InRange(int.Parse(result.Output), 18, 65);
    }

    [Fact]
    public void RenderException_ReturnsRawTemplateWithError()
    {
        // random_alpha_numeric requires a length argument -> render-time exception
        const string template = "{{ random_alpha_numeric }}";

        var result = RenderDetailed(template);

        Assert.Equal(template, result.Output);
        Assert.False(string.IsNullOrWhiteSpace(result.Error));
    }

    [Fact]
    public void ParseError_ReturnsRawTemplateWithError()
    {
        const string template = "{{ 1 + }}";

        var result = RenderDetailed(template);

        Assert.Equal(template, result.Output);
        Assert.False(string.IsNullOrWhiteSpace(result.Error));
    }

    [Fact]
    public void OneBadToken_StillReportsErrorForWholeBody()
    {
        const string template = """{ "ok": {{ random_int 1 5 }}, "bad": {{ random_alpha_numeric }} }""";

        var result = RenderDetailed(template);

        Assert.Equal(template, result.Output);
        Assert.NotNull(result.Error);
    }

    [Fact]
    public void EmptyTemplate_ReturnsEmptyWithoutError()
    {
        var result = RenderDetailed(string.Empty);

        Assert.Equal(string.Empty, result.Output);
        Assert.Null(result.Error);
    }
}
