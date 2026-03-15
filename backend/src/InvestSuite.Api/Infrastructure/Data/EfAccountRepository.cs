using InvestSuite.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace InvestSuite.Api.Infrastructure.Data;

public sealed class EfAccountRepository : IAccountRepository
{
    private readonly InvestSuiteDbContext _db;

    public EfAccountRepository(InvestSuiteDbContext db) => _db = db;

    public Account? GetById(string id)
    {
        var entity = _db.Accounts.AsNoTracking().FirstOrDefault(a => a.Id == id);
        if (entity is null) return null;

        return MapEntity(entity);
    }

    public IReadOnlyDictionary<string, Account> GetAll()
    {
        var all = new Dictionary<string, Account>();

        var dbAccounts = _db.Accounts.AsNoTracking().ToList();

        foreach (var entity in dbAccounts)
        {
            all[entity.Id] = MapEntity(entity);
        }

        return all;
    }

    private static Account MapEntity(AccountEntity entity)
    {
        var risk = Enum.TryParse<RiskProfile>(entity.RiskProfile, ignoreCase: true, out var r)
            ? r : RiskProfile.Moderate;
        var experience = Enum.TryParse<ExperienceLevel>(entity.ExperienceLevel, ignoreCase: true, out var e)
            ? e : ExperienceLevel.Beginner;
        var personality = !string.IsNullOrWhiteSpace(entity.Personality)
            ? entity.Personality
            : "Time-travel simulator — exploring historical market moments";

        return new Account(
            Id: entity.Id,
            FirstName: entity.FirstName,
            LastName: entity.LastName,
            Email: $"{entity.FirstName.ToLowerInvariant()}@simulator.local",
            DateOfBirth: new DateOnly(1995, 1, 1),
            AccountOpened: DateOnly.FromDateTime(entity.CreatedAt.DateTime),
            RiskProfile: risk,
            ExperienceLevel: experience,
            Personality: personality,
            CheckFrequency: "Active session"
        );
    }

    public AccountEntity? GetEntityById(string id) =>
        _db.Accounts.FirstOrDefault(a => a.Id == id);

    public AccountEntity? GetByClerkUserId(string clerkUserId) =>
        _db.Accounts.AsNoTracking().FirstOrDefault(a => a.ClerkUserId == clerkUserId);

    public bool NameExists(string fullName)
    {
        var parts = fullName.Trim().Split(' ', 2);
        var firstName = parts[0];
        var lastName = parts.Length > 1 ? parts[1] : "";

        return _db.Accounts.Any(a => a.FirstName == firstName && a.LastName == lastName);
    }

    public AccountEntity CreateAccount(
        string name,
        string? experienceLevel = null,
        string? riskProfile = null,
        string? personality = null,
        string? clerkUserId = null)
    {
        var parts = name.Trim().Split(' ', 2);

        // Retry up to 3 times on ID collision
        for (var attempt = 0; attempt < 3; attempt++)
        {
            var id = Guid.NewGuid().ToString("N")[..8];
            if (_db.Accounts.Any(a => a.Id == id))
                continue;

            var entity = new AccountEntity
            {
                Id = id,
                FirstName = parts[0],
                LastName = parts.Length > 1 ? parts[1] : "",
                CashBalance = 1000m,
                CreatedAt = DateTimeOffset.UtcNow,
                ExperienceLevel = experienceLevel,
                RiskProfile = riskProfile,
                Personality = personality,
                ClerkUserId = clerkUserId
            };
            _db.Accounts.Add(entity);
            _db.SaveChanges();
            return entity;
        }

        throw new InvalidOperationException("Failed to generate a unique account ID after 3 attempts");
    }
}
