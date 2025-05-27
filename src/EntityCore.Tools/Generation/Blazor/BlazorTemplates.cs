namespace EntityCore.Tools.Generation.Blazor
{
    public static class BlazorTemplates
    {
        public static readonly string ListPageTemplate = @"
@page ""/{{EntityNamePluralLOWER}}""
{{GeneratedDtoUsingStatements}}
@* @using {{ProjectNamespace}}.Models // Assuming ViewModels might be placed here or a shared location *@
@using {{ServiceNamespace}} // For I{EntityName}Service if not fully qualified in inject
@inject I{{EntityName}}Service {{EntityName}}Service
@inject NavigationManager NavigationManager
@inject IJSRuntime JSRuntime // New injection

<h3>{{EntityNamePlural}}</h3>

<p>
    <button class=""btn btn-primary"" @onclick=""GoToCreatePage"">Create New {{EntityName}}</button>
</p>

@if (items == null)
{
    <p><em>Loading...</em></p>
}
else
{
    <table class=""table table-striped"">
        <thead>
            <tr>
                {{ViewModelPropertiesPlaceHolder}}
                <th>Actions</th>
            </tr>
        </thead>
        <tbody>
            @foreach (var item in items)
            {
                <tr>
                    {{ViewModelDataPlaceHolder}}
                    <td>
                        <button class=""btn btn-sm btn-info"" @onclick=""() => ViewItem(item.{{EntityKeyPropertyName}})"">Details</button>
                        <button class=""btn btn-sm btn-warning"" @onclick=""() => EditItem(item.{{EntityKeyPropertyName}})"">Edit</button>
                        <button class=""btn btn-sm btn-danger"" @onclick=""() => ConfirmDelete(item.{{EntityKeyPropertyName}})"">Delete</button>
                    </td>
                </tr>
            }
        </tbody>
    </table>
}

@code {
    private IEnumerable<{{ViewModelName}}> items;
    // Removed TODO comments related to itemToDelete and showDeleteConfirmation as basic confirm is now added.
    // Advanced modal confirmation can be a future enhancement.

    protected override async Task OnInitializedAsync()
    {
        items = await {{EntityName}}Service.GetAllAsync(); // Assuming GetAllAsync exists and returns IEnumerable<ViewModelName>
    }

    void GoToCreatePage()
    {
        NavigationManager.NavigateTo(""{{EntityNamePluralLOWER}}/create"");
    }

    void EditItem(object id) // Changed type to object to handle various PK types
    {
        NavigationManager.NavigateTo($""{{EntityNamePluralLOWER}}/edit/{id}"");
    }

    void ViewItem(object id) // For a potential details page
    {
        NavigationManager.NavigateTo($""{{EntityNamePluralLOWER}}/{id}""); // Assuming details page route
    }

    async Task DeleteItem(object id)
    {
        // This method now focuses only on the deletion after confirmation.
        await {{EntityName}}Service.DeleteAsync(id); // Assuming DeleteAsync takes id
        items = await {{EntityName}}Service.GetAllAsync(); // Refresh list
        StateHasChanged(); // Ensure UI updates
    }
    
    async Task ConfirmDelete(object id)
    {
        bool confirmed = await JSRuntime.InvokeAsync<bool>(""confirm"", ""Are you sure you want to delete this {{EntityName}}?"");
        if (confirmed)
        {
            await DeleteItem(id);
        }
    }
}
";

        public static readonly string CreatePageTemplate = @"
@page ""/{{EntityNamePluralLOWER}}/create""
{{GeneratedDtoUsingStatements}}
@* @using {{ProjectNamespace}}.Models // Assuming DTOs might be here or a shared location *@
@* @using {{ProjectNamespace}}.Models.{{EntityName}}s // More specific if DTOs are in subfolders *@
@using {{ServiceNamespace}}
@inject I{{EntityName}}Service {{EntityName}}Service
@inject NavigationManager NavigationManager

<h3>Create New {{EntityName}}</h3>

<EditForm Model=""@model"" OnValidSubmit=""HandleValidSubmit"">
    <DataAnnotationsValidator />
    <ValidationSummary />

    {{CreationDtoPropertiesPlaceHolder}}

    <button type=""submit"" class=""btn btn-success"">Create {{EntityName}}</button>
    <button type=""button"" class=""btn btn-secondary"" @onclick=""GoBackToList"">Cancel</button>
</EditForm>

@code {
    private {{CreationDtoName}} model = new {{CreationDtoName}}();

    private async Task HandleValidSubmit()
    {
        await {{EntityName}}Service.CreateAsync(model); // Assuming CreateAsync exists
        GoBackToList();
    }

    private void GoBackToList()
    {
        NavigationManager.NavigateTo(""{{EntityNamePluralLOWER}}"");
    }
}
";

        public static readonly string EditPageTemplate = @"
@page ""/{{EntityNamePluralLOWER}}/edit/{id}""
{{GeneratedDtoUsingStatements}}
@* @using {{ProjectNamespace}}.Models // Assuming DTOs might be here or a shared location *@
@* @using {{ProjectNamespace}}.Models.{{EntityName}}s // More specific if DTOs are in subfolders *@
@using {{ServiceNamespace}}
@inject I{{EntityName}}Service {{EntityName}}Service
@inject NavigationManager NavigationManager
@inject AutoMapper.IMapper Mapper // Assuming AutoMapper is used for DTO mapping

<h3>Edit {{EntityName}}</h3>

@if (model == null)
{
    <p><em>Loading entity details...</em></p>
}
else
{
    <EditForm Model=""@model"" OnValidSubmit=""HandleValidSubmit"">
        <DataAnnotationsValidator />
        <ValidationSummary />

        {{ModificationDtoPropertiesPlaceHolder}}

        <button type=""submit"" class=""btn btn-success"">Save Changes</button>
        <button type=""button"" class=""btn btn-secondary"" @onclick=""GoBackToList"">Cancel</button>
    </EditForm>
}

@code {
    [Parameter]
    public string id { get; set; } // The route parameter is always string initially

    private {{ModificationDtoName}} model;
    private {{EntityKeyPropertyType}} typedId;

    protected override async Task OnParametersSetAsync()
    {
        // Convert string id from route to actual key type
        // This requires a helper method or direct parsing logic based on {{EntityKeyPropertyType}}
        typedId = {{ConvertKeyMethod}}(id); 
        
        // Assuming GetByIdAsync returns the ViewModel or the Entity itself
        var viewModel = await {{EntityName}}Service.GetByIdAsync(typedId);
        if (viewModel != null)
        {
            // Map from ViewModel/Entity to ModificationDto
            // This assumes IMapper is available and configured.
            // If not, manual mapping would be needed here.
            model = Mapper.Map<{{ModificationDtoName}}>(viewModel);
        }
        else
        {
            // Handle case where entity is not found, e.g., navigate to a 'not found' page or back to list
            GoBackToList();
        }
    }

    private async Task HandleValidSubmit()
    {
        if (model == null) return; // Should not happen if loaded correctly

        await {{EntityName}}Service.UpdateAsync(typedId, model); // Assuming UpdateAsync takes id and ModificationDto
        GoBackToList();
    }

    private void GoBackToList()
    {
        NavigationManager.NavigateTo(""{{EntityNamePluralLOWER}}"");
    }

    // Helper method to convert string ID to the correct type.
    // This will be generated more specifically later based on {{EntityKeyPropertyType}}.
    private {{EntityKeyPropertyType}} {{ConvertKeyMethod}}(string stringId)
    {
        // Placeholder - actual implementation will depend on {{EntityKeyPropertyType}}
        if (typeof({{EntityKeyPropertyType}}) == typeof(Guid))
        {
            return ({{EntityKeyPropertyType}})(object)Guid.Parse(stringId);
        }
        if (typeof({{EntityKeyPropertyType}}) == typeof(int))
        {
            return ({{EntityKeyPropertyType}})(object)int.Parse(stringId);
        }
        // Add other type conversions as needed (long, etc.)
        return default({{EntityKeyPropertyType}}); // Or throw exception
    }
}
";
    }
}
