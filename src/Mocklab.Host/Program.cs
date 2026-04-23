using Mocklab.Host.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add Mocklab services — reads the "Mocklab" appsettings section automatically
builder.Services.AddMocklab(builder.Configuration);

// Add CORS for frontend development
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy.WithOrigins("http://localhost:3000", "http://localhost:5173")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

builder.Services.AddOpenApi();

builder.Services.AddHealthChecks();

var app = builder.Build();

app.MapOpenApi();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/openapi/v1.json", "Mocklab API");
    c.RoutePrefix = "swagger";
});

// Only use HTTPS redirection in production
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// Enable CORS
app.UseCors("AllowFrontend");

// Enable Mocklab middleware (handles DB migration, seeding, and frontend UI)
app.UseMocklab();

// Map health check endpoint for container liveness/readiness probes
app.MapHealthChecks("/health");

// Map controllers
app.MapControllers();

app.Run();
