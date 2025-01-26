﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace EntityCore.Tools
{
    public partial class CodeGenerator
    {
        public void GenerateService(string dllPath, string projectRoot, string entityName, string? dbContextName, bool withController)
        {
            var entityType = Assembly.LoadFrom(dllPath)
                     .GetTypes()
                     .FirstOrDefault(t => t.Name == entityName);

            if (entityType is null)
                throw new InvalidOperationException($"Entity with name '{entityName}' not found in the specified assembly.");

            var serviceImplementationCode = GenerateServiceImplementationCode(dllPath, entityType, dbContextName);
            var serviceDeclarationCode = GenerateServiceDeclarationCode(dllPath, entityType);

            string outputPath = Path.Combine(projectRoot, "Services");
            Directory.CreateDirectory(outputPath);

            var servicePath = Path.Combine(outputPath, $"{entityName}s");
            Directory.CreateDirectory(servicePath);

            string serviceImplementationPath = Path.Combine(servicePath, $"{entityName}sService.cs");
            File.WriteAllText(serviceImplementationPath, serviceImplementationCode);

            string serviceDeclarationPath = Path.Combine(servicePath, $"I{entityName}sService.cs");
            File.WriteAllText(serviceDeclarationPath, serviceDeclarationCode);

            if (withController)
            {
                var controllerCode = GenerateControllerCode(dllPath, entityType);
                var controllerPath = Path.Combine(projectRoot, "Controllers");
                Directory.CreateDirectory(controllerPath);
                string controllerFilePath = Path.Combine(controllerPath, $"{entityName}sController.cs");
                File.WriteAllText(controllerFilePath, controllerCode);
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Service for '{entityName}' entity generated successfully.");
            Console.ResetColor();
        }

        private string GenerateControllerCode(string dllPath, Type entityType)
        {
            var entityName = entityType.Name;
            var primaryKey = FindKeyProperty(entityType);


            var classDeclaration = GetControllerClassDeclaration(entityName, primaryKey);

            var namespaceDeclaration = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName("Services"))
                .AddMembers(classDeclaration);

            CompilationUnitSyntax syntaxTree = GenerateUsings(namespaceDeclaration, null, entityType);

            return syntaxTree
                .NormalizeWhitespace()
                .ToFullString();
        }

        private string GenerateServiceDeclarationCode(string dllPath, Type entityType)
        {
            var entityName = entityType.Name;
            var primaryKey = FindKeyProperty(entityType);

            InterfaceDeclarationSyntax interfaceDeclaration = GetInterfaceDeclaration(entityName, primaryKey);

            var namespaceDeclaration = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName("Services"))
                .AddMembers(interfaceDeclaration);

            CompilationUnitSyntax syntaxTree = GenerateUsings(namespaceDeclaration, null, entityType);

            return syntaxTree
                .NormalizeWhitespace()
                .ToFullString();
        }

        private InterfaceDeclarationSyntax GetInterfaceDeclaration(string entityName, PropertyInfo primaryKey)
        {
            return SyntaxFactory.InterfaceDeclaration($"I{entityName}sService")
                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                .AddMembers(
                    GenerateAddMethodDecleration(entityName),
                    GenerateGetAllMethodDeclaration(entityName),
                    GenerateGetByIdMethodDeclaration(entityName, primaryKey),
                    GenerateUpdateMethodDeclaration(entityName, primaryKey),
                    GenerateDeleteMethodDeclaration(entityName, primaryKey)
                );
        }

        private string GenerateServiceImplementationCode(string dllPath, Type entityType, string? dbContextName)
        {
            string entityName = entityType.Name;

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

        private static ClassDeclarationSyntax GetClassDeclaration(string entityName, PropertyInfo primaryKey, Type dbContextType)
        {
            var dbContextVariableName = dbContextType.Name.GenerateFieldNameWithUnderscore();

            var classDeclaration = SyntaxFactory.ClassDeclaration($"{entityName}sService")
                        .AddBaseListTypes(SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName($"I{entityName}sService")))
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
                            GenerateAddMethodImplementation(entityName, dbContextVariableName),
                            GenerateGetAllMethodImplementation(entityName, dbContextVariableName),
                            GenerateGetByIdMethodImplementation(entityName, dbContextVariableName, primaryKey),
                            GenerateUpdateMethodImplementation(entityName, dbContextVariableName, primaryKey),
                            GenerateDeleteMethodImplementation(entityName, dbContextVariableName, primaryKey)
                        );

            return classDeclaration;
        }

        private static ClassDeclarationSyntax GetControllerClassDeclaration(string entityName, PropertyInfo primaryKey)
        {
            var serviceVariableName = $"_i{entityName}sService";
            var classDeclaration = SyntaxFactory.ClassDeclaration($"{entityName}sController")
                        .AddBaseListTypes(SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName($"ControllerBase")))
                        .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                        .AddAttributeLists(
                            SyntaxFactory.AttributeList(
                                SyntaxFactory.SingletonSeparatedList(
                                    SyntaxFactory.Attribute(SyntaxFactory.ParseName("Route"))
                                        .WithArgumentList(SyntaxFactory.AttributeArgumentList(
                                            SyntaxFactory.SingletonSeparatedList(
                                                SyntaxFactory.AttributeArgument(
                                                    SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal("api/[controller]"))
                                                )))))),

                            SyntaxFactory.AttributeList(
                                SyntaxFactory.SingletonSeparatedList(
                                    SyntaxFactory.Attribute(SyntaxFactory.ParseName("ApiController"))))
                        )
                        .AddMembers(
                            // fields
                            SyntaxFactory.FieldDeclaration(
                                SyntaxFactory.VariableDeclaration(
                                    SyntaxFactory.ParseTypeName($"I{entityName}sService"))
                                .AddVariables(SyntaxFactory.VariableDeclarator(serviceVariableName)))
                                .AddModifiers(
                                    SyntaxFactory.Token(SyntaxKind.PrivateKeyword),   // private 
                                    SyntaxFactory.Token(SyntaxKind.ReadOnlyKeyword)), // readonly
                            //constructors
                            SyntaxFactory.ConstructorDeclaration($"{entityName}sController")
                                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                                .AddParameterListParameters(SyntaxFactory.Parameter(SyntaxFactory.Identifier($"i{entityName}sService"))
                                    .WithType(SyntaxFactory.ParseTypeName($"I{entityName}sService")))
                                .WithBody(SyntaxFactory.Block(SyntaxFactory.SingletonList<StatementSyntax>(
                                    SyntaxFactory.ExpressionStatement(SyntaxFactory.ParseExpression($"{serviceVariableName} = i{entityName}sService"))
                                )))
                            // methods
                            //GenerateAddActionImplementation(entityName, serviceVariableName),
                            //GenerateGetAllActionImplementation(entityName, serviceVariableName),
                            //GenerateGetByIdActionImplementation(entityName, serviceVariableName, primaryKey),
                            //GenerateUpdateActionImplementation(entityName, serviceVariableName, primaryKey),
                            //GenerateDeleteActionImplementation(entityName, serviceVariableName, primaryKey)
                        );

            return classDeclaration;
        }

        private static MethodDeclarationSyntax GenerateDeleteActionImplementation(string entityName, string serviceVariableName, PropertyInfo primaryKey)
        {
            throw new NotImplementedException();
        }

        private static MethodDeclarationSyntax GenerateUpdateActionImplementation(string entityName, string serviceVariableName, PropertyInfo primaryKey)
        {
            throw new NotImplementedException();
        }

        private static MethodDeclarationSyntax GenerateGetByIdActionImplementation(string entityName, string serviceVariableName, PropertyInfo primaryKey)
        {
            throw new NotImplementedException();
        }

        private static MethodDeclarationSyntax GenerateGetAllActionImplementation(string entityName, string serviceVariableName)
        {
            throw new NotImplementedException();
        }

        private static MethodDeclarationSyntax GenerateAddActionImplementation(string entityName, string serviceVariableName)
        {
            throw new NotImplementedException();
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
