using EntityCore.Tools.Common.Paginations.Models;
using EntityCore.Tools.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Reflection;

namespace EntityCore.Tools.Controllers
{
    public partial class Controller
    {
        private readonly string _entityName;
        private readonly Type _entityType;
        private readonly PropertyInfo _primaryKey;
        private readonly string _returnTypeName;
        public Controller(Type entityType)
        {
            _entityType = entityType;
            _primaryKey = entityType.FindPrimaryKeyProperty();
            _entityName = entityType.Name;
            _returnTypeName = GetReturnTypeName(_entityName);
        }

        /// <summary>
        /// Controllerni Entity Type Orqali Generatsiya qilsin
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>

        public string GenerateControllerCodeWithEntity()
        {
            var classDeclaration = GetControllerClassDeclaration();

            var namespaceDeclaration = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName("Controllers"))
                .AddMembers(classDeclaration);

            CompilationUnitSyntax syntaxTree = GenerateControllerUsings(namespaceDeclaration, _entityType);

            return syntaxTree
                .NormalizeWhitespace()
                .ToFullString();
        }

        private ClassDeclarationSyntax GetControllerClassDeclaration()
        {
            var serviceVariableName = $"{_entityName}sService".GenerateFieldNameWithUnderscore();
            var classDeclaration = SyntaxFactory.ClassDeclaration($"{_entityName}sController")
                        .AddBaseListTypes(SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName($"ControllerBase")))
                        .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                        .AddAttribute("Route", "api/[controller]")
                        .AddAttribute("ApiController")
                        .AddMembers(
                            // fields
                            SyntaxFactory.FieldDeclaration(
                                SyntaxFactory.VariableDeclaration(
                                    SyntaxFactory.ParseTypeName($"I{_entityName}sService"))
                                .AddVariables(SyntaxFactory.VariableDeclarator(serviceVariableName)))
                                .AddModifiers(
                                    SyntaxFactory.Token(SyntaxKind.PrivateKeyword),   // private 
                                    SyntaxFactory.Token(SyntaxKind.ReadOnlyKeyword)), // readonly
                                                                                      //constructors
                            SyntaxFactory.ConstructorDeclaration($"{_entityName}sController")
                                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                                .AddParameterListParameters(SyntaxFactory.Parameter(SyntaxFactory.Identifier($"{_entityName}sService".GenerateFieldName()))
                                    .WithType(SyntaxFactory.ParseTypeName($"I{_entityName}sService")))
                                .WithBody(SyntaxFactory.Block(
                                    SyntaxFactory.ParseStatement($"{serviceVariableName} = {$"{_entityName}sService".GenerateFieldName()};")
                                )),
                            // methods
                            GenerateAddActionImplementation(serviceVariableName),
                            GenerateGetAllActionImplementation(serviceVariableName),
                            GenerateFilterActionImplementation(serviceVariableName),
                            GenerateGetByIdActionImplementation(serviceVariableName),
                            GenerateUpdateActionImplementation(serviceVariableName),
                            GenerateDeleteActionImplementation(serviceVariableName)
                        );

