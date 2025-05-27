using EntityCore.Tools.Common;
using EntityCore.Tools.Common.Paginations.Extensions;
using EntityCore.Tools.Common.Paginations.Models;
using EntityCore.Tools.Common.ServiceAttribute;
using EntityCore.Tools.Controllers;
using EntityCore.Tools.DataTransferObjects;
using EntityCore.Tools.Middlewares;
using EntityCore.Tools.Services;

namespace EntityCore.Tools
{
    public partial class Manager
    {
        private readonly Dictionary<string, string> _arguments;
        private readonly string _projectRoot;
        public Manager(string projectRoot, Dictionary<string, string> arguments)
        {
            _projectRoot = projectRoot;
            var loader = new AssemblyLoader();
            loader.Load(_projectRoot);
            _arguments = arguments;
        }

        public void Generate()
        {
            GenerateDto();
            GenerateService();
            GenerateController();
            GenerateExceptionM();
            GenerateResult();
            GenerateServiceAttribute();
        }

        private void GenerateDto()
        {
            var entityName = _arguments.ContainsKey("dto") ? _arguments["dto"] : null;
            if (entityName is null)
                return;

            Type? entityType = GetEntityType(entityName);

            var baseDtoNamespace = _arguments.ContainsKey("dto-namespace-base") 
                ? _arguments["dto-namespace-base"] 
                : "Application.DataTransferObjects";

            var dtos = new (string[], string, string)[]
            {
                ([baseDtoNamespace, $"{entityName}s"], $"{entityName}CreationDto.cs", new CreationDto(entityType, baseDtoNamespace).Generate()),
                ([baseDtoNamespace, $"{entityName}s"], $"{entityName}ModificationDto.cs", new ModificationDto(entityType, baseDtoNamespace).Generate()),
                ([baseDtoNamespace, $"{entityName}s"], $"{entityName}ViewModel.cs", new ViewModel(entityType, baseDtoNamespace).Generate()),
            };

            foreach (var (directories, fileName, code) in dtos)
            {
                WriteCode(directories, fileName, code);
                ConsoleMessage($"{fileName} generated successfully!");
            }
        }

        private void GenerateServiceAttribute()
        {
            if (_arguments.ContainsKey("serviceAttribute"))
            {
                var serviceAttribute = new ServiceAttributes();
                var serviceAttributeCode = serviceAttribute.Generate();
                WriteCode(new[] { "Common", "ServiceAttributes" }, "ServiceAttributes.cs", serviceAttributeCode);

                var serviceAttributeCollectionExtension = new ServiceAttributeCollectionExtensions();
                var serviceAttributeCollectionExtensionCode = serviceAttributeCollectionExtension.Generate();
                WriteCode(new[] { "Common", "ServiceAttribute" }, "ServiceAttributeCollectionExtensions.cs", serviceAttributeCollectionExtensionCode);

                ConsoleMessage("Service attribute generated successfully!");
            }
        }

        private void GenerateResult()
        {
            if (_arguments.ContainsKey("result"))
            {
                var result = new Result();
                var resultClassesCode = result.GenerateResultClasses();
                WriteCode("Common", "Result.cs", resultClassesCode);

                ConsoleMessage("Result classes generated successfully!");
            }
        }

        private void GenerateExceptionM()
        {
            if (_arguments.ContainsKey("exceptionM"))
            {
                var exceptionHandlerMiddlewareCode = new ExceptionHandlerMiddleware();
                var code = exceptionHandlerMiddlewareCode.Generate();
                WriteCode("Middlewares", "ExceptionHandlerMiddleware.cs", code);
                ConsoleMessage("Exception handler middleware generated successfully!");
            }
        }

        private void GenerateService()
        {
            var entityName = _arguments.ContainsKey("service") ? _arguments["service"] : null;
            if (entityName is null)
                return;

            Type? entityType = GetEntityType(entityName);

            string dbContextName = _arguments.ContainsKey("context") ? _arguments["context"] : null;
            // Console.WriteLine("dbContextName:" + dbContextName); // Debug statement removed

            GeneratePagination();

            var service = new Service(entityType);
            var serviceImplementationCode = service.Generate(dbContextName);

            var iService = new IService(entityType);
            var serviceDeclarationCode = iService.Generate();

            WriteCode(["Services", $"{entityName}s"], $"I{entityName}sService.cs", serviceDeclarationCode);
            WriteCode(["Services", $"{entityName}s"], $"{entityName}sService.cs", serviceImplementationCode);
        }

