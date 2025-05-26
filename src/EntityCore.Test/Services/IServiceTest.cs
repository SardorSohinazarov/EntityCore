using EntityCore.Test.Entities;
using EntityCore.Tools.Extensions;
using EntityCore.Tools.Services;

namespace EntityCore.Test.Services
{
    public class IServiceTest
    {
        [Fact]
        public void Should_Generate_Interface_With_Simple_Entity()
        {
            // Arrange
            var serviceGenerator = new IService(typeof(SimpleEntity));

            // Act
            var generatedCode = serviceGenerator.Generate();

            // Assert
            Assert.Contains("Task<SimpleEntity> AddAsync(SimpleEntity simpleEntity);", generatedCode);
            Assert.Contains("Task<List<SimpleEntity>> GetAllAsync();", generatedCode);
            Assert.Contains("Task<List<SimpleEntity>> FilterAsync(PaginationOptions filter);", generatedCode);
            Assert.Contains("Task<SimpleEntity> GetByIdAsync(Guid id);", generatedCode);
            Assert.Contains("Task<SimpleEntity> UpdateAsync(Guid id, SimpleEntity simpleEntity);", generatedCode);
            Assert.Contains("Task<SimpleEntity> DeleteAsync(Guid id);", generatedCode);
        }

        [Fact]
        public void Should_Generate_Interface_With_Complex_Entity()
        {
            // Arrange
            var serviceGenerator = new IService(typeof(ComplexEntity));

            // Act
            var generatedCode = serviceGenerator.Generate();

            // Assert
            Assert.Contains("Task<ComplexEntityViewModel> AddAsync(ComplexEntityCreationDto complexEntityCreationDto);", generatedCode);
            Assert.Contains("Task<List<ComplexEntityViewModel>> GetAllAsync();", generatedCode);
            Assert.Contains("Task<List<ComplexEntityViewModel>> FilterAsync(PaginationOptions filter);", generatedCode);
            Assert.Contains("Task<ComplexEntityViewModel> GetByIdAsync(long id);", generatedCode);
            Assert.Contains("Task<ComplexEntityViewModel> UpdateAsync(long id, ComplexEntityModificationDto complexEntityModificationDto);", generatedCode);
            Assert.Contains("Task<ComplexEntityViewModel> DeleteAsync(long id);", generatedCode);
        }

        [Theory]
        [InlineData(typeof(SimpleEntity), null, null, null)]
        [InlineData(typeof(ComplexEntity), typeof(ComplexEntityCreationDto), typeof(ComplexEntityModificationDto), typeof(ComplexEntityViewModel))]
        public void Should_Generate_Valid_Service_For_Entity(Type entityType, Type? creationDto, Type? modificationDto, Type? viewModel)
        {
            // Arrange
            var serviceGenerator = new IService(entityType);
            creationDto = creationDto ?? entityType;
            modificationDto = modificationDto ?? entityType;
            viewModel = viewModel ?? entityType;

            var primaryKeyTypeName = entityType.FindPrimaryKeyProperty().PropertyType.ToCSharpTypeName();
            var primaryKeyName = entityType.FindPrimaryKeyProperty().Name.GenerateFieldName();

            // Act
            var generatedCode = serviceGenerator.Generate();

            // Assert
            Assert.NotEmpty(generatedCode); // Ensure the code is not empty

            Assert.Contains($"Task<{viewModel.Name}> AddAsync({creationDto.Name} {creationDto.Name.GenerateFieldName()})", generatedCode);
            Assert.Contains($"Task<List<{viewModel.Name}>> GetAllAsync()", generatedCode);
            Assert.Contains($"Task<List<{viewModel.Name}>> FilterAsync(PaginationOptions filter)", generatedCode);
            Assert.Contains($"Task<{viewModel.Name}> GetByIdAsync({primaryKeyTypeName} {primaryKeyName})", generatedCode);
            Assert.Contains($"Task<{viewModel.Name}> UpdateAsync({primaryKeyTypeName} {primaryKeyName}, {modificationDto.Name} {modificationDto.Name.GenerateFieldName()})", generatedCode);
            Assert.Contains($"Task<{viewModel.Name}> DeleteAsync({primaryKeyTypeName} {primaryKeyName})", generatedCode);
        }
    }
}
