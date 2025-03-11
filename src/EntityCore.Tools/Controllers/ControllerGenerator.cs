using EntityCore.Tools.Common.Paginations.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Reflection;

namespace EntityCore.Tools
{
    public partial class Generator
    {
        // TODO: Entity Typedan emas service typedan ko'rib generatsiya qilishi kerak
        private string GenerateControllerCode(Type entityType)
        {
            var entityName = entityType.Name;
            var primaryKey = FindKeyProperty(entityType);

            var classDeclaration = GetControllerClassDeclaration(entityName, primaryKey);

            var namespaceDeclaration = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName("Controllers"))
                .AddMembers(classDeclaration);

            CompilationUnitSyntax syntaxTree = GenerateControllerUsings(namespaceDeclaration, entityType);

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
                                .AddParameterListParameters(SyntaxFactory.Parameter(SyntaxFactory.Identifier($"{entityName}sService".GenerateFieldName()))
                                    .WithType(SyntaxFactory.ParseTypeName($"I{entityName}sService")))
                                .WithBody(SyntaxFactory.Block(
                                    SyntaxFactory.ParseStatement($"{serviceVariableName} = {$"{entityName}sService".GenerateFieldName()};")
                                )),
                            // methods
                            GenerateAddActionImplementation(entityName, serviceVariableName),
                            GenerateGetAllActionImplementation(entityName, serviceVariableName),
                            GenerateFilterActionImplementation(entityName, serviceVariableName),
                            GenerateGetByIdActionImplementation(entityName, serviceVariableName, primaryKey),
                            GenerateUpdateActionImplementation(entityName, serviceVariableName, primaryKey),
                            GenerateDeleteActionImplementation(entityName, serviceVariableName, primaryKey)
                        );

            return classDeclaration;
        }

        private MethodDeclarationSyntax GenerateAddActionImplementation(string entityName, string serviceVariableName)
        {
            var creationDtoTypeName = GetCreationDtoTypeName(entityName);
            var parametrName = creationDtoTypeName.GenerateFieldName();
            var returnTypeName = GetReturnTypeName(entityName);

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
                                .AddParameterListParameters(SyntaxFactory.Parameter(SyntaxFactory.Identifier(parametrName))
                                    .WithType(SyntaxFactory.ParseTypeName(creationDtoTypeName)))
                                .WithBody(SyntaxFactory.Block(
                                    SyntaxFactory.ParseStatement($"return Ok(await {serviceVariableName}.AddAsync({parametrName}));"))
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

        private MethodDeclarationSyntax GenerateFilterActionImplementation(string entityName, string serviceVariableName)
        {
            return SyntaxFactory.MethodDeclaration(SyntaxFactory.GenericName(SyntaxFactory.Identifier("Task"))
                                .AddTypeArgumentListArguments(SyntaxFactory.ParseTypeName("IActionResult")), "FilterAsync")
                                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword)) // public
                                .AddModifiers(SyntaxFactory.Token(SyntaxKind.AsyncKeyword))  // async
                                .AddAttributeLists(
                                    SyntaxFactory.AttributeList(
                                        SyntaxFactory.SingletonSeparatedList(
                                            SyntaxFactory.Attribute(SyntaxFactory.ParseName("HttpPost"))
                                                .WithArgumentList(
                                                        SyntaxFactory.AttributeArgumentList(
                                                            SyntaxFactory.SingletonSeparatedList(
                                                                SyntaxFactory.AttributeArgument(
                                                                    SyntaxFactory.LiteralExpression(
                                                                        SyntaxKind.StringLiteralExpression,
                                                                        SyntaxFactory.Literal("filter")
                                                                    )
                                                                )
                                                            )
                                                        )
                                                    )
                                        )
                                    )
                                )
                                .AddParameterListParameters(SyntaxFactory.Parameter(SyntaxFactory.Identifier("filter"))
                                   .WithType(SyntaxFactory.ParseTypeName(typeof(PaginationOptions).Name)))
                                .WithBody(SyntaxFactory.Block(
                                    SyntaxFactory.ParseStatement($"return Ok(await {serviceVariableName}.FilterAsync(filter));"))
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
            var modificationDtoTypeName = GetModificationDtoTypeName(entityName);
            var parametrName = modificationDtoTypeName.GenerateFieldName();
            var returnTypeName = GetReturnTypeName(entityName);

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
                                .AddParameterListParameters(SyntaxFactory.Parameter(SyntaxFactory.Identifier(parametrName))
                                    .WithType(SyntaxFactory.ParseTypeName(modificationDtoTypeName)))
                                .WithBody(SyntaxFactory.Block(
                                    SyntaxFactory.ParseStatement($"return Ok(await {serviceVariableName}.UpdateAsync(id,{parametrName}));")
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