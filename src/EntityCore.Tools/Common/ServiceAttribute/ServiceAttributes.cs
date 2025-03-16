using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EntityCore.Tools.Common.ServiceAttribute
{
    public class ServiceAttributes
    {
        public string Generate()
        {
            return @$"using Microsoft.Extensions.DependencyInjection;

namespace Common.ServiceAttribute
{{
    public class ServiceAttribute : Attribute
    {{
        public ServiceLifetime Lifetime {{ get; set; }}
    }}

    [AttributeUsage(AttributeTargets.Class)]
    public class ScopedServiceAttribute : ServiceAttribute
    {{
        public ScopedServiceAttribute()
            => Lifetime = ServiceLifetime.Scoped;
    }}

    [AttributeUsage(AttributeTargets.Class)]
    public class TransientServiceAttribute : ServiceAttribute
    {{
        public TransientServiceAttribute()
            => Lifetime = ServiceLifetime.Transient;
    }}

    [AttributeUsage(AttributeTargets.Class)]
    public class SingletonServiceAttribute : ServiceAttribute
    {{
        public SingletonServiceAttribute()
            => Lifetime = ServiceLifetime.Singleton;
    }}
}}
";
        }
    }
}