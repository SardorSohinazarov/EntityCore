using Microsoft.EntityFrameworkCore;

namespace EntityCore.Test.Entities
{
    public class EntityDb : DbContext
    {
        public EntityDb(DbContextOptions<EntityDb> options)
            : base(options) 
            => Database.Migrate();
    }
}
