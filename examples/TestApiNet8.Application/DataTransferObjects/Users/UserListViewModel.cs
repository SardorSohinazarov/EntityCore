using Common.Paginations.Models;
using DataTransferObjects.Users;

namespace TestApiNet8.Application.DataTransferObjects.Users
{
    public class UserListViewModel
    {
        public PaginationMetadata Pagination { get; set; }
        public List<UserViewModel> Users { get; set; }
    }
}
