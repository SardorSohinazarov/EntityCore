using System.Text;
using EntityCore.Tools.Common;

namespace EntityCore.Tools.DataTransferObjects
{
    public class ViewModel : DtoGenerator
    {
        private readonly Type _entityType;
        private readonly string _name;

        public ViewModel(Type entityType)
        {
            _entityType = entityType;
            _name = _entityType.Name + "ViewModel";
        }

        public string Generate()
        {
            var properties = new List<string>();
            foreach (var propertyInfo in _entityType.GetProperties())
            {
                var propertyString = GenerateViewProperty(propertyInfo);
                if (propertyString != null)
                {
                    properties.Add(propertyString);
                }
            }

            if (!string.IsNullOrEmpty(_entityType.Namespace))
            {
                _namespaces.Add(_entityType.Namespace);
            }

            var result = new StringBuilder();

            foreach (var @namespace in _namespaces)
            {
                result.AppendLine($"using {@namespace};");
            }

            result.AppendLine(); // Blank line after usings

            result.AppendLine($"namespace DataTransferObjects.{_entityType.Name}s;");
            result.AppendLine();
            result.AppendLine($"public class {_name}");
            result.AppendLine("{");

            foreach (var property in properties)
            {
                result.AppendLine($"    {property}");
            }

            result.AppendLine("}");
            return Common.HeaderGenerator.PrependHeader(result.ToString(), false);
        }
    }
}
