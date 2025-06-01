using System.Text.Json;
using Common.Paginations.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

namespace Common.Paginations.Extensions
{
    public static class PaginationExtensions
    {
        public static async Task<(List<T> paginatedList, PaginationMetadata paginationMetadata)> ApplyPaginationAsync<T>(this IQueryable<T> source, PaginationOptions options)
        {
            var totalCount = source.Count();
            var paginationInfo = new PaginationMetadata(totalCount, options.PageSize, options.PageToken);
            var paginatedList = await source.Skip((options.PageToken - 1) * options.PageSize).Take(options.PageSize).ToListAsync();
            return (paginatedList, paginationInfo);
        }
    }
}