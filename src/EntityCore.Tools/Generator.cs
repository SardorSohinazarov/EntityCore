namespace EntityCore.Tools
{
    public class Generator
    {
        protected string GetReturnTypeName(Type entityType)
            => GetReturnTypeName(entityType.Name);

        protected string GetReturnTypeName(string entityName)
        {
            var viewModelType = GetViewModelType(entityName);
            return viewModelType is null ? entityName : viewModelType.Name;
        }

        protected Type GetViewModelType(string entityName)
        {
            var viewModelName = $"{entityName}ViewModel";
            return GetType(viewModelName);
        }

        protected string GetCreationDtoTypeName(string entityName)
        {
            var creationDtoType = GetCreationDtoType(entityName);
            return creationDtoType is null ? entityName : creationDtoType.Name;
        }

        protected Type GetCreationDtoType(string entityName)
        {
            var creationDtoName = $"{entityName}CreationDto";
            return GetType(creationDtoName);
        }

        protected string GetModificationDtoTypeName(string entityName)
        {
            var modificationDtoType = GetModificationDtoType(entityName);
            return modificationDtoType is null ? entityName : modificationDtoType.Name;
        }

        protected Type GetModificationDtoType(string entityName)
        {
            var modificationDtoName = $"{entityName}ModificationDto";
            return GetType(modificationDtoName);
        }

        protected Type GetType(string modelName)
        {
            return AppDomain.CurrentDomain
                            .GetAssemblies()
                            .SelectMany(assembly => assembly.GetTypes())
                            .FirstOrDefault(t => t.Name == modelName);
        }
    }
}
