using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Common.Paginations.Models;
using Common.Paginations.Extensions;
using Common.ServiceAttribute;
using DataTransferObjects.Users;
using TestApiWithNet8;
using TestApiNet8.Domain.Entities;

namespace Services.Users
{
    [ScopedService]
    public class UsersService : IUsersService
    {
        private readonly TestApiNet8Db _applicationDbContext;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContext;
        public UsersService(TestApiNet8Db applicationDbContext, IMapper mapper, IHttpContextAccessor httpContext)
        {
            _applicationDbContext = applicationDbContext;
            _mapper = mapper;
            _httpContext = httpContext;
        }

        public async Task<UserViewModel> AddAsync(UserCreationDto userCreationDto)
        {
            var entity = _mapper.Map<User>(userCreationDto);
            var entry = await _applicationDbContext.Set<User>().AddAsync(entity);
            await _applicationDbContext.SaveChangesAsync();
            return _mapper.Map<UserViewModel>(entry.Entity);
        }

        public async Task<List<UserViewModel>> GetAllAsync()
        {
            var entities = await _applicationDbContext.Set<User>().ToListAsync();
            return _mapper.Map<List<UserViewModel>>(entities);
        }

        public async Task<List<UserViewModel>> FilterAsync(PaginationOptions filter)
        {
            var httpContext = _httpContext.HttpContext;
            var entities = await _applicationDbContext.Set<User>().ApplyPagination(filter, httpContext).ToListAsync();
            return _mapper.Map<List<UserViewModel>>(entities);
        }

        public async Task<UserViewModel> GetByIdAsync(int id)
        {
            var entity = await _applicationDbContext.Set<User>().FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null)
                throw new InvalidOperationException($"User with Id {id} not found.");
            return _mapper.Map<UserViewModel>(entity);
        }

        public async Task<UserViewModel> UpdateAsync(int id, UserModificationDto userModificationDto)
        {
            var entity = await _applicationDbContext.Set<User>().FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null)
                throw new InvalidOperationException($"User with {id} not found.");
            _mapper.Map(userModificationDto, entity);
            var entry = _applicationDbContext.Set<User>().Update(entity);
            await _applicationDbContext.SaveChangesAsync();
            return _mapper.Map<UserViewModel>(entry.Entity);
        }

        public async Task<UserViewModel> DeleteAsync(int id)
        {
            var entity = await _applicationDbContext.Set<User>().FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null)
                throw new InvalidOperationException($"User with {id} not found.");
            var entry = _applicationDbContext.Set<User>().Remove(entity);
            await _applicationDbContext.SaveChangesAsync();
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