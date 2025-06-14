using System.Text;
using EntityCore.Tools.Common;

namespace EntityCore.Tools.DataTransferObjects
{
    public class ModificationDto : DtoGenerator
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
                .Select(x => GenerateProperty(x))
                .Where(x => x != null)
                .Distinct()
                .ToList();

            var result = new StringBuilder();

            foreach (var @namespace in _namespaces)
            {
                result.AppendLine($"using {@namespace};");
            }

            if (_namespaces.Count > 0)
            {
                result.AppendLine(); // Blank line after usings
            }

            result.AppendLine($"namespace DataTransferObjects.{_entityType.Name}s;");
            result.AppendLine();
            result.AppendLine($"public class {_name}");
            result.AppendLine("{");

            foreach (var property in properties)
            {
                result.AppendLine($"\t{property}");
            }

            result.AppendLine("}");
            return Common.HeaderGenerator.PrependHeader(result.ToString(), false);
        }
    }
}
