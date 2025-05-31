using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Common.Paginations.Models;
using Microsoft.EntityFrameworkCore;

namespace Common.Paginations.Extensions
{
    public static class PaginationExtensions
    {
        public static async Task<List<T>> ApplyPaginationAsync<T>(this IQueryable<T> source, PaginationOptions options, HttpContext httpContext)
        {
            var totalCount = source.Count();
            var paginationInfo = new PaginationMetadata(totalCount, options.PageSize, options.PageToken);
            if (!httpContext.Response.Headers.IsReadOnly)
                httpContext.Response.Headers["X-Pagination"] = JsonSerializer.Serialize(paginationInfo);

            var paginatedList = await source
                .Skip((options.PageToken - 1) * options.PageSize)
                .Take(options.PageSize)
                .ToListAsync();

            return paginatedList;
        }
        
        public static async Task<(List<T> paginatedList, PaginationMetadata paginationMetadata)> ApplyPaginationAsync<T>(this IQueryable<T> source, PaginationOptions options)
        {
            var totalCount = source.Count();
            var paginationInfo = new PaginationMetadata(totalCount, options.PageSize, options.PageToken);

            var paginatedList = await source
                .Skip((options.PageToken - 1) * options.PageSize)
                .Take(options.PageSize)
                .ToListAsync();

            return (paginatedList, paginationInfo);
        }
    }
}