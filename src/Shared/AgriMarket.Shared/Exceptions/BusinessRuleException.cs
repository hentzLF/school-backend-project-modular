namespace AgriMarket.Shared.Exceptions;

/// <summary>Thrown when a domain business rule is violated.</summary>
public sealed class BusinessRuleException : Exception
{
    public BusinessRuleException(string message)
        : base(message)
    {
    }
}
