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

        protected Type FindExistingViewModelType(string entityName)
        {
            var viewModelName = $"{entityName}ViewModel";
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .FirstOrDefault(t => t.Name == viewModelName);
        }

        protected string GetCreationDtoTypeName(string entityName)
        {
            var creationDtoType = FindExistingCreationDtoType(entityName);
            return creationDtoType is null ? entityName : creationDtoType.Name;
        }

        protected Type FindExistingCreationDtoType(string entityName)
        {
            var creationDtoName = $"{entityName}CreationDto";
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .FirstOrDefault(t => t.Name == creationDtoName);
        }

        protected string GetModificationDtoTypeName(string entityName)
        {
            var modificationDtoType = FindExistingModificationDtoType(entityName);
            return modificationDtoType is null ? entityName : modificationDtoType.Name;
        }

        protected Type FindExistingModificationDtoType(string entityName)
        {
            var modificationDtoName = $"{entityName}ModificationDto";
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .FirstOrDefault(t => t.Name == modificationDtoName);
        }
    }
}
