using EntityCore.Tools.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using System.Text;

namespace EntityCore.Tools.Views
{
    public class View : Generator
    {
        private readonly string _entityName;
        private readonly Type _entityType;
        private readonly PropertyInfo _primaryKey;
        public View(Type entityType)
        {
            _entityType = entityType;
            _entityName = _entityType.Name;
            _primaryKey = entityType.FindPrimaryKeyProperty();
        }

        public List<(string,string)> Generate(string dbContextName = null)
        {
            Type? dbContextType = null;

            if (dbContextName is not null)
            {
                dbContextType = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(assembly => assembly.GetTypes())
                    .FirstOrDefault(t => typeof(DbContext).IsAssignableFrom(t)
                        && t.IsClass
                        && !t.IsAbstract
                        && t.Name == dbContextName
                    );
            }
            else
            {
                var dbContextTypes = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(assembly => assembly.GetTypes())
                    .Where(t => typeof(DbContext).IsAssignableFrom(t)
                        && t != typeof(DbContext)
                        && t.IsClass
                        && !t.IsAbstract
                    );

                if (dbContextTypes.Count() == 1)
                    dbContextType = dbContextTypes.First();
                else if (dbContextTypes.Count() > 1)
                    throw new InvalidOperationException(
                        $"Multiple DbContexts({string.Join(", ", dbContextTypes.Select(x => x.Name))}) found in the specified assembly." +
                        $"\nPlease choose DbContext name. ex: --context <DbContextName>");
            }

            if (dbContextType is null)
                throw new InvalidOperationException("DbContext not found in the specified assembly.");

            var views = new List<(string, string)>();
            views.Add(("Filter", GenerateListPageCode()));
            views.Add(("Create", GenerateCreatePage()));
            views.Add(("Edit", GenerateEditPage()));
            views.Add(("Details", GenerateDetailsPage()));

            return views;
        }

        private string GenerateListPageCode() 
        {
            // ViewModelName for the List<T> is the entity name itself.
            string listViewModelName = _entityName; 
            string serviceName = $"{_entityName}Service";
            string pluralEntityName = $"{_entityName}s";
            var viewModel = GetViewModel(_entityName);
            string viewModelName = $"{_entityName}ViewModel";
            string _entityNameLower = _entityName.ToLowerInvariant();
            string _primaryKeyType = _primaryKey.PropertyType.Name;

            var properties = _entityType.GetProperties().Where(p => p.Name != _primaryKey.Name).ToList();

            // Determine the namespace for the list's ViewModel/Entity type
            // This assumes the _entityType.Namespace is the correct one for the items in the list.
            string actualViewModelsNamespace = viewModel.Namespace ?? _entityType.Namespace ?? "App.Models";
            if (viewModel.Namespace == null && _entityType.Namespace != null && _entityType.Namespace.Contains("Models"))
            {
                // If a specific viewModelsNamespace isn't provided and the entity is in a "Models" sub-namespace,
                // it's likely the actual type used in the list is from this Models namespace.
                actualViewModelsNamespace = _entityType.Namespace;
            }


            var sb = new StringBuilder();

            sb.AppendLine($"@page \"/{pluralEntityName.ToLower()}\"");
            sb.AppendLine($"@using {actualViewModelsNamespace}"); // Added using for the list item type
            sb.AppendLine($"@inject I{pluralEntityName}Service {serviceName}");
            sb.AppendLine("");
            sb.AppendLine($"<h3>{_entityName} List</h3>");
            sb.AppendLine("");
            sb.AppendLine($"<NavLink class=\"btn btn-primary mb-3\" href=\"/{pluralEntityName.ToLower()}/create\">Create New</NavLink>");
            sb.AppendLine("");
            sb.AppendLine($"@if ({_entityNameLower}s == null)");
            sb.AppendLine("{");
            sb.AppendLine("    <p><em>Loading...</em></p>");
            sb.AppendLine("}");
            sb.AppendLine("else");
            sb.AppendLine("{");
            sb.AppendLine("    <table class=\"table\">");
            sb.AppendLine("        <thead>");
            sb.AppendLine("            <tr>");
            foreach (var prop in properties)
            {
                sb.AppendLine($"                <th>{prop.Name}</th>");
            }
            sb.AppendLine($"                <th>{_primaryKey.Name}</th>"); // Primary key header
            sb.AppendLine("                <th>Actions</th>");
            sb.AppendLine("            </tr>");
            sb.AppendLine("        </thead>");
            sb.AppendLine("        <tbody>");
            sb.AppendLine($"            @foreach (var item in {_entityNameLower}s)");
            sb.AppendLine("            {");
            sb.AppendLine("                <tr>");
            foreach (var prop in properties)
            {
                sb.AppendLine($"                    <td>@item.{prop.Name}</td>");
            }
            sb.AppendLine($"                    <td>@item.{_primaryKey.Name}</td>"); // Primary key value
            sb.AppendLine("                    <td>");
            sb.AppendLine($"                        <NavLink class=\"btn btn-sm btn-info\" href=\"@($\"/{pluralEntityName.ToLower()}/details/{{item.{_primaryKey.Name}}}\")\">Details</NavLink>");
            sb.AppendLine($"                        <NavLink class=\"btn btn-sm btn-warning\" href=\"@($\"/{pluralEntityName.ToLower()}/edit/{{item.{_primaryKey.Name}}}\")\">Edit</NavLink>");
            sb.AppendLine($"                        <button class=\"btn btn-sm btn-danger\" @onclick=\"() => Delete{_entityName}(item.{_primaryKey.Name})\">Delete</button>");
            sb.AppendLine("                    </td>");
            sb.AppendLine("                </tr>");
            sb.AppendLine("            }");
            sb.AppendLine("        </tbody>");
            sb.AppendLine("    </table>");
            sb.AppendLine("}");
            sb.AppendLine("");
            sb.AppendLine("@code {");
            sb.AppendLine($"    private List<{viewModelName}>? {_entityNameLower}s;");
            sb.AppendLine("");
            sb.AppendLine("    protected override async Task OnInitializedAsync()");
            sb.AppendLine("    {");
            sb.AppendLine($"        {_entityNameLower}s = await {serviceName}.GetAllAsync();");
            sb.AppendLine("    }");
            sb.AppendLine("");
            sb.AppendLine($"    async Task Delete{_entityName}({_primaryKeyType} id)");
            sb.AppendLine("    {");
            sb.AppendLine($"        await {serviceName}.DeleteAsync(id);");
            sb.AppendLine($"        {_entityNameLower}s = await {serviceName}.GetAllAsync(); // Refresh list");
            sb.AppendLine("    }");
            sb.AppendLine("}");

            return sb.ToString();
        }

