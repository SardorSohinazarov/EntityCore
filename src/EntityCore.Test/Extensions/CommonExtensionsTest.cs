using EntityCore.Tools.Extensions;
using System.ComponentModel.DataAnnotations;

namespace EntityCore.Test.Extensions
{
    public partial class CommonExtensionsTest
    {
        [Fact]
        public void Should_Return_True_Or_False_Is_Navigational_Property()
        {
            // Arrange
            var primitiveTypes = new[]
            {
                typeof(int), typeof(Guid), typeof(DateTime), typeof(bool),
                typeof(int?), typeof(Guid?), typeof(DateTime?), typeof(bool?),
                typeof(string)
             };

            var nonPrimitiveTypes = new[]
            {
                typeof(EntityWithKeyAttr), typeof(EntityWithIdProp), typeof(EntityWithGuidIdProp),
            };

            // Act & Assert
            foreach (var type in primitiveTypes)
            {
                Assert.False(type.IsNavigationProperty(), $"{type.Name} should not be a navigation property.");
            }

            foreach (var type in nonPrimitiveTypes)
            {
                Assert.True(type.IsNavigationProperty(), $"{type.Name} should be a navigation property.");
            }
        }

        [Theory]
        [InlineData(typeof(int), "int")]
        [InlineData(typeof(int?), "int?")]
        [InlineData(typeof(string), "string")]
        [InlineData(typeof(Guid), "Guid")]
        [InlineData(typeof(Guid?), "Guid?")]
        [InlineData(typeof(DateTime), "DateTime")]
        [InlineData(typeof(DateTime?), "DateTime?")]
        public void Should_Return_True_CSharp_Type_Name(Type type, string expectedName)
        {
            // Arrange
            // Act
            var typeName = type.ToCSharpTypeName();
            // Assert
            Assert.Equal(expectedName, typeName);
        }
    }
}
