using EntityCore.Tools.Extensions;
using System.Collections;
using System.Reflection;
using System.Text;

namespace EntityCore.Tools.Views
{
    public class Details : Generator
    {
        private readonly Type _entityType;
        private readonly string _entityName;
        private readonly Type _viewModelType;
        private readonly PropertyInfo _primaryKey;
        private readonly Type _serviceType;

        public Details(Type entityType)
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
            sb.AppendLine($"@page \"/{pluralEntityName.ToLower()}/{{Id:{_primaryKey.PropertyType.ToCSharpTypeName()}}}\"");
            sb.AppendLine("@rendermode InteractiveServer");
            if(!string.IsNullOrEmpty(_viewModelType.Namespace))
                sb.AppendLine($"@using {_viewModelType.Namespace}");
            if(!string.IsNullOrEmpty(_serviceType.Namespace))
                sb.AppendLine($"@using {_serviceType.Namespace}");
            sb.AppendLine($"@inject {_serviceType.Name} {serviceName}");
            sb.AppendLine($"@inject NavigationManager NavigationManager");
            sb.AppendLine();
            sb.AppendLine($"<h3>{_entityName} Details</h3>");
            sb.AppendLine();
            sb.AppendLine($"@if ({_entityName.GenerateFieldName()} == null)");
            sb.AppendLine("{");
            sb.AppendLine("    <p><em>Loading...</em></p>");
            sb.AppendLine("}");
            sb.AppendLine("else");
            sb.AppendLine("{");
            sb.AppendLine("    <dl class=\"row\">");

            foreach (var prop in properties)
            {
                sb.AppendLine($"        <dt class=\"col-sm-3\">{prop.Name}</dt>");
                sb.AppendLine(GenerateLink(prop));
                //sb.AppendLine($"        <dd class=\"col-sm-9\">@{_entityName.GenerateFieldName()}.{prop.Name}</dd>");
            }

            sb.AppendLine("    </dl>");
            sb.AppendLine("    <div>");
            sb.AppendLine($"        <button class=\"btn btn-primary\" @onclick=\"Save\">Save</button>");
            sb.AppendLine("        <button class=\"btn btn-secondary\" @onclick=\"BackToList\">Back to List</button>");
            sb.AppendLine("    </div>");
            sb.AppendLine("}");
            sb.AppendLine();
            sb.AppendLine("@code {");
            sb.AppendLine($"    [Parameter]");
            sb.AppendLine($"    public {_primaryKey.PropertyType.ToCSharpTypeName()} Id {{ get; set; }}");
            sb.AppendLine();
            sb.AppendLine($"    private {_viewModelType.Name}? {_entityName.GenerateFieldName()};");
            sb.AppendLine();
            sb.AppendLine("    protected override async Task OnParametersSetAsync()");
            sb.AppendLine("    {");
            sb.AppendLine($"        {_entityName.GenerateFieldName()} = await {serviceName}.GetByIdAsync(Id);");
            sb.AppendLine("    }");
            sb.AppendLine();
            sb.AppendLine($"    private void Save()");
            sb.AppendLine("    {");
            sb.AppendLine($"        throw new NotImplementedException(\"Save functionality is not implemented.\");");
            sb.AppendLine("    }");
            sb.AppendLine();
            sb.AppendLine("    private void BackToList()");
            sb.AppendLine("    {");
            sb.AppendLine($"        NavigationManager.NavigateTo(\"/{pluralEntityName.ToLower()}\");");
            sb.AppendLine("    }");
            sb.AppendLine("}");

            return sb.ToString();
        }

        private string GetPropertyName(PropertyInfo property)
        {
            if (typeof(IEnumerable).IsAssignableFrom(property.PropertyType) && property.PropertyType != typeof(string))
                return null;

            return $"                <th>{property.Name}</th>";
        }

        private string GenerateLink(PropertyInfo property)
        {
            if (typeof(IEnumerable).IsAssignableFrom(property.PropertyType) && property.PropertyType != typeof(string))
                return null;

            if (property.PropertyType.IsNavigationProperty())
            {
                var idProperty = GetPropertyId(property);
                if (idProperty is null)
                    return $"                    <dd class=\"col-sm-9\">null</dd>";

                return $"                    <dd class=\"col-sm-9\"><a href=\"/{property.PropertyType.Name}s/@{_entityName.GenerateFieldName()}.{idProperty?.Name}\">link</a></dd>";
            }
            else
                return $"                    <dd class=\"col-sm-9\">@{_entityName.GenerateFieldName()}.{property.Name}</dd>";
        }

        private PropertyInfo? GetPropertyId(PropertyInfo property)
        {
            return _entityType.GetProperties()
                .FirstOrDefault(x => x.Name == $"{property.Name}Id");
        }
    }
}
