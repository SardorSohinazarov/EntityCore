using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EntityCore.Tools.Common
{
    /// <summary>
    /// Generic `Result` class generator
    /// </summary>
    public class Result
    {
        public string GenerateResultClasses()
        {
            // Namespace yaratish
            var namespaceDeclaration = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName("Common"))
                .NormalizeWhitespace();

            // Result klassini yaratish
            var resultClass = SyntaxFactory.ClassDeclaration("Result")
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .AddMembers(
                    // Constructor
                    SyntaxFactory.ConstructorDeclaration("Result")
                        .AddModifiers(SyntaxFactory.Token(SyntaxKind.ProtectedKeyword))
                        .WithBody(SyntaxFactory.Block()),

                    // Message property
                    SyntaxFactory.PropertyDeclaration(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.StringKeyword)), "Message")
                        .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                        .AddAccessorListAccessors(
                            SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                            SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                                .AddModifiers(SyntaxFactory.Token(SyntaxKind.ProtectedKeyword))
                                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
                        )
                        .AddAttributeLists(
                            SyntaxFactory.AttributeList(
                                SyntaxFactory.SingletonSeparatedList(
                                    SyntaxFactory.Attribute(
                                        SyntaxFactory.IdentifierName("JsonPropertyName"))
                                    .WithArgumentList(
                                        SyntaxFactory.AttributeArgumentList(
                                            SyntaxFactory.SingletonSeparatedList(
                                                SyntaxFactory.AttributeArgument(
                                                    SyntaxFactory.LiteralExpression(
                                                        SyntaxKind.StringLiteralExpression,
                                                        SyntaxFactory.Literal("message")
                                                    )
                                                )
                                            )
                                        )
                                    )
                                )
                            )
                        ),

                    // Succeeded property
                    SyntaxFactory.PropertyDeclaration(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.BoolKeyword)), "Succeeded")
                        .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                        .AddAccessorListAccessors(
                            SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                            SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                                .AddModifiers(SyntaxFactory.Token(SyntaxKind.ProtectedKeyword))
                                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
                        )
                        .AddAttributeLists(
                            SyntaxFactory.AttributeList(
                                SyntaxFactory.SingletonSeparatedList(
                                    SyntaxFactory.Attribute(
                                        SyntaxFactory.IdentifierName("JsonPropertyName"))
                                    .WithArgumentList(
                                        SyntaxFactory.AttributeArgumentList(
                                            SyntaxFactory.SingletonSeparatedList(
                                                SyntaxFactory.AttributeArgument(
                                                    SyntaxFactory.LiteralExpression(
                                                        SyntaxKind.StringLiteralExpression,
                                                        SyntaxFactory.Literal("succeeded")
                                                    )
                                                )
                                            )
                                        )
                                    )
                                )
                            )
                        ),

                    // Fail() methods
                    GenerateMethod("Fail", "Result", false),
                    GenerateMethod("Fail", "Result", false, true),
                    GenerateMethod("Success", "Result", true),
                    GenerateMethod("Success", "Result", true, true)
                );

            // Result<T> klassini yaratish
            var genericResultClass = SyntaxFactory.ClassDeclaration("Result")
                .AddTypeParameterListParameters(SyntaxFactory.TypeParameter("T"))
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .AddBaseListTypes(SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName("Result")))
                .AddMembers(
                    // Constructor
                    SyntaxFactory.ConstructorDeclaration("Result")
                        .AddModifiers(SyntaxFactory.Token(SyntaxKind.ProtectedKeyword))
                        .WithBody(SyntaxFactory.Block()),

                    // Data property
                    SyntaxFactory.PropertyDeclaration(SyntaxFactory.IdentifierName("T"), "Data")
                        .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                        .AddAccessorListAccessors(
                            SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                            SyntaxFactory.AccessorDeclaration(SyntaxKind.InitAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
                                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PrivateKeyword))
                                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
                        )
                        .AddAttributeLists(
                            SyntaxFactory.AttributeList(
                                SyntaxFactory.SingletonSeparatedList(
                                    SyntaxFactory.Attribute(
                                        SyntaxFactory.IdentifierName("JsonPropertyName"))
                                    .WithArgumentList(
                                        SyntaxFactory.AttributeArgumentList(
                                            SyntaxFactory.SingletonSeparatedList(
                                                SyntaxFactory.AttributeArgument(
                                                    SyntaxFactory.LiteralExpression(
                                                        SyntaxKind.StringLiteralExpression,
                                                        SyntaxFactory.Literal("data")
                                                    )
                                                )
                                            )
                                        )
                                    )
                                )
                            )
                        ),

                    // Fail() and Success() methods
                    GenerateMethod("Fail", "Result<T>", false, isGeneric: true),
                    GenerateMethod("Fail", "Result<T>", false, true, isGeneric: true),
                    GenerateMethod("Success", "Result<T>", true, isGeneric: true),
                    GenerateMethod("Success", "Result<T>", true, true, isGeneric: true),
                    GenerateMethod("Success", "Result<T>", true, hasMessage: false, isGeneric: true, hasData: true),
                    GenerateMethod("Success", "Result<T>", true, hasMessage: true, isGeneric: true, hasData: true)
                );

            // Namespacega klasslarni qo'shish
            namespaceDeclaration = namespaceDeclaration.AddMembers(resultClass, genericResultClass);

            // usings
            var usings = new[]
            {
                SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Text.Json.Serialization"))
            };

            var code = SyntaxFactory.CompilationUnit()
                .AddUsings(usings)
                .AddMembers(namespaceDeclaration)
                .NormalizeWhitespace()
                .ToFullString();

            return code;
        }

        public MethodDeclarationSyntax GenerateMethod(string methodName, string returnType, bool isSuccess, bool hasMessage = false, bool isGeneric = false, bool hasData = false)
        {
            var parameters = new List<ParameterSyntax>();

            if (hasData)
                parameters.Add(SyntaxFactory.Parameter(SyntaxFactory.Identifier("data")).WithType(SyntaxFactory.IdentifierName("T")));

            if (hasMessage)
                parameters.Add(SyntaxFactory.Parameter(SyntaxFactory.Identifier("message")).WithType(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.StringKeyword))));

            var expressions = new List<ExpressionSyntax>
            {
                SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                    SyntaxFactory.IdentifierName("Succeeded"),
                    SyntaxFactory.LiteralExpression(isSuccess ? SyntaxKind.TrueLiteralExpression : SyntaxKind.FalseLiteralExpression))
            };

            if (hasMessage)
            {
                expressions.Add(SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                    SyntaxFactory.IdentifierName("Message"),
                    SyntaxFactory.IdentifierName("message")));
            }

            if (hasData)
            {
                expressions.Add(SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                    SyntaxFactory.IdentifierName("Data"),
                    SyntaxFactory.IdentifierName("data")));
            }

            var returnExpression = SyntaxFactory.ObjectCreationExpression(SyntaxFactory.ParseTypeName(returnType))
                .WithInitializer(SyntaxFactory.InitializerExpression(SyntaxKind.ObjectInitializerExpression)
                    .AddExpressions(expressions.ToArray()));

            return SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName(returnType), methodName)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.StaticKeyword))
                .AddParameterListParameters(parameters.ToArray())
                .WithBody(SyntaxFactory.Block(SyntaxFactory.ReturnStatement(returnExpression)));
        }
    }
}
