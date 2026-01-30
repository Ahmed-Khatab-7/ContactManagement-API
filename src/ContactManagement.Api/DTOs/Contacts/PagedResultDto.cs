namespace ContactManagement.Api.DTOs.Contacts;

/// <summary>
/// Generic paged result wrapper for pagination support.
/// </summary>
public record PagedResultDto<T>(
    IEnumerable<T> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages
)
{
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}
