using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;

namespace EntityCore.Tools.Middlewares
{
    public class ExceptionHandlerMiddleware
    {
        public string GenerateExceptionHandlingMiddleware()
        {
            // Using directives
            var usingDirectives = new[]
            {
                SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Net")),
                SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Common")),
                SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Text.Json")),
            };

            // ExceptionHandlingMiddleware class
            var classDeclaration = SyntaxFactory.ClassDeclaration("ExceptionHandlingMiddleware")
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .AddMembers(
                    // Private readonly RequestDelegate _next;
                    SyntaxFactory.FieldDeclaration(
                        SyntaxFactory.VariableDeclaration(SyntaxFactory.ParseTypeName("RequestDelegate"))
                        .AddVariables(SyntaxFactory.VariableDeclarator("_next"))
                    ).AddModifiers(SyntaxFactory.Token(SyntaxKind.PrivateKeyword), SyntaxFactory.Token(SyntaxKind.ReadOnlyKeyword)),

                    // Constructor
                    SyntaxFactory.ConstructorDeclaration("ExceptionHandlingMiddleware")
                        .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                        .AddParameterListParameters(
                            SyntaxFactory.Parameter(SyntaxFactory.Identifier("next"))
                            .WithType(SyntaxFactory.ParseTypeName("RequestDelegate"))
                        )
                        .WithBody(SyntaxFactory.Block(
                            SyntaxFactory.ParseStatement("_next = next;")
                        )),

                    // InvokeAsync method
                    SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName("Task"), "InvokeAsync")
                        .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.AsyncKeyword))
                        .AddParameterListParameters(
                            SyntaxFactory.Parameter(SyntaxFactory.Identifier("context"))
                            .WithType(SyntaxFactory.ParseTypeName("HttpContext"))
                        )
                        .WithBody(SyntaxFactory.Block(
                            SyntaxFactory.TryStatement()
                            .WithBlock(SyntaxFactory.Block(
                                SyntaxFactory.ParseStatement("await _next(context);")
                            ))
                            .WithCatches(SyntaxFactory.List(new[]
                            {
                            CreateCatchClause("KeyNotFoundException", "HttpStatusCode.NotFound", "\"Resource not found\""),
                            CreateCatchClause("UnauthorizedAccessException", "HttpStatusCode.Unauthorized", "\"Unauthorized access\""),
                            CreateCatchClause("ArgumentException", "HttpStatusCode.BadRequest", "ex.Message"),
                            CreateCatchClause("Exception", "HttpStatusCode.InternalServerError", "ex.Message")
                            }))
                        )),

                    // HandleExceptionAsync method
                    SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName("Task"), "HandleExceptionAsync")
                        .AddModifiers(SyntaxFactory.Token(SyntaxKind.PrivateKeyword), SyntaxFactory.Token(SyntaxKind.AsyncKeyword))
                        .AddParameterListParameters(
                            SyntaxFactory.Parameter(SyntaxFactory.Identifier("context")).WithType(SyntaxFactory.ParseTypeName("HttpContext")),
                            SyntaxFactory.Parameter(SyntaxFactory.Identifier("statusCode")).WithType(SyntaxFactory.ParseTypeName("HttpStatusCode")),
                            SyntaxFactory.Parameter(SyntaxFactory.Identifier("errorMessage")).WithType(SyntaxFactory.ParseTypeName("string"))
                        )
                        .WithBody(SyntaxFactory.Block(
                            SyntaxFactory.ParseStatement("context.Response.ContentType = \"application/json\";"),
                            SyntaxFactory.ParseStatement("context.Response.StatusCode = (int)statusCode;"),
                            SyntaxFactory.ParseStatement("var result = Result.Fail(errorMessage);"),
                            SyntaxFactory.ParseStatement("var jsonResult = JsonSerializer.Serialize(result);"),
                            SyntaxFactory.ParseStatement("await context.Response.WriteAsync(jsonResult);")
                        ))
                );

            // Extension class for UseExceptionHandlingMiddleware
            var extensionClass = SyntaxFactory.ClassDeclaration("ExceptionHandlingMiddlewareExtensions")
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.StaticKeyword))
                .AddMembers(
                    SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName("IApplicationBuilder"), "UseExceptionHandlingMiddleware")
                        .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.StaticKeyword))
                        .AddParameterListParameters(
                            SyntaxFactory.Parameter(SyntaxFactory.Identifier("app"))
                            .WithType(SyntaxFactory.ParseTypeName("IApplicationBuilder"))
                            .AddModifiers(SyntaxFactory.Token(SyntaxKind.ThisKeyword))
                        )
                        .WithBody(SyntaxFactory.Block(
                            SyntaxFactory.ParseStatement("return app.UseMiddleware<ExceptionHandlingMiddleware>();")
                        ))
                );

            // Namespace
            var namespaceDeclaration = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName("Middlewares"))
                .AddMembers(classDeclaration, extensionClass);

            // Compilation unit
            var compilationUnit = SyntaxFactory.CompilationUnit()
                .AddUsings(usingDirectives)
                .AddMembers(namespaceDeclaration)
                .NormalizeWhitespace();

            // Kodni string formatida qaytarish
            return compilationUnit.ToFullString();
        }

        // Yordamchi metod: Catch clause yaratish
        private CatchClauseSyntax CreateCatchClause(string exceptionType, string statusCode, string errorMessage)
        {
            return SyntaxFactory.CatchClause()
                .WithDeclaration(SyntaxFactory.CatchDeclaration(SyntaxFactory.ParseTypeName(exceptionType))
                    .WithIdentifier(SyntaxFactory.Identifier("ex")))
                .WithBlock(SyntaxFactory.Block(
                    SyntaxFactory.ParseStatement($"await HandleExceptionAsync(context, {statusCode}, {errorMessage});")
                ));
        }
    }

}
