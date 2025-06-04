using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Common.Paginations.Models;
using Common.Paginations.Extensions;
using Common.ServiceAttribute;
using Common;
using DataTransferObjects.Answers;
using TestApiWithNet8;
using TestApiNet8.Domain.Entities;

namespace Services.Answers
{
    [ScopedService]
    public class AnswersService : IAnswersService
    {
        private readonly TestApiNet8Db _testApiNet8Db;
        private readonly IMapper _mapper;
        public AnswersService(TestApiNet8Db testApiNet8Db, IMapper mapper)
        {
            _testApiNet8Db = testApiNet8Db;
            _mapper = mapper;
        }

        public async Task<AnswerViewModel> AddAsync(AnswerCreationDto answerCreationDto)
        {
            var entity = _mapper.Map<Answer>(answerCreationDto);
            var entry = await _testApiNet8Db.Set<Answer>().AddAsync(entity);
            await _testApiNet8Db.SaveChangesAsync();
            return _mapper.Map<AnswerViewModel>(entry.Entity);
        }

        public async Task<List<AnswerViewModel>> GetAllAsync()
        {
            var entities = await _testApiNet8Db.Set<Answer>().ToListAsync();
            return _mapper.Map<List<AnswerViewModel>>(entities);
        }

        public async Task<ListResult<AnswerViewModel>> FilterAsync(PaginationOptions filter)
        {
            var paginatedResult = await _testApiNet8Db.Set<Answer>().ApplyPaginationAsync(filter);
            var Answers = _mapper.Map<List<AnswerViewModel>>(paginatedResult.paginatedList);
            return new ListResult<AnswerViewModel>(paginatedResult.paginationMetadata, Answers);
        }

        public async Task<AnswerViewModel> GetByIdAsync(Guid id)
        {
            var entity = await _testApiNet8Db.Set<Answer>().FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null)
                throw new InvalidOperationException($"Answer with Id {id} not found.");
            return _mapper.Map<AnswerViewModel>(entity);
        }

        public async Task<AnswerViewModel> UpdateAsync(Guid id, AnswerModificationDto answerModificationDto)
        {
            var entity = await _testApiNet8Db.Set<Answer>().FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null)
                throw new InvalidOperationException($"Answer with {id} not found.");
            _mapper.Map(answerModificationDto, entity);
            var entry = _testApiNet8Db.Set<Answer>().Update(entity);
            await _testApiNet8Db.SaveChangesAsync();
            return _mapper.Map<AnswerViewModel>(entry.Entity);
        }

        public async Task<AnswerViewModel> DeleteAsync(Guid id)
        {
            var entity = await _testApiNet8Db.Set<Answer>().FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null)
                throw new InvalidOperationException($"Answer with {id} not found.");
            var entry = _testApiNet8Db.Set<Answer>().Remove(entity);
            await _testApiNet8Db.SaveChangesAsync();
            return _mapper.Map<AnswerViewModel>(entry.Entity);
        }
    }

    /// <summary>
    /// AutoMapper mapping profile for Answer entity.
    /// </summary>
    public class AnswerMappingProfile : Profile
    {
        public AnswerMappingProfile()
        {
            CreateMap<Answer, AnswerViewModel>();
            CreateMap<AnswerCreationDto, Answer>();
            CreateMap<AnswerModificationDto, Answer>();
        }
    }
}