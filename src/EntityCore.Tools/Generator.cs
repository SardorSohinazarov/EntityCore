using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NuGet.Common;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace EntityCore.Tools
{
    public partial class Generator
    {
        private readonly Assembly _assembly;
        private readonly Dictionary<string, string> _arguments;
        private readonly string _projectRoot;
        public Generator(string projectRoot, Dictionary<string, string> arguments)
        {
            _projectRoot = projectRoot;
            var dllPath = FindDllPath(_projectRoot);
            var assembly = Assembly.UnsafeLoadFrom(dllPath);
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

            var entityType = _assembly.GetTypes().FirstOrDefault(t => t.Name == entityName);

            if (entityType is null)
                throw new InvalidOperationException($"Entity with name '{entityName}' not found in the specified _assembly.");

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

            if (withController)
            {
                var controllerCode = GenerateControllerCode(entityType);
                var controllerPath = Path.Combine(_projectRoot, "Controllers");
                Directory.CreateDirectory(controllerPath);
                string controllerFilePath = Path.Combine(controllerPath, $"{entityName}sController.cs");
                File.WriteAllText(controllerFilePath, controllerCode);
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Service for '{entityName}' entity generated successfully.");
            Console.ResetColor();
        }

        static string? FindDllPath(string projectRootPath)
        {
            string[] versions = { "net7.0", "net8.0", "net9.0" };

            var dllName = Path.GetFileName(projectRootPath.TrimEnd(Path.DirectorySeparatorChar));
            Console.WriteLine("dllName:" + dllName);

            string publishPath = "bin/Release";

            foreach (var version in versions)
            {
                string path = Path.Combine(projectRootPath, "bin", "Debug", version, $"{dllName}.dll");

                if (File.Exists(path))
                    return path;
            }

            throw new InvalidOperationException("Dll file not found.");
        }

        // Hozircha kerak emas lekin qo'shimcha dll lar bilan yuklansa balki kerak bo'ladi
        private static void LoadAssembly(Assembly assembly)
        {
            var references = assembly.GetReferencedAssemblies();
            foreach (var reference in references)
            {
                try
                {
                    Assembly.Load(reference);
                }
                catch (Exception)
                {
                    var path = Path.Combine(NuGetEnvironment.GetFolderPath(NuGetFolderPath.NuGetHome), "packages", reference.Name, reference.Version.ToString(), $"{reference.Name}.dll");
                    Assembly.LoadFrom(path);
                }
            }
        }

        private CompilationUnitSyntax GenerateControllerUsings(NamespaceDeclarationSyntax namespaceDeclaration, Type entityType)
        {
            var usings = new List<string>
            {
                "Microsoft.AspNetCore.Mvc",
                $"Services.{entityType.Name}s"
            };

            if (!string.IsNullOrEmpty(entityType?.Namespace))
                usings.Add(entityType.Namespace);

            // Syntax daraxtini yaratish
            var syntaxTree = SyntaxFactory.CompilationUnit()
                .AddUsings(usings.Distinct().Select(u => SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(u))).ToArray())
                .AddMembers(namespaceDeclaration);

            return syntaxTree;
        }

        private static CompilationUnitSyntax GenerateUsings(
            NamespaceDeclarationSyntax namespaceDeclaration,
            Type? dbContextType,
            Type? entityType)
        {
            var usings = new List<string>
            {
                "Microsoft.EntityFrameworkCore"
            };

            // Qo'shimcha using'larni qo'shish (agar namespace mavjud bo'lsa)
            if (!string.IsNullOrEmpty(dbContextType?.Namespace))
                usings.Add(dbContextType.Namespace);

            if (!string.IsNullOrEmpty(entityType?.Namespace))
                usings.Add(entityType.Namespace);

            // Syntax daraxtini yaratish
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
    }
}
