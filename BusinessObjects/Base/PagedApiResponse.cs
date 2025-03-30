using BusinessObjects.Base;

public class PagedApiResponse<T> : ApiResponse<PagedResponse<T>>
{
    public static PagedApiResponse<T> SuccessPagedResponse(
        IEnumerable<T> items,
        int pageNumber,
        int pageSize,
        int totalItems,
        string message = "Data retrieved successfully")
    {
        var pagedResponse = new PagedResponse<T>(items, pageNumber, pageSize, totalItems);
        return new PagedApiResponse<T>
        {
            Success = true,
            Data = pagedResponse,
            Message = message,
            Errors = null
        };
    }
}