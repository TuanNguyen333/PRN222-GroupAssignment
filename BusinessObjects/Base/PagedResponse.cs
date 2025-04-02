using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Base
{
    public class PagedResponse<T>
    {
        public IEnumerable<T> Items { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;

        public PagedResponse(IEnumerable<T> items, int pageNumber, int pageSize, int totalItems)
        {
            Items = items ?? throw new ArgumentNullException(nameof(items));
            PageNumber = pageNumber;
            PageSize = pageSize;
            TotalItems = totalItems;
        }
    }
}

    

