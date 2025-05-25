using EntityCore.Tools.DbContexts;
using EntityCore.Tools.Extensions;
using System.Reflection;

namespace EntityCore.Tools.Views
{
    public class View
    {
        private readonly string _entityName;
        private readonly Type _entityType;
        private readonly PropertyInfo _primaryKey;
        public View(Type entityType)
        {
            _entityType = entityType;
            _entityName = _entityType.Name;
            _primaryKey = entityType.FindPrimaryKeyProperty();
        }

        public string Generate(string dbContextName = null)
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

            return "";
        }
    }
}