        private string GenerateCreatePage(string? dtosNamespace = null)
        {
            string creationDtoName = $"{_entityName}CreationDto";
            string serviceName = $"{_entityName}Service";
            string pluralEntityName = $"{_entityName}s";
            string _entityNameLower = _entityName.ToLowerInvariant();
            string modelInstanceName = $"{_entityNameLower}CreationDto";

            Type? creationDtoType = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .FirstOrDefault(t => t.Name == creationDtoName && !t.IsInterface && !t.IsAbstract);

            if (creationDtoType == null)
            {
                // Fallback or error if DTO is not found
                return $"// Error: {creationDtoName} not found. Please ensure it exists and assemblies are loaded.";
            }

            var dtoProperties = creationDtoType.GetProperties().Where(p => p.CanWrite).ToList();
            string actualDtosNamespace = dtosNamespace ?? creationDtoType.Namespace ?? $"{_entityType.Namespace}.Dtos";


            var sb = new StringBuilder();
            sb.AppendLine($"@page \"/{pluralEntityName.ToLower()}/create\"");
            sb.AppendLine($"@using {actualDtosNamespace}");
            sb.AppendLine($"@inject I{pluralEntityName}Service {serviceName}");
            sb.AppendLine($"@inject NavigationManager NavigationManager");
            sb.AppendLine("");
            sb.AppendLine($"<h3>Create New {_entityName}</h3>");
            sb.AppendLine("");
            sb.AppendLine($"<EditForm Model=\"@{modelInstanceName}\" OnValidSubmit=\"@HandleValidSubmit\">");
            sb.AppendLine("    <DataAnnotationsValidator />");
            sb.AppendLine("    <ValidationSummary />");
            sb.AppendLine("");

            foreach (var prop in dtoProperties)
            {
                sb.AppendLine($"    <div class=\"form-group mb-3\">");
                sb.AppendLine($"        <label for=\"{prop.Name.ToLower()}\">{prop.Name}:</label>");
                sb.AppendLine(GenerateInputControl(prop, modelInstanceName));
                sb.AppendLine($"        <ValidationMessage For=\"@(() => {modelInstanceName}.{prop.Name})\" />");
                sb.AppendLine("    </div>");
            }

            sb.AppendLine("");
            sb.AppendLine("    <button type=\"submit\" class=\"btn btn-primary\">Save</button>");
            sb.AppendLine($"    <button type=\"button\" class=\"btn btn-secondary\" @onclick=\"Cancel\">Cancel</button>");
            sb.AppendLine("</EditForm>");
            sb.AppendLine("");
            sb.AppendLine("@code {");
            sb.AppendLine($"    private {creationDtoName} {modelInstanceName} = new {creationDtoName}();");
            sb.AppendLine("");
            sb.AppendLine("    private async Task HandleValidSubmit()");
            sb.AppendLine("    {");
            sb.AppendLine($"        await {serviceName}.AddAsync({modelInstanceName});");
            sb.AppendLine($"        NavigationManager.NavigateTo(\"/{pluralEntityName.ToLower()}\");");
            sb.AppendLine("    }");
            sb.AppendLine("");
            sb.AppendLine("    private void Cancel()");
            sb.AppendLine("    {");
            sb.AppendLine($"        NavigationManager.NavigateTo(\"/{pluralEntityName.ToLower()}\");");
            sb.AppendLine("    }");
            sb.AppendLine("}");

            return sb.ToString();
        }

