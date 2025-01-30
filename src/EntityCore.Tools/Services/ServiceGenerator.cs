using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using Microsoft.CodeAnalysis;

namespace EntityCore.Tools
{
    public partial class Generator
    {
        private string GenerateServiceImplementationCode(Type entityType, string? dbContextName)
        {
            string entityName = entityType.Name;

            Type? dbContextType = null;

            if (dbContextName is not null)
            {
                dbContextType = _assembly.GetTypes()
                                        .FirstOrDefault(t => typeof(DbContext).IsAssignableFrom(t)
                                            && t.IsClass
                                            && !t.IsAbstract
                                            && t.Name == dbContextName
                                        );
            }
            else
            {
                var dbContextTypes = _assembly.GetTypes()
                                             .Where(t => typeof(DbContext).IsAssignableFrom(t)
                                                && t.IsClass
                                                && !t.IsAbstract
                                             );

                if (dbContextTypes.Count() == 1)
                    dbContextType = dbContextTypes.First();
                else if (dbContextTypes.Count() > 1)
                    throw new InvalidOperationException("Multiple DbContexts found in the specified assembly. Please choose DbContext name. ex: --context <DbContextName>");
            }

            if (dbContextType is null)
                throw new InvalidOperationException("DbContext not found in the specified assembly.");

            var primaryKey = FindKeyProperty(entityType);

            var classDeclaration = GetClassDeclaration(entityName, primaryKey, dbContextType);

            var namespaceDeclaration = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName($"Services.{entityName}s"))
                .AddMembers(classDeclaration)
                .AddMembers(GenerateMappingProfile(entityName));

            CompilationUnitSyntax syntaxTree = GenerateUsings(namespaceDeclaration, dbContextType, entityType);

            return syntaxTree.NormalizeWhitespace()
                .ToFullString();
        }

        private ClassDeclarationSyntax GetClassDeclaration(string entityName, PropertyInfo primaryKey, Type dbContextType)
        {
            var dbContextVariableName = dbContextType.Name.GenerateFieldNameWithUnderscore();

            var classDeclaration = SyntaxFactory.ClassDeclaration($"{entityName}sService")
                        .AddBaseListTypes(SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName($"I{entityName}sService")))
                        .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                        .AddMembers(
                            // fields
                            SyntaxFactory.FieldDeclaration(
                                SyntaxFactory.VariableDeclaration(
                                    SyntaxFactory.ParseTypeName(dbContextType.Name))
                                .AddVariables(SyntaxFactory.VariableDeclarator(dbContextVariableName)))
                                .AddModifiers(
                                    SyntaxFactory.Token(SyntaxKind.PrivateKeyword),   // private 
                                    SyntaxFactory.Token(SyntaxKind.ReadOnlyKeyword)), // readonly
                            SyntaxFactory.FieldDeclaration(
                                SyntaxFactory.VariableDeclaration(
                                    SyntaxFactory.ParseTypeName("IMapper"))
                                .AddVariables(SyntaxFactory.VariableDeclarator("_mapper")))
                                .AddModifiers(
                                    SyntaxFactory.Token(SyntaxKind.PrivateKeyword),   // private 
                                    SyntaxFactory.Token(SyntaxKind.ReadOnlyKeyword)), // readonly
                            // constructor
                            SyntaxFactory.ConstructorDeclaration($"{entityName}sService")
                                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                                .AddParameterListParameters(SyntaxFactory.Parameter(SyntaxFactory.Identifier(dbContextType.Name.GenerateFieldName()))
                                    .WithType(SyntaxFactory.ParseTypeName(dbContextType.Name)))
                                .AddParameterListParameters(SyntaxFactory.Parameter(SyntaxFactory.Identifier("mapper"))
                                    .WithType(SyntaxFactory.ParseTypeName("IMapper")))
                                .WithBody(SyntaxFactory.Block(
                                    SyntaxFactory.ParseStatement($"{dbContextVariableName} = {dbContextType.Name.GenerateFieldName()};"),
                                    SyntaxFactory.ParseStatement($"_mapper = mapper;")
                                )),
                            // methods
                            GenerateAddMethodImplementation(entityName, dbContextVariableName),
                            GenerateGetAllMethodImplementation(entityName, dbContextVariableName),
                            GenerateGetByIdMethodImplementation(entityName, dbContextVariableName, primaryKey),
                            GenerateUpdateMethodImplementation(entityName, dbContextVariableName, primaryKey),
                            GenerateDeleteMethodImplementation(entityName, dbContextVariableName, primaryKey)
                        );

