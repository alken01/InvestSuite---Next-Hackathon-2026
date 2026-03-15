using InvestSuite.Api.Infrastructure.Claude;
using InvestSuite.Api.Infrastructure.Data;
using InvestSuite.Api.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// CORS — allow Next.js frontend
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        var origins = (builder.Configuration.GetValue<string>("AllowedOrigins") ?? "http://localhost:3000")
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        policy.WithOrigins(origins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// Controllers
builder.Services.AddControllers();

// Data repositories
builder.Services.AddDataRepositories(builder.Configuration);

// Claude
builder.Services.Configure<ClaudeOptions>(
    builder.Configuration.GetSection(ClaudeOptions.SECTION_NAME));
builder.Services.AddHttpClient<ClaudeClient>(client =>
{
    var baseUrl = builder.Configuration.GetValue<string>($"{ClaudeOptions.SECTION_NAME}:BaseUrl")
        ?? "https://api.anthropic.com";
    client.BaseAddress = new Uri(baseUrl);
});
builder.Services.AddSingleton<IPromptBuilder, PromptBuilder>();
builder.Services.AddMemoryCache();
builder.Services.AddScoped<IClaudeClient, CachedClaudeClient>();

// Services
builder.Services.AddHttpClient<INewsService, YahooFinanceNewsService>();
builder.Services.AddScoped<IAdaptiveLayoutService, AdaptiveLayoutService>();

var app = builder.Build();

// Ensure SQLite database exists
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<InvestSuiteDbContext>();
    db.Database.EnsureCreated();
}

// Seed Yahoo Finance prices in background
_ = Task.Run(async () =>
{
    try
    {
        var prices = app.Services.GetRequiredService<IHistoricalPriceService>();
        await prices.SeedPricesIfNeededAsync();
    }
    catch (Exception ex)
    {
        app.Logger.LogWarning(ex, "Background price seeding failed — app will still work");
    }
});
// Global Exception Handler
app.UseExceptionHandler(exceptionHandlerApp =>
{
    exceptionHandlerApp.Run(async context =>
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";

        var exceptionHandlerPathFeature =
            context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerPathFeature>();

        var errorResponse = new
        {
            error = "An unexpected error occurred.",
            detail = exceptionHandlerPathFeature?.Error.Message
        };

        app.Logger.LogError(exceptionHandlerPathFeature?.Error, "Unhandled exception caught by global handler");

        await context.Response.WriteAsJsonAsync(errorResponse);
    });
});

app.UseCors();
app.MapControllers();
app.MapGet("/api/health", () => Results.Ok(new { status = "healthy" }));

app.Run();