        private string GenerateInputControl(PropertyInfo prop, string modelName)
        {
            string inputType = "InputText"; // Default
            string bindFormat = ""; // For dates or other specific formats

            if (prop.PropertyType == typeof(int) || prop.PropertyType == typeof(long) || prop.PropertyType == typeof(short) ||
                prop.PropertyType == typeof(decimal) || prop.PropertyType == typeof(double) || prop.PropertyType == typeof(float))
            {
                inputType = "InputNumber";
            }
            else if (prop.PropertyType == typeof(DateTime))
            {
                inputType = "InputDate";
            }
            else if (prop.PropertyType == typeof(bool))
            {
                inputType = "InputCheckbox";
            }
            else if (prop.PropertyType.IsEnum)
            {
                // Basic enum handling, could be enhanced with a select dropdown
                // For now, treating as InputText or requiring manual adjustment
                 return $"        <{inputType} id=\"{prop.Name.ToLower()}\" class=\"form-control\" @bind-Value=\"{modelName}.{prop.Name}\" />\n" +
                        $"        <!-- Enum Type: {prop.PropertyType.Name}. Consider using InputSelect for enums. -->";
            }
            // Add other type mappings as needed, e.g., for Guid, byte[], etc.

            return $"        <{inputType} id=\"{prop.Name.ToLower()}\" class=\"form-control\" @bind-Value=\"{modelName}.{prop.Name}\" {bindFormat}/>";
        }

