using EntityCore.Tools.Extensions;
using System.Reflection;
using System.Text;

namespace EntityCore.Tools.Views
{
    public class Filter : Generator
    {
        private readonly Type _entityType;
        private readonly string _entityName;
        private readonly Type _viewModelType;
        private readonly PropertyInfo _primaryKey;
        private readonly Type _serviceType;
        public Filter(Type entityType)
        {
            _entityType = entityType;
            _entityName = _entityType.Name;
            _primaryKey = _entityType.FindPrimaryKeyProperty();
            _viewModelType = GetViewModelType(_entityName) ?? _entityType;
            _serviceType = GetType($"I{_entityName}sService");
        }

        public string Generate()
        {
            string serviceName = $"{_entityName}Service";
            string pluralEntityName = $"{_entityName}s";

            var properties = _viewModelType.GetProperties().Where(p => (p.PropertyType.Name != _primaryKey.PropertyType.Name && p.Name != _primaryKey.Name)).ToList();

            var sb = new StringBuilder();
            sb.AppendLine($"@page \"/{pluralEntityName.ToLower()}\"");
            sb.AppendLine("@rendermode InteractiveServer");
            sb.AppendLine("@inject NavigationManager NavigationManager");
            sb.AppendLine("@using Microsoft.AspNetCore.WebUtilities");
            sb.AppendLine("@using Common.Paginations.Models");
            if(!string.IsNullOrEmpty(_viewModelType.Namespace))
                sb.AppendLine($"@using {_viewModelType.Namespace}");
            if(!string.IsNullOrEmpty(_serviceType.Namespace))
                sb.AppendLine($"@using {_serviceType.Namespace}");
            sb.AppendLine($"@inject {_serviceType.Name} {serviceName}");
            sb.AppendLine();
            sb.AppendLine($"<h3>{_entityName} List</h3>");
            sb.AppendLine();
            sb.AppendLine($"<NavLink class=\"btn btn-primary mb-3\" href=\"/{pluralEntityName.ToLower()}/create\">Create New</NavLink>");
            sb.AppendLine();
            sb.AppendLine($"@if ({pluralEntityName} == null)");
            sb.AppendLine("{");
            sb.AppendLine("    <p><em>Loading...</em></p>");
            sb.AppendLine("}");
            sb.AppendLine("else");
            sb.AppendLine("{");
            sb.AppendLine("    <table class=\"table\">");
            sb.AppendLine("        <thead>");
            sb.AppendLine("            <tr>");
            sb.AppendLine("                <th>#</th>"); // Index column
            foreach (var prop in properties)
            {
                sb.AppendLine($"                <th>{prop.Name}</th>");
            }
            sb.AppendLine($"                <th>{_primaryKey.Name}</th>"); // Primary key header
            sb.AppendLine("                <th>Actions</th>");
            sb.AppendLine("            </tr>");
            sb.AppendLine("        </thead>");
            sb.AppendLine("        <tbody>");
            sb.AppendLine($"            @foreach (var item in {pluralEntityName})");
            sb.AppendLine("            {");
            sb.AppendLine("                <tr>");
            sb.AppendLine($"                    <td>@item.{_primaryKey.Name}</td>"); // Index column
            foreach (var prop in properties)
            {
                sb.AppendLine($"                    <td>@item.{prop.Name}</td>");
            }
            sb.AppendLine($"                    <td>@item.{_primaryKey.Name}</td>"); // Primary key value
            sb.AppendLine("                    <td>");
            sb.AppendLine($"                        <NavLink class=\"btn btn-sm btn-info\" href=\"@($\"/{pluralEntityName.ToLower()}/{{item.{_primaryKey.Name}}}\")\">Details</NavLink>");
            sb.AppendLine($"                        <button class=\"btn btn-sm btn-danger\" @onclick=\"() => Delete{_entityName}(item.{_primaryKey.Name})\">Delete</button>");
            sb.AppendLine("                    </td>");
            sb.AppendLine("                </tr>");
            sb.AppendLine("            }");
            sb.AppendLine("        </tbody>");
            sb.AppendLine("    </table>");
            sb.AppendLine($"<Pagination PaginationMetadata=\"@PaginationMetadata\" PageName=\"{pluralEntityName}\"></Pagination>");
            sb.AppendLine("}");
            sb.AppendLine();
            sb.AppendLine("@code {");
            sb.AppendLine($"    private List<{_viewModelType.Name}> {pluralEntityName} {{ get; set; }}");
            sb.AppendLine("    private PaginationOptions PaginationOptions { get; set; }");
            sb.AppendLine("    private PaginationMetadata PaginationMetadata { get; set; }");
            sb.AppendLine();
            sb.AppendLine("    protected override void OnInitialized()");
            sb.AppendLine("    {");
            sb.AppendLine("        NavigationManager.LocationChanged += OnLocationChanged;");
            sb.AppendLine("        ReadQueryAndLoadData();");
            sb.AppendLine("    }");
            sb.AppendLine();
            sb.AppendLine("    private void OnLocationChanged(object sender, LocationChangedEventArgs e)");
            sb.AppendLine("    {");
            sb.AppendLine("        ReadQueryAndLoadData();");
            sb.AppendLine("    }");
            sb.AppendLine();
            sb.AppendLine("    private void ReadQueryAndLoadData()");
            sb.AppendLine("    {");
            sb.AppendLine("        var uri = NavigationManager.ToAbsoluteUri(NavigationManager.Uri);");
            sb.AppendLine("        var query = QueryHelpers.ParseQuery(uri.Query);");
            sb.AppendLine("        PaginationOptions = new PaginationOptions(5, 1);");
            sb.AppendLine();
            sb.AppendLine("        if (query.TryGetValue(\"page\", out var pageStr) && int.TryParse(pageStr, out var page))");
            sb.AppendLine("            PaginationOptions.PageToken = page;");
            sb.AppendLine();
            sb.AppendLine("        if (query.TryGetValue(\"size\", out var sizeStr) && int.TryParse(sizeStr, out var size))");
            sb.AppendLine("            PaginationOptions.PageSize = size;");
            sb.AppendLine();
            sb.AppendLine("        _ = LoadAsync();");
            sb.AppendLine("    }");
            sb.AppendLine();
            sb.AppendLine("    async Task LoadAsync()");
            sb.AppendLine("    {");
            sb.AppendLine($"        var filteredData = await {serviceName}.FilterAsync(PaginationOptions);");
            sb.AppendLine($"        {pluralEntityName} = filteredData.Items;");
            sb.AppendLine($"        PaginationMetadata = filteredData.Pagination;");
            sb.AppendLine($"        await InvokeAsync(StateHasChanged);");
            sb.AppendLine("    }");
            sb.AppendLine();
            sb.AppendLine($"    async Task Delete{_entityName}({_primaryKey.PropertyType.ToCSharpTypeName()} id)");
            sb.AppendLine("    {");
            sb.AppendLine($"        await {serviceName}.DeleteAsync(id);");
            sb.AppendLine($"        var filteredData = await {serviceName}.FilterAsync(PaginationOptions);");
            sb.AppendLine($"        {pluralEntityName} = filteredData.Items;");
            sb.AppendLine($"        PaginationMetadata = filteredData.Pagination;");
            sb.AppendLine($"        await InvokeAsync(StateHasChanged);");
            sb.AppendLine("    }");
            sb.AppendLine();
            sb.AppendLine("    public void Dispose()");
            sb.AppendLine("    {");
            sb.AppendLine("        NavigationManager.LocationChanged -= OnLocationChanged;");
            sb.AppendLine("    }");
            sb.AppendLine("}");

            return sb.ToString();
        }
    }
}