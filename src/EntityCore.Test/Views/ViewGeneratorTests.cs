using Microsoft.VisualStudio.TestTools.UnitTesting;
using EntityCore.Tools.Views;
using EntityCore.Test.Entities;
using EntityCore.Test.TestSupport; // For our test DTOs/ViewModels
using System.Collections.Generic;
using System.Linq;

namespace EntityCore.Test.Views
{
    [TestClass]
    public class ViewGeneratorTests
    {
        private const string DtosNamespace = "EntityCore.Test.TestSupport";
        private const string ViewModelsNamespace = "EntityCore.Test.TestSupport";
        private static Dictionary<string, string> _generatedPages = new Dictionary<string, string>();

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            // Ensure that the test DTOs/ViewModels are discoverable by referencing them.
            // This helps AppDomain.CurrentDomain.GetAssemblies() find them.
            var _ = typeof(SimpleEntityCreationDto);
            var __ = typeof(SimpleEntityModificationDto);
            var ___ = typeof(SimpleEntityViewModel);

            var entityType = typeof(SimpleEntity);
            var viewGenerator = new View(entityType);
            _generatedPages = viewGenerator.GenerateCrudPages(DtosNamespace, ViewModelsNamespace);
        }

        [TestMethod]
        public void GenerateCrudPages_ReturnsAllFourPages()
        {
            Assert.IsNotNull(_generatedPages, "Generated pages dictionary should not be null.");
            Assert.AreEqual(4, _generatedPages.Count, "Should generate exactly four pages.");
            Assert.IsTrue(_generatedPages.ContainsKey("SimpleEntityList.razor"), "List page missing.");
            Assert.IsTrue(_generatedPages.ContainsKey("SimpleEntityCreate.razor"), "Create page missing.");
            Assert.IsTrue(_generatedPages.ContainsKey("SimpleEntityEdit.razor"), "Edit page missing.");
            Assert.IsTrue(_generatedPages.ContainsKey("SimpleEntityDetails.razor"), "Details page missing.");
        }

        [TestMethod]
        public void SimpleEntity_ListPage_GeneratesCorrectly()
        {
            Assert.IsTrue(_generatedPages.TryGetValue("SimpleEntityList.razor", out var razorCode), "List page not found in generated pages.");
            Assert.IsNotNull(razorCode, "List page code should not be null.");

            // General Structure & Injections
            StringAssert.Contains(razorCode, "@page \"/simpleentitys\"", "List page: @page directive incorrect.");
            StringAssert.Contains(razorCode, $"@using {ViewModelsNamespace}", "List page: @using for ViewModel namespace incorrect.");
            StringAssert.Contains(razorCode, "@inject ISimpleEntitysService SimpleEntityService", "List page: Service injection incorrect.");
            StringAssert.Contains(razorCode, "<h3>SimpleEntity List</h3>", "List page: Title incorrect.");

            // Table Headers (SimpleEntity has Id, Name, Description, CreatedDate)
            StringAssert.Contains(razorCode, "<th>Name</th>", "List page: Name header missing.");
            StringAssert.Contains(razorCode, "<th>Description</th>", "List page: Description header missing.");
            StringAssert.Contains(razorCode, "<th>CreatedDate</th>", "List page: CreatedDate header missing.");
            StringAssert.Contains(razorCode, "<th>Id</th>", "List page: Id header missing (primary key).");
            
            // Loop and Item Display
            StringAssert.Contains(razorCode, "@foreach (var item in simpleentitys)", "List page: foreach loop missing.");
            StringAssert.Contains(razorCode, "<td>@item.Name</td>", "List page: item.Name display missing.");
            StringAssert.Contains(razorCode, "<td>@item.Description</td>", "List page: item.Description display missing.");
            StringAssert.Contains(razorCode, "<td>@item.CreatedDate</td>", "List page: item.CreatedDate display missing.");
            StringAssert.Contains(razorCode, "<td>@item.Id</td>", "List page: item.Id display missing.");

            // Action Buttons/Links
            StringAssert.Contains(razorCode, "href=\"/simpleentitys/create\"", "List page: Create New link missing.");
            StringAssert.Contains(razorCode, "href=\"@($\"/simpleentitys/details/{item.Id}\")\"", "List page: Details link missing.");
            StringAssert.Contains(razorCode, "href=\"@($\"/simpleentitys/edit/{item.Id}\")\"", "List page: Edit link missing.");
            StringAssert.Contains(razorCode, "@onclick=\"() => DeleteSimpleEntity(item.Id)\"", "List page: Delete button missing.");

            // Code Block
            StringAssert.Contains(razorCode, "private List<SimpleEntityViewModel>? simpleentitys;", "List page: List variable incorrect in @code.");
            StringAssert.Contains(razorCode, "simpleentitys = await SimpleEntityService.GetAllAsync();", "List page: GetAllAsync call missing.");
            StringAssert.Contains(razorCode, "async Task DeleteSimpleEntity(int id)", "List page: Delete method signature incorrect.");
        }

