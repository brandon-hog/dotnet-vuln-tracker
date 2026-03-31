namespace Shared.Dtos;

public class PagedResponse<T>
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalRecords { get; set; }
    public List<T> Data { get; set; } = [];
}
