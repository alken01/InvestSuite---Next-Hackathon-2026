using InvestSuite.Api.Infrastructure.Data;
using InvestSuite.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InvestSuite.Api.Controllers;

[ApiController]
[Route("api/accounts")]
public sealed class AccountController : ControllerBase
{
    private readonly EfAccountRepository _accounts;

    public AccountController(EfAccountRepository accounts) => _accounts = accounts;

    [HttpGet("by-clerk-id/{clerkUserId}")]
    public ActionResult<object> GetByClerkId(string clerkUserId)
    {
        var entity = _accounts.GetByClerkUserId(clerkUserId);
        if (entity is null)
            return NotFound(new ErrorResponse("No account found for this Clerk user"));

        return Ok(new
        {
            id = entity.Id,
            first_name = entity.FirstName,
            last_name = entity.LastName,
            cash_balance = entity.CashBalance
        });
    }

    [HttpPost]
    public ActionResult<object> CreateAccount([FromBody] CreateAccountRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(new ErrorResponse("Name is required"));

        var trimmedName = request.Name.Trim();

        if (trimmedName.Length is < 2 or > 100)
            return BadRequest(new ErrorResponse("Name must be between 2 and 100 characters"));

        if (request.ExperienceLevel is not null &&
            !Enum.TryParse<ExperienceLevel>(request.ExperienceLevel, ignoreCase: true, out _))
            return BadRequest(new ErrorResponse("Invalid experience level"));

        if (request.RiskProfile is not null &&
            !Enum.TryParse<RiskProfile>(request.RiskProfile, ignoreCase: true, out _))
            return BadRequest(new ErrorResponse("Invalid risk profile"));

        if (request.Personality is not null && request.Personality.Length > 200)
            return BadRequest(new ErrorResponse("Personality must be under 200 characters"));

        if (_accounts.NameExists(trimmedName))
            return Conflict(new ErrorResponse("An account with this name already exists"));

        try
        {
            var entity = _accounts.CreateAccount(
                trimmedName,
                request.ExperienceLevel,
                request.RiskProfile,
                request.Personality,
                request.ClerkUserId);
            return Created($"/api/accounts/{entity.Id}", new
            {
                id = entity.Id,
                first_name = entity.FirstName,
                last_name = entity.LastName,
                cash_balance = entity.CashBalance
            });
        }
        catch (DbUpdateException)
        {
            return Conflict(new ErrorResponse("An account with this name already exists"));
        }
    }
}
