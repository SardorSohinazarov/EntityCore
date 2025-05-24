using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using System.Reflection;
using EntityCore.Tools.Extensions;
using EntityCore.Tools.Common.Paginations.Models;
using Microsoft.CodeAnalysis;

namespace EntityCore.Tools.Services
{
    public class IService : Generator
    {
        private readonly string _entityName;
        private readonly Type _entityType;
        private readonly PropertyInfo _primaryKey;
        public IService(Type entityType)
        {
            _entityType = entityType;
            _entityName = _entityType.Name;
            _primaryKey = entityType.FindPrimaryKeyProperty();
        }

        public string Generate()
        {
            InterfaceDeclarationSyntax interfaceDeclaration = GetInterfaceDeclaration();

            var namespaceDeclaration = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName($"Services.{_entityName}s"))
                .AddMembers(interfaceDeclaration);

            var syntaxTree = GenerateUsings(namespaceDeclaration, null, _entityType);

            return syntaxTree
                .NormalizeWhitespace()
                .ToFullString();
        }

        private InterfaceDeclarationSyntax GetInterfaceDeclaration()
        {
            return SyntaxFactory.InterfaceDeclaration($"I{_entityName}sService")
                                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                                .AddMembers(
                                    GenerateAddMethodDecleration(),
                                    GenerateGetAllMethodDeclaration(),
                                    GenerateFilterMethodDeclaration(),
                                    GenerateGetByIdMethodDeclaration(),
                                    GenerateUpdateMethodDeclaration(),
                                    GenerateDeleteMethodDeclaration()
                                );
        }

        private MemberDeclarationSyntax GenerateAddMethodDecleration()
        {
            var creationDtoTypeName = GetCreationDtoTypeName(_entityName);
            var returnTypeName = GetReturnTypeName(_entityName);

            return SyntaxFactory.MethodDeclaration(SyntaxFactory.GenericName(SyntaxFactory.Identifier("Task"))
                                .AddTypeArgumentListArguments(SyntaxFactory.ParseTypeName(returnTypeName)), "AddAsync")
                                .AddParameterListParameters(SyntaxFactory.Parameter(SyntaxFactory.Identifier("entity"))
                                    .WithType(SyntaxFactory.ParseTypeName(creationDtoTypeName)))
                                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
        }

        private MethodDeclarationSyntax GenerateGetAllMethodDeclaration()
        {
            var returnTypeName = GetReturnTypeName(_entityName);

            return SyntaxFactory.MethodDeclaration(SyntaxFactory.GenericName(SyntaxFactory.Identifier("Task"))
                                .AddTypeArgumentListArguments(SyntaxFactory.ParseTypeName($"List<{returnTypeName}>")), "GetAllAsync")
                                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)
                                );
        }

        private MethodDeclarationSyntax GenerateFilterMethodDeclaration()
        {
            var returnTypeName = GetReturnTypeName(_entityName);

            return SyntaxFactory.MethodDeclaration(SyntaxFactory.GenericName(SyntaxFactory.Identifier("Task"))
                                .AddTypeArgumentListArguments(SyntaxFactory.ParseTypeName($"List<{returnTypeName}>")), "FilterAsync")
                                .AddParameterListParameters(SyntaxFactory.Parameter(SyntaxFactory.Identifier("filter"))
                                                    .WithType(SyntaxFactory.ParseTypeName(typeof(PaginationOptions).Name)))
                                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)
                                );
        }

        private MethodDeclarationSyntax GenerateGetByIdMethodDeclaration()
        {
            var returnTypeName = GetReturnTypeName(_entityName);

            return SyntaxFactory.MethodDeclaration(SyntaxFactory.GenericName(SyntaxFactory.Identifier("Task"))
                                .AddTypeArgumentListArguments(SyntaxFactory.ParseTypeName(returnTypeName)), "GetByIdAsync")
                                .AddParameterListParameters(SyntaxFactory.Parameter(SyntaxFactory.Identifier("id"))
                                    .WithType(SyntaxFactory.ParseTypeName(_primaryKey.PropertyType.ToCSharpTypeName())))
                                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)
                                );
        }

        private MethodDeclarationSyntax GenerateUpdateMethodDeclaration()
        {
            var modificationDtoTypeName = GetModificationDtoTypeName(_entityName);
            var returnTypeName = GetReturnTypeName(_entityName);

            return SyntaxFactory.MethodDeclaration(SyntaxFactory.GenericName(SyntaxFactory.Identifier("Task"))
                                .AddTypeArgumentListArguments(SyntaxFactory.ParseTypeName(returnTypeName)), "UpdateAsync")
                                .AddParameterListParameters(SyntaxFactory.Parameter(SyntaxFactory.Identifier("id"))
                                    .WithType(SyntaxFactory.ParseTypeName(_primaryKey.PropertyType.ToCSharpTypeName())))
                                .AddParameterListParameters(SyntaxFactory.Parameter(SyntaxFactory.Identifier("entity"))
                                    .WithType(SyntaxFactory.ParseTypeName(modificationDtoTypeName)))
                                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)
                                );
        }

        private MethodDeclarationSyntax GenerateDeleteMethodDeclaration()
        {
            var returnTypeName = GetReturnTypeName(_entityName);

            return SyntaxFactory.MethodDeclaration(SyntaxFactory.GenericName(SyntaxFactory.Identifier("Task"))
                                .AddTypeArgumentListArguments(SyntaxFactory.ParseTypeName(returnTypeName)), "DeleteAsync")
                                .AddParameterListParameters(SyntaxFactory.Parameter(SyntaxFactory.Identifier("id"))
                                    .WithType(SyntaxFactory.ParseTypeName(_primaryKey.PropertyType.ToCSharpTypeName())))
                                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)
                                );
        }

        private CompilationUnitSyntax GenerateUsings(
            NamespaceDeclarationSyntax namespaceDeclaration,
            Type? dbContextType,
            Type? entityType)
        {
            var usings = new List<string>
            {
                "AutoMapper",
                "Microsoft.EntityFrameworkCore",
                "Microsoft.AspNetCore.Http",
                "Common.Paginations.Models",
                "Common.Paginations.Extensions"
            };

            var viewModelType = FindExistingViewModelType(entityType.Name);
            if (!string.IsNullOrEmpty(viewModelType?.Namespace))
                usings.Add(viewModelType.Namespace);

            var creationDtoType = FindExistingCreationDtoType(entityType.Name);
            if (!string.IsNullOrEmpty(creationDtoType?.Namespace))
                usings.Add(creationDtoType.Namespace);

            var modificationDtoType = FindExistingModificationDtoType(entityType.Name);
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
    }
}
