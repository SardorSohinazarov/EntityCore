using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;

namespace EntityCore.Tools
{
    public partial class Generator
    {
        private ClassDeclarationSyntax GenerateMappingProfile(string entityName)
        {
            var viewModel = GetViewModel(entityName);
            var creationDto = GetCreationDto(entityName);
            var modificationDto = GetModificationDto(entityName);

            if(viewModel == null && creationDto == null && modificationDto == null)
            {
                Console.WriteLine($"Data transfer objects not found for this entity -> {entityName}");
                return null;
            }

            var xmlComment = SyntaxFactory.TriviaList(
                SyntaxFactory.Comment("/// <summary>"),
                SyntaxFactory.Comment($"/// AutoMapper mapping profile for {entityName} entity."),
                SyntaxFactory.Comment("/// </summary>")
            );

            var constructorStatements = new List<StatementSyntax>();

            if (viewModel != null)
                constructorStatements.Add(SyntaxFactory.ParseStatement($"CreateMap<{entityName}, {viewModel.Name}>();"));

            if (creationDto != null)
                constructorStatements.Add(SyntaxFactory.ParseStatement($"CreateMap<{creationDto.Name}, {entityName}>();"));

            if (modificationDto != null)
                constructorStatements.Add(SyntaxFactory.ParseStatement($"CreateMap<{modificationDto.Name}, {entityName}>();"));

            var mappingProfile = SyntaxFactory.ClassDeclaration($"{entityName}MappingProfile")
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .WithLeadingTrivia(xmlComment)
                .AddBaseListTypes(SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName($"Profile")))
                .AddMembers(
                    SyntaxFactory.ConstructorDeclaration($"{entityName}MappingProfile")
                        .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                        .WithBody(SyntaxFactory.Block(constructorStatements))
                );

            return mappingProfile;
        }
    }
}
