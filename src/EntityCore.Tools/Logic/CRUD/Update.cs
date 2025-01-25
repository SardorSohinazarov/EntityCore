﻿using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using System.Reflection;

namespace EntityCore.Tools
{
    public partial class CodeGenerator
    {
        private static MethodDeclarationSyntax GenerateUpdateMethod(string entityName, PropertyInfo primaryKey)
        {
            return SyntaxFactory.MethodDeclaration(SyntaxFactory.GenericName(SyntaxFactory.Identifier("Task"))
                                .AddTypeArgumentListArguments(SyntaxFactory.ParseTypeName(entityName)), "UpdateAsync")
                                .AddModifiers(
                                    SyntaxFactory.Token(SyntaxKind.PublicKeyword), // public
                                    SyntaxFactory.Token(SyntaxKind.AsyncKeyword))  // async
                                .AddParameterListParameters(SyntaxFactory.Parameter(SyntaxFactory.Identifier("id"))
                                    .WithType(SyntaxFactory.ParseTypeName(primaryKey.PropertyType.ToCSharpTypeName())))
                                .AddParameterListParameters(SyntaxFactory.Parameter(SyntaxFactory.Identifier("entity"))
                                    .WithType(SyntaxFactory.ParseTypeName(entityName)))
                                .WithBody(SyntaxFactory.Block(
                                    SyntaxFactory.ParseStatement($"var entry = _applicationDbContext.Set<{entityName}>().Update(entity);"),
                                    SyntaxFactory.ParseStatement($"await _applicationDbContext.SaveChangesAsync();"),
                                    SyntaxFactory.ParseStatement($"return entry.Entity;")
                                ));
        }
    }
}