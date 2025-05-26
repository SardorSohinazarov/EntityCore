using AutoMapper;
using EntityCore.Tools.Common.Paginations.Models;
using EntityCore.Tools.Common.ServiceAttribute;
using EntityCore.Test.Entities; // For SimpleEntity, ComplexEntity, RelatedEntity
using EntityCore.Test.Mocks;   // For TestDbContext, ISimpleEntitysService, IComplexEntitysService
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using System.Reflection; // Added for Assembly.GetExecutingAssembly()
using System.IO;         // Added for Path and File operations
using TestApiNet8.Domain.Entities;   // For Student, Teacher
using DataTransferObjects.Students;   // For Student DTOs/ViewModels
using TestApiWithNet8;              // For TestApiNet8Db
// Note: IStudentsService (Services.Students) is in a separate mock file.

namespace EntityCore.Test.Services
{
    public partial class ServiceGenerationSimpleEntityTests // Made partial
    {
        [Fact]
        public void Generate_Service_For_SimpleEntity_Correctly()
        {
            var serviceGenerator = new EntityCore.Tools.Services.Service(typeof(SimpleEntity));
            string generatedCode = serviceGenerator.Generate(dbContextName: "TestDbContext");

            // Namespace
            Assert.Contains("namespace Services.SimpleEntitys", generatedCode);
            // Class Signature
            Assert.Contains("public class SimpleEntitysService : ISimpleEntitysService", generatedCode);
            // Attribute
            Assert.Contains("[ScopedService]", generatedCode);
            // Fields
            Assert.Contains("private readonly TestDbContext _testDbContext;", generatedCode);
            Assert.Contains("private readonly IMapper _mapper;", generatedCode);
            Assert.Contains("private readonly IHttpContextAccessor _httpContext;", generatedCode);
            // Constructor Signature
            Assert.Contains("public SimpleEntitysService(TestDbContext testDbContext, IMapper mapper, IHttpContextAccessor httpContext)", generatedCode);
            // Constructor Body
            Assert.Contains("_testDbContext = testDbContext;", generatedCode);
            Assert.Contains("_mapper = mapper;", generatedCode);
            Assert.Contains("_httpContext = httpContext;", generatedCode);
            // AddAsync Method
            Assert.Contains("public async Task<SimpleEntity> AddAsync(SimpleEntity simpleEntity)", generatedCode);
            Assert.Contains("var entity = _mapper.Map<SimpleEntity>(simpleEntity);", generatedCode);
            Assert.Contains("_testDbContext.Set<SimpleEntity>().AddAsync(entity);", generatedCode); 
            Assert.Contains("await _testDbContext.SaveChangesAsync();", generatedCode);
            Assert.Contains("return entry.Entity;", generatedCode);
            // GetAllAsync Method
            Assert.Contains("public async Task<List<SimpleEntity>> GetAllAsync()", generatedCode);
            Assert.Contains("await _testDbContext.Set<SimpleEntity>().ToListAsync();", generatedCode);
            Assert.Contains("return entities;", generatedCode);
            // FilterAsync Method
            Assert.Contains("public async Task<List<SimpleEntity>> FilterAsync(PaginationOptions filter)", generatedCode);
            Assert.Contains("_testDbContext.Set<SimpleEntity>().ApplyPagination(filter, httpContext).ToListAsync();", generatedCode);
            Assert.Contains("return entities;", generatedCode);
            // GetByIdAsync Method
            Assert.Contains("public async Task<SimpleEntity> GetByIdAsync(Guid id)", generatedCode);
            Assert.Contains("var entity = await _testDbContext.Set<SimpleEntity>().FirstOrDefaultAsync(x => x.Id == id);", generatedCode);
            Assert.Contains("return entity;", generatedCode);
            // UpdateAsync Method
            Assert.Contains("public async Task<SimpleEntity> UpdateAsync(Guid id, SimpleEntity simpleEntity)", generatedCode);
            Assert.Contains("var entity = await _testDbContext.Set<SimpleEntity>().FirstOrDefaultAsync(x => x.Id == id);", generatedCode);
            Assert.Contains("_mapper.Map(simpleEntity, entity);", generatedCode);
            Assert.Contains("_testDbContext.Set<SimpleEntity>().Update(entity);", generatedCode);
            Assert.Contains("return entry.Entity;", generatedCode);
            // DeleteAsync Method
            Assert.Contains("public async Task<SimpleEntity> DeleteAsync(Guid id)", generatedCode);
            Assert.Contains("var entity = await _testDbContext.Set<SimpleEntity>().FirstOrDefaultAsync(x => x.Id == id);", generatedCode);
            Assert.Contains("_testDbContext.Set<SimpleEntity>().Remove(entity);", generatedCode);
            Assert.Contains("return entry.Entity;", generatedCode);
            // Usings
            Assert.Contains("using AutoMapper;", generatedCode);
            Assert.Contains("using Microsoft.EntityFrameworkCore;", generatedCode);
            Assert.Contains("using EntityCore.Test.Entities;", generatedCode);
            Assert.Contains("using EntityCore.Test.Mocks;", generatedCode);
            Assert.Contains("using Common.Paginations.Models;", generatedCode);
            Assert.Contains("using Common.Paginations.Extensions;", generatedCode);
            Assert.Contains("using Common.ServiceAttribute;", generatedCode);
            Assert.Contains("using Microsoft.AspNetCore.Http;", generatedCode);
        }
    }

