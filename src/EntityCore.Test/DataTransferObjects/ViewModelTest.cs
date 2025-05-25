using EntityCore.Test.Entities;
using EntityCore.Tools.DataTransferObjects;

namespace EntityCore.Test.DataTransferObjects
{
    public class ViewModelTest : DtoGeneratorTest
    {
        [Fact]
        public void Generate_Simple_Entity_ViewModel_Correctly()
        {
            // Arrange
            var viewModel = new ViewModel(typeof(SimpleEntity));
            // Act
            string generatedCode = viewModel.Generate();
            // Assert
            AssertPropertyExists(generatedCode, "public Guid Id { get; set; }");
            AssertPropertyExists(generatedCode, "public string Name { get; set; }");
            AssertPropertyExists(generatedCode, "public int Value { get; set; }");
            AssertPropertyExists(generatedCode, "public bool IsActive { get; set; }");
        }

        [Fact]
        public void Generate_Complex_Entity_ViewModel_With_Required_Properties()
        {
            // Arrange
            var viewModel = new ViewModel(typeof(ComplexEntity));

            // Act
            string generatedCode = viewModel.Generate();

            // Assert
            AssertPropertyExists(generatedCode, "public long Id { get; set; }");
            AssertPropertyExists(generatedCode, "public string MainProperty { get; set; }");
            AssertPropertyExists(generatedCode, "public long? OptionalRelatedId { get; set; }");
            AssertPropertyExists(generatedCode, "public RelatedEntity OptionalRelated { get; set; }");
            AssertPropertyExists(generatedCode, "public long RequiredRelatedId { get; set; }");
            AssertPropertyExists(generatedCode, "public RelatedEntity RequiredRelated { get; set; }");
            AssertPropertyDoesNotExist(generatedCode, "List<long> RelatedCollectionIds");
            AssertPropertyExists(generatedCode, "public List<RelatedEntity> RelatedCollection { get; set; }");
            AssertPropertyDoesNotExist(generatedCode, "public List<Guid> SimpleItemsIds");
            AssertPropertyExists(generatedCode, "public List<SimpleEntity> SimpleItems { get; set; }");
            AssertPropertyExists(generatedCode, "public bool IsDeleted { get; set; }");
            AssertPropertyExists(generatedCode, "public DateTime? DeletedAt { get; set; }");
            AssertPropertyExists(generatedCode, "public DateTime CreatedAt { get; set; }");
            AssertPropertyExists(generatedCode, "public string CreatedBy { get; set; }");
            AssertPropertyExists(generatedCode, "public DateTime? UpdatedAt { get; set; }");
            AssertPropertyExists(generatedCode, "public string UpdatedBy { get; set; }");
        }
    }
}
