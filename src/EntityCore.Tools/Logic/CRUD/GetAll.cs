using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;

namespace EntityCore.Tools
{
    public partial class CodeGenerator
    {
        private static MethodDeclarationSyntax GenerateGetAllMethodDeclaration(string entityName)
        {
            return SyntaxFactory.MethodDeclaration(SyntaxFactory.GenericName(SyntaxFactory.Identifier("Task"))
                                .AddTypeArgumentListArguments(SyntaxFactory.ParseTypeName($"List<{entityName}>")), "GetAllAsync")
                                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)
                                );
        }

        private static MethodDeclarationSyntax GenerateGetAllMethodImplementation(string entityName, string dbContextVariableName)
        {
            return SyntaxFactory.MethodDeclaration(SyntaxFactory.GenericName(SyntaxFactory.Identifier("Task"))
                                .AddTypeArgumentListArguments(SyntaxFactory.ParseTypeName($"List<{entityName}>")), "GetAllAsync")
                                .AddModifiers(
                                    SyntaxFactory.Token(SyntaxKind.PublicKeyword), // public
                                    SyntaxFactory.Token(SyntaxKind.AsyncKeyword))  // async
                                .WithBody(SyntaxFactory.Block(
                                    SyntaxFactory.ParseStatement($"return await {dbContextVariableName}.Set<{entityName}>().ToListAsync();")
                                ));
        }
    }
}