        private string GenerateEditPage(string? dtosNamespace = null, string? viewModelsNamespace = null)
        {
            string modificationDtoName = $"{_entityName}ModificationDto";
            string serviceName = $"{_entityName}Service";
            string pluralEntityName = $"{_entityName}s";
            string _entityNameLower = _entityName.ToLowerInvariant();
            string modelInstanceName = $"{_entityNameLower}ModificationDto";
            string _primaryKeyType = _primaryKey.PropertyType.Name;
            string primaryKeyRouteConstraint = GetPrimaryKeyRouteConstraint(_primaryKey.PropertyType);
            // Assuming ViewModel name is same as EntityName for GetByIdAsync return type and for mapping
            string viewModelName = _entityName; 

            Type? modificationDtoType = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .FirstOrDefault(t => t.Name == modificationDtoName && !t.IsInterface && !t.IsAbstract);

            if (modificationDtoType == null)
            {
                return $"// Error: {modificationDtoName} not found. Please ensure it exists and assemblies are loaded.";
            }

            var dtoProperties = modificationDtoType.GetProperties().Where(p => p.CanWrite).ToList();
            string actualDtosNamespace = dtosNamespace ?? modificationDtoType.Namespace ?? $"{_entityType.Namespace}.Dtos";

            // Attempt to infer ViewModel namespace if not provided, assuming it's related to entity's namespace or a common "ViewModels" sub-namespace
            string actualViewModelsNamespace = viewModelsNamespace ?? $"{_entityType.Namespace?.Replace(".Models", "")}.ViewModels"; 
            if (_entityType.Namespace?.Contains("Models") == false && viewModelsNamespace == null) // If entity is not in a "Models" subfolder
            {
                 actualViewModelsNamespace = _entityType.Namespace ?? "App.ViewModels"; // Default or root namespace
            }


            var sb = new StringBuilder();
            sb.AppendLine($"@page \"/{pluralEntityName.ToLower()}/edit/{{Id:{primaryKeyRouteConstraint}}}\"");
            sb.AppendLine($"@using {actualDtosNamespace}");
            // It's possible the ViewModel (if GetByIdAsync returns one) is in a different namespace
            // For now, let's assume it's either the same as DTOs or a common ViewModels namespace
            // If GetByIdAsync returns the entity itself, its namespace is _entityType.Namespace
            sb.AppendLine($"@using {actualViewModelsNamespace}"); // Ensure this namespace is correct for the ViewModel/Entity
            sb.AppendLine($"@inject I{pluralEntityName}Service {serviceName}");
            sb.AppendLine($"@inject NavigationManager NavigationManager");
            sb.AppendLine($"@inject AutoMapper.IMapper _mapper"); // Assuming IMapper is registered
            sb.AppendLine("");
            sb.AppendLine($"<h3>Edit {_entityName}</h3>");
            sb.AppendLine("");
            sb.AppendLine($"@if ({modelInstanceName} == null)");
            sb.AppendLine("{");
            sb.AppendLine("    <p><em>Loading...</em></p>");
            sb.AppendLine("}");
            sb.AppendLine("else");
            sb.AppendLine("{");
            sb.AppendLine($"    <EditForm Model=\"@{modelInstanceName}\" OnValidSubmit=\"@HandleValidSubmit\">");
            sb.AppendLine("        <DataAnnotationsValidator />");
            sb.AppendLine("        <ValidationSummary />");
            sb.AppendLine("");

            foreach (var prop in dtoProperties)
            {
                sb.AppendLine($"        <div class=\"form-group mb-3\">");
                sb.AppendLine($"            <label for=\"{prop.Name.ToLower()}\">{prop.Name}:</label>");
                sb.AppendLine(GenerateInputControl(prop, modelInstanceName));
                sb.AppendLine($"            <ValidationMessage For=\"@(() => {modelInstanceName}.{prop.Name})\" />");
                sb.AppendLine("        </div>");
            }

            sb.AppendLine("");
            sb.AppendLine("        <button type=\"submit\" class=\"btn btn-primary\">Save</button>");
            sb.AppendLine($"        <button type=\"button\" class=\"btn btn-secondary\" @onclick=\"Cancel\">Cancel</button>");
            sb.AppendLine("    </EditForm>");
            sb.AppendLine("}");
            sb.AppendLine("");
            sb.AppendLine("@code {");
            sb.AppendLine($"    [Parameter]");
            sb.AppendLine($"    public {_primaryKeyType} Id {{ get; set; }}");
            sb.AppendLine("");
            sb.AppendLine($"    private {modificationDtoName}? {modelInstanceName};");
            sb.AppendLine("");
            sb.AppendLine("    protected override async Task OnParametersSetAsync()");
            sb.AppendLine("    {");
            sb.AppendLine($"        var entityViewModel = await {serviceName}.GetByIdAsync(Id);");
            sb.AppendLine($"        if (entityViewModel != null)");
            sb.AppendLine("        {");
            sb.AppendLine($"            {modelInstanceName} = _mapper.Map<{modificationDtoName}>(entityViewModel);");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine("");
            sb.AppendLine("    private async Task HandleValidSubmit()");
            sb.AppendLine("    {");
            sb.AppendLine($"        if ({modelInstanceName} != null)");
            sb.AppendLine("        {");
            sb.AppendLine($"            await {serviceName}.UpdateAsync(Id, {modelInstanceName});");
            sb.AppendLine($"            NavigationManager.NavigateTo(\"/{pluralEntityName.ToLower()}\");");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine("");
            sb.AppendLine("    private void Cancel()");
            sb.AppendLine("    {");
            sb.AppendLine($"        NavigationManager.NavigateTo(\"/{pluralEntityName.ToLower()}\");");
            sb.AppendLine("    }");
            sb.AppendLine("}");

            return sb.ToString();
        }

