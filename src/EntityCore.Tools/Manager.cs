using EntityCore.Tools.Common;
using EntityCore.Tools.Common.Paginations.Extensions;
using EntityCore.Tools.Common.Paginations.Models;
using EntityCore.Tools.Common.ServiceAttribute;
using EntityCore.Tools.Controllers;
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
            GenerateService(_arguments);
            GenerateController(_arguments);
            GenerateExceptionM(_arguments);
            GenerateResult(_arguments);
            GenerateServiceAttribute(_arguments);
        }

        private void GenerateServiceAttribute(Dictionary<string, string> arguments)
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

        private void GenerateResult(Dictionary<string, string> arguments)
        {
            if (_arguments.ContainsKey("result"))
            {
                var result = new Result();
                var resultClassesCode = result.GenerateResultClasses();
                WriteCode("Common", "Result.cs", resultClassesCode);

                ConsoleMessage("Result classes generated successfully!");
            }
        }

        private void GenerateExceptionM(Dictionary<string, string> arguments)
        {
            if (_arguments.ContainsKey("exceptionM"))
            {
                var exceptionHandlerMiddlewareCode = new ExceptionHandlerMiddleware();
                var code = exceptionHandlerMiddlewareCode.Generate();
                WriteCode("Middlewares", "ExceptionHandlerMiddleware.cs", code);
                ConsoleMessage("Exception handler middleware generated successfully!");
            }
        }

        private void GenerateService(Dictionary<string, string> arguments)
        {
            var serviceEntityName = _arguments.ContainsKey("service") ? _arguments["service"] : null;
            if (serviceEntityName is null)
                return;

            Type? entityType = GetEntityType(serviceEntityName);

            string dbContextName = _arguments.ContainsKey("context") ? _arguments["context"] : null;
            Console.WriteLine("dbContextName:" + dbContextName);

            GeneratePagination();

            var service = new Service(entityType);
            var serviceImplementationCode = service.Generate(dbContextName);

            var iService = new IService(entityType);
            var serviceDeclarationCode = iService.Generate();

            WriteCode("Services", $"I{serviceEntityName}sService.cs", serviceDeclarationCode);
            WriteCode("Services", $"{serviceEntityName}sService.cs", serviceImplementationCode);
        }

        private void GenerateController(Dictionary<string, string> arguments)
        {
            var controllerEntityName = _arguments.ContainsKey("controller") ? _arguments["controller"] : null;
            if (controllerEntityName is null)
                return;

            Type? entityType = GetEntityType(controllerEntityName);

            var controller = new Controller(entityType);
            var code = controller.GenerateControllerCodeWithEntity();
            WriteCode("Controllers", $"{controllerEntityName}sController.cs", code);
            ConsoleMessage($"Controller for {controllerEntityName} generated successfully!");
        }

        private void GeneratePagination()
        {
            var paginationComponents = new (string[], string, string)[]
            {
                (new[] { "Common", "Pagination" }, "PaginationOptions.cs", new PaginationOptions().GeneratePaginationOptionsClass()),
                (new[] { "Common", "Pagination" }, "PaginationExtensions.cs", new PaginationExtensions().GeneratePaginationExtensions()),
                (new[] { "Common", "Pagination" }, "PaginationMetadata.cs", new PaginationMetadata().GeneratePaginationMetadataClass()),
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
    }
}