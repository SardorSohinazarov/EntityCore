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
            return $@"";
        }
    }
}
