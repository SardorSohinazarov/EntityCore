using Microsoft.VisualStudio.TestTools.UnitTesting;
using EntityCore.Tools.DataTransferObjects;
using EntityCore.Tools.Tests.TestEntities;
using System;
using System.Text.RegularExpressions; // For cleaner assertions on generated code

namespace EntityCore.Tools.Tests
{
    [TestClass]
    public class DtoGenerationTests
    {
        private const string BaseNamespace = "TestApp.Application.Features";

        private void AssertPropertyExists(string generatedCode, string propertySignature)
        {
            // Normalize whitespace in the property signature and code for robust matching
            string normalizedSignature = Regex.Replace(propertySignature, @"\s+", " ").Trim();
            string normalizedCode = Regex.Replace(generatedCode, @"\s+", " ").Trim();
            Assert.IsTrue(normalizedCode.Contains(normalizedSignature), $"Expected property '{normalizedSignature}' not found in '{normalizedCode}'.");
        }

        private void AssertPropertyDoesNotExist(string generatedCode, string propertyName)
        {
            // Checks if a property with the given name (and typical { get; set; } structure) exists.
            // This is a simplified check and might need adjustment if property structures vary significantly.
            string normalizedCode = Regex.Replace(generatedCode, @"\s+", " ").Trim();
            Assert.IsFalse(normalizedCode.Contains($"public {propertyName} {{ get; set; }}") || normalizedCode.Contains($" {propertyName} {{ get; set; }}"), $"Property '{propertyName}' should not exist but was found in '{normalizedCode}'.");
        }

        // --- CreationDto Tests ---

        [TestMethod]
        public void Generate_SimpleEntityCreationDto_Correctly()
        {
            var creationDto = new CreationDto(typeof(SimpleEntity), BaseNamespace);
            string generatedCode = creationDto.Generate();

            StringAssert.Contains(generatedCode, $"namespace {BaseNamespace}.SimpleEntities;", "Namespace mismatch.");
            StringAssert.Contains(generatedCode, "public class SimpleEntityCreationDto", "Class name mismatch.");

            AssertPropertyDoesNotExist(generatedCode, "Guid Id"); // PK from BaseEntity
            
            // Properties from BaseEntity (excluding PK)
            AssertPropertyExists(generatedCode, "public DateTime CreatedAt { get; set; }");

            // Properties from SimpleEntity
            AssertPropertyExists(generatedCode, "public string Name { get; set; }");
            AssertPropertyExists(generatedCode, "public int Value { get; set; }");
            AssertPropertyExists(generatedCode, "public bool IsActive { get; set; }");
        }

        [TestMethod]
        public void Generate_MainTestEntityCreationDto_CorrectlyHandlesNavigation()
        {
            var creationDto = new CreationDto(typeof(MainTestEntity), BaseNamespace);
            string generatedCode = creationDto.Generate();

            StringAssert.Contains(generatedCode, $"namespace {BaseNamespace}.MainTestEntities;", "Namespace mismatch.");
            StringAssert.Contains(generatedCode, "public class MainTestEntityCreationDto", "Class name mismatch.");

            AssertPropertyDoesNotExist(generatedCode, "Guid Id"); // PK from BaseEntity

            // Properties from BaseEntity (excluding PK)
            AssertPropertyExists(generatedCode, "public DateTime CreatedAt { get; set; }");

            // Properties from MainTestEntity
            AssertPropertyExists(generatedCode, "public string MainProperty { get; set; }");

            // Navigation properties converted to Ids
            // Based on DtoPropertyGenerator logic: public {type.FindPrimaryKeyProperty().PropertyType.ToCSharpTypeName()} {property.Name}Id {{ get; set; }}
            // For OptionalRelated (RelatedEntity, PK Guid), it should be Guid?. OptionalRelatedId.
            // The DtoPropertyGenerator should handle the nullability of the FK based on the nullability of the navigation property itself.
            // However, the current DtoPropertyGenerator logic for single navigation properties is:
            // return $"public {type.FindPrimaryKeyProperty().PropertyType.ToCSharpTypeName()} {property.Name}Id {{ get; set; }}";
            // This does not automatically make it nullable.
            // Let's assume the current generator makes it non-nullable Guid if PK is Guid, and nullable Guid? if PK is Guid?
            // The PK of RelatedEntity (Id from BaseEntity) is Guid.
            // The property OptionalRelated is nullable. The FK OptionalRelatedId is Guid?.
            // The generator should ideally respect the FK definition if it exists and is properly named.
            // If the FK `OptionalRelatedId` (Guid?) is directly included, that's fine.
            // If `OptionalRelated` (Navigation) is processed, it becomes `OptionalRelatedId` (Guid).
            // Current DtoPropertyGenerator does not make it nullable based on navigation property nullability.
            
            AssertPropertyExists(generatedCode, "public Guid? OptionalRelatedId { get; set; }"); // FK field itself
            AssertPropertyExists(generatedCode, "public Guid RequiredRelatedId { get; set; }"); // FK field itself
            
            // DtoPropertyGenerator logic for collections: public List<{type.FindPrimaryKeyProperty().PropertyType.ToCSharpTypeName()}> {property.Name}Ids {{ get; set; }}
            AssertPropertyExists(generatedCode, "public List<Guid> RelatedCollectionIds { get; set; }");
            AssertPropertyExists(generatedCode, "public List<Guid> SimpleItemsIds { get; set; }");
        }

        // --- ModificationDto Tests ---

