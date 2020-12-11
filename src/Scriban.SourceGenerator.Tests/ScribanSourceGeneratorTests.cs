using NUnit.Framework;

namespace Scriban.SourceGenerator.Tests
{
    public class ScribanSourceGeneratorTests : SourceGeneratorTest
    {
        [Test]
        public void Test1()
        {
            RunSourceGeneratorTest<ScribanSourceGenerator>();
        }
    }
}