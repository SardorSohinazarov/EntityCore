using EntityCore.Tools.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace EntityCore.Tools.DataTransferObjects
{
    public class ViewModel
    {
        private readonly Type _entityType;
        private readonly string _name;
        private readonly string _baseNamespace;
        private readonly HashSet<string> _namespaces;

        public ViewModel(Type entityType, string baseNamespace)
        {
            _entityType = entityType;
            _name = _entityType.Name + "ViewModel";
            _baseNamespace = baseNamespace;
            _namespaces = new HashSet<string> { "System", "System.Collections.Generic" }; // Default namespaces
        }

        public string Generate()
        {
            var properties = new List<string>();
            foreach (var propertyInfo in _entityType.GetProperties())
            {
                var propertyString = GenerateProperty(propertyInfo);
                if (propertyString != null)
                {
                    properties.Add(propertyString);
                }
            }

            // Add entity's own namespace
            if (!string.IsNullOrEmpty(_entityType.Namespace))
            {
                _namespaces.Add(_entityType.Namespace);
            }

            var result = new StringBuilder();

            foreach (var ns in _namespaces.OrderBy(n => n))
            {
                result.AppendLine($"using {ns};");
            }
            result.AppendLine(); // Blank line after usings

            result.AppendLine($"namespace {_baseNamespace}.{_entityType.Name}s;");
            result.AppendLine();
            result.AppendLine($"public class {_name}");
            result.AppendLine("{");

            foreach (var property in properties)
            {
                result.AppendLine($"    {property}"); // Indent properties
            }

            result.AppendLine("}");
            return result.ToString();
        }

        private string GenerateProperty(PropertyInfo property)
        {
            Type type = property.PropertyType;

            // Add type's namespace
            if (!string.IsNullOrEmpty(type.Namespace))
            {
                _namespaces.Add(type.Namespace);
            }

            if (!type.IsNavigationProperty())
            {
                // For non-navigation properties, use ToCSharpTypeName for accurate representation (e.g., int?, List<string>)
                // _namespaces.Add(type.Namespace); // This is already added at the beginning of the method.
                if (type.IsGenericType)
                {
                    foreach (var genArgType in type.GetGenericArguments())
                    {
                        if (!string.IsNullOrEmpty(genArgType.Namespace)) { _namespaces.Add(genArgType.Namespace); }
                    }
                }
                return $"public {type.ToCSharpTypeName()} {property.Name} {{ get; set; }}";
            }
            else
            {
                if (typeof(IEnumerable).IsAssignableFrom(type))
                {
                    // Collection navigation property
                    Type genericArgument = type.GetGenericArguments().FirstOrDefault();
                    if (genericArgument == null) // Non-generic IEnumerable, less common for navigation properties
                    {
                        _namespaces.Add(typeof(IEnumerable).Namespace);
                        return $"public IEnumerable {property.Name} {{ get; set; }}";
                    }

                    // Add namespace of the generic argument
                    if (!string.IsNullOrEmpty(genericArgument.Namespace))
                    {
                        _namespaces.Add(genericArgument.Namespace);
                    }
                    // Assuming we want List<RelatedEntityTypeViewModel> or List<RelatedEntityType>
                    // For now, sticking to List<RelatedEntityType> as per subtask description
                    return $"public List<{genericArgument.Name}> {property.Name} {{ get; set; }}";
                }
                else
                {
                    // Single navigation property
                    // Add namespace of the property type (the related entity)
                    if (!string.IsNullOrEmpty(type.Namespace))
                    {
                        _namespaces.Add(type.Namespace);
                    }
                    // Use type.Name, assuming the namespace will be imported via 'using'
                    return $"public {type.Name} {property.Name} {{ get; set; }}";
                }
            }
        }
    }
}
