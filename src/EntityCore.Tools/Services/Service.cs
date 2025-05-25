using EntityCore.Tools.Common.Paginations.Models;
using EntityCore.Tools.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using System.Collections;
using System.Reflection;

namespace EntityCore.Tools.Services
{
    public class Service : Generator
    {
        private readonly string _entityName;
        private readonly Type _entityType;
        private readonly PropertyInfo _primaryKey;
        public Service(Type entityType)
        {
            _entityType = entityType;
            _entityName = _entityType.Name;
            _primaryKey = entityType.FindPrimaryKeyProperty();
        }

        public string Generate(string dbContextName = null)
        {
            Type? dbContextType = null;

            if (dbContextName is not null)
            {
                dbContextType = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(assembly => assembly.GetTypes())
                    .FirstOrDefault(t => typeof(DbContext).IsAssignableFrom(t)
                        && t.IsClass
                        && !t.IsAbstract
                        && t.Name == dbContextName
                    );
            }
            else
            {
                var dbContextTypes = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(assembly => assembly.GetTypes())
                    .Where(t => typeof(DbContext).IsAssignableFrom(t)
                        && t != typeof(DbContext)
                        && t.IsClass
                        && !t.IsAbstract
                    );

                if (dbContextTypes.Count() == 1)
                    dbContextType = dbContextTypes.First();
                else if (dbContextTypes.Count() > 1)
                    throw new InvalidOperationException(
                        $"Multiple DbContexts({string.Join(", ", dbContextTypes.Select(x => x.Name))}) found in the specified assembly." +
                        $"\nPlease choose DbContext name. ex: --context <DbContextName>");
            }

            if (dbContextType is null)
                throw new InvalidOperationException("DbContext not found in the specified assembly.");

            var namespaceDeclaration = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName($"Services.{_entityName}s"));

            var classDeclaration = GetClassDeclaration(dbContextType);

            if (classDeclaration is not null)
                namespaceDeclaration = namespaceDeclaration.AddMembers(classDeclaration);

            var mappingProfile = new MappingProfile();
            var mappingProfileCode = mappingProfile.GenerateMappingProfile(_entityName);

            if (mappingProfileCode is not null)
                namespaceDeclaration = namespaceDeclaration.AddMembers(mappingProfileCode);

            var syntaxTree = GenerateUsings(namespaceDeclaration, dbContextType, _entityType);

            return syntaxTree
                .NormalizeWhitespace()
                .ToFullString();
        }

