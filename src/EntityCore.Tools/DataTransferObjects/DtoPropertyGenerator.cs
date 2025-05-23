using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EntityCore.Tools.Extensions;

namespace EntityCore.Tools.DataTransferObjects
{
    public class DtoPropertyGenerator
    {
        public static string GenerateProperty(PropertyInfo property)
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
                    type = type.GetGenericArguments().First();

                    if (!type.IsNavigationProperty())
                    {
                        // This case might need further clarification based on actual usage,
                        // but for now, let's assume it's a collection of non-navigational complex types.
                        // We'll represent them as a list of their C# type names.
                        return $"public List<{type.ToCSharpTypeName()}> {property.Name} {{ get; set; }}";
                    }
                    else
                    {
                        // Collection of navigation properties
                        return $"public List<{type.FindPrimaryKeyProperty().PropertyType.ToCSharpTypeName()}> {property.Name}Ids {{ get; set; }}";
                    }
                }
                else
                {
                    // Single navigation property
                    return $"public {type.FindPrimaryKeyProperty().PropertyType.ToCSharpTypeName()} {property.Name}Id {{ get; set; }}";
                }
            }
        }
    }
}
