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
    }
}
