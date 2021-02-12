using System;
using System.Threading.Tasks;
using NUnit.Framework;
using Scriban.Parsing;
using Scriban.Runtime;
using Scriban.Syntax;
using NSubstitute;

namespace Scriban.Tests
{
    public class ScriptRuntimeExceptionTests
    {
        [Test]
        public void TestInnerExceptionExists()
        {
            ScriptRuntimeException.EnableDisplayInnerException = true;
            SourceSpan testSourcespanObject = new SourceSpan("fileName", new TextPosition(0, 0, 0), new TextPosition(0, 0, 0));
            Exception exception = Substitute.For<Exception>();
            exception.StackTrace.Returns("TestStacTrace");
            exception.Message.Returns("Test RunTime message");

            ScriptRuntimeException testScriptruntimeObject = new ScriptRuntimeException(testSourcespanObject, "Any string", exception);
            
            Assert.True(testScriptruntimeObject.ToString().Contains("TestStacTrace"));
            Assert.True(testScriptruntimeObject.ToString().Contains("Test RunTime message"));
        }

        [Test]
        public void TestInnerExceptiondosentExists()
        {
            ScriptRuntimeException.EnableDisplayInnerException = true;
            SourceSpan testSourcespanObject = new SourceSpan("fileName", new TextPosition(0, 0, 0), new TextPosition(0, 0, 0));

            ScriptRuntimeException testScriptruntimeObject = new ScriptRuntimeException(testSourcespanObject, "Any string");

            Assert.AreEqual(testScriptruntimeObject.ToString(), testScriptruntimeObject.Message);
        }

        [Test]
        public void TestInnerExceptionDisabled()
        {
            ScriptRuntimeException.EnableDisplayInnerException = false;
            SourceSpan testSourcespanObject = new SourceSpan("fileName", new TextPosition(0, 0, 0), new TextPosition(0, 0, 0));
            Exception exception = Substitute.For<Exception>();
            exception.StackTrace.Returns("TestStacTrace");
            exception.Message.Returns("Test RunTime message");

            ScriptRuntimeException testScriptruntimeObject = new ScriptRuntimeException(testSourcespanObject, "Any string", exception);

            Assert.AreEqual(testScriptruntimeObject.ToString(), testScriptruntimeObject.Message);           
        }
    }
}
