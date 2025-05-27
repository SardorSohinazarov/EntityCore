using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EntityCore.Tools
{
    /// <summary>
    /// Automapper Mapping profiler generator
    /// </summary>
    public class MappingProfile : Generator
    {
        public ClassDeclarationSyntax GenerateMappingProfile(Type entityType) 
            => GenerateMappingProfile(entityType.Name);

        public ClassDeclarationSyntax GenerateMappingProfile(string entityName)
        {
            var viewModel = FindExistingViewModelType(entityName);
            var creationDto = FindExistingCreationDtoType(entityName);
            var modificationDto = FindExistingModificationDtoType(entityName);

            if (viewModel == null && creationDto == null && modificationDto == null)
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
