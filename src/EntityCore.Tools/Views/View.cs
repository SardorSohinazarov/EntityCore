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

        public string Generate()
        {
            var viewName = $"{_entityName}View";
            var namespaceName = $"Views.{_entityName}s";
            var properties = _entityType.GetProperties()
                .Select(x => GenerateProperty(x))
                .Where(x => x != null)
                .Distinct()
                .ToList();
            var code = $@"
        }
    }
}
