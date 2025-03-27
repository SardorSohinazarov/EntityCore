﻿using EntityCore.Tools.Extensions;
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

        private string GenerateProperty(PropertyInfo property)
        {
            if (property.IsPrimaryKeyProperty())
            {
                return null;
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
                    type = type.GetGenericArguments().First();

                    if (!type.IsNavigationProperty())
                    {
                        return $"public List<{type.ToCSharpTypeName()}> {property.Name} {{ get; set; }}";
                    }
                    else
                    {
                        return $"public List<{type.FindPrimaryKeyProperty().PropertyType.ToCSharpTypeName()}> {property.Name}Ids {{ get; set; }}";
                    }
                }
                else
                {
                    return $"public {type.FindPrimaryKeyProperty().PropertyType.ToCSharpTypeName()} {property.Name}Id {{ get; set; }}";
                }
            }
        }
    }
}