        [TestMethod]
        public void SimpleEntity_CreatePage_GeneratesCorrectly()
        {
            Assert.IsTrue(_generatedPages.TryGetValue("SimpleEntityCreate.razor", out var razorCode), "Create page not found.");
            Assert.IsNotNull(razorCode);

            StringAssert.Contains(razorCode, "@page \"/simpleentitys/create\"", "Create page: @page directive incorrect.");
            StringAssert.Contains(razorCode, $"@using {DtosNamespace}", "Create page: @using for DTO namespace incorrect.");
            StringAssert.Contains(razorCode, "@inject ISimpleEntitysService SimpleEntityService", "Create page: Service injection incorrect.");
            StringAssert.Contains(razorCode, "@inject NavigationManager NavigationManager", "Create page: NavigationManager injection incorrect.");
            StringAssert.Contains(razorCode, "<h3>Create New SimpleEntity</h3>", "Create page: Title incorrect.");

            // Form and Inputs (SimpleEntityCreationDto has Name, Description)
            StringAssert.Contains(razorCode, "<EditForm Model=\"@simpleEntityCreationDto\" OnValidSubmit=\"@HandleValidSubmit\">", "Create page: EditForm incorrect.");
            StringAssert.Contains(razorCode, "<label for=\"name\">Name:</label>", "Create page: Name label missing.");
            StringAssert.Contains(razorCode, "@bind-Value=\"simpleEntityCreationDto.Name\"", "Create page: Name input binding incorrect.");
            StringAssert.Contains(razorCode, "<label for=\"description\">Description:</label>", "Create page: Description label missing.");
            StringAssert.Contains(razorCode, "@bind-Value=\"simpleEntityCreationDto.Description\"", "Create page: Description input binding incorrect.");

            // Buttons
            StringAssert.Contains(razorCode, "<button type=\"submit\" class=\"btn btn-primary\">Save</button>", "Create page: Save button missing.");
            StringAssert.Contains(razorCode, "<button type=\"button\" class=\"btn btn-secondary\" @onclick=\"Cancel\">Cancel</button>", "Create page: Cancel button missing.");

            // Code Block
            StringAssert.Contains(razorCode, "private SimpleEntityCreationDto simpleEntityCreationDto = new SimpleEntityCreationDto();", "Create page: DTO instance incorrect.");
            StringAssert.Contains(razorCode, "await SimpleEntityService.AddAsync(simpleEntityCreationDto);", "Create page: AddAsync call missing.");
            StringAssert.Contains(razorCode, "NavigationManager.NavigateTo(\"/simpleentitys\");", "Create page: Navigation after add or cancel incorrect.");
        }

        [TestMethod]
        public void SimpleEntity_EditPage_GeneratesCorrectly()
        {
            Assert.IsTrue(_generatedPages.TryGetValue("SimpleEntityEdit.razor", out var razorCode), "Edit page not found.");
            Assert.IsNotNull(razorCode);

            StringAssert.Contains(razorCode, "@page \"/simpleentitys/edit/{Id:int}\"", "Edit page: @page directive incorrect.");
            StringAssert.Contains(razorCode, $"@using {DtosNamespace}", "Edit page: @using for DTO namespace incorrect.");
            StringAssert.Contains(razorCode, $"@using {ViewModelsNamespace}", "Edit page: @using for ViewModel namespace incorrect.");
            StringAssert.Contains(razorCode, "@inject ISimpleEntitysService SimpleEntityService", "Edit page: Service injection incorrect.");
            StringAssert.Contains(razorCode, "@inject AutoMapper.IMapper _mapper", "Edit page: IMapper injection incorrect.");
            StringAssert.Contains(razorCode, "<h3>Edit SimpleEntity</h3>", "Edit page: Title incorrect.");
            
            // Parameters and Data Loading
            StringAssert.Contains(razorCode, "[Parameter]", "Edit page: Id parameter attribute missing.");
            StringAssert.Contains(razorCode, "public int Id { get; set; }", "Edit page: Id parameter incorrect."); // SimpleEntity PK is int
            StringAssert.Contains(razorCode, "private SimpleEntityModificationDto? simpleEntityModificationDto;", "Edit page: DTO instance incorrect.");
            StringAssert.Contains(razorCode, "var entityViewModel = await SimpleEntityService.GetByIdAsync(Id);", "Edit page: GetByIdAsync call missing.");
            StringAssert.Contains(razorCode, "simpleEntityModificationDto = _mapper.Map<SimpleEntityModificationDto>(entityViewModel);", "Edit page: Mapper call incorrect.");

            // Form and Inputs (SimpleEntityModificationDto has Id, Name, Description)
            StringAssert.Contains(razorCode, "<EditForm Model=\"@simpleEntityModificationDto\" OnValidSubmit=\"@HandleValidSubmit\">", "Edit page: EditForm incorrect.");
            // Inputs for Name and Description (Id is usually not editable in the form directly)
            StringAssert.Contains(razorCode, "<label for=\"name\">Name:</label>", "Edit page: Name label missing.");
            StringAssert.Contains(razorCode, "@bind-Value=\"simpleEntityModificationDto.Name\"", "Edit page: Name input binding incorrect.");
            StringAssert.Contains(razorCode, "<label for=\"description\">Description:</label>", "Edit page: Description label missing.");
            StringAssert.Contains(razorCode, "@bind-Value=\"simpleEntityModificationDto.Description\"", "Edit page: Description input binding incorrect.");

            // Buttons
            StringAssert.Contains(razorCode, "<button type=\"submit\" class=\"btn btn-primary\">Save</button>", "Edit page: Save button missing.");
            StringAssert.Contains(razorCode, "<button type=\"button\" class=\"btn btn-secondary\" @onclick=\"Cancel\">Cancel</button>", "Edit page: Cancel button missing.");
            
            // Code Block Logic
            StringAssert.Contains(razorCode, "await SimpleEntityService.UpdateAsync(Id, simpleEntityModificationDto);", "Edit page: UpdateAsync call missing.");
            StringAssert.Contains(razorCode, "NavigationManager.NavigateTo(\"/simpleentitys\");", "Edit page: Navigation after update or cancel incorrect.");
        }

