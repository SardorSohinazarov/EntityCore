using EntityCore.Tools.Common.ServiceAttribute;

namespace EntityCore.Test.Common.ServiceAttribute
{
    public class ServiceAttributeTest
    {
        [Fact]
        public void Generate_Should_Return_True_Code()
        {
            // Arrange
            var expectedCode = @$"using Microsoft.Extensions.DependencyInjection;

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
            var service = new ServiceAttributes();

            // Act
            var code = service.Generate();

            // Assert
            Assert.Equal(expectedCode, code, ignoreLineEndingDifferences: true);
        }
    }
}
