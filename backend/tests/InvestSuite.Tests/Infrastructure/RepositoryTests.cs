using FluentAssertions;
using InvestSuite.Api.Infrastructure.Data;
using InvestSuite.Api.Models;

namespace InvestSuite.Tests.Infrastructure;

// ── ScenarioRepository ────────────────────────────────────────────────

public class ScenarioRepositoryTests
{
    private readonly ScenarioRepository _repo = new();

    [Fact]
    public void GetSignals_UnknownAccount_ReturnsNull()
    {
        _repo.GetSignals("nobody", null).Should().BeNull();
    }

    [Fact]
    public void GetDefaultSignals_UnknownAccount_ReturnsNull()
    {
        _repo.GetDefaultSignals("unknown").Should().BeNull();
    }

    [Fact]
    public void GetScenariosForAccount_UnknownAccount_ReturnsEmpty()
    {
        _repo.GetScenariosForAccount("nobody").Should().BeEmpty();
    }
}

// ── StockRepository ──────────────────────────────────────────────────

public class StockRepositoryTests
{
    private readonly StockRepository _repo = new();

    [Fact]
    public void GetStock_ASML_ReturnsCorrectDetails()
    {
        var stock = _repo.GetStock("ASML");

        stock.Should().NotBeNull();
        stock!.Price.Should().Be(745.20m);
        stock.AnalystConsensus.Should().Be("Strong Buy");
    }

    [Fact]
    public void GetStock_UnknownSymbol_ReturnsNull()
    {
        _repo.GetStock("FAKE").Should().BeNull();
    }

    [Fact]
    public void GetAllStocks_ContainsMultipleStocks()
    {
        var stocks = _repo.GetAllStocks();

        stocks.Should().ContainKey("ASML");
        stocks.Should().ContainKey("ABI");
        stocks.Should().ContainKey("SAP");
        stocks.Count.Should().BeGreaterThanOrEqualTo(8);
    }
}
