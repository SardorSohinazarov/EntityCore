namespace Common.Paginations.Models
{
    public class PaginationMetadata
    {
        public int TotalCount { get; }
        public int TotalPages { get; }
        public int CurrentPage { get; }
        public int PageSize { get; }

        public PaginationMetadata(int totalCount, int pageSize, int currentPage)
        {
            TotalCount = totalCount;
            PageSize = pageSize;
            CurrentPage = currentPage;
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        }
    }
}