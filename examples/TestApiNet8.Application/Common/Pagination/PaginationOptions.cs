namespace Common.Paginations.Models
{
    public class PaginationOptions
    {
        private int _pageSize;
        private int _pageToken;
        public PaginationOptions(int pageSize, int pageToken)
        {
            (PageSize, PageToken) = (pageSize, pageToken);
        }

        public int PageSize { get => _pageSize; set => _pageSize = value <= 0 ? 20 : value; }
        public int PageToken { get => _pageToken; set => _pageToken = value <= 0 ? 1 : value; }
    }
}