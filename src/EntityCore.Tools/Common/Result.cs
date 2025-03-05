using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;

namespace EntityCore.Tools
{
    public partial class Generator
    {
        public string GenerateResultClasses(string namespaceName)
        {
            // Namespace yaratish
            var namespaceDeclaration = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName(namespaceName))
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
                        ),

                    // Succeeded property
                    SyntaxFactory.PropertyDeclaration(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.BoolKeyword)), "Succeeded")
                        .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                        .AddAccessorListAccessors(
                            SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                            SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                                .AddModifiers(SyntaxFactory.Token(SyntaxKind.ProtectedKeyword))
                                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
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
                            SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
                        )
                        .WithInitializer(SyntaxFactory.EqualsValueClause(SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression)))
                        .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),

                    // Fail() methods
                    GenerateMethod("Fail", "Result<T>", false, isGeneric: true),
                    GenerateMethod("Fail", "Result<T>", false, true, isGeneric: true),
                    GenerateMethod("Success", "Result<T>", true, isGeneric: true),
                    GenerateMethod("Success", "Result<T>", true, true, isGeneric: true)
                );

            // Namespacega klasslarni qo'shish
            namespaceDeclaration = namespaceDeclaration.AddMembers(resultClass, genericResultClass);

            // Kodni chiroyli qilish
            var code = namespaceDeclaration
                .NormalizeWhitespace()
                .ToFullString();

            return code;
        }

        public MethodDeclarationSyntax GenerateMethod(string methodName, string returnType, bool isSuccess, bool hasMessage = false, bool isGeneric = false)
        {
            var parameters = hasMessage
                ? new[] { SyntaxFactory.Parameter(SyntaxFactory.Identifier("message")).WithType(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.StringKeyword))) }
                : Array.Empty<ParameterSyntax>();

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

            var returnExpression = SyntaxFactory.ObjectCreationExpression(SyntaxFactory.ParseTypeName(returnType))
                .WithInitializer(SyntaxFactory.InitializerExpression(SyntaxKind.ObjectInitializerExpression)
                    .AddExpressions(expressions.ToArray()));

            return SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName(returnType), methodName)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.StaticKeyword))
                .AddParameterListParameters(parameters)
                .WithBody(SyntaxFactory.Block(SyntaxFactory.ReturnStatement(returnExpression)));
        }
    }
}
