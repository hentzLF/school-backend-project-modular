namespace AgriMarket.Shared.Dtos;

/// <summary>
/// Generic page wrapper shared by every module's paginated query results and
/// API responses.
/// </summary>
public sealed class PaginatedResponse<T>
{
    public IEnumerable<T> Items { get; init; } = [];
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
}
