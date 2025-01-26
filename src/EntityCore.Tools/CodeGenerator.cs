using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace EntityCore.Tools
{
    public partial class CodeGenerator
    {
        public void GenerateService(string dllPath, string projectRoot, string entityName, string? dbContextName)
        {
            var serviceCode = GenerateServiceCode(dllPath, entityName, dbContextName);

            string outputPath = Path.Combine(projectRoot, "Services", $"{entityName}sService.cs");

            Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
            File.WriteAllText(outputPath, serviceCode);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Service for '{entityName}' entity generated successfully.");
            Console.ResetColor();
        }

        private string GenerateServiceCode(string dllPath, string entityName, string? dbContextName)
        {
            var entityType = Assembly.LoadFrom(dllPath)
                                 .GetTypes()
                                 .FirstOrDefault(t => t.Name == entityName);

            Type? dbContextType = null;

            if (dbContextName is not null)
            { 
                dbContextType = Assembly.LoadFrom(dllPath)
                                              .GetTypes()
                                              .FirstOrDefault(t => typeof(DbContext).IsAssignableFrom(t) 
                                                       && t.IsClass
                                                       && !t.IsAbstract
                                                       && t.Name == dbContextName
                                                       );
            }
            else
            {
                var dbContextTypes = Assembly.LoadFrom(dllPath)
                                              .GetTypes()
                                              .Where(t => typeof(DbContext).IsAssignableFrom(t)
                                                       && t.IsClass
                                                       && !t.IsAbstract);

                if(dbContextTypes.Count() == 1)
                    dbContextType = dbContextTypes.First();
                else if(dbContextTypes.Count() > 1)
                    throw new InvalidOperationException("Multiple DbContexts found in the specified assembly. Please choose DbContext name. ex: --context <DbContextName>");
            }

            if (entityType is null)
                throw new InvalidOperationException($"Entity with name '{entityName}' not found in the specified assembly.");

            if (dbContextType is null)
                throw new InvalidOperationException("DbContext not found in the specified assembly.");

            var primaryKey = FindKeyProperty(entityType);

            var classDeclaration = GetClassDeclaration(entityName, primaryKey, dbContextType);

            var namespaceDeclaration = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName("Services"))
                .AddMembers(classDeclaration);

            CompilationUnitSyntax syntaxTree = GenerateUsings(namespaceDeclaration, dbContextType, entityType);

            return syntaxTree
                .NormalizeWhitespace()
                .ToFullString();
        }

        private static CompilationUnitSyntax GenerateUsings(
            NamespaceDeclarationSyntax namespaceDeclaration,
            Type? dbContextType,
            Type? entityType)
        {
            var usings = new List<string>
            {
                "System",
                "System.Collections.Generic",
                "System.Linq",
                "System.Text",
                "System.Threading.Tasks",
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

        private static ClassDeclarationSyntax GetClassDeclaration(string entityName, PropertyInfo primaryKey, Type dbContextType)
        {
            var dbContextVariableName = dbContextType.Name.GenerateFieldNameWithUnderscore();

            var classDeclaration = SyntaxFactory.ClassDeclaration($"{entityName}sService")
                        .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                        .AddMembers(
                            // fields
                            SyntaxFactory.FieldDeclaration(
                                SyntaxFactory.VariableDeclaration(
                                    SyntaxFactory.ParseTypeName(dbContextType.Name))
                                .AddVariables(SyntaxFactory.VariableDeclarator(dbContextVariableName)))
                                .AddModifiers(
                                    SyntaxFactory.Token(SyntaxKind.PrivateKeyword),   // private 
                                    SyntaxFactory.Token(SyntaxKind.ReadOnlyKeyword)), // readonly
                            //constructors
                            SyntaxFactory.ConstructorDeclaration($"{entityName}sService")
                                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                                .AddParameterListParameters(SyntaxFactory.Parameter(SyntaxFactory.Identifier(dbContextType.Name.GenerateFieldName()))
                                    .WithType(SyntaxFactory.ParseTypeName(dbContextType.Name)))
                                .WithBody(SyntaxFactory.Block(SyntaxFactory.SingletonList<StatementSyntax>(
                                    SyntaxFactory.ExpressionStatement(SyntaxFactory.ParseExpression($"{dbContextVariableName} = {dbContextType.Name.GenerateFieldName()}"))
                                ))),

                            // methods
                            GenerateAddMethod(entityName, dbContextVariableName),
                            GenerateGetAllMethod(entityName, dbContextVariableName),
                            GenerateGetByIdMethod(entityName, dbContextVariableName, primaryKey),
                            GenerateUpdateMethod(entityName, dbContextVariableName, primaryKey),
                            GenerateDeleteMothod(entityName, dbContextVariableName, primaryKey)
                        );

            return classDeclaration;
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
