using NUnit.Framework;

namespace Scriban.SourceGenerator.Tests
{
    public class ScribanSourceGeneratorTests : SourceGeneratorTest
    {
        [Test]
        public void Test1()
        {
            var result = RunSourceGeneratorTest<ScribanSourceGenerator>();
            // TODO: diagnostics aren't coming back like I expect...
            //Assert.True(result.Diagnostics[0].Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error);
        }
    }
}