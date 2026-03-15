using InvestSuite.Api.Infrastructure.Data;
using InvestSuite.Api.Infrastructure.Services;
using InvestSuite.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace InvestSuite.Api.Controllers;

[ApiController]
[Route("api")]
public sealed class LayoutController : ControllerBase
{
    private readonly IAdaptiveLayoutService _layoutService;
    private readonly ILogger<LayoutController> _logger;

    public LayoutController(IAdaptiveLayoutService layout, ILogger<LayoutController> logger)
    {
        _layoutService = layout;
        _logger = logger;
    }

    [HttpGet("context/{accountId}")]
    [ProducesResponseType(typeof(LayoutPayload), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public ActionResult<LayoutPayload> GetContext(
        string accountId,
        [FromQuery] string? scenario = null,
        [FromQuery] string? date = null)
    {
        var asOfDate = ParseDate(date);
        var layout = _layoutService.BuildContextLayout(accountId, scenario, asOfDate);
        if (layout is null)
            return NotFound(new ErrorResponse($"Unknown account: {accountId}"));

        return Ok(layout);
    }

    [HttpPost("layout")]
    [ProducesResponseType(typeof(LayoutPayload), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<LayoutPayload>> PostLayout(
        [FromBody] LayoutRequest request,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.AccountId))
            return BadRequest(new ErrorResponse("AccountId is required"));

        var accountId = request.AccountId;
        var asOfDate = ParseDate(request.Date);

        var layout = _layoutService.BuildContextLayout(accountId, request.Scenario, asOfDate);
        if (layout is null)
            return NotFound(new ErrorResponse($"Unknown account: {accountId}"));

        try
        {
            var aiLayout = await _layoutService.GenerateLayoutPayloadAsync(
                accountId, request.Query, request.Scenario, asOfDate, ct);
            return Ok(aiLayout);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "LLM generation failed for {AccountId}", accountId);
            return StatusCode(500, new ErrorResponse("AI generation failed", ex.Message));
        }
    }

    [HttpGet("accounts")]
    [ProducesResponseType(typeof(IReadOnlyList<AccountSummary>), StatusCodes.Status200OK)]
    public ActionResult<IReadOnlyList<AccountSummary>> GetAccounts()
    {
        return Ok(_layoutService.GetAccounts());
    }

    [HttpGet("key-moments")]
    public ActionResult<IReadOnlyList<KeyMoment>> GetKeyMoments()
    {
        return Ok(KeyMomentRepository.Moments);
    }

    private static DateOnly? ParseDate(string? date) =>
        date is not null && DateOnly.TryParse(date, out var d) ? d : null;
}
