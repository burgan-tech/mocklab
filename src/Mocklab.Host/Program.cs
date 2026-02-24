using Mocklab.Host.Extensions;
using Mocklab.Host.Services;

var builder = WebApplication.CreateBuilder(args);

// Add Mocklab services with configuration from appsettings.json
builder.Services.AddMocklab(options =>
{
    // Bind configuration from appsettings.json "Mocklab" section
    builder.Configuration.GetSection("Mocklab").Bind(options);
    
    // Override connection string if not set in config
    if (string.IsNullOrEmpty(options.ConnectionString))
    {
        options.ConnectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
                                    ?? "Data Source=mocklab.db";
    }
});

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

// Add OpenAPI/Swagger
builder.Services.AddOpenApi();

// Add HttpClient for cURL import feature
builder.Services.AddHttpClient();

// Register business services
builder.Services.AddScoped<IMockImportService, MockImportService>();
builder.Services.AddSingleton<ITemplateProcessor, ScribanTemplateProcessor>();
builder.Services.AddSingleton<IRuleEvaluator, RuleEvaluator>();
builder.Services.AddSingleton<ISequenceStateManager, SequenceStateManager>();

var app = builder.Build();

// Enable OpenAPI endpoint
app.MapOpenApi();

// Only use HTTPS redirection in production
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// Enable CORS
app.UseCors("AllowFrontend");

// Enable Mocklab middleware (handles DB migration, seeding, and frontend UI)
app.UseMocklab();

// Map controllers
app.MapControllers();

app.Run();
