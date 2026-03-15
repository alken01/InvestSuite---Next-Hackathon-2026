using System.Text.Json.Serialization;

namespace InvestSuite.Api.Models;

[JsonConverter(typeof(SnakeCaseEnumConverter))]
public enum RiskProfile { Conservative, Moderate, Aggressive }

[JsonConverter(typeof(SnakeCaseEnumConverter))]
public enum ExperienceLevel { Beginner, Intermediate, Expert }

[JsonConverter(typeof(SnakeCaseEnumConverter))]
public enum TransactionType { Buy, Sell, Dividend, Deposit, Withdrawal }

[JsonConverter(typeof(SnakeCaseEnumConverter))]
public enum HoldingType { Stock, Etf, Bond, Cash }
