using Common.Paginations.Models;
using Common;
using DataTransferObjects.Answers;
using TestApiNet8.Domain.Entities;

namespace Services.Answers
{
    public interface IAnswersService
    {
        Task<AnswerViewModel> AddAsync(AnswerCreationDto answerCreationDto);
        Task<List<AnswerViewModel>> GetAllAsync();
        Task<ListResult<AnswerViewModel>> FilterAsync(PaginationOptions filter);
        Task<AnswerViewModel> GetByIdAsync(Guid id);
        Task<AnswerViewModel> UpdateAsync(Guid id, AnswerModificationDto answerModificationDto);
        Task<AnswerViewModel> DeleteAsync(Guid id);
    }
}