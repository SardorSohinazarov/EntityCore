using Common.Paginations.Models;
using Common;
using DataTransferObjects.Questions;
using TestApiNet8.Domain.Entities;

namespace Services.Questions
{
    public interface IQuestionsService
    {
        Task<QuestionViewModel> AddAsync(QuestionCreationDto questionCreationDto);
        Task<List<QuestionViewModel>> GetAllAsync();
        Task<ListResult<QuestionViewModel>> FilterAsync(PaginationOptions filter);
        Task<QuestionViewModel> GetByIdAsync(Guid id);
        Task<QuestionViewModel> UpdateAsync(Guid id, QuestionModificationDto questionModificationDto);
        Task<QuestionViewModel> DeleteAsync(Guid id);
    }
}