        private string GenerateDetailsPage(string? viewModelsNamespace = null)
        {
            string serviceName = $"{_entityName}Service";
            string pluralEntityName = $"{_entityName}s";
            string _entityNameLower = _entityName.ToLowerInvariant();
            string _primaryKeyType = _primaryKey.PropertyType.Name;
            string primaryKeyRouteConstraint = GetPrimaryKeyRouteConstraint(_primaryKey.PropertyType);

            string preferredViewModelName = $"{_entityName}ViewModel";
            string viewModelInstanceName = $"{_entityNameLower}Details"; // Variable name in the @code block

            Type? displayType = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .FirstOrDefault(t => t.Name == preferredViewModelName && !t.IsInterface && !t.IsAbstract);

            string actualViewModelName;
            string actualViewModelsNamespace;

            if (displayType != null)
            {
                actualViewModelName = preferredViewModelName;
                actualViewModelsNamespace = viewModelsNamespace ?? displayType.Namespace ?? $"{_entityType.Namespace}.ViewModels";
            }
            else
            {
                // Fallback to using the entity itself if ViewModel is not found
                displayType = _entityType;
                actualViewModelName = _entityName;
                actualViewModelsNamespace = viewModelsNamespace ?? _entityType.Namespace ?? "App.Models";
            }

            var propertiesToDisplay = displayType.GetProperties().ToList();

            var sb = new StringBuilder();
            sb.AppendLine($"@page \"/{pluralEntityName.ToLower()}/details/{{Id:{primaryKeyRouteConstraint}}}\"");
            sb.AppendLine($"@using {actualViewModelsNamespace}");
            sb.AppendLine($"@inject I{pluralEntityName}Service {serviceName}");
            sb.AppendLine($"@inject NavigationManager NavigationManager");
            sb.AppendLine("");
            sb.AppendLine($"<h3>{_entityName} Details</h3>");
            sb.AppendLine("");
            sb.AppendLine($"@if ({viewModelInstanceName} == null)");
            sb.AppendLine("{");
            sb.AppendLine("    <p><em>Loading...</em></p>");
            sb.AppendLine("}");
            sb.AppendLine("else");
            sb.AppendLine("{");
            sb.AppendLine("    <dl class=\"row\">");

            foreach (var prop in propertiesToDisplay)
            {
                sb.AppendLine($"        <dt class=\"col-sm-3\">{prop.Name}</dt>");
                sb.AppendLine($"        <dd class=\"col-sm-9\">@{viewModelInstanceName}.{prop.Name}</dd>");
            }

            sb.AppendLine("    </dl>");
            sb.AppendLine("    <div>");
            sb.AppendLine($"        <button class=\"btn btn-primary\" @onclick=\"Edit{_entityName}\">Edit</button>");
            sb.AppendLine("        <button class=\"btn btn-secondary\" @onclick=\"BackToList\">Back to List</button>");
            sb.AppendLine("    </div>");
            sb.AppendLine("}");
            sb.AppendLine("");
            sb.AppendLine("@code {");
            sb.AppendLine($"    [Parameter]");
            sb.AppendLine($"    public {_primaryKeyType} Id {{ get; set; }}");
            sb.AppendLine("");
            sb.AppendLine($"    private {actualViewModelName}? {viewModelInstanceName};");
            sb.AppendLine("");
            sb.AppendLine("    protected override async Task OnParametersSetAsync()");
            sb.AppendLine("    {");
            sb.AppendLine($"        {viewModelInstanceName} = await {serviceName}.GetByIdAsync(Id);");
            sb.AppendLine("    }");
            sb.AppendLine("");
            sb.AppendLine($"    private void Edit{_entityName}()");
            sb.AppendLine("    {");
            sb.AppendLine($"        NavigationManager.NavigateTo($\"/{pluralEntityName.ToLower()}/edit/{{Id}}\");");
            sb.AppendLine("    }");
            sb.AppendLine("");
            sb.AppendLine("    private void BackToList()");
            sb.AppendLine("    {");
            sb.AppendLine($"        NavigationManager.NavigateTo(\"/{pluralEntityName.ToLower()}\");");
            sb.AppendLine("    }");
            sb.AppendLine("}");

            return sb.ToString();
        }

        private string GetPrimaryKeyRouteConstraint(Type type)
        {
            if (type == typeof(int) || type == typeof(long) || type == typeof(short)) return "int";
            if (type == typeof(Guid)) return "guid";
            // Add other constraints as needed, e.g., for strings if applicable
            return type.Name.ToLower(); // Default to type name, might need adjustment
        }
    }
}
