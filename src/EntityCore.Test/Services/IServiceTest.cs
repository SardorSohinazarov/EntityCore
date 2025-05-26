using EntityCore.Test.Entities;
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
            Assert.Contains("Task<SimpleEntity> AddAsync(SimpleEntity entity);", generatedCode);
            Assert.Contains("Task<List<SimpleEntity>> GetAllAsync();", generatedCode);
            Assert.Contains("Task<List<SimpleEntity>> FilterAsync(PaginationOptions filter);", generatedCode);
            Assert.Contains("Task<SimpleEntity> GetByIdAsync(Guid id);", generatedCode);
            Assert.Contains("Task<SimpleEntity> UpdateAsync(Guid id, SimpleEntity entity);", generatedCode);
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
            Assert.Contains("Task<ComplexEntityViewModel> AddAsync(ComplexEntityCreationDto entity);", generatedCode);
            Assert.Contains("Task<List<ComplexEntityViewModel>> GetAllAsync();", generatedCode);
            Assert.Contains("Task<List<ComplexEntityViewModel>> FilterAsync(PaginationOptions filter);", generatedCode);
            Assert.Contains("Task<ComplexEntityViewModel> GetByIdAsync(long id);", generatedCode);
            Assert.Contains("Task<ComplexEntityViewModel> UpdateAsync(long id, ComplexEntityModificationDto entity);", generatedCode);
            Assert.Contains("Task<ComplexEntityViewModel> DeleteAsync(long id);", generatedCode);
        }
    }
}
