using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace EntityCore.Tools
{
    public class Generator
    {
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
            return GetType(viewModelName);
        }

        protected string GetCreationDtoTypeName(string entityName)
        {
            var creationDtoType = GetCreationDto(entityName);
            return creationDtoType is null ? entityName : creationDtoType.Name;
        }

        protected Type GetCreationDto(string entityName)
        {
            var creationDtoName = $"{entityName}CreationDto";
            return GetType(creationDtoName);
        }

        protected string GetModificationDtoTypeName(string entityName)
        {
            var modificationDtoType = GetModificationDto(entityName);
            return modificationDtoType is null ? entityName : modificationDtoType.Name;
        }

        protected Type GetModificationDto(string entityName)
        {
            var modificationDtoName = $"{entityName}ModificationDto";
            return GetType(modificationDtoName);
        }

        private Type GetType(string modelName)
        {
            return AppDomain.CurrentDomain
                            .GetAssemblies()
                            .SelectMany(assembly => assembly.GetTypes())
                            .FirstOrDefault(t => t.Name == modelName);
        }
    }
}
