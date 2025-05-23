using EntityCore.Tools.Extensions;
using System.Collections;
using System.Reflection;
using System.Text;

namespace EntityCore.Tools.DataTransferObjects
{
    public class ModificationDto
    {
        private readonly Type _entityType;
        private readonly string _name;
        public ModificationDto(Type entityType)
        {
            _entityType = entityType;
            _name = _entityType.Name + "ModificationDto";
        }

        public string Generate()
        {
            var properties = _entityType.GetProperties()
                .Select(x => DtoPropertyGenerator.GenerateProperty(x))
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
