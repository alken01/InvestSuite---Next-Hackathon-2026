using InvestSuite.Api.Infrastructure.Data;
using InvestSuite.Api.Infrastructure.Services;
using InvestSuite.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace InvestSuite.Api.Controllers;

[ApiController]
[Route("api/stocks")]
public sealed class StockController : ControllerBase
{
    private readonly IHistoricalPriceService _prices;
    private readonly IStockRepository _stocks;

    public StockController(IHistoricalPriceService prices, IStockRepository stocks)
    {
        _prices = prices;
        _stocks = stocks;
    }

    [HttpGet("{symbol}/price")]
    public ActionResult<object> GetPrice(string symbol, [FromQuery] string? date = null)
    {
        if (_stocks.GetStock(symbol) is null)
            return NotFound(new ErrorResponse($"Unknown stock: {symbol}"));

        var targetDate = date is not null && DateOnly.TryParse(date, out var d)
            ? d : DateOnly.FromDateTime(DateTime.UtcNow);

        var price = _prices.GetClosingPrice(symbol, targetDate);
        if (!price.HasValue)
            return NotFound(new ErrorResponse($"No price data for {symbol} on {targetDate}"));

        return Ok(new { symbol, date = targetDate.ToString("yyyy-MM-dd"), price = price.Value });
    }
}
