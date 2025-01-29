using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using System.Reflection;
using Microsoft.CodeAnalysis;

namespace EntityCore.Tools
{
    public partial class Generator
    {
        private string GenerateServiceDeclarationCode(Type entityType)
        {
            var entityName = entityType.Name;
            var primaryKey = FindKeyProperty(entityType);

            InterfaceDeclarationSyntax interfaceDeclaration = GetInterfaceDeclaration(entityName, primaryKey);

            var namespaceDeclaration = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName($"Services.{entityName}s"))
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

        private MemberDeclarationSyntax GenerateAddMethodDecleration(string entityName)
        {
            var creationDtoTypeName = GetCreationDtoTypeName(entityName);
            var returnTypeName = GetReturnTypeName(entityName);

            return SyntaxFactory.MethodDeclaration(SyntaxFactory.GenericName(SyntaxFactory.Identifier("Task"))
                                .AddTypeArgumentListArguments(SyntaxFactory.ParseTypeName(returnTypeName)), "AddAsync")
                                .AddParameterListParameters(SyntaxFactory.Parameter(SyntaxFactory.Identifier("entity"))
                                    .WithType(SyntaxFactory.ParseTypeName(creationDtoTypeName)))
                                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
        }

        private MethodDeclarationSyntax GenerateGetAllMethodDeclaration(string entityName)
        {
            var returnTypeName = GetReturnTypeName(entityName);

            return SyntaxFactory.MethodDeclaration(SyntaxFactory.GenericName(SyntaxFactory.Identifier("Task"))
                                .AddTypeArgumentListArguments(SyntaxFactory.ParseTypeName($"List<{returnTypeName}>")), "GetAllAsync")
                                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)
                                );
        }

        private MethodDeclarationSyntax GenerateGetByIdMethodDeclaration(string entityName, PropertyInfo primaryKey)
        {
            var returnTypeName = GetReturnTypeName(entityName);

            return SyntaxFactory.MethodDeclaration(SyntaxFactory.GenericName(SyntaxFactory.Identifier("Task"))
                                .AddTypeArgumentListArguments(SyntaxFactory.ParseTypeName(returnTypeName)), "GetByIdAsync")
                                .AddParameterListParameters(SyntaxFactory.Parameter(SyntaxFactory.Identifier("id"))
                                    .WithType(SyntaxFactory.ParseTypeName(primaryKey.PropertyType.ToCSharpTypeName())))
                                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)
                                );
        }

        private MethodDeclarationSyntax GenerateUpdateMethodDeclaration(string entityName, PropertyInfo primaryKey)
        {
            var modificationDtoTypeName = GetModificationDtoTypeName(entityName);
            var returnTypeName = GetReturnTypeName(entityName);

            return SyntaxFactory.MethodDeclaration(SyntaxFactory.GenericName(SyntaxFactory.Identifier("Task"))
                                .AddTypeArgumentListArguments(SyntaxFactory.ParseTypeName(returnTypeName)), "UpdateAsync")
                                .AddParameterListParameters(SyntaxFactory.Parameter(SyntaxFactory.Identifier("id"))
                                    .WithType(SyntaxFactory.ParseTypeName(primaryKey.PropertyType.ToCSharpTypeName())))
                                .AddParameterListParameters(SyntaxFactory.Parameter(SyntaxFactory.Identifier("entity"))
                                    .WithType(SyntaxFactory.ParseTypeName(modificationDtoTypeName)))
                                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)
                                );
        }

        private MethodDeclarationSyntax GenerateDeleteMethodDeclaration(string entityName, PropertyInfo primaryKey)
        {
            var returnTypeName = GetReturnTypeName(entityName);

            return SyntaxFactory.MethodDeclaration(SyntaxFactory.GenericName(SyntaxFactory.Identifier("Task"))
                                .AddTypeArgumentListArguments(SyntaxFactory.ParseTypeName(returnTypeName)), "DeleteAsync")
                                .AddParameterListParameters(SyntaxFactory.Parameter(SyntaxFactory.Identifier("id"))
                                    .WithType(SyntaxFactory.ParseTypeName(primaryKey.PropertyType.ToCSharpTypeName())))
                                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)
                                );
        }
    }
}
