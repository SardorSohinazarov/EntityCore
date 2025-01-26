using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using System.Reflection;

namespace EntityCore.Tools
{
    public partial class CodeGenerator
    {
        private static MethodDeclarationSyntax GenerateDeleteMethodDeclaration(string entityName, PropertyInfo primaryKey)
        {
            return SyntaxFactory.MethodDeclaration(SyntaxFactory.GenericName(SyntaxFactory.Identifier("Task"))
                                .AddTypeArgumentListArguments(SyntaxFactory.ParseTypeName(entityName)), "DeleteAsync")
                                .AddParameterListParameters(SyntaxFactory.Parameter(SyntaxFactory.Identifier("id"))
                                    .WithType(SyntaxFactory.ParseTypeName(primaryKey.PropertyType.ToCSharpTypeName())))
                                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)
                                );
        }

        private static MethodDeclarationSyntax GenerateDeleteMethodImplementation(string entityName, string dbContextVariableName, PropertyInfo primaryKey)
        {
            return SyntaxFactory.MethodDeclaration(SyntaxFactory.GenericName(SyntaxFactory.Identifier("Task"))
                                .AddTypeArgumentListArguments(SyntaxFactory.ParseTypeName(entityName)), "DeleteAsync")
                                .AddModifiers(
                                    SyntaxFactory.Token(SyntaxKind.PublicKeyword), // public
                                    SyntaxFactory.Token(SyntaxKind.AsyncKeyword))  // async
                                .AddParameterListParameters(SyntaxFactory.Parameter(SyntaxFactory.Identifier("id"))
                                    .WithType(SyntaxFactory.ParseTypeName(primaryKey.PropertyType.ToCSharpTypeName())))
                                .WithBody(SyntaxFactory.Block(
                                    SyntaxFactory.ParseStatement($"var entity = await {dbContextVariableName}.Set<{entityName}>().FirstOrDefaultAsync(x => x.{primaryKey.Name} == id);"),
                                    SyntaxFactory.ParseStatement($"if (entity == null) throw new InvalidOperationException($\"{entityName} with {{id}} not found.\");"),
                                    SyntaxFactory.ParseStatement($"var entry = {dbContextVariableName}.Set<{entityName}>().Remove(entity);"),
                                    SyntaxFactory.ParseStatement($"await {dbContextVariableName}.SaveChangesAsync();"),
                                    SyntaxFactory.ParseStatement($"return entry.Entity;")
                                ));
        }
    }
}