            return classDeclaration;
        }

        private MethodDeclarationSyntax GenerateAddActionImplementation(string serviceVariableName)
        {
            var creationDtoTypeName = GetCreationDtoTypeName(_entityName);
            var parametrName = creationDtoTypeName.GenerateFieldName();

            return SyntaxFactory.MethodDeclaration(SyntaxFactory.GenericName(SyntaxFactory.Identifier("Task"))
                                .AddTypeArgumentListArguments(SyntaxFactory.ParseTypeName($"Result<{_returnTypeName}>")), "AddAsync")
                                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword)) // public
                                .AddModifiers(SyntaxFactory.Token(SyntaxKind.AsyncKeyword))  // async
                                .AddAttribute("HttpPost")
                                .AddParameterListParameters(SyntaxFactory.Parameter(SyntaxFactory.Identifier(parametrName))
                                    .WithType(SyntaxFactory.ParseTypeName(creationDtoTypeName)))
                                .WithBody(SyntaxFactory.Block(
                                    SyntaxFactory.ParseStatement($"return Result<{_returnTypeName}>.Success(await {serviceVariableName}.AddAsync({parametrName}));"))
                                );
        }

        private MethodDeclarationSyntax GenerateGetAllActionImplementation(string serviceVariableName)
        {
            return SyntaxFactory.MethodDeclaration(SyntaxFactory.GenericName(SyntaxFactory.Identifier("Task"))
                                .AddTypeArgumentListArguments(SyntaxFactory.ParseTypeName($"Result<List<{_returnTypeName}>>")), "GetAllAsync")
                                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword)) // public
                                .AddModifiers(SyntaxFactory.Token(SyntaxKind.AsyncKeyword))  // async
                                .AddAttribute("HttpGet")
                                .WithBody(SyntaxFactory.Block(
                                    SyntaxFactory.ParseStatement($"return Result<List<{_returnTypeName}>>.Success(await {serviceVariableName}.GetAllAsync());"))
                                );
        }

        private MethodDeclarationSyntax GenerateFilterActionImplementation(string serviceVariableName)
        {
            return SyntaxFactory.MethodDeclaration(SyntaxFactory.GenericName(SyntaxFactory.Identifier("Task"))
                                .AddTypeArgumentListArguments(SyntaxFactory.ParseTypeName($"Result<List<{_returnTypeName}>>")), "FilterAsync")
                                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword)) // public
                                .AddModifiers(SyntaxFactory.Token(SyntaxKind.AsyncKeyword))  // async
                                .AddAttribute("HttpPost", "filter")
                                .AddParameterListParameters(SyntaxFactory.Parameter(SyntaxFactory.Identifier("filter"))
                                   .WithType(SyntaxFactory.ParseTypeName(typeof(PaginationOptions).Name)))
                                .WithBody(SyntaxFactory.Block(
                                    SyntaxFactory.ParseStatement($"return Result<List<{_returnTypeName}>>.Success(await {serviceVariableName}.FilterAsync(filter));"))
                                );
        }

        private MethodDeclarationSyntax GenerateGetByIdActionImplementation(string serviceVariableName)
        {
            return SyntaxFactory.MethodDeclaration(SyntaxFactory.GenericName(SyntaxFactory.Identifier("Task"))
                                .AddTypeArgumentListArguments(SyntaxFactory.ParseTypeName($"Result<{_returnTypeName}>")), "GetByIdAsync")
                                .AddModifiers(
                                    SyntaxFactory.Token(SyntaxKind.PublicKeyword), // public
                                    SyntaxFactory.Token(SyntaxKind.AsyncKeyword))  // async
                                .AddAttribute("HttpGet", "{id}")
                                .AddParameterListParameters(SyntaxFactory.Parameter(SyntaxFactory.Identifier("id"))
                                    .WithType(SyntaxFactory.ParseTypeName(_primaryKey.PropertyType.ToCSharpTypeName())))
                                .WithBody(SyntaxFactory.Block(
                                    SyntaxFactory.ParseStatement($"return Result<{_returnTypeName}>.Success(await {serviceVariableName}.GetByIdAsync(id));")
                                ));
        }

        private MethodDeclarationSyntax GenerateUpdateActionImplementation(string serviceVariableName)
        {
            var modificationDtoTypeName = GetModificationDtoTypeName(_entityName);
            var parametrName = modificationDtoTypeName.GenerateFieldName();
            var returnTypeName = GetReturnTypeName(_entityName);

            return SyntaxFactory.MethodDeclaration(SyntaxFactory.GenericName(SyntaxFactory.Identifier("Task"))
                                .AddTypeArgumentListArguments(SyntaxFactory.ParseTypeName($"Result<{_returnTypeName}>")), "UpdateAsync")
                                .AddModifiers(
                                    SyntaxFactory.Token(SyntaxKind.PublicKeyword), // public
                                    SyntaxFactory.Token(SyntaxKind.AsyncKeyword))  // async
                                .AddAttribute("HttpPut", "{id}")
                                .AddParameterListParameters(SyntaxFactory.Parameter(SyntaxFactory.Identifier("id"))
                                    .WithType(SyntaxFactory.ParseTypeName(_primaryKey.PropertyType.ToCSharpTypeName())))
                                .AddParameterListParameters(SyntaxFactory.Parameter(SyntaxFactory.Identifier(parametrName))
                                    .WithType(SyntaxFactory.ParseTypeName(modificationDtoTypeName)))
                                .WithBody(SyntaxFactory.Block(
                                    SyntaxFactory.ParseStatement($"return Result<{_returnTypeName}>.Success(await {serviceVariableName}.UpdateAsync(id,{parametrName}));")
                                ));
        }

        private MethodDeclarationSyntax GenerateDeleteActionImplementation(string serviceVariableName)
        {
            return SyntaxFactory.MethodDeclaration(SyntaxFactory.GenericName(SyntaxFactory.Identifier("Task"))
                                .AddTypeArgumentListArguments(SyntaxFactory.ParseTypeName($"Result<{_returnTypeName}>")), "DeleteAsync")
                                .AddModifiers(
                                    SyntaxFactory.Token(SyntaxKind.PublicKeyword), // public
                                    SyntaxFactory.Token(SyntaxKind.AsyncKeyword))  // async
                                .AddAttribute("HttpDelete", "{id}")
                                .AddParameterListParameters(SyntaxFactory.Parameter(SyntaxFactory.Identifier("id"))
                                .WithType(SyntaxFactory.ParseTypeName(_primaryKey.PropertyType.ToCSharpTypeName())))
                                .WithBody(SyntaxFactory.Block(
                                    SyntaxFactory.ParseStatement($"return Result<{_returnTypeName}>.Success(await {serviceVariableName}.DeleteAsync(id));")
                                ));
        }

        private CompilationUnitSyntax GenerateControllerUsings(NamespaceDeclarationSyntax namespaceDeclaration, Type entityType)
        {
            var usings = new List<string>
            {
                "Microsoft.AspNetCore.Mvc",
                $"Services.{entityType.Name}s",
                "Common.Paginations.Models",
                "Common"
            };

            var viewModelType = GetViewModelType(entityType.Name);
            if (!string.IsNullOrEmpty(viewModelType?.Namespace))
                usings.Add(viewModelType.Namespace);

            var creationDtoType = GetCreationDtoType(entityType.Name);
            if (!string.IsNullOrEmpty(creationDtoType?.Namespace))
                usings.Add(creationDtoType.Namespace);

            var modificationDtoType = GetModificationDtoType(entityType.Name);
            if (!string.IsNullOrEmpty(modificationDtoType?.Namespace))
                usings.Add(modificationDtoType.Namespace);

            if (!string.IsNullOrEmpty(entityType?.Namespace))
                usings.Add(entityType.Namespace);

            var syntaxTree = SyntaxFactory.CompilationUnit()
                .AddUsings(usings.Distinct().Select(u => SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(u))).ToArray())
                .AddMembers(namespaceDeclaration);

            return syntaxTree;
        }
    }
}
