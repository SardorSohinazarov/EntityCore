using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace EntityCore.Tools
{
    public class Generator
    {
        protected PropertyInfo FindKeyProperty(Type entityType)
        {
            // 1. Find property marked with [Key] attribute
            var keyProperty = entityType
                .GetProperties()
                .FirstOrDefault(prop => prop.GetCustomAttribute<KeyAttribute>() != null);

            if (keyProperty is not null)
                return keyProperty;

            // 2. If [Key] is not found, search for a property named "Id"
            keyProperty = entityType
                .GetProperties()
                .FirstOrDefault(prop => string.Equals(prop.Name, "Id"));

            if (keyProperty is null)
                throw new InvalidOperationException("Entity must have a key property.");

            return keyProperty;
        }

        protected string GetReturnTypeName(Type entityType)
            => ResolveReturnTypeName(entityType.Name);

        protected string ResolveReturnTypeName(string entityName)
        {
            var viewModelType = FindExistingViewModelType(entityName);
            return viewModelType is null ? entityName : viewModelType.Name;
        }

        public static Type FindExistingViewModelType(string entityName, IEnumerable<Assembly> assembliesToSearch)
        {
            var viewModelName = $"{entityName}ViewModel";
            foreach (var assembly in assembliesToSearch)
            {
                var type = assembly.GetTypes().FirstOrDefault(t => t.Name == viewModelName && !t.IsNested);
                if (type != null) return type;
            }
            return null;
        }

        protected string GetCreationDtoTypeName(string entityName)
        {
            // This method might need access to the same assemblies as Manager if used by subclasses
            // For now, assuming it's called where AppDomain.CurrentDomain.GetAssemblies() is acceptable context
            var creationDtoType = FindExistingCreationDtoType(entityName, AppDomain.CurrentDomain.GetAssemblies());
            return creationDtoType is null ? entityName : creationDtoType.Name;
        }

        public static Type FindExistingCreationDtoType(string entityName, IEnumerable<Assembly> assembliesToSearch)
        {
            var creationDtoName = $"{entityName}CreationDto";
            foreach (var assembly in assembliesToSearch)
            {
                var type = assembly.GetTypes().FirstOrDefault(t => t.Name == creationDtoName && !t.IsNested);
                if (type != null) return type;
            }
            return null;
        }

        protected string GetModificationDtoTypeName(string entityName)
        {
            // Similar to GetCreationDtoTypeName
            var modificationDtoType = FindExistingModificationDtoType(entityName, AppDomain.CurrentDomain.GetAssemblies());
            return modificationDtoType is null ? entityName : modificationDtoType.Name;
        }

        public static Type FindExistingModificationDtoType(string entityName, IEnumerable<Assembly> assembliesToSearch)
        {
            var modificationDtoName = $"{entityName}ModificationDto";
            foreach (var assembly in assembliesToSearch)
            {
                var type = assembly.GetTypes().FirstOrDefault(t => t.Name == modificationDtoName && !t.IsNested);
                if (type != null) return type;
            }
            return null;
        }
    }
}
