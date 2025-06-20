using System.Text;

namespace EntityCore.Tools.DataTransferObjects
{
    public class CreationDto : DtoGenerator
    {
        private readonly Type _entityType;
        private readonly string _name;
        public CreationDto(Type entityType)
        {
            _entityType = entityType;
            _name = _entityType.Name + "CreationDto";
        }

        public string Generate()
        {
            var properties = _entityType.GetProperties()
                .Select(x => GenerateProperty(x))
                .Where(x => x != null)
                .Distinct()
                .ToList();

            var result = new StringBuilder();

            foreach (var @using in _usings)
                result.AppendLine($"using {@using};");

            if (_usings.Count > 0)
                result.AppendLine(); // Blank line after usings

            result.AppendLine($"namespace DataTransferObjects.{_entityType.Name}s;");
            result.AppendLine();
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
