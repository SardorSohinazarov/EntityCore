using Microsoft.EntityFrameworkCore;

namespace EntityCore.Tools.Views
{
    public class View : Generator
    {
        private readonly Type _entityType;
        public View(Type entityType)
        {
            _entityType = entityType;
        }

        public List<(string,string)> Generate(string dbContextName = null)
        {
            Type? dbContextType = null;

            if (dbContextName is not null)
            {
                dbContextType = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(assembly => assembly.GetTypes())
                    .FirstOrDefault(t => typeof(DbContext).IsAssignableFrom(t)
                        && t.IsClass
                        && !t.IsAbstract
                        && t.Name == dbContextName
                    );
            }
            else
            {
                var dbContextTypes = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(assembly => assembly.GetTypes())
                    .Where(t => typeof(DbContext).IsAssignableFrom(t)
                        && t != typeof(DbContext)
                        && t.IsClass
                        && !t.IsAbstract
                    );

                if (dbContextTypes.Count() == 1)
                    dbContextType = dbContextTypes.First();
                else if (dbContextTypes.Count() > 1)
                    throw new InvalidOperationException(
                        $"Multiple DbContexts({string.Join(", ", dbContextTypes.Select(x => x.Name))}) found in the specified assembly." +
                        $"\nPlease choose DbContext name. ex: --context <DbContextName>");
            }

            if (dbContextType is null)
                throw new InvalidOperationException("DbContext not found in the specified assembly.");

            var views = new List<(string, string)>();
            var filter = new Filter(_entityType);
            views.Add(("Filter", filter.Generate()));
            var create = new Create(_entityType);
            views.Add(("Create", create.Generate()));
            var details = new Details(_entityType);
            views.Add(("Details", details.Generate()));

            return views;
        }
    }
}