        private ClassDeclarationSyntax GetClassDeclaration(Type dbContextType)
        {
            var dbContextVariableName = dbContextType.Name.GenerateFieldNameWithUnderscore();

            var classDeclaration = SyntaxFactory.ClassDeclaration($"{_entityName}sService")
                        .AddBaseListTypes(SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName($"I{_entityName}sService")))
                        .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                        .AddAttribute("ScopedService")
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
                            SyntaxFactory.FieldDeclaration(
                                SyntaxFactory.VariableDeclaration(
                                    SyntaxFactory.ParseTypeName("IHttpContextAccessor"))
                                .AddVariables(SyntaxFactory.VariableDeclarator("_httpContext")))
                                .AddModifiers(
                                    SyntaxFactory.Token(SyntaxKind.PrivateKeyword),   // private 
                                    SyntaxFactory.Token(SyntaxKind.ReadOnlyKeyword)), // readonly
                                                                                      // constructor
                            SyntaxFactory.ConstructorDeclaration($"{_entityName}sService")
                                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                                .AddParameterListParameters(SyntaxFactory.Parameter(SyntaxFactory.Identifier(dbContextType.Name.GenerateFieldName()))
                                    .WithType(SyntaxFactory.ParseTypeName(dbContextType.Name)))
                                .AddParameterListParameters(SyntaxFactory.Parameter(SyntaxFactory.Identifier("mapper"))
                                    .WithType(SyntaxFactory.ParseTypeName("IMapper")))
                                .AddParameterListParameters(SyntaxFactory.Parameter(SyntaxFactory.Identifier("httpContext"))
                                    .WithType(SyntaxFactory.ParseTypeName("IHttpContextAccessor")))
                                .WithBody(SyntaxFactory.Block(
                                    SyntaxFactory.ParseStatement($"{dbContextVariableName} = {dbContextType.Name.GenerateFieldName()};"),
                                    SyntaxFactory.ParseStatement($"_mapper = mapper;"),
                                    SyntaxFactory.ParseStatement($"_httpContext = httpContext;")
                                )),
                            // methods
                            GenerateAddMethodImplementation(dbContextVariableName),
                            GenerateGetAllMethodImplementation(dbContextVariableName),
                            GenerateFilterMethodImplementation(dbContextVariableName),
                            GenerateGetByIdMethodImplementation(dbContextVariableName),
                            GenerateUpdateMethodImplementation(dbContextVariableName),
                            GenerateDeleteMethodImplementation(dbContextVariableName)
                        );

            return classDeclaration;
        }

        private MethodDeclarationSyntax GenerateAddMethodImplementation(string dbContextVariableName)
        {
            var creationDtoType = GetCreationDto(_entityName);
            var creationDtoTypeName = GetCreationDtoTypeName(_entityName);
            var parametrName = creationDtoTypeName.GenerateFieldName();
            var returnTypeName = GetReturnTypeName(_entityName);

            var collectionProperties = _entityType.GetProperties().Where(x => typeof(IEnumerable).IsAssignableFrom(x.PropertyType));
            var collectionPropertiesNames = collectionProperties.Select(x => $"{x.Name}Ids");
            var idsProperties = creationDtoType.GetProperties().Where(x => x.Name.EndsWith("Ids"));
            idsProperties = idsProperties.Where(x => collectionPropertiesNames.Contains(x.Name));
            collectionProperties = collectionProperties.Where(x => idsProperties.Any(y => y.Name == $"{x.Name}Ids")).ToList();

            var idsPropertiesStatements = collectionProperties.Select(x =>
                SyntaxFactory.ParseStatement(
                    $"entity.{x.Name} = await {dbContextVariableName}.Set<{GetCollectionElementType(x.PropertyType).Name}>()" +
                    $"      .Where(x => {parametrName}.{idsProperties.FirstOrDefault(y => $"{x.Name}Ids" == y.Name).Name}.Contains(x.{GetCollectionElementType(x.PropertyType).FindPrimaryKeyProperty().Name}))" +
                    $"      .ToListAsync();"));

            List<StatementSyntax> statementSyntaxes = [SyntaxFactory.ParseStatement($"var entity = _mapper.Map<{_entityName}>({parametrName});")];
            if(idsPropertiesStatements.Any())
                statementSyntaxes.AddRange(idsPropertiesStatements);
            statementSyntaxes.Add(SyntaxFactory.ParseStatement($"var entry = await {dbContextVariableName}.Set<{_entityName}>().AddAsync(entity);"));
            statementSyntaxes.Add(SyntaxFactory.ParseStatement($"await {dbContextVariableName}.SaveChangesAsync();"));
            statementSyntaxes.Add(SyntaxFactory.ParseStatement($"return {GenerateReturn()};"));

            return SyntaxFactory.MethodDeclaration(SyntaxFactory.GenericName(SyntaxFactory.Identifier("Task"))
                                .AddTypeArgumentListArguments(SyntaxFactory.ParseTypeName(returnTypeName)), "AddAsync")
                                .AddModifiers(
                                    SyntaxFactory.Token(SyntaxKind.PublicKeyword), // public
                                    SyntaxFactory.Token(SyntaxKind.AsyncKeyword))  // async
                                .AddParameterListParameters(SyntaxFactory.Parameter(SyntaxFactory.Identifier(parametrName))
                                    .WithType(SyntaxFactory.ParseTypeName(creationDtoTypeName)))
                                .WithBody(SyntaxFactory.Block(
                                    statementSyntaxes
                                ));
        }

        private MethodDeclarationSyntax GenerateGetAllMethodImplementation(string dbContextVariableName)
        {
            var returnTypeName = GetReturnTypeName(_entityName);

            return SyntaxFactory.MethodDeclaration(SyntaxFactory.GenericName(SyntaxFactory.Identifier("Task"))
                                .AddTypeArgumentListArguments(SyntaxFactory.ParseTypeName($"List<{returnTypeName}>")), "GetAllAsync")
                                .AddModifiers(
                                    SyntaxFactory.Token(SyntaxKind.PublicKeyword), // public
                                    SyntaxFactory.Token(SyntaxKind.AsyncKeyword))  // async
                                .WithBody(SyntaxFactory.Block(
                                    SyntaxFactory.ParseStatement($"var entities = await {dbContextVariableName}.Set<{_entityName}>().ToListAsync();"),
                                    SyntaxFactory.ParseStatement($"return {GenerateRuturnForGetAll()};")
                                ));
        }

        private MethodDeclarationSyntax GenerateFilterMethodImplementation(string dbContextVariableName)
        {
            var returnTypeName = GetReturnTypeName(_entityName);

            return SyntaxFactory.MethodDeclaration(SyntaxFactory.GenericName(SyntaxFactory.Identifier(nameof(Task)))
                                .AddTypeArgumentListArguments(SyntaxFactory.ParseTypeName($"List<{returnTypeName}>")), "FilterAsync")
                                .AddModifiers(
                                    SyntaxFactory.Token(SyntaxKind.PublicKeyword), // public
                                    SyntaxFactory.Token(SyntaxKind.AsyncKeyword))  // async
                                .AddParameterListParameters(SyntaxFactory.Parameter(SyntaxFactory.Identifier("filter"))
                                    .WithType(SyntaxFactory.ParseTypeName(typeof(PaginationOptions).Name)))
                                .WithBody(SyntaxFactory.Block(
                                    SyntaxFactory.ParseStatement($"var httpContext = _httpContext.HttpContext;"),
                                    SyntaxFactory.ParseStatement($"var entities = await {dbContextVariableName}.Set<{_entityName}>().ApplyPagination(filter,httpContext).ToListAsync();"),
                                    SyntaxFactory.ParseStatement($"return {GenerateRuturnForGetAll()};")
                                ));
        }

        private MethodDeclarationSyntax GenerateGetByIdMethodImplementation(string dbContextVariableName)
        {
            var returnTypeName = GetReturnTypeName(_entityName);

            return SyntaxFactory.MethodDeclaration(SyntaxFactory.GenericName(SyntaxFactory.Identifier("Task"))
                                .AddTypeArgumentListArguments(SyntaxFactory.ParseTypeName(returnTypeName)), "GetByIdAsync")
                                .AddModifiers(
                                    SyntaxFactory.Token(SyntaxKind.PublicKeyword), // public
                                    SyntaxFactory.Token(SyntaxKind.AsyncKeyword))  // async
                                .AddParameterListParameters(SyntaxFactory.Parameter(SyntaxFactory.Identifier("id"))
                                    .WithType(SyntaxFactory.ParseTypeName(_primaryKey.PropertyType.ToCSharpTypeName())))
                                .WithBody(SyntaxFactory.Block(
                                    SyntaxFactory.ParseStatement($"var entity = await {dbContextVariableName}.Set<{_entityName}>().FirstOrDefaultAsync(x => x.{_primaryKey.Name} == id);"),
                                    SyntaxFactory.ParseStatement($"if (entity == null) throw new InvalidOperationException($\"{_entityName} with Id {{id}} not found.\");"),
                                    SyntaxFactory.ParseStatement($"return {GenerateReturnForGet()};")
                                ));
        }

        private MethodDeclarationSyntax GenerateUpdateMethodImplementation(string dbContextVariableName)
        {
            var modificationDtoTypeName = GetModificationDtoTypeName(_entityName);
            var parametrName = modificationDtoTypeName.GenerateFieldName();
            var returnTypeName = GetReturnTypeName(_entityName);

            return SyntaxFactory.MethodDeclaration(SyntaxFactory.GenericName(SyntaxFactory.Identifier("Task"))
                                .AddTypeArgumentListArguments(SyntaxFactory.ParseTypeName(returnTypeName)), "UpdateAsync")
                                .AddModifiers(
                                    SyntaxFactory.Token(SyntaxKind.PublicKeyword), // public
                                    SyntaxFactory.Token(SyntaxKind.AsyncKeyword))  // async
                                .AddParameterListParameters(SyntaxFactory.Parameter(SyntaxFactory.Identifier("id"))
                                    .WithType(SyntaxFactory.ParseTypeName(_primaryKey.PropertyType.ToCSharpTypeName())))
                                .AddParameterListParameters(SyntaxFactory.Parameter(SyntaxFactory.Identifier(parametrName))
                                    .WithType(SyntaxFactory.ParseTypeName(modificationDtoTypeName)))
                                .WithBody(SyntaxFactory.Block(
                                    SyntaxFactory.ParseStatement($"var entity = await {dbContextVariableName}.Set<{_entityName}>().FirstOrDefaultAsync(x => x.{_primaryKey.Name} == id);"),
                                    SyntaxFactory.ParseStatement($"if (entity == null) throw new InvalidOperationException($\"{_entityName} with {{id}} not found.\");"),
                                    SyntaxFactory.ParseStatement($"_mapper.Map({parametrName}, entity);"),
                                    SyntaxFactory.ParseStatement($"var entry = {dbContextVariableName}.Set<{_entityName}>().Update(entity);"),
                                    SyntaxFactory.ParseStatement($"await {dbContextVariableName}.SaveChangesAsync();"),
                                    SyntaxFactory.ParseStatement($"return {GenerateReturn()};")
                                ));
        }

        private MethodDeclarationSyntax GenerateDeleteMethodImplementation(string dbContextVariableName)
        {
            var returnTypeName = GetReturnTypeName(_entityName);

            return SyntaxFactory.MethodDeclaration(SyntaxFactory.GenericName(SyntaxFactory.Identifier("Task"))
                                .AddTypeArgumentListArguments(SyntaxFactory.ParseTypeName(returnTypeName)), "DeleteAsync")
                                .AddModifiers(
                                    SyntaxFactory.Token(SyntaxKind.PublicKeyword), // public
                                    SyntaxFactory.Token(SyntaxKind.AsyncKeyword))  // async
                                .AddParameterListParameters(SyntaxFactory.Parameter(SyntaxFactory.Identifier("id"))
                                    .WithType(SyntaxFactory.ParseTypeName(_primaryKey.PropertyType.ToCSharpTypeName())))
                                .WithBody(SyntaxFactory.Block(
                                    SyntaxFactory.ParseStatement($"var entity = await {dbContextVariableName}.Set<{_entityName}>().FirstOrDefaultAsync(x => x.{_primaryKey.Name} == id);"),
                                    SyntaxFactory.ParseStatement($"if (entity == null) throw new InvalidOperationException($\"{_entityName} with {{id}} not found.\");"),
                                    SyntaxFactory.ParseStatement($"var entry = {dbContextVariableName}.Set<{_entityName}>().Remove(entity);"),
                                    SyntaxFactory.ParseStatement($"await {dbContextVariableName}.SaveChangesAsync();"),
                                    SyntaxFactory.ParseStatement($"return {GenerateReturn()};")
                                ));
        }

        private CompilationUnitSyntax GenerateUsings(
            NamespaceDeclarationSyntax namespaceDeclaration,
            Type? dbContextType,
            Type? entityType)
        {
            var usings = new List<string>
            {
                "AutoMapper",
                "Microsoft.EntityFrameworkCore",
                "Microsoft.AspNetCore.Http",
                "Common.Paginations.Models",
                "Common.Paginations.Extensions",
                "Common.ServiceAttribute"
            };

            var viewModelType = GetViewModel(entityType.Name);
            if (!string.IsNullOrEmpty(viewModelType?.Namespace))
                usings.Add(viewModelType.Namespace);

            var creationDtoType = GetCreationDto(entityType.Name);
            if (!string.IsNullOrEmpty(creationDtoType?.Namespace))
                usings.Add(creationDtoType.Namespace);

            var modificationDtoType = GetModificationDto(entityType.Name);
            if (!string.IsNullOrEmpty(modificationDtoType?.Namespace))
                usings.Add(modificationDtoType.Namespace);

            if (!string.IsNullOrEmpty(dbContextType?.Namespace))
                usings.Add(dbContextType.Namespace);

            if (!string.IsNullOrEmpty(entityType?.Namespace))
                usings.Add(entityType.Namespace);

            var syntaxTree = SyntaxFactory.CompilationUnit()
                .AddUsings(usings.Distinct().Select(u => SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(u))).ToArray())
                .AddMembers(namespaceDeclaration);

            return syntaxTree;
        }

        protected string GenerateReturn()
        {
            var viewModel = GetViewModel(_entityName);

            if (viewModel is null)
                return "entry.Entity";

            return $"_mapper.Map<{viewModel.Name}>(entry.Entity)";
        }

        protected string GenerateRuturnForGetAll()
        {
            var viewModel = GetViewModel(_entityName);
            if (viewModel is null)
                return "entities";
            return $"_mapper.Map<List<{viewModel.Name}>>(entities)";
        }

        protected string GenerateReturnForGet()
        {
            var viewModel = GetViewModel(_entityName);
            if (viewModel is null)
                return "entity";
            return $"_mapper.Map<{viewModel.Name}>(entity)";
        }

        private Type GetCollectionElementType(Type collectionType)
        {
            if (collectionType.IsArray)
                return collectionType.GetElementType();

            if (collectionType.IsGenericType)
                return collectionType.GetGenericArguments().First();

            // Fallback: check interfaces
            var enumerableInterface = collectionType.GetInterfaces()
                .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));

            if (enumerableInterface != null)
                return enumerableInterface.GetGenericArguments().First();

            return typeof(object); // unknown fallback
        }
    }
}