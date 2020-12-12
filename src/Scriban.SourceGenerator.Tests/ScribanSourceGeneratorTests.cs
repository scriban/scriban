using Microsoft.CodeAnalysis;
using NUnit.Framework;
using System.Linq;

namespace Scriban.SourceGenerator.Tests
{
    public class ScribanSourceGeneratorTests : SourceGeneratorTest
    {
        [Test]
        public void NullTransform()
        {
            var compileResult = RunSourceGeneratorTest<ScribanSourceGenerator>();
            compileResult.AssertSuccess();
            var invokeResult = compileResult.Invoke();
            Assert.AreEqual("Hello, code gen!", invokeResult.Output);
        }

        [Test]
        public void BasicTemplate()
        {
            var result = RunSourceGeneratorTest<ScribanSourceGenerator>();
            result.AssertSuccess();
            var ir = result.Invoke();
            Assert.AreEqual("AB", ir.Output);
        }

        [Test]
        public void Error()
        {
            var result = RunSourceGeneratorTest<ScribanSourceGenerator>();
            // TODO: probably need some simple API for diagnostic assertions.
            Assert.True(result.SourceGeneratorDiagnostics.Any(d => d.Severity == DiagnosticSeverity.Error));
        }

        [Test]
        public void ExcessiveIteration()
        {
            var result = RunSourceGeneratorTest<ScribanSourceGenerator>();

            Assert.True(result.SourceGeneratorDiagnostics.Any(d => d.Severity == DiagnosticSeverity.Error));
        }
    }
}