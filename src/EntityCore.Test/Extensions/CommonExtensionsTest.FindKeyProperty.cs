using EntityCore.Tools.Extensions;
using System.ComponentModel.DataAnnotations;

namespace EntityCore.Test.Extensions
{
    public partial class CommonExtensionsTest
    {
        [Theory]
        [InlineData(typeof(EntityWithKeyAttr), "CustomKey", typeof(int))]
        [InlineData(typeof(EntityWithIdProp), "Id", typeof(int))]
        [InlineData(typeof(EntityWithGuidIdProp), "Id", typeof(Guid))]
        [InlineData(typeof(EntityWithKeyAndId), "ExplicitKey", typeof(string))]
        public void Should_Return_Primary_Key_Property(Type entityType, string propertyName, Type keyType)
        {
            // Arrange
            // Act
            var keyProp = entityType.FindPrimaryKeyProperty();
            // Assert
            Assert.NotNull(keyProp);
            Assert.Equal(propertyName, keyProp.Name);
            Assert.True(keyProp.PropertyType == keyType);
        }

        [Theory]
        [InlineData(typeof(EntityWithLowercaseIdProp), $"Entity {nameof(EntityWithLowercaseIdProp)} does not have a primary key defined. " +
                    "Please ensure it has a property with [Key] attribute or named 'Id'.")]
        [InlineData(typeof(EntityWithNoKey), $"Entity {nameof(EntityWithNoKey)} does not have a primary key defined. " +
                    "Please ensure it has a property with [Key] attribute or named 'Id'.")]
        public void Should_Throw_Exception_When_No_Primary_Key_Property(Type entityType, string expectedMessage)
        {
            // Arrange
            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => entityType.FindPrimaryKeyProperty());
            Assert.Equal(expectedMessage, exception.Message);
        }
    }

    // Test Entities
    public class EntityWithKeyAttr { [Key] public int CustomKey { get; set; } }
    public class EntityWithIdProp { public int Id { get; set; } }
    public class EntityWithGuidIdProp { public Guid Id { get; set; } }
    public class EntityWithLowercaseIdProp { public int id { get; set; } }
    public class EntityWithNoKey { public string Name { get; set; } }
    public class EntityWithKeyAndId { [Key] public string ExplicitKey { get; set; } public int Id { get; set; } }
}
