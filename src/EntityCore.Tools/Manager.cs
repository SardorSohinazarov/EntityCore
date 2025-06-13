using EntityCore.Tools.Common;
using EntityCore.Tools.Common.Paginations.Extensions;
using EntityCore.Tools.Common.Paginations.Models;
using EntityCore.Tools.Common.ServiceAttribute;
using EntityCore.Tools.Controllers;
using EntityCore.Tools.DataTransferObjects;
using EntityCore.Tools.Middlewares;
using EntityCore.Tools.Services;
using EntityCore.Tools.Views;
using EntityCore.Tools.Views.Components;

namespace EntityCore.Tools
{
    public partial class Manager
    {
        private readonly List<string> _generatedFiles = new List<string>();
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
            // Backend generation
            GenerateDto();
            GenerateService();
            GenerateController();
            GenerateExceptionM();
            GenerateResult();
            GenerateServiceAttribute();

            // Frontend generation
            GenerateView();

            if (_generatedFiles.Any())
            {
                ConsoleMessage("\nSuccessfully generated files:", ConsoleColor.Cyan); // Or another distinct color
                foreach (var filePath in _generatedFiles)
                {
                    Console.WriteLine($"- {filePath}"); // Standard color for the list
                }
            }
        }

        private void GenerateDto()
        {
            var entityName = _arguments.GetValueOrDefault("_entityName");
            if (string.IsNullOrEmpty(entityName) || !_arguments.ContainsKey("dto"))
            {
                return;
            }

            Type? entityType = GetEntityType(entityName);

            var dtos = new (string[], string, string)[]
            {
                (["DataTransferObjects", $"{entityName}s"], $"{entityName}CreationDto.cs", new CreationDto(entityType).Generate()),
                (["DataTransferObjects", $"{entityName}s"], $"{entityName}ModificationDto.cs", new ModificationDto(entityType).Generate()),
                (["DataTransferObjects", $"{entityName}s"], $"{entityName}ViewModel.cs", new ViewModel(entityType).Generate()),
            };

            foreach (var (directories, fileName, code) in dtos)
            {
                WriteCode(directories, fileName, code);
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
            }
        }

        private void GenerateResult()
        {
            if (_arguments.ContainsKey("result"))
            {
                var result = new Result();
                var resultClassesCode = result.Generate();
                WriteCode("Common", "Result.cs", resultClassesCode);
            }
        }

        private void GenerateExceptionM()
        {
            if (_arguments.ContainsKey("exceptionM"))
            {
                var exceptionHandlerMiddlewareCode = new ExceptionHandlerMiddleware();
                var code = exceptionHandlerMiddlewareCode.Generate();
                WriteCode("Middlewares", "ExceptionHandlerMiddleware.cs", code);
            }
        }

        private void GenerateService()
        {
            var entityName = _arguments.GetValueOrDefault("_entityName");
            if (string.IsNullOrEmpty(entityName) || !_arguments.ContainsKey("service"))
            {
                return;
            }

            Type? entityType = GetEntityType(entityName);
            var dbContextName = _arguments.GetValueOrDefault("context");
            // Console.WriteLine("dbContextName:" + dbContextName); // Removed as per subtask instructions

            GeneratePagination(); // These generate shared files, flags are checked internally
            GenerateListResult(); // These generate shared files, flags are checked internally

            var service = new Service(entityType);
            var serviceImplementationCode = service.Generate(dbContextName);

            var iService = new IService(entityType);
            var serviceDeclarationCode = iService.Generate();

            WriteCode(["Services", $"{entityName}s"], $"I{entityName}sService.cs", serviceDeclarationCode);
            WriteCode(["Services", $"{entityName}s"], $"{entityName}sService.cs", serviceImplementationCode);
        }

        private void GenerateListResult()
        {
            ListResult listResult = new ListResult();
            var code = listResult.Generate();
            WriteCode("Common", "ListResult.cs", code);
        }

