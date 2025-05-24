using EntityCore.Test.Entities;
using EntityCore.Tools.DataTransferObjects;

namespace EntityCore.Test.DataTransferObjects
{
    public class ModificationDtoTest : DtoGeneratorTest
    {
        [Fact]
        public void Generate_Simple_Entity_ModificationDto_Correctly()
        {
            // Arrange
            var modificationDto = new ModificationDto(typeof(SimpleEntity));

            // Act
            string generatedCode = modificationDto.Generate();

            // Assert
            AssertPropertyDoesNotExist(generatedCode, "Guid Id");
            AssertPropertyExists(generatedCode, "public string Name { get; set; }");
            AssertPropertyExists(generatedCode, "public int Value { get; set; }");
            AssertPropertyExists(generatedCode, "public bool IsActive { get; set; }");
        }
    }
}
