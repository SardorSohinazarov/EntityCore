using System;
using System.Reflection;
using System.ComponentModel.DataAnnotations;
using Xunit;
using EntityCore.Tools; // For EntityCore.Tools.Generator
using EntityCore.Test.Common.MockDtosAndVms; // For Mock DTOs/VMs

namespace EntityCore.Test.Common
{
    // Mock Entity Classes (from previous step)
    public class EntityWithKeyAttr { [Key] public int CustomKey { get; set; } }
    public class EntityWithIdProp { public int Id { get; set; } }
    public class EntityWithGuidIdProp { public Guid Id { get; set; } }
    public class EntityWithLowercaseIdProp { public int id { get; set; } }
    public class EntityWithNoKey { public string Name { get; set; } }
    public class EntityWithKeyAndId { [Key] public string ExplicitKey { get; set; } public int Id { get; set; } }

    // Helper ConcreteGenerator Class
    public class ConcreteGenerator : Generator
    {
        public PropertyInfo InvokeFindKeyProperty(Type entityType)
        {
            return base.FindKeyProperty(entityType);
        }

        // Wrappers for DTO/ViewModel resolver methods
        public Type InvokeGetViewModel(string entityName) => base.GetViewModel(entityName);
        public string InvokeGetReturnTypeName(string entityName) => base.GetReturnTypeName(entityName);
        public Type InvokeGetCreationDto(string entityName) => base.GetCreationDto(entityName);
        public string InvokeGetCreationDtoTypeName(string entityName) => base.GetCreationDtoTypeName(entityName);
        public Type InvokeGetModificationDto(string entityName) => base.GetModificationDto(entityName);
        public string InvokeGetModificationDtoTypeName(string entityName) => base.GetModificationDtoTypeName(entityName);
    }

    public class FindKeyPropertyTests
    {
        [Fact]
        public void FindKeyProperty_Should_Return_Property_With_KeyAttribute()
        {
            // Arrange
            var generator = new ConcreteGenerator();
            // Act
            var keyProp = generator.InvokeFindKeyProperty(typeof(EntityWithKeyAttr));
            // Assert
            Assert.NotNull(keyProp);
            Assert.Equal("CustomKey", keyProp.Name);
        }

        [Fact]
        public void FindKeyProperty_Should_Return_Id_Property_When_No_KeyAttribute()
        {
            // Arrange
            var generator = new ConcreteGenerator();
            // Act
            var keyProp = generator.InvokeFindKeyProperty(typeof(EntityWithIdProp));
            // Assert
            Assert.NotNull(keyProp);
            Assert.Equal("Id", keyProp.Name);
        }

        [Fact]
        public void FindKeyProperty_Should_Return_Guid_Id_Property()
        {
            // Arrange
            var generator = new ConcreteGenerator();
            // Act
            var keyProp = generator.InvokeFindKeyProperty(typeof(EntityWithGuidIdProp));
            // Assert
            Assert.NotNull(keyProp);
            Assert.Equal("Id", keyProp.Name);
            Assert.Equal(typeof(Guid), keyProp.PropertyType);
        }

        [Fact]
        public void FindKeyProperty_Should_Prefer_KeyAttribute_Over_Id_Property()
        {
            // Arrange
            var generator = new ConcreteGenerator();
            // Act
            var keyProp = generator.InvokeFindKeyProperty(typeof(EntityWithKeyAndId));
            // Assert
            Assert.NotNull(keyProp);
            Assert.Equal("ExplicitKey", keyProp.Name);
        }

        [Fact]
        public void FindKeyProperty_Should_Fail_For_Lowercase_id_When_No_KeyAttribute()
        {
            // Arrange
            var generator = new ConcreteGenerator();
            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => generator.InvokeFindKeyProperty(typeof(EntityWithLowercaseIdProp)));
            Assert.Equal("Entity must have a key property.", exception.Message);
        }

