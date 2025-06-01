using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Common.Paginations.Models;
using Common.Paginations.Extensions;
using Common.ServiceAttribute;
using Common;
using DataTransferObjects.Users;
using TestApiWithNet8;
using TestApiNet8.Domain.Entities;

namespace Services.Users
{
    [ScopedService]
    public class UsersService : IUsersService
    {
        private readonly TestApiNet8Db _testApiNet8Db;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContext;
        public UsersService(TestApiNet8Db testApiNet8Db, IMapper mapper, IHttpContextAccessor httpContext)
        {
            _testApiNet8Db = testApiNet8Db;
            _mapper = mapper;
            _httpContext = httpContext;
        }

        public async Task<UserViewModel> AddAsync(UserCreationDto userCreationDto)
        {
            var entity = _mapper.Map<User>(userCreationDto);
            var entry = await _testApiNet8Db.Set<User>().AddAsync(entity);
            await _testApiNet8Db.SaveChangesAsync();
            return _mapper.Map<UserViewModel>(entry.Entity);
        }

        public async Task<List<UserViewModel>> GetAllAsync()
        {
            var entities = await _testApiNet8Db.Set<User>().ToListAsync();
            return _mapper.Map<List<UserViewModel>>(entities);
        }

        public async Task<ListResult<UserViewModel>> FilterAsync(PaginationOptions filter)
        {
            var httpContext = _httpContext.HttpContext;
            var paginatedResult = await _testApiNet8Db.Set<User>().ApplyPaginationAsync(filter);
            var Users = _mapper.Map<List<UserViewModel>>(paginatedResult.paginatedList);
            return new ListResult<UserViewModel>(paginatedResult.paginationMetadata, Users);
        }

        public async Task<UserViewModel> GetByIdAsync(int id)
        {
            var entity = await _testApiNet8Db.Set<User>().FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null)
                throw new InvalidOperationException($"User with Id {id} not found.");
            return _mapper.Map<UserViewModel>(entity);
        }

        public async Task<UserViewModel> UpdateAsync(int id, UserModificationDto userModificationDto)
        {
            var entity = await _testApiNet8Db.Set<User>().FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null)
                throw new InvalidOperationException($"User with {id} not found.");
            _mapper.Map(userModificationDto, entity);
            var entry = _testApiNet8Db.Set<User>().Update(entity);
            await _testApiNet8Db.SaveChangesAsync();
            return _mapper.Map<UserViewModel>(entry.Entity);
        }

        public async Task<UserViewModel> DeleteAsync(int id)
        {
            var entity = await _testApiNet8Db.Set<User>().FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null)
                throw new InvalidOperationException($"User with {id} not found.");
            var entry = _testApiNet8Db.Set<User>().Remove(entity);
            await _testApiNet8Db.SaveChangesAsync();
            return _mapper.Map<UserViewModel>(entry.Entity);
        }
    }

    /// <summary>
    /// AutoMapper mapping profile for User entity.
    /// </summary>
    public class UserMappingProfile : Profile
    {
        public UserMappingProfile()
        {
            CreateMap<User, UserViewModel>();
            CreateMap<UserCreationDto, User>();
            CreateMap<UserModificationDto, User>();
        }
    }
}