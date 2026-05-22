namespace AgriMarket.Modules.Messaging.Dtos;

internal sealed class PaginatedResponse<T>
{
    public IEnumerable<T> Items { get; init; } = [];
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
}
