using EntityCore.Tools.Extensions;
using System.Collections;
using System.Reflection;
using System.Text;

namespace EntityCore.Tools.DataTransferObjects
{
    public class ViewModel
    {
        private readonly Type _entityType;
        private readonly string _name;
        private readonly HashSet<string> _namespaces;

        public ViewModel(Type entityType)
        {
            _entityType = entityType;
            _name = _entityType.Name + "ViewModel";
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
                return $"public {type.ToCSharpTypeName()} {property.Name} {{ get; set; }}";
            }
            else
            {
                if (typeof(IEnumerable).IsAssignableFrom(type))
                {
                    Type elementType = GetCollectionElementType(type);

                    _namespaces.Add(elementType.Namespace);

                    if (!elementType.IsNavigationProperty())
                    {
                        return $"public {type.ToCSharpTypeName()} {property.Name} {{ get; set; }}";
                    }
                    else
                    {
                        string genericTypeName = GetGenericTypeName(type, elementType);
                        return $"public {genericTypeName}<{elementType.ToCSharpTypeName()}> {property.Name} {{ get; set; }}";
                    }
                }
                else
                {
                    _namespaces.Add(type.Namespace);

                    return $"public {type.Name} {property.Name} {{ get; set; }}";
                }
            }
        }

        private string GetGenericTypeName(Type type, Type elementType)
        {
            var genericType = type.IsGenericType 
                ? type.GetGenericTypeDefinition()
                : typeof(IEnumerable<>);

            return genericType.Name.Split('`')[0];
        }

        private Type GetCollectionElementType(Type collectionType)
        {
            if (collectionType.IsArray)
                return collectionType.GetElementType();

            if (collectionType.IsGenericType)
                return collectionType.GetGenericArguments().First();

            // Fallback: check interfaces
            var enumerableInterface = collectionType.GetInterfaces()
                .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));

            if (enumerableInterface != null)
                return enumerableInterface.GetGenericArguments().First();

            return typeof(object); // unknown fallback
        }
    }
}
