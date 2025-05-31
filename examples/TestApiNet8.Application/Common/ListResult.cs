using Common.Paginations.Models;

namespace TestApiNet8.Application.Common
{
    public class ListResult<T>
    {
        public PaginationMetadata Pagination { get; set; }
        public List<T> Items { get; set; }
    }
}
