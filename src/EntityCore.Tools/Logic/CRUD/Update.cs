using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using System.Reflection;

namespace EntityCore.Tools
{
    public partial class CodeGenerator
    {
        private static MethodDeclarationSyntax GenerateUpdateMethodDeclaration(string entityName, PropertyInfo primaryKey)
        {
            return SyntaxFactory.MethodDeclaration(SyntaxFactory.GenericName(SyntaxFactory.Identifier("Task"))
                                .AddTypeArgumentListArguments(SyntaxFactory.ParseTypeName(entityName)), "UpdateAsync")
                                .AddParameterListParameters(SyntaxFactory.Parameter(SyntaxFactory.Identifier("id"))
                                    .WithType(SyntaxFactory.ParseTypeName(primaryKey.PropertyType.ToCSharpTypeName())))
                                .AddParameterListParameters(SyntaxFactory.Parameter(SyntaxFactory.Identifier("entity"))
                                    .WithType(SyntaxFactory.ParseTypeName(entityName)))
                                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)
                                );
        }

        private static MethodDeclarationSyntax GenerateUpdateMethodImplementation(string entityName, string dbContextVariableName, PropertyInfo primaryKey)
        {
            return SyntaxFactory.MethodDeclaration(SyntaxFactory.GenericName(SyntaxFactory.Identifier("Task"))
                                .AddTypeArgumentListArguments(SyntaxFactory.ParseTypeName(entityName)), "UpdateAsync")
                                .AddModifiers(
                                    SyntaxFactory.Token(SyntaxKind.PublicKeyword), // public
                                    SyntaxFactory.Token(SyntaxKind.AsyncKeyword))  // async
                                .AddParameterListParameters(SyntaxFactory.Parameter(SyntaxFactory.Identifier("id"))
                                    .WithType(SyntaxFactory.ParseTypeName(primaryKey.PropertyType.ToCSharpTypeName())))
                                .AddParameterListParameters(SyntaxFactory.Parameter(SyntaxFactory.Identifier("entity"))
                                    .WithType(SyntaxFactory.ParseTypeName(entityName)))
                                .WithBody(SyntaxFactory.Block(
                                    SyntaxFactory.ParseStatement($"var entry = {dbContextVariableName}.Set<{entityName}>().Update(entity);"),
                                    SyntaxFactory.ParseStatement($"await {dbContextVariableName}.SaveChangesAsync();"),
                                    SyntaxFactory.ParseStatement($"return entry.Entity;")
                                ));
        }
    }
}