        [TestMethod]
        public void SimpleEntity_DetailsPage_GeneratesCorrectly()
        {
            Assert.IsTrue(_generatedPages.TryGetValue("SimpleEntityDetails.razor", out var razorCode), "Details page not found.");
            Assert.IsNotNull(razorCode);

            StringAssert.Contains(razorCode, "@page \"/simpleentitys/details/{Id:int}\"", "Details page: @page directive incorrect.");
            StringAssert.Contains(razorCode, $"@using {ViewModelsNamespace}", "Details page: @using for ViewModel namespace incorrect.");
            StringAssert.Contains(razorCode, "@inject ISimpleEntitysService SimpleEntityService", "Details page: Service injection incorrect.");
            StringAssert.Contains(razorCode, "<h3>SimpleEntity Details</h3>", "Details page: Title incorrect.");

            // Parameters and Data Loading
            StringAssert.Contains(razorCode, "[Parameter]", "Details page: Id parameter attribute missing.");
            StringAssert.Contains(razorCode, "public int Id { get; set; }", "Details page: Id parameter incorrect.");
            StringAssert.Contains(razorCode, "private SimpleEntityViewModel? simpleEntityDetails;", "Details page: ViewModel instance incorrect.");
            StringAssert.Contains(razorCode, "simpleEntityDetails = await SimpleEntityService.GetByIdAsync(Id);", "Details page: GetByIdAsync call missing.");

            // Displaying Properties (SimpleEntityViewModel has Id, Name, Description, CreatedDate)
            StringAssert.Contains(razorCode, "<dt class=\"col-sm-3\">Name</dt>", "Details page: Name display term missing.");
            StringAssert.Contains(razorCode, "<dd class=\"col-sm-9\">@simpleEntityDetails.Name</dd>", "Details page: Name display value missing.");
            StringAssert.Contains(razorCode, "<dt class=\"col-sm-3\">Description</dt>", "Details page: Description display term missing.");
            StringAssert.Contains(razorCode, "<dd class=\"col-sm-9\">@simpleEntityDetails.Description</dd>", "Details page: Description display value missing.");
            StringAssert.Contains(razorCode, "<dt class=\"col-sm-3\">CreatedDate</dt>", "Details page: CreatedDate display term missing.");
            StringAssert.Contains(razorCode, "<dd class=\"col-sm-9\">@simpleEntityDetails.CreatedDate</dd>", "Details page: CreatedDate display value missing.");
            StringAssert.Contains(razorCode, "<dt class=\"col-sm-3\">Id</dt>", "Details page: Id display term missing.");
            StringAssert.Contains(razorCode, "<dd class=\"col-sm-9\">@simpleEntityDetails.Id</dd>", "Details page: Id display value missing.");

            // Buttons
            StringAssert.Contains(razorCode, "@onclick=\"EditSimpleEntity\"", "Details page: Edit button missing.");
            StringAssert.Contains(razorCode, "@onclick=\"BackToList\"", "Details page: Back to List button missing.");

            // Code Block Logic
            StringAssert.Contains(razorCode, "NavigationManager.NavigateTo($\"/simpleentitys/edit/{Id}\");", "Details page: Navigation to Edit incorrect.");
            StringAssert.Contains(razorCode, "NavigationManager.NavigateTo(\"/simpleentitys\");", "Details page: Navigation to List incorrect.");
        }
    }
}
