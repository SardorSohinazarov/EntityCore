using EntityCore.Tools.Middlewares;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace EntityCore.Tools
{
    public partial class Generator
    {
        private readonly Dictionary<string, string> _arguments;
        private readonly string _projectRoot;
        public Generator(string projectRoot, Dictionary<string, string> arguments)
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

            var entityType = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .FirstOrDefault(t => t.Name == entityName);

            if (entityType is null)
                throw new InvalidOperationException($"Entity with name '{entityName}' not found in Assembly");

            if (withService)
            {
                var serviceImplementationCode = GenerateServiceImplementationCode(entityType, dbContextName);
                var serviceDeclarationCode = GenerateServiceDeclarationCode(entityType);

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

                var controllerCode = GenerateControllerCode(entityType);
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
                var resultClassesCode = result.GenerateResultClasses("Common");
                var commonDirectoryPath = Path.Combine(_projectRoot, "Common");
                Directory.CreateDirectory(commonDirectoryPath);
                string resultClassesFilePath = Path.Combine(commonDirectoryPath, "Result.cs");
                File.WriteAllText(resultClassesFilePath, resultClassesCode);
            }

            Console.ForegroundColor = ConsoleColor.Green;
            string serviceOrControllerName = withController ? "Controller" : "Service";
            Console.WriteLine($"{serviceOrControllerName} for '{entityName}' entity generated successfully.");
            Console.ResetColor();
        }

        private CompilationUnitSyntax GenerateControllerUsings(NamespaceDeclarationSyntax namespaceDeclaration, Type entityType)
        {
            var usings = new List<string>
            {
                "Microsoft.AspNetCore.Mvc",
                $"Services.{entityType.Name}s"
            };

            var viewModelType = GetViewModel(entityType.Name);
            if (!string.IsNullOrEmpty(viewModelType?.Namespace))
                usings.Add(viewModelType.Namespace);

            var creationDtoType = GetCreationDto(entityType.Name);
            if (!string.IsNullOrEmpty(creationDtoType?.Namespace))
                usings.Add(creationDtoType.Namespace);

            var modificationDtoType = GetModificationDto(entityType.Name);
            if (!string.IsNullOrEmpty(modificationDtoType?.Namespace))
                usings.Add(modificationDtoType.Namespace);

            if (!string.IsNullOrEmpty(entityType?.Namespace))
                usings.Add(entityType.Namespace);

            var syntaxTree = SyntaxFactory.CompilationUnit()
                .AddUsings(usings.Distinct().Select(u => SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(u))).ToArray())
                .AddMembers(namespaceDeclaration);

            return syntaxTree;
        }

        private CompilationUnitSyntax GenerateUsings(
            NamespaceDeclarationSyntax namespaceDeclaration,
            Type? dbContextType,
            Type? entityType)
        {
            var usings = new List<string>
            {
                "AutoMapper",
                "Microsoft.EntityFrameworkCore"
            };

            var viewModelType = GetViewModel(entityType.Name);
            if (!string.IsNullOrEmpty(viewModelType?.Namespace))
                usings.Add(viewModelType.Namespace);

            var creationDtoType = GetCreationDto(entityType.Name);
            if (!string.IsNullOrEmpty(creationDtoType?.Namespace))
                usings.Add(creationDtoType.Namespace);

            var modificationDtoType = GetModificationDto(entityType.Name);
            if (!string.IsNullOrEmpty(modificationDtoType?.Namespace))
                usings.Add(modificationDtoType.Namespace);

            if (!string.IsNullOrEmpty(dbContextType?.Namespace))
                usings.Add(dbContextType.Namespace);

            if (!string.IsNullOrEmpty(entityType?.Namespace))
                usings.Add(entityType.Namespace);

            var syntaxTree = SyntaxFactory.CompilationUnit()
                .AddUsings(usings.Distinct().Select(u => SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(u))).ToArray())
                .AddMembers(namespaceDeclaration);

            return syntaxTree;
        }

        private PropertyInfo FindKeyProperty(Type entityType)
        {
            // 1. [Key] atributi bilan belgilangan propertyni topish
            var keyProperty = entityType
                .GetProperties()
                .FirstOrDefault(prop => prop.GetCustomAttribute<KeyAttribute>() != null);

            if (keyProperty is not null)
                return keyProperty;

            // 2. Agar [Key] topilmasa, "Id" nomli propertyni qidirish
            keyProperty = entityType
                .GetProperties()
                .FirstOrDefault(prop => string.Equals(prop.Name, "Id", StringComparison.OrdinalIgnoreCase));

            if (keyProperty is null)
                throw new InvalidOperationException("Entity must have a key property.");

            return keyProperty;
        }

        private string GetReturnTypeName(string entityName)
        {
            var viewModelType = GetViewModel(entityName);
            return viewModelType is null ? entityName : viewModelType.Name;
        }

        private Type GetViewModel(string entityName)
        {
            var viewModelName = $"{entityName}ViewModel";
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .FirstOrDefault(t => t.Name == viewModelName);
        }

        private string GetCreationDtoTypeName(string entityName)
        {
            var creationDtoType = GetCreationDto(entityName);
            return creationDtoType is null ? entityName : creationDtoType.Name;
        }

        private Type GetCreationDto(string entityName)
        {
            var creationDtoName = $"{entityName}CreationDto";
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .FirstOrDefault(t => t.Name == creationDtoName);
        }

        private string GetModificationDtoTypeName(string entityName)
        {
            var modificationDtoType = GetModificationDto(entityName);
            return modificationDtoType is null ? entityName : modificationDtoType.Name;
        }

        private Type GetModificationDto(string entityName)
        {
            var modificationDtoName = $"{entityName}ModificationDto";
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .FirstOrDefault(t => t.Name == modificationDtoName);
        }
    }
}