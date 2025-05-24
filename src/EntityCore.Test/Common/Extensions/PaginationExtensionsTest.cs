using EntityCore.Tools.Common.Paginations.Extensions;

namespace EntityCore.Test.Common.Extensions
{
    public class PaginationExtensionsTest
    {
        [Fact]
        public void Generate_Should_Return_True_Code()
        {
            // Arrange

            var expectedCode = @"using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Common.Paginations.Models;

namespace Common.Paginations.Extensions
{
    public static class PaginationExtensions
    {
        public static IQueryable<T> ApplyPagination<T>(this IQueryable<T> source, PaginationOptions options, HttpContext httpContext)
        {
            var totalCount = source.Count();
            var paginationInfo = new PaginationMetadata(totalCount, options.PageSize, options.PageToken);
            httpContext.Response.Headers[""X-Pagination""] = JsonSerializer.Serialize(paginationInfo);
            return source.Skip((options.PageToken - 1) * options.PageSize).Take(options.PageSize);
        }
    }
}";

            var service = new PaginationExtensions();

            // Act
            var code = service.GeneratePaginationExtensions();

            // Assert
            Assert.Equal(expectedCode, code, ignoreLineEndingDifferences: true);
        }
    }
}
