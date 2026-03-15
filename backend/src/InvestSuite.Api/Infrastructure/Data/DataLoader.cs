using InvestSuite.Api.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace InvestSuite.Api.Infrastructure.Data;

public static class DataLoader
{
    public static IServiceCollection AddDataRepositories(this IServiceCollection services, IConfiguration configuration)
    {
        // Static repos (scenario signals and stock data)
        services.AddSingleton<IScenarioRepository, ScenarioRepository>();
        services.AddSingleton<IStockRepository, StockRepository>();

        // SQLite + EF Core
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Data Source=investsuite.db";
        services.AddDbContext<InvestSuiteDbContext>(options =>
            options.UseSqlite(connectionString));

        // EF-backed repos (wrap legacy + SQLite)
        services.AddScoped<EfAccountRepository>();
        services.AddScoped<IAccountRepository>(sp => sp.GetRequiredService<EfAccountRepository>());
        services.AddScoped<EfTransactionRepository>();
        services.AddScoped<ITransactionRepository>(sp => sp.GetRequiredService<EfTransactionRepository>());

        // Historical prices
        services.AddHttpClient<YahooFinancePriceService>();
        services.AddSingleton<IHistoricalPriceService>(sp =>
        {
            var scopeFactory = sp.GetRequiredService<IServiceScopeFactory>();
            var http = sp.GetRequiredService<IHttpClientFactory>().CreateClient();
            var logger = sp.GetRequiredService<ILogger<YahooFinancePriceService>>();
            return new YahooFinancePriceService(scopeFactory, http, logger);
        });

        // Holdings calculator
        services.AddScoped<HoldingsCalculator>();

        return services;
    }
}
