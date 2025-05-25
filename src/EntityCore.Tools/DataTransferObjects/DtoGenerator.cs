using System.Collections;
using System.Reflection;
using EntityCore.Tools.Extensions;

namespace EntityCore.Tools.DataTransferObjects
{
    public class DtoGenerator
    {
        protected HashSet<string> _namespaces;
        public DtoGenerator()
        {
            _namespaces = new HashSet<string>();
        }

        protected string GenerateProperty(PropertyInfo property)
        {
            if (property.IsPrimaryKeyProperty())
            {
                return null; // Skip primary key
            }

            Type type = property.PropertyType;

            if (!type.IsNavigationProperty())
            {
                return $"public {type.ToCSharpTypeName()} {property.Name} {{ get; set; }}";
            }
            else
            {
                if (typeof(IEnumerable).IsAssignableFrom(type))
                {
                    Type elementType = GetCollectionElementType(type);

                    if (!elementType.IsNavigationProperty())
                    {
                        return $"public {type.ToCSharpTypeName()} {property.Name} {{ get; set; }}";
                    }
                    else
                    {
                        Type pkType = elementType.FindPrimaryKeyProperty().PropertyType;
                        string genericTypeName = GetGenericTypeName(type, elementType);
                        return $"public {genericTypeName}<{pkType.ToCSharpTypeName()}> {property.Name}Ids {{ get; set; }}";
                    }
                }
                else
                {
                    return $"public {type.FindPrimaryKeyProperty().PropertyType.ToCSharpTypeName()} {property.Name}Id {{ get; set; }}";
                }
            }
        }

        protected string GenerateViewProperty(PropertyInfo property)
        {
            Type type = property.PropertyType;

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
