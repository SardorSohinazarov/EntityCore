﻿using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EntityCore.Tools
{
    public partial class CodeGenerator
    {
        private static MethodDeclarationSyntax GenerateAddMethod(string entityName)
        {
            return SyntaxFactory.MethodDeclaration(SyntaxFactory.GenericName(SyntaxFactory.Identifier("Task"))
                                .AddTypeArgumentListArguments(SyntaxFactory.ParseTypeName(entityName)), "AddAsync")
                                .AddModifiers(
                                    SyntaxFactory.Token(SyntaxKind.PublicKeyword), // public
                                    SyntaxFactory.Token(SyntaxKind.AsyncKeyword))  // async
                                .AddParameterListParameters(SyntaxFactory.Parameter(SyntaxFactory.Identifier("entity"))
                                    .WithType(SyntaxFactory.ParseTypeName(entityName)))
                                .WithBody(SyntaxFactory.Block(
                                    SyntaxFactory.ParseStatement($"var entry = await _applicationDbContext.Set<{entityName}>().AddAsync(entity);"),
                                    SyntaxFactory.ParseStatement($"await _applicationDbContext.SaveChangesAsync();"),
                                    SyntaxFactory.ParseStatement($"return entry.Entity;")
                                ));
        }
    }
}