    public partial class ServiceGenerationSimpleEntityTests // Name matches to add method to same class
    {
        [Fact]
        public void Generate_Service_For_Student_Entity_With_DTOs_Correctly()
        {
            // Ensure dependent assemblies are loaded for AppDomain.CurrentDomain.GetAssemblies() to find types.
            var ensureInfrastructureLoaded = typeof(TestApiNet8Db).Assembly; 
            var ensureApplicationLoaded = typeof(StudentViewModel).Assembly; 
            var ensureDomainEntitiesLoaded = typeof(TestApiNet8.Domain.Entities.Teacher).Assembly; // Ensure Teacher is resolvable
            
            var serviceGenerator = new EntityCore.Tools.Services.Service(typeof(Student)); // Student is from TestApiNet8.Domain.Entities
            string generatedCode = serviceGenerator.Generate(dbContextName: "TestApiNet8Db");

            // Namespace
            Assert.Contains("namespace Services.Students", generatedCode);
            // Class Signature
            Assert.Contains("public class StudentsService : IStudentsService", generatedCode);
            // Attribute
            Assert.Contains("[ScopedService]", generatedCode);
            // Fields
            Assert.Contains("private readonly TestApiNet8Db _testApiNet8Db;", generatedCode);
            Assert.Contains("private readonly IMapper _mapper;", generatedCode);
            Assert.Contains("private readonly IHttpContextAccessor _httpContext;", generatedCode);
            // Constructor Signature
            Assert.Contains("public StudentsService(TestApiNet8Db testApiNet8Db, IMapper mapper, IHttpContextAccessor httpContext)", generatedCode);
            // Constructor Body
            Assert.Contains("_testApiNet8Db = testApiNet8Db;", generatedCode);
            Assert.Contains("_mapper = mapper;", generatedCode);
            Assert.Contains("_httpContext = httpContext;", generatedCode);
            // AddAsync Method
            Assert.Contains("public async Task<StudentViewModel> AddAsync(StudentCreationDto studentCreationDto)", generatedCode);
            Assert.Contains("var entity = _mapper.Map<Student>(studentCreationDto);", generatedCode);
            Assert.Contains("entity.Teachers = await _testApiNet8Db.Set<Teacher>().Where(x => studentCreationDto.TeachersIds.Contains(x.Id)).ToListAsync();", generatedCode);
            Assert.Contains("var entry = await _testApiNet8Db.Set<Student>().AddAsync(entity);", generatedCode);
            Assert.Contains("await _testApiNet8Db.SaveChangesAsync();", generatedCode);
            Assert.Contains("return _mapper.Map<StudentViewModel>(entry.Entity);", generatedCode);
            // GetAllAsync Method
            Assert.Contains("public async Task<List<StudentViewModel>> GetAllAsync()", generatedCode);
            Assert.Contains("var entities = await _testApiNet8Db.Set<Student>().ToListAsync();", generatedCode);
            Assert.Contains("return _mapper.Map<List<StudentViewModel>>(entities);", generatedCode);
            // FilterAsync Method
            Assert.Contains("public async Task<List<StudentViewModel>> FilterAsync(PaginationOptions filter)", generatedCode);
            Assert.Contains("var httpContext = _httpContext.HttpContext;", generatedCode);
            Assert.Contains("var entities = await _testApiNet8Db.Set<Student>().ApplyPagination(filter, httpContext).ToListAsync();", generatedCode);
            Assert.Contains("return _mapper.Map<List<StudentViewModel>>(entities);", generatedCode);
            // GetByIdAsync Method
            Assert.Contains("public async Task<StudentViewModel> GetByIdAsync(int id)", generatedCode);
            Assert.Contains("var entity = await _testApiNet8Db.Set<Student>().FirstOrDefaultAsync(x => x.Id == id);", generatedCode);
            Assert.Contains("if (entity == null) throw new InvalidOperationException($\"Student with Id {id} not found.\");", generatedCode);
            Assert.Contains("return _mapper.Map<StudentViewModel>(entity);", generatedCode);
            // UpdateAsync Method
            Assert.Contains("public async Task<StudentViewModel> UpdateAsync(int id, StudentModificationDto studentModificationDto)", generatedCode);
            Assert.Contains("var entity = await _testApiNet8Db.Set<Student>().FirstOrDefaultAsync(x => x.Id == id);", generatedCode);
            Assert.Contains("if (entity == null) throw new InvalidOperationException($\"Student with {id} not found.\");", generatedCode); 
            Assert.Contains("_mapper.Map(studentModificationDto, entity);", generatedCode);
            Assert.Contains("entity.Teachers = await _testApiNet8Db.Set<Teacher>().Where(x => studentModificationDto.TeachersIds.Contains(x.Id)).ToListAsync();", generatedCode);
            Assert.Contains("var entry = _testApiNet8Db.Set<Student>().Update(entity);", generatedCode);
            Assert.Contains("await _testApiNet8Db.SaveChangesAsync();", generatedCode);
            Assert.Contains("return _mapper.Map<StudentViewModel>(entry.Entity);", generatedCode);
            // DeleteAsync Method
            Assert.Contains("public async Task<StudentViewModel> DeleteAsync(int id)", generatedCode);
            Assert.Contains("var entity = await _testApiNet8Db.Set<Student>().FirstOrDefaultAsync(x => x.Id == id);", generatedCode);
            Assert.Contains("if (entity == null) throw new InvalidOperationException($\"Student with Id {id} not found.\");", generatedCode); 
            Assert.Contains("var entry = _testApiNet8Db.Set<Student>().Remove(entity);", generatedCode);
            Assert.Contains("await _testApiNet8Db.SaveChangesAsync();", generatedCode);
            Assert.Contains("return _mapper.Map<StudentViewModel>(entry.Entity);", generatedCode);
            // Usings
            Assert.Contains("using TestApiNet8.Domain.Entities;", generatedCode); 
            Assert.Contains("using DataTransferObjects.Students;", generatedCode); 
            Assert.Contains("using TestApiWithNet8;", generatedCode); 
            Assert.Contains("using AutoMapper;", generatedCode);
            Assert.Contains("using Microsoft.EntityFrameworkCore;", generatedCode);
            Assert.Contains("using Microsoft.AspNetCore.Http;", generatedCode);
            Assert.Contains("using Common.Paginations.Models;", generatedCode);
            Assert.Contains("using Common.Paginations.Extensions;", generatedCode);
            Assert.Contains("using Common.ServiceAttribute;", generatedCode);
        }
    }

