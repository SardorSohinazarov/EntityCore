using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace EntityCore.Tools.Extensions
{
    public static class CommonExtensions
    {
        private static readonly Dictionary<Type, string> TypeMap = new()
        {
            { typeof(sbyte), "sbyte" },
            { typeof(sbyte?), "sbyte?" },
            { typeof(byte), "byte" },
            { typeof(byte?), "byte?" },
            { typeof(short), "short" },
            { typeof(short?), "short?" },
            { typeof(ushort), "ushort" },
            { typeof(ushort?), "ushort?" },
            { typeof(int), "int" },
            { typeof(int?), "int?" },
            { typeof(uint), "uint" },
            { typeof(uint?), "uint?" },
            { typeof(long), "long" },
            { typeof(long?), "long?" },
            { typeof(ulong), "ulong" },
            { typeof(ulong?), "ulong?" },
            { typeof(float), "float" },
            { typeof(float?), "float?" },
            { typeof(double), "double" },
            { typeof(double?), "double?" },
            { typeof(decimal), "decimal" },
            { typeof(decimal?), "decimal?" },
            { typeof(bool), "bool" },
            { typeof(bool?), "bool?" },
            { typeof(char), "char" },
            { typeof(char?), "char?" },
            { typeof(string), "string" },
            { typeof(Guid), "Guid" },
            { typeof(Guid?), "Guid?" },
            { typeof(DateTime), "DateTime" },
            { typeof(DateTime?), "DateTime?" },
        };

        public static string ToCSharpTypeName(this Type type)
        {
            return TypeMap.TryGetValue(type, out var alias) ? alias : type.Name;
        }

        public static string GenerateFieldName(this string str)
            => $"{char.ToLower(str[0])}{str.Substring(1)}";

        public static string GenerateFieldNameWithUnderscore(this string str)
            => $"_{str.GenerateFieldName()}";

        public static bool IsNavigationProperty(this Type type)
        {
            foreach (var primitiveType in TypeMap.Keys)
            {
                if (type == primitiveType || type == Nullable.GetUnderlyingType(primitiveType))
                {
                    return false;
                }
            }

            return true;
        }

        public static PropertyInfo FindPrimaryKeyProperty(this Type entityType)
        {
            var properties = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            // 1. [Key] atributi bilan belgilangan propertyni topish
            var keyProperty = properties
                .FirstOrDefault(prop => prop.GetCustomAttribute<KeyAttribute>() != null);

            if (keyProperty is not null)
                return keyProperty;

            // 2. Agar [Key] topilmasa, "Id" nomli propertyni qidirish
            keyProperty = properties
                .FirstOrDefault(prop => string.Equals(prop.Name, "Id"));

            if(keyProperty is null)
                throw new InvalidOperationException($"Entity {entityType.Name} does not have a primary key defined. " +
                    "Please ensure it has a property with [Key] attribute or named 'Id'.");

            return keyProperty;
        }

        public static bool IsPrimaryKeyProperty(this PropertyInfo propertyInfo)
        {
            // 1. [Key] atributi bilan belgilanganmi
            var keyAttribute = propertyInfo.GetCustomAttribute<KeyAttribute>();
            if (keyAttribute is not null)
                return true;

            // 2. Agar [Key] topilmasa, "Id" nomli propertymi
            return string.Equals(propertyInfo.Name, "Id");
        }
    }
}
