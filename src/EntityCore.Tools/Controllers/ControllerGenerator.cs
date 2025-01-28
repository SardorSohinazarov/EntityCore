using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using System.Reflection;
using Microsoft.CodeAnalysis;

namespace EntityCore.Tools
{
    public partial class Generator
    {
        private string GenerateControllerCode(Assembly assembly, Type entityType)
        {
            var entityName = entityType.Name;
            var primaryKey = FindKeyProperty(entityType);

            var classDeclaration = GetControllerClassDeclaration(entityName, primaryKey);

            var namespaceDeclaration = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName("Controllers"))
                .AddMembers(classDeclaration);

            CompilationUnitSyntax syntaxTree = GenerateControllerUsings(assembly, namespaceDeclaration, entityType);

            return syntaxTree
                .NormalizeWhitespace()
                .ToFullString();
        }

        private ClassDeclarationSyntax GetControllerClassDeclaration(string entityName, PropertyInfo primaryKey)
        {
            var serviceVariableName = $"{entityName}sService".GenerateFieldNameWithUnderscore();
            var classDeclaration = SyntaxFactory.ClassDeclaration($"{entityName}sController")
                        .AddBaseListTypes(SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName($"ControllerBase")))
                        .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                        .AddAttributeLists(
                            SyntaxFactory.AttributeList(
                                SyntaxFactory.SingletonSeparatedList(
                                    SyntaxFactory.Attribute(SyntaxFactory.ParseName("Route"))
                                        .WithArgumentList(SyntaxFactory.AttributeArgumentList(
                                            SyntaxFactory.SingletonSeparatedList(
                                                SyntaxFactory.AttributeArgument(
                                                    SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal("api/[controller]"))
                                                )))))),

