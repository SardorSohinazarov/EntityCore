using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace EntityCore.Tools
{
    public partial class CodeGenerator
    {
        public void GenerateService(string dllPath, string projectRoot, string entityName)
        {
            var serviceCode = GenerateServiceCode(dllPath, entityName);

            string outputPath = Path.Combine(projectRoot, "Services", $"{entityName}sService.cs");

            Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
            File.WriteAllText(outputPath, serviceCode);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Service for '{entityName}' entity generated successfully.");
            Console.ResetColor();
        }

        private string GenerateServiceCode(string dllPath, string entityName)
        {
            var entityType = Assembly.LoadFrom(dllPath)
                                 .GetTypes()
                                 .FirstOrDefault(t => t.Name == entityName);

            var applicationDbContext = Assembly.LoadFrom(dllPath)
                                              .GetTypes()
                                              .FirstOrDefault(t => t.Name == "ApplicationDbContext");

            if (entityType == null)
                throw new InvalidOperationException($"Entity with name '{entityName}' not found in the specified assembly.");

            var primaryKey = FindKeyProperty(entityType);

            var classDeclaration = GetClassDeclaration(entityName, primaryKey);

            var namespaceDeclaration = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName("Services"))
                .AddMembers(classDeclaration);

            CompilationUnitSyntax syntaxTree = GenerateUsings(namespaceDeclaration);

            return syntaxTree
                .NormalizeWhitespace()
                .ToFullString();
        }

        private static CompilationUnitSyntax GenerateUsings(NamespaceDeclarationSyntax namespaceDeclaration)
        {
            var syntaxTree = SyntaxFactory.CompilationUnit()
                .AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System")))
                .AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Collections.Generic")))
                .AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Linq")))
                .AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Text")))
                .AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Threading.Tasks")))
                .AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Microsoft.EntityFrameworkCore")))
                // Todo : avval namespace bor yoki yo'qligini tekshirish kerak
                //.AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(entityType.Namespace!)))
                //.AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(applicationDbContext!.Namespace!)))
                .AddMembers(namespaceDeclaration);
            return syntaxTree;
        }

        private static ClassDeclarationSyntax GetClassDeclaration(string entityName, PropertyInfo primaryKey)
        {
            return SyntaxFactory.ClassDeclaration($"{entityName}sService")
                        .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                        .AddMembers(
                            // fields
                            SyntaxFactory.FieldDeclaration(
                                SyntaxFactory.VariableDeclaration(
                                    SyntaxFactory.ParseTypeName("ApplicationDbContext"))
                                .AddVariables(SyntaxFactory.VariableDeclarator("_applicationDbContext")))
                                .AddModifiers(
                                    SyntaxFactory.Token(SyntaxKind.PrivateKeyword),   // private 
                                    SyntaxFactory.Token(SyntaxKind.ReadOnlyKeyword)), // readonly
                            //constructors
                            SyntaxFactory.ConstructorDeclaration($"{entityName}sService")
                                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                                .AddParameterListParameters(SyntaxFactory.Parameter(SyntaxFactory.Identifier("applicationDbContext"))
                                    .WithType(SyntaxFactory.ParseTypeName("ApplicationDbContext")))
                                .WithBody(SyntaxFactory.Block(SyntaxFactory.SingletonList<StatementSyntax>(
                                    SyntaxFactory.ExpressionStatement(SyntaxFactory.ParseExpression("_applicationDbContext = applicationDbContext"))
                                ))),

                            // methods
                            GenerateAddMethod(entityName),
                            GenerateGetAllMethod(entityName),
                            GenerateGetByIdMethod(entityName, primaryKey),
                            GenerateUpdateMethod(entityName, primaryKey),
                            GenerateDeleteMothod(entityName, primaryKey)
                        );
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
