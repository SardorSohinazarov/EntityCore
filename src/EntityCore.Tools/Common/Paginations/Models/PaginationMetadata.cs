using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace EntityCore.Tools.Common.Paginations.Models
{
    /// <summary>
    /// Pagination metadata generator for writing response header
    /// x-pagination : {"size":20, "page":1, "total":123}
    /// </summary>
    public class PaginationMetadata
    {
        public string GeneratePaginationMetadataClass()
        {
            var namespaceDeclaration = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName("Common.Paginations.Models"));

            var classDeclaration = SyntaxFactory.ClassDeclaration("PaginationMetadata")
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .AddMembers(
                    // TotalCount property
                    SyntaxFactory.PropertyDeclaration(SyntaxFactory.ParseTypeName("int"), "TotalCount")
                        .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                        .WithAccessorList(
                            SyntaxFactory.AccessorList(SyntaxFactory.List(new[]
                            {
                            SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
                            }))
                        ),
                    // TotalPages property
                    SyntaxFactory.PropertyDeclaration(SyntaxFactory.ParseTypeName("int"), "TotalPages")
                        .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                        .WithAccessorList(
                            SyntaxFactory.AccessorList(SyntaxFactory.List(new[]
                            {
                            SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
                            }))
                        ),
                    // CurrentPage property
                    SyntaxFactory.PropertyDeclaration(SyntaxFactory.ParseTypeName("int"), "CurrentPage")
                        .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                        .WithAccessorList(
                            SyntaxFactory.AccessorList(SyntaxFactory.List(new[]
                            {
                            SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
                            }))
                        ),
                    // PageSize property
                    SyntaxFactory.PropertyDeclaration(SyntaxFactory.ParseTypeName("int"), "PageSize")
                        .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                        .WithAccessorList(
                            SyntaxFactory.AccessorList(SyntaxFactory.List(new[]
                            {
                            SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
                            }))
                        ),
                    // Constructor
                    SyntaxFactory.ConstructorDeclaration("PaginationMetadata")
                        .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                        .AddParameterListParameters(
                            SyntaxFactory.Parameter(SyntaxFactory.Identifier("totalCount"))
                                .WithType(SyntaxFactory.ParseTypeName("int")),
                            SyntaxFactory.Parameter(SyntaxFactory.Identifier("pageSize"))
                                .WithType(SyntaxFactory.ParseTypeName("int")),
                            SyntaxFactory.Parameter(SyntaxFactory.Identifier("currentPage"))
                                .WithType(SyntaxFactory.ParseTypeName("int"))
                        )
                        .WithBody(SyntaxFactory.Block(
                            SyntaxFactory.ParseStatement("TotalCount = totalCount;"),
                            SyntaxFactory.ParseStatement("PageSize = pageSize;"),
                            SyntaxFactory.ParseStatement("CurrentPage = currentPage;"),
                            SyntaxFactory.ParseStatement("TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);")
                        ))
                );

            // Namespacega classni qo'shish
            namespaceDeclaration = namespaceDeclaration.AddMembers(classDeclaration);

            // Kodni formatlash va stringga aylantirish
            var code = namespaceDeclaration.NormalizeWhitespace().ToFullString();
            return code;
        }
    }
}
