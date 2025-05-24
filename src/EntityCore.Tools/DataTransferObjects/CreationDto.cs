using System.Text;

namespace EntityCore.Tools.DataTransferObjects
{
    public class CreationDto : DtoGenerator
    {
        private readonly Type _entityType;
        private readonly string _name;
        private readonly string _baseNamespace;

        public CreationDto(Type entityType, string baseNamespace)
        {
            _entityType = entityType;
            _name = _entityType.Name + "CreationDto";
            _baseNamespace = baseNamespace;
        }

        public string Generate()
        {
            var properties = _entityType.GetProperties()
                .Select(x => GenerateProperty(x))
                .Where(x => x != null)
                .Distinct()
                .ToList();

            var result = new StringBuilder($"namespace {_baseNamespace}.{_entityType.Name}s;\n\n");
            result.AppendLine($"public class {_name}");
            result.AppendLine("{");

            foreach (var property in properties)
            {
                result.AppendLine($"\t{property}");
            }

            result.AppendLine("}");
            return result.ToString();
        }
    }
}
