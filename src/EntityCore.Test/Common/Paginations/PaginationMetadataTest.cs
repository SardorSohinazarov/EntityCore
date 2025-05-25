using EntityCore.Tools.Common.Paginations.Models;

namespace EntityCore.Test.Common.Paginations
{
    public class PaginationMetadataTest
    {
        [Fact]
        public void Generate_Should_Return_True_Code()
        {
            // Arrange
            var expectedCode = @"namespace Common.Paginations.Models
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
}";

            var service = new PaginationMetadata();

            // Act
            var code = service.Generate();

            // Assert
            Assert.Equal(expectedCode, code, ignoreLineEndingDifferences: true);
        }
    }
}
