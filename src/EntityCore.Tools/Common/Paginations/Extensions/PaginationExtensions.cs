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
        //public string GeneratePaginationExtensions()
        //{
        //    // using'larni yaratish
        //    var usings = new[]
        //    {
        //        SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Text.Json")),
        //        SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Microsoft.AspNetCore.Http")),
        //        SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Common.Paginations.Models"))
        //    };

        //    // ApplyPagination metodini yaratish
        //    var method = SyntaxFactory.MethodDeclaration(
        //            SyntaxFactory.GenericName("IQueryable")
        //                .WithTypeArgumentList(
        //                    SyntaxFactory.TypeArgumentList(
        //                        SyntaxFactory.SingletonSeparatedList<TypeSyntax>(
        //                            SyntaxFactory.IdentifierName("T")
        //                        )
        //                    )
        //                ),
        //            SyntaxFactory.Identifier("ApplyPagination")
        //        )
        //        .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.StaticKeyword))
        //        .AddTypeParameterListParameters(SyntaxFactory.TypeParameter("T"))
        //        .AddParameterListParameters(
        //            SyntaxFactory.Parameter(SyntaxFactory.Identifier("source"))
        //                .WithType(
        //                    SyntaxFactory.GenericName("IQueryable")
        //                        .WithTypeArgumentList(
        //                            SyntaxFactory.TypeArgumentList(
        //                                SyntaxFactory.SingletonSeparatedList<TypeSyntax>(
        //                                    SyntaxFactory.IdentifierName("T")
        //                                )
        //                            )
        //                        )
        //                ).WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.ThisKeyword))),
        //            SyntaxFactory.Parameter(SyntaxFactory.Identifier("options"))
        //                .WithType(SyntaxFactory.IdentifierName("PaginationOptions")),
        //            SyntaxFactory.Parameter(SyntaxFactory.Identifier("httpContext"))
        //                .WithType(SyntaxFactory.IdentifierName("HttpContext"))
        //        )
        //        .WithBody(SyntaxFactory.Block(
        //            SyntaxFactory.ParseStatement("var totalCount = source.Count();"),
        //            SyntaxFactory.ParseStatement("var paginationInfo = new PaginationMetadata(totalCount, options.PageSize, options.PageToken);"),
        //            SyntaxFactory.ParseStatement("if(!httpContext.Response.Headers.IsReadOnly)"),
        //            SyntaxFactory.ParseStatement("  httpContext.Response.Headers[\"X-Pagination\"] = JsonSerializer.Serialize(paginationInfo);"),
        //            SyntaxFactory.ParseStatement("return source.Skip((options.PageToken - 1) * options.PageSize).Take(options.PageSize);")
        //        ));

        //    // PaginationExtensions klassini yaratish
        //    var classDeclaration = SyntaxFactory.ClassDeclaration("PaginationExtensions")
        //        .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.StaticKeyword))
        //        .AddMembers(method);

        //    // namespace yaratish
        //    var namespaceDeclaration = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName("Common.Paginations.Extensions"))
        //        .AddMembers(classDeclaration);

        //    // using'lar va namespace'ni birlashtirish
        //    var compilationUnit = SyntaxFactory.CompilationUnit()
        //        .AddUsings(usings)
        //        .AddMembers(namespaceDeclaration)
        //        .NormalizeWhitespace();

        //    // Kodni string ko'rinishda qaytarish
        //    return compilationUnit.ToFullString();
        //}

        public string GeneratePaginationExtensions()
        {
            // Using directivelarni yaratish
            var usings = new[]
            {
                SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Text.Json")),
                SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Common.Paginations.Models")),
                SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Microsoft.EntityFrameworkCore")),
                SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Threading.Tasks")),
                SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Linq")),
                SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Collections.Generic"))
            };

            // Method declaration
            var tupleReturnType = SyntaxFactory.TupleType(
                SyntaxFactory.SeparatedList<TupleElementSyntax>(
                    new SyntaxNodeOrToken[]
                    {
                        SyntaxFactory.TupleElement(
                            SyntaxFactory.GenericName("List")
                                .WithTypeArgumentList(
                                    SyntaxFactory.TypeArgumentList(
                                        SyntaxFactory.SingletonSeparatedList<TypeSyntax>(
                                            SyntaxFactory.IdentifierName("T")
                                        )
                                    )
                                )
                        ).WithIdentifier(SyntaxFactory.Identifier("paginatedList")),
                        SyntaxFactory.Token(SyntaxKind.CommaToken),
                        SyntaxFactory.TupleElement(
                            SyntaxFactory.IdentifierName("PaginationMetadata")
                        ).WithIdentifier(SyntaxFactory.Identifier("paginationMetadata"))
                    }
                )
            );

            var method = SyntaxFactory.MethodDeclaration(
                    SyntaxFactory.GenericName("Task")
                        .WithTypeArgumentList(
                            SyntaxFactory.TypeArgumentList(
                                SyntaxFactory.SingletonSeparatedList<TypeSyntax>(tupleReturnType)
                            )
                        ),
                    SyntaxFactory.Identifier("ApplyPaginationAsync")
                )
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.StaticKeyword), SyntaxFactory.Token(SyntaxKind.AsyncKeyword))
                .AddTypeParameterListParameters(SyntaxFactory.TypeParameter("T"))
                .AddParameterListParameters(
                    SyntaxFactory.Parameter(SyntaxFactory.Identifier("source"))
                        .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.ThisKeyword)))
                        .WithType(
                            SyntaxFactory.GenericName("IQueryable")
                                .WithTypeArgumentList(
                                    SyntaxFactory.TypeArgumentList(
                                        SyntaxFactory.SingletonSeparatedList<TypeSyntax>(
                                            SyntaxFactory.IdentifierName("T")
                                        )
                                    )
                                )
                        ),
                    SyntaxFactory.Parameter(SyntaxFactory.Identifier("options"))
                        .WithType(SyntaxFactory.IdentifierName("PaginationOptions"))
                )
                .WithBody(SyntaxFactory.Block(
                    SyntaxFactory.ParseStatement("var totalCount = source.Count();"),
                    SyntaxFactory.ParseStatement("var paginationInfo = new PaginationMetadata(totalCount, options.PageSize, options.PageToken);"),
                    SyntaxFactory.ParseStatement("var paginatedList = await source.Skip((options.PageToken - 1) * options.PageSize).Take(options.PageSize).ToListAsync();"),
                    SyntaxFactory.ParseStatement("return (paginatedList, paginationInfo);")
                ));

            // Class declaration
            var classDeclaration = SyntaxFactory.ClassDeclaration("PaginationExtensions")
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.StaticKeyword))
                .AddMembers(method);

            // Namespace declaration
            var namespaceDeclaration = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName("Common.Paginations.Extensions"))
                .AddMembers(classDeclaration);

            // Compilation unit
            var compilationUnit = SyntaxFactory.CompilationUnit()
                .AddUsings(usings)
                .AddMembers(namespaceDeclaration)
                .NormalizeWhitespace();

            // Return generated code as string
            return compilationUnit.ToFullString();
        }
    }
}
