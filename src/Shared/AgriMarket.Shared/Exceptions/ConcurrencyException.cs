namespace AgriMarket.Shared.Exceptions;

/// <summary>Thrown when an optimistic-concurrency conflict is detected on save.</summary>
public sealed class ConcurrencyException(string message) : Exception(message);
