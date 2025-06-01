using Common.Paginations.Models;

namespace Common
{
    public class ListResult<T>
    {
        public ListResult(PaginationMetadata pagination, List<T> items)
        {
            Pagination = pagination;
            Items = items;
        }

        public PaginationMetadata Pagination { get; set; }
        public List<T> Items { get; set; }
    }
}