        [Fact]
        public void FindKeyProperty_Should_Throw_InvalidOperationException_When_No_Key_Found()
        {
            // Arrange
            var generator = new ConcreteGenerator();
            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => generator.InvokeFindKeyProperty(typeof(EntityWithNoKey)));
            Assert.Equal("Entity must have a key property.", exception.Message);
        }
    }

    public class DtoViewModelResolverTests
    {
        private readonly ConcreteGenerator _generator;

        public DtoViewModelResolverTests()
        {
            _generator = new ConcreteGenerator();
            // Ensure the assembly containing mock DTOs/VMs is loaded
            _ = typeof(ProductViewModel).Assembly; // ProductViewModel is in EntityCore.Test.Common.MockDtosAndVms
        }

        // Tests for "Product" (all DTOs/VM exist)
        [Fact]
        public void GetViewModel_Product_ReturnsCorrectType()
        {
            var type = _generator.InvokeGetViewModel("Product");
            Assert.Equal(typeof(ProductViewModel), type);
        }

        [Fact]
        public void GetReturnTypeName_Product_ReturnsViewModelName()
        {
            var name = _generator.InvokeGetReturnTypeName("Product");
            Assert.Equal("ProductViewModel", name);
        }

        [Fact]
        public void GetCreationDto_Product_ReturnsCorrectType()
        {
            var type = _generator.InvokeGetCreationDto("Product");
            Assert.Equal(typeof(ProductCreationDto), type);
        }

        [Fact]
        public void GetCreationDtoTypeName_Product_ReturnsDtoName()
        {
            var name = _generator.InvokeGetCreationDtoTypeName("Product");
            Assert.Equal("ProductCreationDto", name);
        }

        [Fact]
        public void GetModificationDto_Product_ReturnsCorrectType()
        {
            var type = _generator.InvokeGetModificationDto("Product");
            Assert.Equal(typeof(ProductModificationDto), type);
        }

        [Fact]
        public void GetModificationDtoTypeName_Product_ReturnsDtoName()
        {
            var name = _generator.InvokeGetModificationDtoTypeName("Product");
            Assert.Equal("ProductModificationDto", name);
        }

        // Tests for "Customer" (NO DTOs/VM exist)
        [Fact]
        public void GetViewModel_Customer_ReturnsNull()
        {
            var type = _generator.InvokeGetViewModel("Customer");
            Assert.Null(type);
        }

        [Fact]
        public void GetReturnTypeName_Customer_ReturnsEntityName()
        {
            var name = _generator.InvokeGetReturnTypeName("Customer");
            Assert.Equal("Customer", name); // Fallback behavior
        }

        [Fact]
        public void GetCreationDto_Customer_ReturnsNull()
        {
            var type = _generator.InvokeGetCreationDto("Customer");
            Assert.Null(type);
        }

        [Fact]
        public void GetCreationDtoTypeName_Customer_ReturnsEntityName()
        {
            var name = _generator.InvokeGetCreationDtoTypeName("Customer");
            Assert.Equal("Customer", name); // Fallback behavior
        }

        [Fact]
        public void GetModificationDto_Customer_ReturnsNull()
        {
            var type = _generator.InvokeGetModificationDto("Customer");
            Assert.Null(type);
        }

        [Fact]
        public void GetModificationDtoTypeName_Customer_ReturnsEntityName()
        {
            var name = _generator.InvokeGetModificationDtoTypeName("Customer");
            Assert.Equal("Customer", name); // Fallback behavior
        }

        // Tests for "Order" (ViewModel exists, DTOs do NOT)
        [Fact]
        public void GetViewModel_Order_ReturnsCorrectType()
        {
            var type = _generator.InvokeGetViewModel("Order");
            Assert.Equal(typeof(OrderViewModel), type);
        }

        [Fact]
        public void GetReturnTypeName_Order_ReturnsViewModelName()
        {
            var name = _generator.InvokeGetReturnTypeName("Order");
            Assert.Equal("OrderViewModel", name);
        }

        [Fact]
        public void GetCreationDto_Order_ReturnsNull()
        {
            var type = _generator.InvokeGetCreationDto("Order");
            Assert.Null(type);
        }

        [Fact]
        public void GetCreationDtoTypeName_Order_ReturnsEntityName()
        {
            var name = _generator.InvokeGetCreationDtoTypeName("Order");
            Assert.Equal("Order", name); // Fallback behavior
        }

        [Fact]
        public void GetModificationDto_Order_ReturnsNull()
        {
            var type = _generator.InvokeGetModificationDto("Order");
            Assert.Null(type);
        }

        [Fact]
        public void GetModificationDtoTypeName_Order_ReturnsEntityName()
        {
            var name = _generator.InvokeGetModificationDtoTypeName("Order");
            Assert.Equal("Order", name); // Fallback behavior
        }
    }
}