        private void GenerateView()
        {
            var entityName = _arguments.GetValueOrDefault("_entityName");
            if (string.IsNullOrEmpty(entityName) || !_arguments.ContainsKey("view"))
            {
                return;
            }

            Type? entityType = GetEntityType(entityName);

            GeneratePaginationComponent(); // These generate shared files, flags are checked internally
            GenerateInputGuid(); // These generate shared files, flags are checked internally

            var view = new View(entityType);
            var viewCodes = view.Generate();

            foreach(var viewCode in viewCodes)
            {
                WriteCode(["Components", "Pages", $"{entityName}s"], $"{entityName}.{viewCode.Item1}.razor", viewCode.Item2);
            }
        }

        private void GenerateController()
        {
            var entityName = _arguments.GetValueOrDefault("_entityName");
            if (string.IsNullOrEmpty(entityName) || !_arguments.ContainsKey("controller"))
            {
                return;
            }

            Type? entityType = GetEntityType(entityName);

            var controller = new Controller(entityType);
            var code = controller.GenerateControllerCodeWithEntity();
            WriteCode("Controllers", $"{entityName}sController.cs", code);
        }

        private void GeneratePaginationComponent()
        {
            var paginationComponent = new PaginationComponent();
            var code = paginationComponent.Generate();
            WriteCode(["Components"], "Pagination.razor", code);

            var style = paginationComponent.Style();
            WriteCode(["Components"], "Pagination.razor.css", style);
        }

        private void GenerateInputGuid()
        {
            var inputGuid = new InputGuid();
            var code = inputGuid.Generate();
            WriteCode(["Components"], "InputGuid.cs", code);
        }

        private void GeneratePagination() // This method generates shared files. Check for its own flag if necessary or assume it's always called if service is.
        {
            // Assuming GeneratePagination, GenerateListResult, GeneratePaginationComponent, GenerateInputGuid
            // are either always generated when their parent (like Service or View) is generated,
            // or they should have their own flags if their generation is optional independent of parent.
            // For now, their direct call from parent methods (like GenerateService, GenerateView) means they run if parent runs.
            // If they need independent flags (e.g. --pagination true/false), that would be a different change.
            // The subtask states "Methods like GenerateResult... their logic for checking _arguments.ContainsKey(...) should remain as is".
            // This applies to GeneratePagination if it were checking a flag like _arguments.ContainsKey("pagination").
            // Since it's called directly, it implies it's part of service/view generation.

            var paginationComponents = new (string[], string, string)[]
            {
                (["Common", "Pagination"], "PaginationOptions.cs", new PaginationOptions().GeneratePaginationOptionsClass()),
                (["Common", "Pagination"], "PaginationExtensions.cs", new PaginationExtensions().GeneratePaginationExtensions()),
                (["Common", "Pagination"], "PaginationMetadata.cs", new PaginationMetadata().Generate()),
            };

            foreach (var (directories, fileName, code) in paginationComponents)
            {
                WriteCode(directories, fileName, code);
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

        private void WriteCode(string directory, string fileName, string code)
        {
            var directoryPath = Path.Combine(_projectRoot, directory);
            Directory.CreateDirectory(directoryPath);
            string filePath = Path.Combine(directoryPath, fileName);

            if (File.Exists(filePath))
            {
                ConsoleMessage($"!!! {fileName} already exists.", ConsoleColor.Yellow);
                return;
            }

            File.WriteAllText(filePath, code);
            _generatedFiles.Add(Path.GetFullPath(filePath)); // Store full path
            ConsoleMessage($"Generated: {Path.GetFullPath(filePath)}");
        }

        private void WriteCode(string[] directories, string fileName, string code)
        {
            string directoryPath = _projectRoot;

            foreach (var directory in directories)
            {
                directoryPath = Path.Combine(directoryPath, directory);
            }

            Directory.CreateDirectory(directoryPath);
            string filePath = Path.Combine(directoryPath, fileName);

            if (File.Exists(filePath))
            {
                ConsoleMessage($"!!! {fileName} already exists.", ConsoleColor.Yellow);
                return;
            }

            File.WriteAllText(filePath, code);
            _generatedFiles.Add(Path.GetFullPath(filePath)); // Store full path
            ConsoleMessage($"Generated: {Path.GetFullPath(filePath)}");
        }

        private void ConsoleMessage(string message, ConsoleColor consoleColor = ConsoleColor.Green)
        {
            Console.ForegroundColor = consoleColor;
            Console.WriteLine(message);
            Console.ResetColor();
        }
    }
}