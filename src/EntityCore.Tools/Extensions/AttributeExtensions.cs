using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EntityCore.Tools.Extensions
{
    public static class AttributeExtensions
    {
        /// <summary>
        /// Add attribute to property,
        /// ex : [JsonPropertyName("message")]
        /// ex : [Required("Name is required")]
        /// </summary>
        /// <param name="propertyDeclarationSyntax"></param>
        /// <param name="attribteName"></param>
        /// <param name="paramName"></param>
        /// <returns></returns>
        public static PropertyDeclarationSyntax AddAttribute(this PropertyDeclarationSyntax propertyDeclarationSyntax, string attributeName, string attributeParameter = null)
        {
            if (attributeParameter is null)
            {
                return propertyDeclarationSyntax.AddAttributeLists(
                                SyntaxFactory.AttributeList(
                                    SyntaxFactory.SingletonSeparatedList(
                                        SyntaxFactory.Attribute(
                                            SyntaxFactory.IdentifierName(attributeName))
                                    )
                                )
                            );
            }
            else
            {
                return propertyDeclarationSyntax.AddAttributeLists(
                                SyntaxFactory.AttributeList(
                                    SyntaxFactory.SingletonSeparatedList(
                                        SyntaxFactory.Attribute(
                                            SyntaxFactory.IdentifierName(attributeName))
                                        .WithArgumentList(
                                            SyntaxFactory.AttributeArgumentList(
                                                SyntaxFactory.SingletonSeparatedList(
                                                    SyntaxFactory.AttributeArgument(
                                                        SyntaxFactory.LiteralExpression(
                                                            SyntaxKind.StringLiteralExpression,
                                                            SyntaxFactory.Literal(attributeParameter)
                                                        )
                                                    )
                                                )
                                            )
                                        )
                                    )
                                )
                            );
            }
        }

        /// <summary>
        /// Add attribute to class,
        /// ex : [ApiController]
        /// ex : [Route("api/[controller]")]
        /// </summary>
        /// <param name="classDeclarationSyntax"></param>
        /// <param name="attributeName"></param>
        /// <param name="attributeParameterName"></param>
        /// <returns></returns>
        public static ClassDeclarationSyntax AddAttribute(this ClassDeclarationSyntax classDeclarationSyntax, string attributeName, string attributeParameter = null)
        {
            if (attributeParameter is null)
            {

                return classDeclarationSyntax.AddAttributeLists(
                            SyntaxFactory.AttributeList(
                                SyntaxFactory.SingletonSeparatedList(
                                    SyntaxFactory.Attribute(SyntaxFactory.ParseName(attributeName))
                                )
                            )
                        );
            }
            else
            {

                return classDeclarationSyntax.AddAttributeLists(
                            SyntaxFactory.AttributeList(
                                SyntaxFactory.SingletonSeparatedList(
                                    SyntaxFactory.Attribute(SyntaxFactory.ParseName(attributeName))
                                        .WithArgumentList(SyntaxFactory.AttributeArgumentList(
                                            SyntaxFactory.SingletonSeparatedList(
                                                SyntaxFactory.AttributeArgument(
                                                    SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(attributeParameter))
                                                )
                                            )
                                        )
                                    )
                                )
                            )
                        );
            }
        }

        /// <summary>
        /// Add attribute to method,
        /// ex : [HttpGet]
        /// ex : [HttpGet("{id}")]
        /// </summary>
        /// <param name="methodDeclarationSyntax"></param>
        /// <param name="attributeName"></param>
        /// <param name="attributeParameter"></param>
        /// <returns></returns>
        public static MethodDeclarationSyntax AddAttribute(this MethodDeclarationSyntax methodDeclarationSyntax, string attributeName, string attributeParameter = null)
        {
            if (attributeParameter is null)
            {
                return methodDeclarationSyntax.AddAttributeLists(
                                        SyntaxFactory.AttributeList(
                                            SyntaxFactory.SingletonSeparatedList(
                                                SyntaxFactory.Attribute(SyntaxFactory.ParseName(attributeName))
                                            )
                                        )
                                    );
            }
            else
            {
                return methodDeclarationSyntax.AddAttributeLists(
                                    SyntaxFactory.AttributeList(
                                        SyntaxFactory.SingletonSeparatedList(
                                            SyntaxFactory.Attribute(SyntaxFactory.ParseName(attributeName))
                                                .WithArgumentList(
                                                        SyntaxFactory.AttributeArgumentList(
                                                            SyntaxFactory.SingletonSeparatedList(
                                                                SyntaxFactory.AttributeArgument(
                                                                    SyntaxFactory.LiteralExpression(
                                                                        SyntaxKind.StringLiteralExpression,
                                                                        SyntaxFactory.Literal(attributeParameter)
                                                                    )
                                                                )
                                                            )
                                                        )
                                                    )
                                        )
                                    )
                                );
            }
        }
    }
}
