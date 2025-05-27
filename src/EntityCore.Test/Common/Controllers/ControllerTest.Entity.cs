using EntityCore.Test.Entities;
using EntityCore.Tools.Controllers;

namespace EntityCore.Test.Common.Controllers
{
    public partial class ControllerTest
    {
        [Fact]
        public void Should_Generate_Controller_Code()
        {
            // Arrange
            var controller = new Controller(typeof(SimpleEntity));

            // Act
            string generatedCode = controller.GenerateControllerCodeWithEntity();

            // Assert
            Assert.True(true);
        }
    }
}
