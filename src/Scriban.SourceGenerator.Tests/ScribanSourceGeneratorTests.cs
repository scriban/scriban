using NUnit.Framework;

namespace Scriban.SourceGenerator.Tests
{
    public class ScribanSourceGeneratorTests : SourceGeneratorTest
    {
        [Test]
        public void Test1()
        {
            var result = RunSourceGeneratorTest<ScribanSourceGenerator>();
            Assert.True(result.Diagnostics[0].Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error);
        }
    }
}