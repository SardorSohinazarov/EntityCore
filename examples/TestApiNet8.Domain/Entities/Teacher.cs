﻿using TestApiWithNet8.Entities;

namespace TestApiNet8.Domain.Entities
{
    public class Teacher
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }

        public ICollection<Student> Students { get; set; }
    }
}
