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
        }

        private void GenerateDto()
        {
            var entityName = _arguments.ContainsKey("dto") ? _arguments["dto"] : null;
            if (entityName is null)
                return;

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
            var entityName = _arguments.ContainsKey("service") ? _arguments["service"] : null;
            if (entityName is null)
                return;

            Type? entityType = GetEntityType(entityName);

            var dbContextName = _arguments.ContainsKey("context") ? _arguments["context"] : null;
            Console.WriteLine("dbContextName:" + dbContextName);

            GeneratePagination();
            GenerateListResult();

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
            var entityName = _arguments.ContainsKey("view") ? _arguments["view"] : null;
            if (entityName is null)
                return;

            Type? entityType = GetEntityType(entityName);

            GeneratePaginationComponent();

            var view = new View(entityType);
            var viewCodes = view.Generate();

            foreach(var viewCode in viewCodes)
            {
                WriteCode(["Components", "Pages", $"{entityName}s"], $"{entityName}.{viewCode.Item1}.razor", viewCode.Item2);
            }
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
        }

        private void GeneratePaginationComponent()
        {
            var paginationComponent = new PaginationComponent();
            var code = paginationComponent.Generate();
            WriteCode(["Components"], "Pagination.razor", code);

            var style = paginationComponent.Style();
            WriteCode(["Components"], "Pagination.razor.css", style);
        }

        private void GeneratePagination()
        {
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

            
            ConsoleMessage($"{fileName} generated successfully!");
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

            ConsoleMessage($"{fileName} generated successfully!");
        }

        private void ConsoleMessage(string message, ConsoleColor consoleColor = ConsoleColor.Green)
        {
            Console.ForegroundColor = consoleColor;
            Console.WriteLine(message);
            Console.ResetColor();
        }
    }
}