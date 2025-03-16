using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace EntityCore.Tools
{
    public class Generator
    {
        protected PropertyInfo FindKeyProperty(Type entityType)
        {
            // 1. [Key] atributi bilan belgilangan propertyni topish
            var keyProperty = entityType
                .GetProperties()
                .FirstOrDefault(prop => prop.GetCustomAttribute<KeyAttribute>() != null);

            if (keyProperty is not null)
                return keyProperty;

            // 2. Agar [Key] topilmasa, "Id" nomli propertyni qidirish
            keyProperty = entityType
                .GetProperties()
                .FirstOrDefault(prop => string.Equals(prop.Name, "Id", StringComparison.OrdinalIgnoreCase));

            if (keyProperty is null)
                throw new InvalidOperationException("Entity must have a key property.");

            return keyProperty;
        }

        protected string GetReturnTypeName(Type entityType)
            => GetReturnTypeName(entityType.Name);

        protected string GetReturnTypeName(string entityName)
        {
            var viewModelType = GetViewModel(entityName);
            return viewModelType is null ? entityName : viewModelType.Name;
        }

        protected Type GetViewModel(string entityName)
        {
            var viewModelName = $"{entityName}ViewModel";
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .FirstOrDefault(t => t.Name == viewModelName);
        }

        protected string GetCreationDtoTypeName(string entityName)
        {
            var creationDtoType = GetCreationDto(entityName);
            return creationDtoType is null ? entityName : creationDtoType.Name;
        }

        protected Type GetCreationDto(string entityName)
        {
            var creationDtoName = $"{entityName}CreationDto";
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .FirstOrDefault(t => t.Name == creationDtoName);
        }

        protected string GetModificationDtoTypeName(string entityName)
        {
            var modificationDtoType = GetModificationDto(entityName);
            return modificationDtoType is null ? entityName : modificationDtoType.Name;
        }

        protected Type GetModificationDto(string entityName)
        {
            var modificationDtoName = $"{entityName}ModificationDto";
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .FirstOrDefault(t => t.Name == modificationDtoName);
        }
    }
}