                            SyntaxFactory.AttributeList(
                                SyntaxFactory.SingletonSeparatedList(
                                    SyntaxFactory.Attribute(SyntaxFactory.ParseName("ApiController"))))
                        )
                        .AddMembers(
                            // fields
                            SyntaxFactory.FieldDeclaration(
                                SyntaxFactory.VariableDeclaration(
                                    SyntaxFactory.ParseTypeName($"I{entityName}sService"))
                                .AddVariables(SyntaxFactory.VariableDeclarator(serviceVariableName)))
                                .AddModifiers(
                                    SyntaxFactory.Token(SyntaxKind.PrivateKeyword),   // private 
                                    SyntaxFactory.Token(SyntaxKind.ReadOnlyKeyword)), // readonly
                                                                                      //constructors
                            SyntaxFactory.ConstructorDeclaration($"{entityName}sController")
                                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                                .AddParameterListParameters(SyntaxFactory.Parameter(SyntaxFactory.Identifier($"i{entityName}sService"))
                                    .WithType(SyntaxFactory.ParseTypeName($"I{entityName}sService")))
                                .WithBody(SyntaxFactory.Block(SyntaxFactory.SingletonList<StatementSyntax>(
                                    SyntaxFactory.ExpressionStatement(SyntaxFactory.ParseExpression($"{serviceVariableName} = i{entityName}sService"))
                                ))),
                            // methods
                            GenerateAddActionImplementation(entityName, serviceVariableName),
                            GenerateGetAllActionImplementation(entityName, serviceVariableName),
                            GenerateGetByIdActionImplementation(entityName, serviceVariableName, primaryKey),
                            GenerateUpdateActionImplementation(entityName, serviceVariableName, primaryKey),
                            GenerateDeleteActionImplementation(entityName, serviceVariableName, primaryKey)
                        );

            return classDeclaration;
        }

        private MethodDeclarationSyntax GenerateAddActionImplementation(string entityName, string serviceVariableName)
        {
            return SyntaxFactory.MethodDeclaration(SyntaxFactory.GenericName(SyntaxFactory.Identifier("Task"))
                                .AddTypeArgumentListArguments(SyntaxFactory.ParseTypeName("IActionResult")), "AddAsync")
                                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword)) // public
                                .AddModifiers(SyntaxFactory.Token(SyntaxKind.AsyncKeyword))  // async
                                .AddAttributeLists(
                                    SyntaxFactory.AttributeList(
                                        SyntaxFactory.SingletonSeparatedList(
                                            SyntaxFactory.Attribute(SyntaxFactory.ParseName("HttpPost"))
                                        )
                                    )
                                )
                                .AddParameterListParameters(SyntaxFactory.Parameter(SyntaxFactory.Identifier("entity"))
                                    .WithType(SyntaxFactory.ParseTypeName(entityName)))
                                .WithBody(SyntaxFactory.Block(
                                    SyntaxFactory.ParseStatement($"return Ok(await {serviceVariableName}.AddAsync(entity));"))
                                );
        }

        private MethodDeclarationSyntax GenerateGetAllActionImplementation(string entityName, string serviceVariableName)
        {
            return SyntaxFactory.MethodDeclaration(SyntaxFactory.GenericName(SyntaxFactory.Identifier("Task"))
                                .AddTypeArgumentListArguments(SyntaxFactory.ParseTypeName("IActionResult")), "GetAllAsync")
                                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword)) // public
                                .AddModifiers(SyntaxFactory.Token(SyntaxKind.AsyncKeyword))  // async
                                .AddAttributeLists(
                                    SyntaxFactory.AttributeList(
                                        SyntaxFactory.SingletonSeparatedList(
                                            SyntaxFactory.Attribute(SyntaxFactory.ParseName("HttpGet"))
                                        )
                                    )
                                )
                                .WithBody(SyntaxFactory.Block(
                                    SyntaxFactory.ParseStatement($"return Ok(await {serviceVariableName}.GetAllAsync());"))
                                );
        }

        private MethodDeclarationSyntax GenerateGetByIdActionImplementation(string entityName, string serviceVariableName, PropertyInfo primaryKey)
        {
            return SyntaxFactory.MethodDeclaration(SyntaxFactory.GenericName(SyntaxFactory.Identifier("Task"))
                               .AddTypeArgumentListArguments(SyntaxFactory.ParseTypeName("IActionResult")), "GetByIdAsync")
                               .AddModifiers(
                                   SyntaxFactory.Token(SyntaxKind.PublicKeyword), // public
                                   SyntaxFactory.Token(SyntaxKind.AsyncKeyword))  // async
                                .AddAttributeLists(
                                    SyntaxFactory.AttributeList(
                                        SyntaxFactory.SingletonSeparatedList(
                                            SyntaxFactory.Attribute(SyntaxFactory.ParseName("HttpGet"))
                                                .WithArgumentList(
                                                    SyntaxFactory.AttributeArgumentList(
                                                        SyntaxFactory.SingletonSeparatedList(
                                                            SyntaxFactory.AttributeArgument(
                                                                SyntaxFactory.LiteralExpression(
                                                                    SyntaxKind.StringLiteralExpression,
                                                                    SyntaxFactory.Literal("{id}")
                                                                )
                                                            )
                                                        )
                                                    )
                                                )
                                        )
                                    )
                                )
                               .AddParameterListParameters(SyntaxFactory.Parameter(SyntaxFactory.Identifier("id"))
                                   .WithType(SyntaxFactory.ParseTypeName(primaryKey.PropertyType.ToCSharpTypeName())))
                               .WithBody(SyntaxFactory.Block(
                                   SyntaxFactory.ParseStatement($"return Ok(await {serviceVariableName}.GetByIdAsync(id));")
                               ));
        }

        private MethodDeclarationSyntax GenerateUpdateActionImplementation(string entityName, string serviceVariableName, PropertyInfo primaryKey)
        {
            return SyntaxFactory.MethodDeclaration(SyntaxFactory.GenericName(SyntaxFactory.Identifier("Task"))
                                .AddTypeArgumentListArguments(SyntaxFactory.ParseTypeName("IActionResult")), "UpdateAsync")
                                .AddModifiers(
                                    SyntaxFactory.Token(SyntaxKind.PublicKeyword), // public
                                    SyntaxFactory.Token(SyntaxKind.AsyncKeyword))  // async
                                .AddAttributeLists(
                                    SyntaxFactory.AttributeList(
                                        SyntaxFactory.SingletonSeparatedList(
                                            SyntaxFactory.Attribute(SyntaxFactory.ParseName("HttpPut"))
                                                .WithArgumentList(
                                                    SyntaxFactory.AttributeArgumentList(
                                                        SyntaxFactory.SingletonSeparatedList(
                                                            SyntaxFactory.AttributeArgument(
                                                                SyntaxFactory.LiteralExpression(
                                                                    SyntaxKind.StringLiteralExpression,
                                                                    SyntaxFactory.Literal("{id}")
                                                                )
                                                            )
                                                        )
                                                    )
                                                )
                                        )
                                    )
                                )
                                .AddParameterListParameters(SyntaxFactory.Parameter(SyntaxFactory.Identifier("id"))
                                    .WithType(SyntaxFactory.ParseTypeName(primaryKey.PropertyType.ToCSharpTypeName())))
                                .AddParameterListParameters(SyntaxFactory.Parameter(SyntaxFactory.Identifier("entity"))
                                    .WithType(SyntaxFactory.ParseTypeName(entityName)))
                                .WithBody(SyntaxFactory.Block(
                                    SyntaxFactory.ParseStatement($"return Ok(await {serviceVariableName}.UpdateAsync(id,entity));")
                                ));
        }

        private MethodDeclarationSyntax GenerateDeleteActionImplementation(string entityName, string serviceVariableName, PropertyInfo primaryKey)
        {
            return SyntaxFactory.MethodDeclaration(SyntaxFactory.GenericName(SyntaxFactory.Identifier("Task"))
                                .AddTypeArgumentListArguments(SyntaxFactory.ParseTypeName("IActionResult")), "DeleteAsync")
                                .AddModifiers(
                                    SyntaxFactory.Token(SyntaxKind.PublicKeyword), // public
                                    SyntaxFactory.Token(SyntaxKind.AsyncKeyword))  // async
                                .AddAttributeLists(
                                    SyntaxFactory.AttributeList(
                                        SyntaxFactory.SingletonSeparatedList(
                                            SyntaxFactory.Attribute(SyntaxFactory.ParseName("HttpDelete"))
                                                .WithArgumentList(
                                                    SyntaxFactory.AttributeArgumentList(
                                                        SyntaxFactory.SingletonSeparatedList(
                                                            SyntaxFactory.AttributeArgument(
                                                                SyntaxFactory.LiteralExpression(
                                                                    SyntaxKind.StringLiteralExpression,
                                                                    SyntaxFactory.Literal("{id}")
                                                                )
                                                            )
                                                        )
                                                    )
                                                )
                                        )
                                    )
                                )
                                .AddParameterListParameters(SyntaxFactory.Parameter(SyntaxFactory.Identifier("id"))
                                .WithType(SyntaxFactory.ParseTypeName(primaryKey.PropertyType.ToCSharpTypeName())))
                                .WithBody(SyntaxFactory.Block(
                                    SyntaxFactory.ParseStatement($"return Ok(await {serviceVariableName}.DeleteAsync(id));")
                                ));
        }
    }
}