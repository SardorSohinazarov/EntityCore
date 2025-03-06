using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace EntityCore.Tools.QueryOptions.Models
{
    public class PaginationOptions
    {
        public string GeneratePaginationOptionsClass()
        {
            var namespaceDeclaration = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName("QueryOptions.Models"))
                .AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System")));

            var classDeclaration = SyntaxFactory.ClassDeclaration("PaginationOptions")
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .AddMembers(
                    // Private fields
                    SyntaxFactory.FieldDeclaration(SyntaxFactory.VariableDeclaration(SyntaxFactory.ParseTypeName("int"))
                        .AddVariables(SyntaxFactory.VariableDeclarator("_pageSize")))
                        .AddModifiers(SyntaxFactory.Token(SyntaxKind.PrivateKeyword)),

                    SyntaxFactory.FieldDeclaration(SyntaxFactory.VariableDeclaration(SyntaxFactory.ParseTypeName("int"))
                        .AddVariables(SyntaxFactory.VariableDeclarator("_pageToken")))
                        .AddModifiers(SyntaxFactory.Token(SyntaxKind.PrivateKeyword)),

                    // Constructor
                    SyntaxFactory.ConstructorDeclaration("PaginationOptions")
                        .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                        .AddParameterListParameters(
                            SyntaxFactory.Parameter(SyntaxFactory.Identifier("pageSize")).WithType(SyntaxFactory.ParseTypeName("int")),
                            SyntaxFactory.Parameter(SyntaxFactory.Identifier("pageToken")).WithType(SyntaxFactory.ParseTypeName("int"))
                        )
                        .WithBody(SyntaxFactory.Block(
                            SyntaxFactory.ParseStatement("(PageSize, PageToken) = (pageSize, pageToken);")
                        )),

                    // PageSize Property
                    SyntaxFactory.PropertyDeclaration(SyntaxFactory.ParseTypeName("int"), "PageSize")
                        .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                        .AddAccessorListAccessors(
                            SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                                .WithExpressionBody(SyntaxFactory.ArrowExpressionClause(SyntaxFactory.ParseExpression("_pageSize")))
                                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                            SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                                .WithExpressionBody(SyntaxFactory.ArrowExpressionClause(
                                    SyntaxFactory.ParseExpression("_pageSize = value <= 0 ? 20 : value")))
                                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
                        ),

                    // PageToken Property
                    SyntaxFactory.PropertyDeclaration(SyntaxFactory.ParseTypeName("int"), "PageToken")
                        .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                        .AddAccessorListAccessors(
                            SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                                .WithExpressionBody(SyntaxFactory.ArrowExpressionClause(SyntaxFactory.ParseExpression("_pageToken")))
                                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                            SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                                .WithExpressionBody(SyntaxFactory.ArrowExpressionClause(
                                    SyntaxFactory.ParseExpression("_pageToken = value <= 0 ? 1 : value")))
                                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
                        )
                );

            // Namespacega classni qo'shish
            namespaceDeclaration = namespaceDeclaration.AddMembers(classDeclaration);

            // Kodni string ko'rinishida olish
            return namespaceDeclaration.NormalizeWhitespace().ToFullString();
        }
    }
}
