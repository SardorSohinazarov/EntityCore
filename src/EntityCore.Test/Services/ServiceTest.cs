using EntityCore.Test.Entities;
using EntityCore.Tools.Extensions;
using EntityCore.Tools.Services;

namespace EntityCore.Test.Services
{
    public class ServiceTest
    {
        [Theory]
        [InlineData(typeof(SimpleEntity), null, null, null)]
        [InlineData(typeof(ComplexEntity), typeof(ComplexEntityCreationDto), typeof(ComplexEntityModificationDto), typeof(ComplexEntityViewModel))]
        public void Should_Generate_Valid_Service_For_Entity(Type entityType, Type? creationDto, Type? modificationDto, Type? viewModel)
        {
            // Arrange
            var serviceGenerator = new Service(entityType);
            creationDto = creationDto ?? entityType;
            modificationDto = modificationDto ?? entityType;
            viewModel = viewModel ?? entityType;

            var primaryKeyTypeName = entityType.FindPrimaryKeyProperty().PropertyType.ToCSharpTypeName();
            var primaryKeyName = entityType.FindPrimaryKeyProperty().Name.GenerateFieldName();

            // Act
            var generatedCode = serviceGenerator.Generate();

            // Assert
            Assert.NotEmpty(generatedCode); // Ensure the code is not empty
            Assert.Contains("[ScopedService]", generatedCode); // Check for ScopedService attribute

            // Usinglar va namespacelarni, interface nomini tekshirish

            Assert.Contains($"Task<{viewModel.Name}> AddAsync({creationDto.Name} {creationDto.Name.GenerateFieldName()})", generatedCode);
            Assert.Contains($"Task<List<{viewModel.Name}>> GetAllAsync()", generatedCode);
            Assert.Contains($"Task<List<{viewModel.Name}>> FilterAsync(PaginationOptions filter)", generatedCode);
            Assert.Contains($"Task<{viewModel.Name}> GetByIdAsync({primaryKeyTypeName} {primaryKeyName})", generatedCode);
            Assert.Contains($"Task<{viewModel.Name}> UpdateAsync({primaryKeyTypeName} {primaryKeyName}, {modificationDto.Name} {modificationDto.Name.GenerateFieldName()})", generatedCode);
            Assert.Contains($"Task<{viewModel.Name}> DeleteAsync({primaryKeyTypeName} {primaryKeyName})", generatedCode);
        }
    }
}
