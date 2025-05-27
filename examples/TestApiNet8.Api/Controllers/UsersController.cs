using Microsoft.AspNetCore.Mvc;
using Services.Users;
using Common.Paginations.Models;
using Common;
using DataTransferObjects.Users;

namespace Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUsersService _usersService;
        public UsersController(IUsersService usersService)
        {
            _usersService = usersService;
        }

        [HttpPost]
        public async Task<Result<UserViewModel>> AddAsync(UserCreationDto userCreationDto)
        {
            return Result<UserViewModel>.Success(await _usersService.AddAsync(userCreationDto));
        }

        [HttpGet]
        public async Task<Result<List<UserViewModel>>> GetAllAsync()
        {
            return Result<List<UserViewModel>>.Success(await _usersService.GetAllAsync());
        }

        [HttpPost("filter")]
        public async Task<Result<List<UserViewModel>>> FilterAsync(PaginationOptions filter)
        {
            return Result<List<UserViewModel>>.Success(await _usersService.FilterAsync(filter));
        }

        [HttpGet("{id}")]
        public async Task<Result<UserViewModel>> GetByIdAsync(int id)
        {
            return Result<UserViewModel>.Success(await _usersService.GetByIdAsync(id));
        }

        [HttpPut("{id}")]
        public async Task<Result<UserViewModel>> UpdateAsync(int id, UserModificationDto userModificationDto)
        {
            return Result<UserViewModel>.Success(await _usersService.UpdateAsync(id, userModificationDto));
        }

        [HttpDelete("{id}")]
        public async Task<Result<UserViewModel>> DeleteAsync(int id)
        {
            return Result<UserViewModel>.Success(await _usersService.DeleteAsync(id));
        }
    }
}