using EntityCore.Tools.Extensions;
using System.Reflection;
using System.Text;

namespace EntityCore.Tools.Views
{
    public class Create : Generator
    {
        private readonly Type _entityType;
        private readonly string _entityName;
        private readonly Type _creationDtoType;
        private readonly Type _serviceType;

        public Create(Type entityType)
        {
            _entityType = entityType;
            _entityName = _entityType.Name;
            _creationDtoType = GetCreationDtoType(_entityName) ?? _entityType;
            _serviceType = GetType($"I{_entityName}sService");
        }

        public string Generate()
        {
            string serviceName = $"{_entityName}Service";
            string pluralEntityName = $"{_entityName}s";

            var dtoProperties = _creationDtoType.GetProperties().Where(p => p.CanWrite).ToList();

            var sb = new StringBuilder();
            sb.AppendLine($"@page \"/{pluralEntityName.ToLower()}/create\"");
            sb.AppendLine("@rendermode InteractiveServer");
            if(!string.IsNullOrEmpty(_creationDtoType.Namespace))
                sb.AppendLine($"@using {_creationDtoType.Namespace}");
            if(!string.IsNullOrEmpty(_serviceType.Namespace))
                sb.AppendLine($"@using {_serviceType.Namespace}");
            sb.AppendLine($"@inject I{pluralEntityName}Service {serviceName}");
            sb.AppendLine($"@inject NavigationManager NavigationManager");
            sb.AppendLine("");
            sb.AppendLine($"<h3>Create New {_entityName}</h3>");
            sb.AppendLine("");
            sb.AppendLine($"<EditForm FormName=\"{_creationDtoType.Name}\" Model=\"@{_entityName.GenerateFieldName()}\" OnValidSubmit=\"() => HandleValidSubmit()\">");
            sb.AppendLine("    <DataAnnotationsValidator />");
            sb.AppendLine("    <ValidationSummary />");
            sb.AppendLine("");

            foreach (var prop in dtoProperties)
            {
                sb.AppendLine($"    <div class=\"form-group mb-3\">");
                sb.AppendLine($"        <label for=\"{prop.Name.ToLower()}\">{prop.Name}:</label>");
                sb.AppendLine(GenerateInputControl(prop, _entityName.GenerateFieldName()));
                sb.AppendLine($"        <ValidationMessage For=\"@(() => {_entityName.GenerateFieldName()}.{prop.Name})\" />");
                sb.AppendLine("    </div>");
            }

            sb.AppendLine("");
            sb.AppendLine("    <button type=\"submit\" class=\"btn btn-primary\">Save</button>");
            sb.AppendLine($"    <button type=\"button\" class=\"btn btn-secondary\" @onclick=\"Cancel\">Cancel</button>");
            sb.AppendLine("</EditForm>");
            sb.AppendLine("");
            sb.AppendLine("@code {");
            sb.AppendLine($"    [SupplyParameterFromForm]");
            sb.AppendLine($"    public {_creationDtoType.Name} {_entityName.GenerateFieldName()} {{ get; set; }} = new {_creationDtoType.Name}();");
            sb.AppendLine("");
            sb.AppendLine("    private async Task HandleValidSubmit()");
            sb.AppendLine("    {");
            sb.AppendLine($"        await {serviceName}.AddAsync({_entityName.GenerateFieldName()});");
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
                 return $"        <{inputType} id=\"{prop.Name.ToLower()}\" class=\"form-control\" @bind-Value=\"{modelName}.{prop.Name}\" />\n" +
                        $"        <!-- Enum Type: {prop.PropertyType.Name}. Consider using InputSelect for enums. -->";
            }
            // List bo'lsa get all qilib shunda tanlanadigan select qilish kerak

            return $"        <{inputType} id=\"{prop.Name.ToLower()}\" class=\"form-control\" @bind-Value=\"{modelName}.{prop.Name}\" {bindFormat}/>";
        }
    }
}
