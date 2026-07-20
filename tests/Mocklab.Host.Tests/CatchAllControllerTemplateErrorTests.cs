using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Mocklab.Host.Controllers;
using Mocklab.Host.Data;
using Mocklab.Host.Extensions;
using Mocklab.Host.Models;
using Mocklab.Host.Services;

namespace Mocklab.Host.Tests;

public class CatchAllControllerTemplateErrorTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly MocklabDbContext _dbContext;

    public CatchAllControllerTemplateErrorTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        var options = new DbContextOptionsBuilder<MocklabDbContext>()
            .UseSqlite(_connection)
            .Options;
        _dbContext = new MocklabDbContext(options);
        _dbContext.Database.EnsureCreated();
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        _connection.Dispose();
    }

    private CatchAllController CreateController(string method, string path)
    {
        var controller = new CatchAllController(
            NullLogger<CatchAllController>.Instance,
            _dbContext,
            Options.Create(new MocklabOptions()),
            new ScribanTemplateProcessor(NullLogger<ScribanTemplateProcessor>.Instance),
            new RuleEvaluator(NullLogger<RuleEvaluator>.Instance),
            new SequenceStateManager());

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Method = method;
        httpContext.Request.Path = path;
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
        return controller;
    }

    private async Task SeedMockAsync(string route, string responseBody)
    {
        _dbContext.MockResponses.Add(new MockResponse
        {
            HttpMethod = "GET",
            Route = route,
            ResponseBody = responseBody,
            ContentType = "application/json",
            StatusCode = 200,
            IsActive = true
        });
        await _dbContext.SaveChangesAsync();
    }

    [Fact]
    public async Task ValidTemplate_RendersBody_NoErrorHeader()
    {
        await SeedMockAsync("/api/score", """{ "kkbScore": {{ random_int 18 65 }} }""");
        var controller = CreateController("GET", "/api/score");

        var result = await controller.HandleAllRequests("api/score");

        var content = Assert.IsType<ContentResult>(result);
        Assert.DoesNotContain("{{", content.Content);
        Assert.False(controller.Response.Headers.ContainsKey("X-Mocklab-Template-Error"));
    }

    [Fact]
    public async Task BrokenTemplate_ReturnsRawBody_WithErrorHeaderAndLog()
    {
        // random_alpha_numeric requires a length argument -> render error
        const string body = """{ "value": {{ random_alpha_numeric }} }""";
        await SeedMockAsync("/api/broken", body);
        var controller = CreateController("GET", "/api/broken");

        var result = await controller.HandleAllRequests("api/broken");

        var content = Assert.IsType<ContentResult>(result);
        Assert.Equal(body, content.Content);
        Assert.True(controller.Response.Headers.ContainsKey("X-Mocklab-Template-Error"));

        var log = await _dbContext.RequestLogs.SingleAsync();
        Assert.False(string.IsNullOrWhiteSpace(log.TemplateError));
    }

    [Fact]
    public async Task ValidTemplate_LogsWithoutTemplateError()
    {
        await SeedMockAsync("/api/score", """{ "kkbScore": {{ random_int 18 65 }} }""");
        var controller = CreateController("GET", "/api/score");

        await controller.HandleAllRequests("api/score");

        var log = await _dbContext.RequestLogs.SingleAsync();
        Assert.Null(log.TemplateError);
    }
}
