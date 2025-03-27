using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using EntityCore.Tools.Extensions;

namespace EntityCore.Tools.Controllers
{
    public partial class Controller
    {
        /// <summary>
        /// Controllerni Service Type Orqali Generatsiya qilsin
        /// Har qanday service methodlari uchun action yozib berolsin
        /// </summary>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        private string GenerateControllerCodeWithService(Type serviceType)
        {
            var serviceName = serviceType.Name;
            var primaryKey = serviceType.FindPrimaryKeyProperty();

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