            return classDeclaration;
        }

        private MethodDeclarationSyntax GenerateAddMethodImplementation(string entityName, string dbContextVariableName)
        {
            var creationDtoTypeName = GetCreationDtoTypeName(entityName);
            var parametrName = creationDtoTypeName.GenerateFieldName();
            var returnTypeName = GetReturnTypeName(entityName);

            return SyntaxFactory.MethodDeclaration(SyntaxFactory.GenericName(SyntaxFactory.Identifier("Task"))
                                .AddTypeArgumentListArguments(SyntaxFactory.ParseTypeName(returnTypeName)), "AddAsync")
                                .AddModifiers(
                                    SyntaxFactory.Token(SyntaxKind.PublicKeyword), // public
                                    SyntaxFactory.Token(SyntaxKind.AsyncKeyword))  // async
                                .AddParameterListParameters(SyntaxFactory.Parameter(SyntaxFactory.Identifier(parametrName))
                                    .WithType(SyntaxFactory.ParseTypeName(creationDtoTypeName)))
                                .WithBody(SyntaxFactory.Block(
                                    SyntaxFactory.ParseStatement($"var entity = _mapper.Map<{entityName}>({parametrName});"),
                                    SyntaxFactory.ParseStatement($"var entry = await {dbContextVariableName}.Set<{entityName}>().AddAsync(entity);"),
                                    SyntaxFactory.ParseStatement($"await {dbContextVariableName}.SaveChangesAsync();"),
                                    SyntaxFactory.ParseStatement($"return {GenerateReturn(entityName)};")
                                ));
        }

        private MethodDeclarationSyntax GenerateGetAllMethodImplementation(string entityName, string dbContextVariableName)
        {
            var returnTypeName = GetReturnTypeName(entityName);

            return SyntaxFactory.MethodDeclaration(SyntaxFactory.GenericName(SyntaxFactory.Identifier("Task"))
                                .AddTypeArgumentListArguments(SyntaxFactory.ParseTypeName($"List<{returnTypeName}>")), "GetAllAsync")
                                .AddModifiers(
                                    SyntaxFactory.Token(SyntaxKind.PublicKeyword), // public
                                    SyntaxFactory.Token(SyntaxKind.AsyncKeyword))  // async
                                .WithBody(SyntaxFactory.Block(
                                    SyntaxFactory.ParseStatement($"var entities = await {dbContextVariableName}.Set<{entityName}>().ToListAsync();"),
                                    SyntaxFactory.ParseStatement($"return {GenerateRuturnForGetAll(entityName)};")
                                ));
        }

        private MethodDeclarationSyntax GenerateGetByIdMethodImplementation(string entityName, string dbContextVariableName, PropertyInfo primaryKey)
        {
            var returnTypeName = GetReturnTypeName(entityName);

            return SyntaxFactory.MethodDeclaration(SyntaxFactory.GenericName(SyntaxFactory.Identifier("Task"))
                                .AddTypeArgumentListArguments(SyntaxFactory.ParseTypeName(returnTypeName)), "GetByIdAsync")
                                .AddModifiers(
                                    SyntaxFactory.Token(SyntaxKind.PublicKeyword), // public
                                    SyntaxFactory.Token(SyntaxKind.AsyncKeyword))  // async
                                .AddParameterListParameters(SyntaxFactory.Parameter(SyntaxFactory.Identifier("id"))
                                    .WithType(SyntaxFactory.ParseTypeName(primaryKey.PropertyType.ToCSharpTypeName())))
                                .WithBody(SyntaxFactory.Block(
                                    SyntaxFactory.ParseStatement($"var entity = await {dbContextVariableName}.Set<{entityName}>().FirstOrDefaultAsync(x => x.{primaryKey.Name} == id);"),
                                    SyntaxFactory.ParseStatement($"if (entity == null) throw new InvalidOperationException($\"{entityName} with Id {{id}} not found.\");"),
                                    SyntaxFactory.ParseStatement($"return {GenerateReturnForGet(entityName)};")
                                ));
        }

        private MethodDeclarationSyntax GenerateUpdateMethodImplementation(string entityName, string dbContextVariableName, PropertyInfo primaryKey)
        {
            var modificationDtoTypeName = GetModificationDtoTypeName(entityName);
            var parametrName = modificationDtoTypeName.GenerateFieldName();
            var returnTypeName = GetReturnTypeName(entityName);

            return SyntaxFactory.MethodDeclaration(SyntaxFactory.GenericName(SyntaxFactory.Identifier("Task"))
                                .AddTypeArgumentListArguments(SyntaxFactory.ParseTypeName(returnTypeName)), "UpdateAsync")
                                .AddModifiers(
                                    SyntaxFactory.Token(SyntaxKind.PublicKeyword), // public
                                    SyntaxFactory.Token(SyntaxKind.AsyncKeyword))  // async
                                .AddParameterListParameters(SyntaxFactory.Parameter(SyntaxFactory.Identifier("id"))
                                    .WithType(SyntaxFactory.ParseTypeName(primaryKey.PropertyType.ToCSharpTypeName())))
                                .AddParameterListParameters(SyntaxFactory.Parameter(SyntaxFactory.Identifier(parametrName))
                                    .WithType(SyntaxFactory.ParseTypeName(modificationDtoTypeName)))
                                .WithBody(SyntaxFactory.Block(
                                    SyntaxFactory.ParseStatement($"var entity = await {dbContextVariableName}.Set<{entityName}>().FirstOrDefaultAsync(x => x.Id == id);"),
                                    SyntaxFactory.ParseStatement($"if (entity == null) throw new InvalidOperationException($\"{entityName} with {{id}} not found.\");"),
                                    SyntaxFactory.ParseStatement($"_mapper.Map({parametrName}, entity);"),
                                    SyntaxFactory.ParseStatement($"var entry = {dbContextVariableName}.Set<{entityName}>().Update(entity);"),
                                    SyntaxFactory.ParseStatement($"await {dbContextVariableName}.SaveChangesAsync();"),
                                    SyntaxFactory.ParseStatement($"return {GenerateReturn(entityName)};")
                                ));
        }

        private MethodDeclarationSyntax GenerateDeleteMethodImplementation(string entityName, string dbContextVariableName, PropertyInfo primaryKey)
        {
            var returnTypeName = GetReturnTypeName(entityName);

            return SyntaxFactory.MethodDeclaration(SyntaxFactory.GenericName(SyntaxFactory.Identifier("Task"))
                                .AddTypeArgumentListArguments(SyntaxFactory.ParseTypeName(returnTypeName)), "DeleteAsync")
                                .AddModifiers(
                                    SyntaxFactory.Token(SyntaxKind.PublicKeyword), // public
                                    SyntaxFactory.Token(SyntaxKind.AsyncKeyword))  // async
                                .AddParameterListParameters(SyntaxFactory.Parameter(SyntaxFactory.Identifier("id"))
                                    .WithType(SyntaxFactory.ParseTypeName(primaryKey.PropertyType.ToCSharpTypeName())))
                                .WithBody(SyntaxFactory.Block(
                                    SyntaxFactory.ParseStatement($"var entity = await {dbContextVariableName}.Set<{entityName}>().FirstOrDefaultAsync(x => x.{primaryKey.Name} == id);"),
                                    SyntaxFactory.ParseStatement($"if (entity == null) throw new InvalidOperationException($\"{entityName} with {{id}} not found.\");"),
                                    SyntaxFactory.ParseStatement($"var entry = {dbContextVariableName}.Set<{entityName}>().Remove(entity);"),
                                    SyntaxFactory.ParseStatement($"await {dbContextVariableName}.SaveChangesAsync();"),
                                    SyntaxFactory.ParseStatement($"return {GenerateReturn(entityName)};")
                                ));
        }

        private string GenerateReturn(string entityName)
        {
            var viewModel = GetViewModel(entityName);

            if(viewModel is null)
                return "entry.Entity";

            return $"_mapper.Map<{viewModel.Name}>(entry.Entity)";
        }

        private string GenerateRuturnForGetAll(string entityName)
        {
            var viewModel = GetViewModel(entityName);
            if (viewModel is null)
                return "entities";
            return $"_mapper.Map<List<{viewModel.Name}>>(entities)";
        }

        private string GenerateReturnForGet(string entityName)
        {
            var viewModel = GetViewModel(entityName);
            if (viewModel is null)
                return "entity";
            return $"_mapper.Map<{viewModel.Name}>(entity)";
        }
    }
}