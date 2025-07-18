﻿using EntityCore.Tools.Common.Paginations.Models;
using EntityCore.Tools.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Reflection;

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
            var parametrName = creationDtoTypeName.GenerateFieldName();
            var returnTypeName = GetReturnTypeName(_entityName);

            return SyntaxFactory.MethodDeclaration(SyntaxFactory.GenericName(SyntaxFactory.Identifier("Task"))
                                .AddTypeArgumentListArguments(SyntaxFactory.ParseTypeName(returnTypeName)), "AddAsync")
                                .AddParameterListParameters(SyntaxFactory.Parameter(SyntaxFactory.Identifier(parametrName))
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
                                .AddTypeArgumentListArguments(SyntaxFactory.ParseTypeName($"ListResult<{returnTypeName}>")), "FilterAsync")
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
            var parametrName = modificationDtoTypeName.GenerateFieldName();
            var returnTypeName = GetReturnTypeName(_entityName);

            return SyntaxFactory.MethodDeclaration(SyntaxFactory.GenericName(SyntaxFactory.Identifier("Task"))
                                .AddTypeArgumentListArguments(SyntaxFactory.ParseTypeName(returnTypeName)), "UpdateAsync")
                                .AddParameterListParameters(SyntaxFactory.Parameter(SyntaxFactory.Identifier("id"))
                                    .WithType(SyntaxFactory.ParseTypeName(_primaryKey.PropertyType.ToCSharpTypeName())))
                                .AddParameterListParameters(SyntaxFactory.Parameter(SyntaxFactory.Identifier(parametrName))
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
