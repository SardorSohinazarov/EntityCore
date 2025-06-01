using System;
using TestApiNet8.Domain.Entities;

namespace DataTransferObjects.Users;

public class UserViewModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
}
