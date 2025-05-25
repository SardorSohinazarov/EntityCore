using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EntityCore.Tools.Common.Paginations.Extensions
{
    /// <summary>
    /// IQueryable Extensions for Paginations
    /// </summary>
    public class PaginationExtensions
    {
        public string GeneratePaginationExtensions()
        {
            // using'larni yaratish
            var usings = new[]
            {
                SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Text.Json")),
                SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Microsoft.AspNetCore.Http")),
                SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Common.Paginations.Models"))
            };

            // ApplyPagination metodini yaratish
            var method = SyntaxFactory.MethodDeclaration(
                    SyntaxFactory.GenericName("IQueryable")
                        .WithTypeArgumentList(
                            SyntaxFactory.TypeArgumentList(
                                SyntaxFactory.SingletonSeparatedList<TypeSyntax>(
                                    SyntaxFactory.IdentifierName("T")
                                )
                            )
                        ),
                    SyntaxFactory.Identifier("ApplyPagination")
                )
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.StaticKeyword))
                .AddTypeParameterListParameters(SyntaxFactory.TypeParameter("T"))
                .AddParameterListParameters(
                    SyntaxFactory.Parameter(SyntaxFactory.Identifier("source"))
                        .WithType(
                            SyntaxFactory.GenericName("IQueryable")
                                .WithTypeArgumentList(
                                    SyntaxFactory.TypeArgumentList(
                                        SyntaxFactory.SingletonSeparatedList<TypeSyntax>(
                                            SyntaxFactory.IdentifierName("T")
                                        )
                                    )
                                )
                        ).WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.ThisKeyword))),
                    SyntaxFactory.Parameter(SyntaxFactory.Identifier("options"))
                        .WithType(SyntaxFactory.IdentifierName("PaginationOptions")),
                    SyntaxFactory.Parameter(SyntaxFactory.Identifier("httpContext"))
                        .WithType(SyntaxFactory.IdentifierName("HttpContext"))
                )
                .WithBody(SyntaxFactory.Block(
                    SyntaxFactory.ParseStatement("var totalCount = source.Count();"),
                    SyntaxFactory.ParseStatement("var paginationInfo = new PaginationMetadata(totalCount, options.PageSize, options.PageToken);"),
                    SyntaxFactory.ParseStatement("httpContext.Response.Headers[\"X-Pagination\"] = JsonSerializer.Serialize(paginationInfo);"),
                    SyntaxFactory.ParseStatement("return source.Skip((options.PageToken - 1) * options.PageSize).Take(options.PageSize);")
                ));

            // PaginationExtensions klassini yaratish
            var classDeclaration = SyntaxFactory.ClassDeclaration("PaginationExtensions")
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.StaticKeyword))
                .AddMembers(method);

            // namespace yaratish
            var namespaceDeclaration = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName("Common.Paginations.Extensions"))
                .AddMembers(classDeclaration);

            // using'lar va namespace'ni birlashtirish
            var compilationUnit = SyntaxFactory.CompilationUnit()
                .AddUsings(usings)
                .AddMembers(namespaceDeclaration)
                .NormalizeWhitespace();

            // Kodni string ko'rinishda qaytarish
            return compilationUnit.ToFullString();
        }
    }
}