    public partial class ServiceGenerationSimpleEntityTests // Name matches to add method to same class
    {
        [Fact]
        public void Generate_Service_For_ComplexEntity_Correctly()
        {
            // Ensure required assemblies are loaded for type discovery
            _ = typeof(EntityCore.Test.Entities.ComplexEntity).Assembly;
            _ = typeof(EntityCore.Test.Mocks.TestDbContext).Assembly;
            _ = typeof(EntityCore.Test.Entities.RelatedEntity).Assembly;


            var serviceGenerator = new EntityCore.Tools.Services.Service(typeof(ComplexEntity));
            string generatedCode = serviceGenerator.Generate(dbContextName: "TestDbContext");
            
            // Ensure these usings are present at the top of the file:
            // using System.Reflection;
            // using System.IO;

            string generatedCodeString = generatedCode ?? "GENERATION_FAILED_OR_NULL";

            // Attempt to write to src/EntityCore.Test/ path
            try
            {
                string assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                // Path from assembly output (e.g., src/EntityCore.Test/bin/Debug/netX.X) to src/EntityCore.Test/
                string targetDir = Path.Combine(assemblyDir, "..", "..", ".."); 
                string filePath = Path.GetFullPath(Path.Combine(targetDir, "complex_entity_generated_code.txt"));
                System.IO.File.WriteAllText(filePath, generatedCodeString + "_FullPath");
            }
            catch (System.Exception ex)
            {
                // Fallback: Write a file indicating the primary path failed, including the exception.
                System.IO.File.WriteAllText("complex_entity_path_write_error.txt", $"Failed to write to full path: {ex.Message} {ex.StackTrace}");
            }
            
            // Fallback: Write to a path relative to the execution directory (e.g., bin/Debug/netX.X)
            System.IO.File.WriteAllText("complex_entity_generated_code_local.txt", generatedCodeString + "_LocalPath");
            
            /*
            // Namespace
            Assert.Contains("namespace Services.ComplexEntitys", generatedCode);

            // Class Signature
            Assert.Contains("public class ComplexEntitysService : IComplexEntitysService", generatedCode);

            // Attribute
            Assert.Contains("[ScopedService]", generatedCode);

            // Fields
            Assert.Contains("private readonly TestDbContext _testDbContext;", generatedCode);
            Assert.Contains("private readonly IMapper _mapper;", generatedCode);
            Assert.Contains("private readonly IHttpContextAccessor _httpContext;", generatedCode);

            // Constructor Signature
            Assert.Contains("public ComplexEntitysService(TestDbContext testDbContext, IMapper mapper, IHttpContextAccessor httpContext)", generatedCode);

            // Constructor Body
            Assert.Contains("_testDbContext = testDbContext;", generatedCode);
            Assert.Contains("_mapper = mapper;", generatedCode);
            Assert.Contains("_httpContext = httpContext;", generatedCode);

            // AddAsync Method
            Assert.Contains("public async Task<ComplexEntity> AddAsync(ComplexEntity complexEntity)", generatedCode);
            Assert.Contains("var entity = _mapper.Map<ComplexEntity>(complexEntity);", generatedCode);
            // Assert NOT present: linking logic for collections via Ids properties
            Assert.DoesNotContain("RelatedCollectionIds", generatedCode);
            Assert.DoesNotContain("OptionalRelatedCollectionIds", generatedCode);
            Assert.DoesNotContain("entity.RequiredRelatedCollection = await _testDbContext.Set<RelatedEntity>()", generatedCode); // Example of what NOT to find
            Assert.Contains("var entry = await _testDbContext.Set<ComplexEntity>().AddAsync(entity);", generatedCode);
            Assert.Contains("await _testDbContext.SaveChangesAsync();", generatedCode);
            Assert.Contains("return entry.Entity;", generatedCode);

            // GetAllAsync Method
            Assert.Contains("public async Task<List<ComplexEntity>> GetAllAsync()", generatedCode);
            Assert.Contains("var entities = await _testDbContext.Set<ComplexEntity>().ToListAsync();", generatedCode);
            Assert.Contains("return entities;", generatedCode);

            // FilterAsync Method
            Assert.Contains("public async Task<List<ComplexEntity>> FilterAsync(PaginationOptions filter)", generatedCode);
            Assert.Contains("var httpContext = _httpContext.HttpContext;", generatedCode);
            Assert.Contains("var entities = await _testDbContext.Set<ComplexEntity>().ApplyPagination(filter, httpContext).ToListAsync();", generatedCode);
            Assert.Contains("return entities;", generatedCode);

            // GetByIdAsync Method (ComplexEntity PK is long)
            Assert.Contains("public async Task<ComplexEntity> GetByIdAsync(long id)", generatedCode);
            Assert.Contains("var entity = await _testDbContext.Set<ComplexEntity>().FirstOrDefaultAsync(x => x.Id == id);", generatedCode);
            Assert.Contains("if (entity == null) throw new InvalidOperationException($\"ComplexEntity with Id {id} not found.\");", generatedCode);
            Assert.Contains("return entity;", generatedCode);

            // UpdateAsync Method
            Assert.Contains("public async Task<ComplexEntity> UpdateAsync(long id, ComplexEntity complexEntity)", generatedCode);
            Assert.Contains("var entity = await _testDbContext.Set<ComplexEntity>().FirstOrDefaultAsync(x => x.Id == id);", generatedCode);
            Assert.Contains("if (entity == null) throw new InvalidOperationException($\"ComplexEntity with {id} not found.\");", generatedCode);
            Assert.Contains("_mapper.Map(complexEntity, entity);", generatedCode);
            // Assert NOT present: linking logic for collections via Ids properties
            Assert.DoesNotContain("RelatedCollectionIds", generatedCode); // Check again in update
            Assert.DoesNotContain("OptionalRelatedCollectionIds", generatedCode);
            Assert.DoesNotContain("entity.RequiredRelatedCollection = await _testDbContext.Set<RelatedEntity>()", generatedCode); // Example for update
            Assert.Contains("var entry = _testDbContext.Set<ComplexEntity>().Update(entity);", generatedCode);
            Assert.Contains("await _testDbContext.SaveChangesAsync();", generatedCode);
            Assert.Contains("return entry.Entity;", generatedCode);

            // DeleteAsync Method (ComplexEntity PK is long)
            Assert.Contains("public async Task<ComplexEntity> DeleteAsync(long id)", generatedCode);
            Assert.Contains("var entity = await _testDbContext.Set<ComplexEntity>().FirstOrDefaultAsync(x => x.Id == id);", generatedCode);
            Assert.Contains("if (entity == null) throw new InvalidOperationException($\"ComplexEntity with Id {id} not found.\");", generatedCode);
            Assert.Contains("var entry = _testDbContext.Set<ComplexEntity>().Remove(entity);", generatedCode);
            Assert.Contains("await _testDbContext.SaveChangesAsync();", generatedCode);
            Assert.Contains("return entry.Entity;", generatedCode);

            // Usings
            Assert.Contains("using EntityCore.Test.Entities;", generatedCode); // For ComplexEntity
            Assert.Contains("using EntityCore.Test.Mocks;", generatedCode);   // For TestDbContext
            Assert.Contains("using AutoMapper;", generatedCode);
            Assert.Contains("using Microsoft.EntityFrameworkCore;", generatedCode);
            Assert.Contains("using Microsoft.AspNetCore.Http;", generatedCode);
            Assert.Contains("using Common.Paginations.Models;", generatedCode);
            Assert.Contains("using Common.Paginations.Extensions;", generatedCode);
            Assert.Contains("using Common.ServiceAttribute;", generatedCode);
            */
            throw new System.InvalidOperationException("ComplexEntityFileWriteAttemptedSignal");
        }
    }
}
