using System.Text.RegularExpressions;

namespace EntityCore.Test.DataTransferObjects
{
    public class DtoGeneratorTest
    {
        protected void AssertPropertyExists(string generatedCode, string propertySignature)
        {
            string normalizedCode = Regex.Replace(generatedCode, @"\s+", " ").Trim();
            Assert.True(normalizedCode.Contains(propertySignature), $"Expected property '{propertySignature}' not found in '{normalizedCode}'.");
        }

        protected void AssertPropertyDoesNotExist(string generatedCode, string propertySignature)
        {
            string normalizedCode = Regex.Replace(generatedCode, @"\s+", " ").Trim();
            Assert.False(normalizedCode.Contains(propertySignature) || normalizedCode.Contains(propertySignature), $"Property '{propertySignature}' should not exist but was found in '{normalizedCode}'.");
        }

        protected void AssertNamespaceEqualTo(string generatedCode, string namespaceName)
        {
            string normalizedNamespace = Regex.Replace(namespaceName, @"\s+", " ").Trim();
            string normalizedCode = Regex.Replace(generatedCode, @"\s+", " ").Trim();
            Assert.True(normalizedCode.Contains($"namespace {normalizedNamespace}"), $"Expected namespace '{normalizedNamespace}' not found in '{normalizedCode}'.");
        }

        protected void AssertUsingDirectiveExists(string generatedCode, string usingDirective)
        {
            string normalizedUsing = Regex.Replace(usingDirective, @"\s+", " ").Trim();
            string normalizedCode = Regex.Replace(generatedCode, @"\s+", " ").Trim();
            Assert.True(normalizedCode.Contains($"using {normalizedUsing};"), $"Expected using directive '{normalizedUsing}' not found in '{normalizedCode}'.");
        }
    }
}
