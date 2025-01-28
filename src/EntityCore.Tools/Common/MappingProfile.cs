using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;

namespace EntityCore.Tools
{
    public partial class Generator
    {
        private ClassDeclarationSyntax GenerateMappingProfile(string entityName)
        {
            /// <summary>
            /// AutoMapper mapping profile for {EntityName} entity.
            /// </summary>
            //Todo : Shunaqa xml comment o'shadigan qilish kerak

            var viewModel = GetViewModel(entityName);
            if (viewModel is null)
                return null;

            var mappingProfile = SyntaxFactory.ClassDeclaration($"{entityName}MappingProfile")
                        .AddBaseListTypes(SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName($"Profile")))
                        .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                        .AddMembers(
                            SyntaxFactory.ConstructorDeclaration($"{entityName}MappingProfile")
                                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                                .WithBody(SyntaxFactory.Block(
                                    //Todo :  agar view model null bo'lsa qo'shmasin
                                    SyntaxFactory.ParseStatement($"CreateMap<{entityName},{viewModel.Name}>();")
                                )));

            return mappingProfile;
        }
    }
}
