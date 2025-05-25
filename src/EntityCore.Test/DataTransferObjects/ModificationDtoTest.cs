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
            AssertPropertyDoesNotExist(generatedCode, "public Guid Id { get; set; }");
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

            // Properties
            AssertPropertyDoesNotExist(generatedCode, "public long Id { get; set; }");

            AssertPropertyExists(generatedCode, "public string MainProperty { get; set; }");

            AssertPropertyExists(generatedCode, "public long? OptionalRelatedId { get; set; }");
            AssertPropertyDoesNotExist(generatedCode, "public RelatedEntity? OptionalRelated { get; set; }");

            AssertPropertyExists(generatedCode, "public long RequiredRelatedId { get; set; }");
            AssertPropertyDoesNotExist(generatedCode, "public RelatedEntity RequiredRelated { get; set; }");

            AssertPropertyExists(generatedCode, "public ICollection<long> RelatedCollectionIds { get; set; }");
            AssertPropertyDoesNotExist(generatedCode, "public ICollection<RelatedEntity> RelatedCollection { get; set; }");
            AssertPropertyExists(generatedCode, "public List<Guid> SimpleItemsIds { get; set; }");
            AssertPropertyDoesNotExist(generatedCode, "public List<SimpleEntity> SimpleItems { get; set; }");

            AssertPropertyExists(generatedCode, "public bool IsDeleted { get; set; }");
            AssertPropertyExists(generatedCode, "public DateTime? DeletedAt { get; set; }");
            AssertPropertyExists(generatedCode, "public DateTime CreatedAt { get; set; }");
            AssertPropertyExists(generatedCode, "public string CreatedBy { get; set; }");
            AssertPropertyExists(generatedCode, "public DateTime? UpdatedAt { get; set; }");
            AssertPropertyExists(generatedCode, "public string UpdatedBy { get; set; }");
        }
    }
}
