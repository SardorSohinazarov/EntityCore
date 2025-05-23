using EntityCore.Tools.Extensions;
using System.Collections;
using System.Reflection;
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

            var result = new StringBuilder($"namespace DataTransferObjects.{_entityType.Name}s;\n\n");
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
