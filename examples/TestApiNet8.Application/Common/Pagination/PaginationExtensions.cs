using System;
using System.Linq;
using System.Text.Json;
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
            httpContext.Response.Headers["X-Pagination"] = JsonSerializer.Serialize(paginationInfo);
            return source.Skip((options.PageToken - 1) * options.PageSize).Take(options.PageSize);
        }
    }
}