        private void GenerateController()
        {
            var entityName = _arguments.ContainsKey("controller") ? _arguments["controller"] : null;
            if (entityName is null)
                return;

            Type? entityType = GetEntityType(entityName);

            var controller = new Controller(entityType);
            var code = controller.GenerateControllerCodeWithEntity();
            WriteCode("Controllers", $"{entityName}sController.cs", code);
            ConsoleMessage($"Controller for {entityName} generated successfully!");
        }

        private void GeneratePagination()
        {
            var paginationComponents = new (string[], string, string)[]
            {
                (["Common", "Pagination"], "PaginationOptions.cs", new PaginationOptions().GeneratePaginationOptionsClass()),
                (["Common", "Pagination"], "PaginationExtensions.cs", new PaginationExtensions().GeneratePaginationExtensions()),
                (["Common", "Pagination"], "PaginationMetadata.cs", new PaginationMetadata().GeneratePaginationMetadataClass()),
            };

            foreach (var (directories, fileName, code) in paginationComponents)
            {
                WriteCode(directories, fileName, code);
                ConsoleMessage($"{fileName} generated successfully!");
            }
        }

        private Type GetEntityType(string entityName)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var entityType = assembly.GetTypes()
                    .FirstOrDefault(x => x.Name == entityName);

                if (entityType is not null)
                    return entityType;
            }

