using EntityCore.Tools.Common;
using EntityCore.Tools.Common.Paginations.Extensions;
using EntityCore.Tools.Common.Paginations.Models;
using EntityCore.Tools.Common.ServiceAttribute;
using EntityCore.Tools.Controllers;
using EntityCore.Tools.Middlewares;
using EntityCore.Tools.Services;
using Microsoft.CodeAnalysis;

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
            var entityName = _arguments["entity"];
            Console.WriteLine("entityName:" + entityName);
            string dbContextName = _arguments.ContainsKey("context") ? _arguments["context"] : null;
            Console.WriteLine("dbContextName:" + dbContextName);
            bool withController = _arguments.ContainsKey("controller") ? bool.TryParse(_arguments["controller"], out withController) : false;
            Console.WriteLine("withcontroller:" + withController);
            bool withView = _arguments.ContainsKey("view") ? bool.TryParse(_arguments["view"], out withView) : false;
            Console.WriteLine("withView:" + withView);
            bool withResult = _arguments.ContainsKey("result") ? bool.TryParse(_arguments["result"], out withResult) : false;
            Console.WriteLine("withResult:" + withResult);
            bool withService = _arguments.ContainsKey("service") ? bool.TryParse(_arguments["service"], out withService) : false;
            Console.WriteLine("withService:" + withService);
            bool exceptionM = _arguments.ContainsKey("exceptionM") ? bool.TryParse(_arguments["exceptionM"], out exceptionM) : false;
            Console.WriteLine("exceptionM:" + exceptionM);
            bool serviceAttribute = _arguments.ContainsKey("serviceAttribute") ? bool.TryParse(_arguments["serviceAttribute"], out serviceAttribute) : false;
            Console.WriteLine("serviceAttribute:" + serviceAttribute);

            var entityType = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .FirstOrDefault(t => t.Name == entityName);

            if (entityType is null)
                throw new InvalidOperationException($"Entity with name '{entityName}' not found in Assembly");

            // Todo : Har bir generatsiya uchun alohida methodlar va loglar yaratish
            if (withService)
            {
                PaginationOptions paginationOptions = new PaginationOptions();
                string paginationOptionsCode = paginationOptions.GeneratePaginationOptionsClass();
                string commonDirectoryPath = Path.Combine(_projectRoot, "Common", "Pagination");
                Directory.CreateDirectory(commonDirectoryPath);
                var paginationOptionsPath = Path.Combine(commonDirectoryPath, "PaginationOptions.cs");
                File.WriteAllText(paginationOptionsPath, paginationOptionsCode);

                PaginationExtensions paginationExtensions = new PaginationExtensions();
                string paginationExtensionsCode = paginationExtensions.GeneratePaginationExtensions();
                var paginationExtensionsPath = Path.Combine(commonDirectoryPath, "PaginationExtensions.cs");
                File.WriteAllText(paginationExtensionsPath, paginationExtensionsCode);

                PaginationMetadata paginationMetadata = new PaginationMetadata();
                string paginationMetadataCode = paginationMetadata.GeneratePaginationMetadataClass();
                var paginationMetadataPath = Path.Combine(commonDirectoryPath, "PaginationMetadata.cs");
                File.WriteAllText(paginationMetadataPath, paginationMetadataCode);

                var service = new Service(entityType);
                var serviceImplementationCode = service.Generate(dbContextName);

                var iService = new IService(entityType);
                var serviceDeclarationCode = iService.Generate();

                string outputPath = Path.Combine(_projectRoot, "Services");
                Directory.CreateDirectory(outputPath);

                var servicePath = Path.Combine(outputPath, $"{entityName}s");
                Directory.CreateDirectory(servicePath);

                string serviceImplementationPath = Path.Combine(servicePath, $"{entityName}sService.cs");
                File.WriteAllText(serviceImplementationPath, serviceImplementationCode);

                string serviceDeclarationPath = Path.Combine(servicePath, $"I{entityName}sService.cs");
                File.WriteAllText(serviceDeclarationPath, serviceDeclarationCode);
            }

            if (withController)
            {
                var controller = new Controller(entityType);
                var controllerCode = controller.GenerateControllerCodeWithEntity();
                var controllerPath = Path.Combine(_projectRoot, "Controllers");
                Directory.CreateDirectory(controllerPath);
                string controllerFilePath = Path.Combine(controllerPath, $"{entityName}sController.cs");
                File.WriteAllText(controllerFilePath, controllerCode);
            }

            if (exceptionM)
            {
                var exceptionHandlerMiddlewareCode = new ExceptionHandlerMiddleware();
                var exceptionHandlerMiddlewarePath = Path.Combine(_projectRoot, "Middlewares");
                Directory.CreateDirectory(exceptionHandlerMiddlewarePath);
                string exceptionHandlerMiddlewareFilePath = Path.Combine(exceptionHandlerMiddlewarePath, "ExceptionHandlerMiddleware.cs");
                File.WriteAllText(exceptionHandlerMiddlewareFilePath, exceptionHandlerMiddlewareCode.GenerateExceptionHandlingMiddleware());
            }

            if (withResult)
            {
                var result = new Result();
                var resultClassesCode = result.GenerateResultClasses();
                var commonDirectoryPath = Path.Combine(_projectRoot, "Common");
                Directory.CreateDirectory(commonDirectoryPath);
                string resultClassesFilePath = Path.Combine(commonDirectoryPath, "Result.cs");
               
                File.WriteAllText(resultClassesFilePath, resultClassesCode);
            }

            if(serviceAttribute)
            {
                var serviceAttributeCode = new ServiceAttributes();
                var serviceAttributePath = Path.Combine(_projectRoot, "Common", "ServiceAttributes");
                Directory.CreateDirectory(serviceAttributePath);
                string serviceAttributeFilePath = Path.Combine(serviceAttributePath, "ServiceAttribute.cs");
                File.WriteAllText(serviceAttributeFilePath, serviceAttributeCode.Generate());

                var serviceAttributeCollectionExtension = new ServiceAttributeCollectionExtensions();
                var serviceAttributeCollectionExtensionPath = Path.Combine(serviceAttributePath, "ServiceAttributeCollectionExtension.cs");
                File.WriteAllText(serviceAttributeCollectionExtensionPath, serviceAttributeCollectionExtension.Generate());
            }
        }
    }
}