        [TestMethod]
        public void Generate_SimpleEntityModificationDto_Correctly()
        {
            var modificationDto = new ModificationDto(typeof(SimpleEntity), BaseNamespace);
            string generatedCode = modificationDto.Generate();

            StringAssert.Contains(generatedCode, $"namespace {BaseNamespace}.SimpleEntities;", "Namespace mismatch.");
            StringAssert.Contains(generatedCode, "public class SimpleEntityModificationDto", "Class name mismatch.");
            
            AssertPropertyDoesNotExist(generatedCode, "Guid Id"); // PK

            AssertPropertyExists(generatedCode, "public DateTime CreatedAt { get; set; }");
            AssertPropertyExists(generatedCode, "public string Name { get; set; }");
            AssertPropertyExists(generatedCode, "public int Value { get; set; }");
            AssertPropertyExists(generatedCode, "public bool IsActive { get; set; }");
        }

        [TestMethod]
        public void Generate_MainTestEntityModificationDto_CorrectlyHandlesNavigation()
        {
            var modificationDto = new ModificationDto(typeof(MainTestEntity), BaseNamespace);
            string generatedCode = modificationDto.Generate();

            StringAssert.Contains(generatedCode, $"namespace {BaseNamespace}.MainTestEntities;", "Namespace mismatch.");
            StringAssert.Contains(generatedCode, "public class MainTestEntityModificationDto", "Class name mismatch.");

            AssertPropertyDoesNotExist(generatedCode, "Guid Id"); // PK

            AssertPropertyExists(generatedCode, "public DateTime CreatedAt { get; set; }");
            AssertPropertyExists(generatedCode, "public string MainProperty { get; set; }");

            AssertPropertyExists(generatedCode, "public Guid? OptionalRelatedId { get; set; }");
            AssertPropertyExists(generatedCode, "public Guid RequiredRelatedId { get; set; }");
            
            AssertPropertyExists(generatedCode, "public List<Guid> RelatedCollectionIds { get; set; }");
            AssertPropertyExists(generatedCode, "public List<Guid> SimpleItemsIds { get; set; }");
        }

        // --- ViewModel Tests ---

        [TestMethod]
        public void Generate_SimpleEntityViewModel_Correctly()
        {
            var viewModel = new ViewModel(typeof(SimpleEntity), BaseNamespace);
            string generatedCode = viewModel.Generate();

            StringAssert.Contains(generatedCode, $"namespace {BaseNamespace}.SimpleEntities;", "Namespace mismatch.");
            StringAssert.Contains(generatedCode, "public class SimpleEntityViewModel", "Class name mismatch.");

            // PK IS present for ViewModels
            AssertPropertyExists(generatedCode, "public Guid Id { get; set; }");
            AssertPropertyExists(generatedCode, "public DateTime CreatedAt { get; set; }");
            AssertPropertyExists(generatedCode, "public string Name { get; set; }");
            AssertPropertyExists(generatedCode, "public int Value { get; set; }");
            AssertPropertyExists(generatedCode, "public bool IsActive { get; set; }");

            // Check for using statement for the entity's namespace
            StringAssert.Contains(generatedCode, "using EntityCore.Tools.Tests.TestEntities;", "Entity namespace using statement missing.");
        }

        [TestMethod]
        public void Generate_MainTestEntityViewModel_CorrectlyHandlesNavigation()
        {
            var viewModel = new ViewModel(typeof(MainTestEntity), BaseNamespace);
            string generatedCode = viewModel.Generate();

            StringAssert.Contains(generatedCode, $"namespace {BaseNamespace}.MainTestEntities;", "Namespace mismatch.");
            StringAssert.Contains(generatedCode, "public class MainTestEntityViewModel", "Class name mismatch.");

            // PK IS present for ViewModels
            AssertPropertyExists(generatedCode, "public Guid Id { get; set; }");
            AssertPropertyExists(generatedCode, "public DateTime CreatedAt { get; set; }");
            AssertPropertyExists(generatedCode, "public string MainProperty { get; set; }");

            // Foreign Key properties should be present
            AssertPropertyExists(generatedCode, "public Guid? OptionalRelatedId { get; set; }");
            AssertPropertyExists(generatedCode, "public Guid RequiredRelatedId { get; set; }");

            // Navigation properties should be present as their actual types
            // For nullable reference types, C# 8.0+ with <Nullable>enable</Nullable> would expect RelatedEntity?
            // The current ViewModel generator uses `type.Name` which doesn't add the '?' for reference types.
            // We will test for what it currently generates.
            AssertPropertyExists(generatedCode, "public RelatedEntity OptionalRelated { get; set; }"); // Assuming current generator doesn't add '?'
            AssertPropertyExists(generatedCode, "public RelatedEntity RequiredRelated { get; set; }");
            
            AssertPropertyExists(generatedCode, "public List<RelatedEntity> RelatedCollection { get; set; }");
            AssertPropertyExists(generatedCode, "public List<SimpleEntity> SimpleItems { get; set; }");
        }

        [TestMethod]
        public void Generate_MainTestEntityViewModel_IncludesNecessaryUsings()
        {
            var viewModel = new ViewModel(typeof(MainTestEntity), BaseNamespace);
            string generatedCode = viewModel.Generate();

            StringAssert.Contains(generatedCode, "using System;", "System namespace using statement missing.");
            StringAssert.Contains(generatedCode, "using System.Collections.Generic;", "System.Collections.Generic using statement missing.");
            StringAssert.Contains(generatedCode, "using EntityCore.Tools.Tests.TestEntities;", "Entity namespace using statement missing for MainTestEntity, RelatedEntity, SimpleEntity.");
        }
    }
}
