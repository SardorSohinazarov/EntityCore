using EntityCore.Test.Entities;
using EntityCore.Tools.DataTransferObjects;
using System.Collections.Generic;

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

        [Fact]
        public void Generate_Complex_Entity_ModificationDto_With_Required_Properties()
        {
            // Arrange
            var creationDto = new CreationDto(typeof(ComplexEntity));

            // Act
            string generatedCode = creationDto.Generate();

            // Assert

            // Namespace and Usings
            AssertNamespaceEqualTo(generatedCode, "DataTransferObjects.ComplexEntitys");
            AssertUsingDirectiveExists(generatedCode, "System");
            AssertUsingDirectiveExists(generatedCode, "System.Collections.Generic");
            AssertUsingDirectiveExists(generatedCode, "EntityCore.Test.Entities");

            // Properties
            AssertPropertyDoesNotExist(generatedCode, "long Id");

            AssertPropertyExists(generatedCode, "public string MainProperty { get; set; }");

            AssertPropertyExists(generatedCode, "public long? OptionalRelatedId { get; set; }");
            AssertPropertyDoesNotExist(generatedCode, "RelatedEntity? OptionalRelated");

            AssertPropertyExists(generatedCode, "public long RequiredRelatedId { get; set; }");
            AssertPropertyDoesNotExist(generatedCode, "RelatedEntity RequiredRelated");

            AssertPropertyExists(generatedCode, "public ICollection<long> RelatedCollectionIds { get; set; }");
            AssertPropertyDoesNotExist(generatedCode, "ICollection<RelatedEntity> RelatedCollection");
            AssertPropertyExists(generatedCode, "public List<Guid> SimpleItemsIds { get; set; }");
            AssertPropertyDoesNotExist(generatedCode, "List<SimpleEntity> SimpleItems");

            AssertPropertyExists(generatedCode, "public bool IsDeleted { get; set; }");
            AssertPropertyExists(generatedCode, "public DateTime? DeletedAt { get; set; }");
            AssertPropertyExists(generatedCode, "public DateTime CreatedAt { get; set; }");
            AssertPropertyExists(generatedCode, "public string CreatedBy { get; set; }");
            AssertPropertyExists(generatedCode, "public DateTime? UpdatedAt { get; set; }");
            AssertPropertyExists(generatedCode, "public string UpdatedBy { get; set; }");
        }
    }
}
