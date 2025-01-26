using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EntityCore.Tools
{
    public partial class CodeGenerator
    {
        private MemberDeclarationSyntax GenerateAddMethodDecleration(string entityName)
        {
            return SyntaxFactory.MethodDeclaration(SyntaxFactory.GenericName(SyntaxFactory.Identifier("Task"))
                    .AddTypeArgumentListArguments(SyntaxFactory.ParseTypeName(entityName)), "AddAsync")
                    .AddParameterListParameters(SyntaxFactory.Parameter(SyntaxFactory.Identifier("entity"))
                        .WithType(SyntaxFactory.ParseTypeName(entityName)))
                    .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
        }

        private static MethodDeclarationSyntax GenerateAddMethodImplementation(string entityName, string dbContextVariableName)
        {
            return SyntaxFactory.MethodDeclaration(SyntaxFactory.GenericName(SyntaxFactory.Identifier("Task"))
                                .AddTypeArgumentListArguments(SyntaxFactory.ParseTypeName(entityName)), "AddAsync")
                                .AddModifiers(
                                    SyntaxFactory.Token(SyntaxKind.PublicKeyword), // public
                                    SyntaxFactory.Token(SyntaxKind.AsyncKeyword))  // async
                                .AddParameterListParameters(SyntaxFactory.Parameter(SyntaxFactory.Identifier("entity"))
                                    .WithType(SyntaxFactory.ParseTypeName(entityName)))
                                .WithBody(SyntaxFactory.Block(
                                    SyntaxFactory.ParseStatement($"var entry = await {dbContextVariableName}.Set<{entityName}>().AddAsync(entity);"),
                                    SyntaxFactory.ParseStatement($"await {dbContextVariableName}.SaveChangesAsync();"),
                                    SyntaxFactory.ParseStatement($"return entry.Entity;")
                                ));
        }
    }
}
