namespace EntityCore.Tools
{
    public static class Extentions
    {
        private static readonly Dictionary<Type, string> TypeMap = new()
        {
            { typeof(byte), "byte" },
            { typeof(short), "short" },
            { typeof(int), "int" },
            { typeof(long), "long" },
            { typeof(float), "float" },
            { typeof(double), "double" },
            { typeof(decimal), "decimal" },
            { typeof(bool), "bool" },
            { typeof(char), "char" },
            { typeof(string), "string" }
        };

        public static string ToCSharpTypeName(this Type type)
        {
            return TypeMap.TryGetValue(type, out var alias) ? alias : type.Name;
        }

        public static string GenerateFieldName(this string str) 
            => $"{char.ToLower(str[0])}{str.Substring(1)}";

        public static string GenerateFieldNameWithUnderscore(this string str)
            => $"_{str.GenerateFieldName()}";
    }
}
