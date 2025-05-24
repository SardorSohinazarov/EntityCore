using System.Text.RegularExpressions;

namespace EntityCore.Test.DataTransferObjects
{
    public class DtoGeneratorTest
    {
        protected void AssertPropertyExists(string generatedCode, string propertySignature)
        {
            // Normalize whitespace in the property signature and code for robust matching
            string normalizedSignature = Regex.Replace(propertySignature, @"\s+", " ").Trim();
            string normalizedCode = Regex.Replace(generatedCode, @"\s+", " ").Trim();
            Assert.True(normalizedCode.Contains(normalizedSignature), $"Expected property '{normalizedSignature}' not found in '{normalizedCode}'.");
        }

        protected void AssertPropertyDoesNotExist(string generatedCode, string propertyName)
        {
            // Checks if a property with the given name (and typical { get; set; } structure) exists.
            // This is a simplified check and might need adjustment if property structures vary significantly.
            string normalizedCode = Regex.Replace(generatedCode, @"\s+", " ").Trim();
            Assert.False(normalizedCode.Contains($"public {propertyName} {{ get; set; }}") || normalizedCode.Contains($" {propertyName} {{ get; set; }}"), $"Property '{propertyName}' should not exist but was found in '{normalizedCode}'.");
        }
    }
}
