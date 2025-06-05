using System;
using TestApiNet8.Domain.Entities;

namespace DataTransferObjects.Tests;

public class TestViewModel
{
    public string Name { get; set; }
    public User Owner { get; set; }
    public int OwnerId { get; set; }
    public Guid Id { get; set; }
}
