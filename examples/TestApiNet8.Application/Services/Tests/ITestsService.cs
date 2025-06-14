using Common.Paginations.Models;
using Common;
using DataTransferObjects.Tests;
using TestApiNet8.Domain.Entities;

namespace Services.Tests
{
    public interface ITestsService
    {
        Task<TestViewModel> AddAsync(TestCreationDto testCreationDto);
        Task<List<TestViewModel>> GetAllAsync();
        Task<ListResult<TestViewModel>> FilterAsync(PaginationOptions filter);
        Task<TestViewModel> GetByIdAsync(Guid id);
        Task<TestViewModel> UpdateAsync(Guid id, TestModificationDto testModificationDto);
        Task<TestViewModel> DeleteAsync(Guid id);
    }
}