            throw new InvalidOperationException($"Entity with name '{entityName}' not found in Assembly");
        }

        private static void ConsoleMessage(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        private void WriteCode(string directory, string fileName, string code)
        {
            var directoryPath = Path.Combine(_projectRoot, directory);
            Directory.CreateDirectory(directoryPath);
            string filePath = Path.Combine(directoryPath, fileName);
            File.WriteAllText(filePath, code);
        }
        
        private void WriteCode(string[] directories, string fileName, string code)
        {
            string directoryPath = _projectRoot;

            foreach (var directory in directories) {
                directoryPath = Path.Combine(directoryPath, directory);
            }

            Directory.CreateDirectory(directoryPath);
            string filePath = Path.Combine(directoryPath, fileName);
            File.WriteAllText(filePath, code);
        }

        public void GenerateBlazorViews(string entityName)
        {
            Type entityType = GetEntityType(entityName); // Throws if not found

            // These are placeholders and will need to be configurable later
            string assumedBlazorProjectRootPath = "../YourSolution.BlazorServer"; // Or some relative path
            string assumedBlazorProjectNamespace = "YourSolution.BlazorServer"; // Root namespace of the Blazor project
            string assumedServiceNamespace = "YourSolution.Application.Contracts.Services"; // Example
            string assumedServiceInterfaceName = $"I{entityName}Service";
            string assumedViewModelName = $"{entityName}ViewModel";
            string assumedCreationDtoName = $"{entityName}CreationDto";
            string assumedModificationDtoName = $"{entityName}ModificationDto";
            
            var primaryKeyProperty = entityType.FindPrimaryKeyProperty();
            string entityKeyPropertyTypeName = primaryKeyProperty.PropertyType.ToCSharpTypeName();
            string entityKeyPropertyName = primaryKeyProperty.Name;

            // --- Placeholder Information (can be removed or refined later) ---
            Console.WriteLine($"--- Generating Blazor Views for Entity: {entityName} ---");
            Console.WriteLine($"Entity Type Found: {entityType.FullName}");
            Console.WriteLine($"Assumed Blazor Project Root: {assumedBlazorProjectRootPath}");
            Console.WriteLine($"Assumed Blazor Project Namespace: {assumedBlazorProjectNamespace}");
            // ... (other console writelines for assumed names can be kept or removed as needed)

            // --- 1. Retrieve Template ---
            string template = EntityCore.Tools.Generation.Blazor.BlazorTemplates.ListPageTemplate;

            // --- 2. Load ViewModel Type ---
            Type viewModelType = Generator.FindExistingViewModelType(entityName);
            if (viewModelType == null)
            {
                throw new InvalidOperationException($"ViewModel '{assumedViewModelName}' not found for entity '{entityName}'. Please ensure it exists and is discoverable.");
            }
            string actualViewModelName = viewModelType.Name; // Use the actual name found

            // --- 3. Generate ViewModel Property Placeholders ---
            var viewModelProperties = viewModelType.GetProperties().Where(p => p.CanRead && p.GetMethod.IsPublic);
            var viewModelHeaders = new System.Text.StringBuilder();
            var viewModelCells = new System.Text.StringBuilder();

            foreach (var prop in viewModelProperties)
            {
                viewModelHeaders.AppendLine($"                <th>{prop.Name}</th>");
                viewModelCells.AppendLine($"                    <td>@item.{prop.Name}</td>");
            }

            // --- 4. String Replacements ---
            string generatedCode = template
                .Replace("{{EntityName}}", entityName)
                .Replace("{{EntityNamePluralLOWER}}", entityName.ToLower() + "s") // Simple pluralization
                .Replace("{{ServiceNamespace}}", assumedServiceNamespace)
                .Replace("{{ProjectNamespace}}", assumedBlazorProjectNamespace)
                .Replace("{{ViewModelName}}", actualViewModelName) // Use actual ViewModel name
                .Replace("{{EntityKeyPropertyName}}", entityKeyPropertyName)
                .Replace("{{ViewModelPropertiesPlaceHolder}}", viewModelHeaders.ToString().TrimEnd('\r', '\n'))
                .Replace("{{ViewModelDataPlaceHolder}}", viewModelCells.ToString().TrimEnd('\r', '\n'));

            // --- 5. File Path and Writing ---
            // Note: assumedBlazorProjectRootPath is relative to EntityCore.Tools project for now.
            // It should ideally be an absolute path or a configurable relative path from the target project.
            // For this step, we write into a path relative to where the tool is executed if _projectRoot is the target.
            // Or, if we want to place it inside the tool's own structure for now for testing:
            // string[] outputDirectories = { "GeneratedBlazorViews", entityName }; // Example relative to tool's execution
            
            // The subtask implies writing to the *assumedBlazorProjectRootPath*.
            // This path needs to be handled carefully. If it's `../YourSolution.BlazorServer`,
            // and _projectRoot is `src/YourSolution.TargetProject`, then Path.Combine(_projectRoot, assumedBlazorProjectRootPath, ...)
            // might be what's intended.
            // For now, let's assume assumedBlazorProjectRootPath is a path that can be combined with _projectRoot
            // or is an absolute path itself if configured differently.
            // The WriteCode method prepends _projectRoot.
            
            string[] directoryParts = { assumedBlazorProjectRootPath, "Pages", "Entities", entityName };
            string fileName = $"{entityName}List.razor";

            // We need to construct the full path before calling WriteCode, or adjust WriteCode.
            // Let's assume WriteCode handles combining _projectRoot with the first element of directoryParts.
            // If assumedBlazorProjectRootPath is like "../OtherProject", then this is tricky.
            // For this subtask, I will adapt to write it into a known subfolder of the *current* project (_projectRoot)
            // to avoid complex relative path issues outside the current project scope without more configuration.
            // This means the output will be inside the EntityCore.Tools project structure for now.
            // Example: _projectRoot/GeneratedBlazor/Pages/Entities/EntityName/EntityNameList.razor

            string[] effectiveOutputDirectories = { "GeneratedBlazor", "Pages", "Entities", entityName };
            
            WriteCode(effectiveOutputDirectories, fileName, generatedCode);

            Console.WriteLine($"Generated Blazor List Page: {Path.Combine(_projectRoot, Path.Combine(effectiveOutputDirectories), fileName)}");
            Console.WriteLine("--- Blazor List Page generation complete ---");
        }
    }
}