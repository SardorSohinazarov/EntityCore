using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using EntityCore.Tools.Extensions;

namespace EntityCore.Tools.Controllers
{
    public partial class Controller
    {
        /// <summary>
        /// Generate Controller by Service Type
        /// Should be able to write actions for any service methods
        /// </summary>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        private string GenerateControllerCodeWithService(Type serviceType)
        {
            var serviceName = serviceType.Name;
            // var primaryKey = serviceType.FindPrimaryKeyProperty(); // Unused variable

            var classDeclaration = GetControllerClassDeclaration();

            var namespaceDeclaration = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName("Controllers"))
                .AddMembers(classDeclaration);

            CompilationUnitSyntax syntaxTree = GenerateControllerUsings(namespaceDeclaration, serviceType);

            return syntaxTree
                .NormalizeWhitespace()
                .ToFullString();
        }
    }
}
