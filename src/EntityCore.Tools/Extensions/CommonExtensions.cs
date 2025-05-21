using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace EntityCore.Tools.Extensions
{
    public static class CommonExtensions
    {
        private static readonly Dictionary<Type, string> TypeMap = new()
        {
            { typeof(sbyte), "sbyte" },
            { typeof(byte), "byte" },
            { typeof(short), "short" },
            { typeof(ushort), "ushort" },
            { typeof(int), "int" },
            { typeof(uint), "uint" },
            { typeof(long), "long" },
            { typeof(ulong), "ulong" },
            { typeof(float), "float" },
            { typeof(double), "double" },
            { typeof(decimal), "decimal" },
            { typeof(bool), "bool" },
            { typeof(char), "char" },
            { typeof(string), "string" },
            { typeof(Guid), "Guid" },
            { typeof(DateTime), "DateTime" },
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
            => !(type.IsPrimitive
                || type == typeof(string)
                || type == typeof(decimal)
                || type == typeof(Guid)
                || type == typeof(DateTime));

        public static PropertyInfo FindPrimaryKeyProperty(this Type entityType)
        {
            // 1. [Key] atributi bilan belgilangan propertyni topish
            var keyProperty = entityType
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(prop => prop.GetCustomAttribute<KeyAttribute>() != null);

            if (keyProperty is not null)
                return keyProperty;

            // 2. Agar [Key] topilmasa, "Id" nomli propertyni qidirish
            keyProperty = entityType
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(prop => string.Equals(prop.Name, "Id"));

            if (keyProperty is null)
                throw new InvalidOperationException($"Entity must have a key property.{entityType.Name}");

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
