using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Common.Paginations.Models;
using Common.Paginations.Extensions;
using Common.ServiceAttribute;
using Common;
using DataTransferObjects.Questions;
using TestApiWithNet8;
using TestApiNet8.Domain.Entities;

namespace Services.Questions
{
    [ScopedService]
    public class QuestionsService : IQuestionsService
    {
        private readonly TestApiNet8Db _testApiNet8Db;
        private readonly IMapper _mapper;
        public QuestionsService(TestApiNet8Db testApiNet8Db, IMapper mapper)
        {
            _testApiNet8Db = testApiNet8Db;
            _mapper = mapper;
        }

        public async Task<QuestionViewModel> AddAsync(QuestionCreationDto questionCreationDto)
        {
            var entity = _mapper.Map<Question>(questionCreationDto);
            var entry = await _testApiNet8Db.Set<Question>().AddAsync(entity);
            await _testApiNet8Db.SaveChangesAsync();
            return _mapper.Map<QuestionViewModel>(entry.Entity);
        }

        public async Task<List<QuestionViewModel>> GetAllAsync()
        {
            var entities = await _testApiNet8Db.Set<Question>().ToListAsync();
            return _mapper.Map<List<QuestionViewModel>>(entities);
        }

        public async Task<ListResult<QuestionViewModel>> FilterAsync(PaginationOptions filter)
        {
            var paginatedResult = await _testApiNet8Db.Set<Question>().ApplyPaginationAsync(filter);
            var Questions = _mapper.Map<List<QuestionViewModel>>(paginatedResult.paginatedList);
            return new ListResult<QuestionViewModel>(paginatedResult.paginationMetadata, Questions);
        }

        public async Task<QuestionViewModel> GetByIdAsync(Guid id)
        {
            var entity = await _testApiNet8Db.Set<Question>().FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null)
                throw new InvalidOperationException($"Question with Id {id} not found.");
            return _mapper.Map<QuestionViewModel>(entity);
        }

        public async Task<QuestionViewModel> UpdateAsync(Guid id, QuestionModificationDto questionModificationDto)
        {
            var entity = await _testApiNet8Db.Set<Question>().FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null)
                throw new InvalidOperationException($"Question with {id} not found.");
            _mapper.Map(questionModificationDto, entity);
            var entry = _testApiNet8Db.Set<Question>().Update(entity);
            await _testApiNet8Db.SaveChangesAsync();
            return _mapper.Map<QuestionViewModel>(entry.Entity);
        }

        public async Task<QuestionViewModel> DeleteAsync(Guid id)
        {
            var entity = await _testApiNet8Db.Set<Question>().FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null)
                throw new InvalidOperationException($"Question with {id} not found.");
            var entry = _testApiNet8Db.Set<Question>().Remove(entity);
            await _testApiNet8Db.SaveChangesAsync();
            return _mapper.Map<QuestionViewModel>(entry.Entity);
        }
    }

    /// <summary>
    /// AutoMapper mapping profile for Question entity.
    /// </summary>
    public class QuestionMappingProfile : Profile
    {
        public QuestionMappingProfile()
        {
            CreateMap<Question, QuestionViewModel>();
            CreateMap<QuestionCreationDto, Question>();
            CreateMap<QuestionModificationDto, Question>();
        }
    }
}