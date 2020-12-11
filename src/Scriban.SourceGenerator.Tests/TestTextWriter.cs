using System;
using System.IO;
using System.Text;
using System.Threading;

namespace Scriban.SourceGenerator.Tests
{
    // Can be used to capture Console.Out on a per-async-context basis
    // allows Console.Out calls to be used in unit test implementations,
    // where multiple tests might be running concurrently.
    sealed class TestTextWriter : TextWriter
    {
        static bool attachedToConsoleOut;
        static readonly object sync = new object();

        static void AttachToConsoleOut(TestTextWriter w)
        {
            if (!attachedToConsoleOut)
            {
                lock (sync)
                {
                    if (!attachedToConsoleOut)
                    {
                        Console.SetOut(w);
                        attachedToConsoleOut = true;
                    }
                }
            }
        }

        public static string GetOutput()
        {
            return localWriter.Value?.ToString();
        }

        static AsyncLocal<StringWriter> localWriter = new AsyncLocal<StringWriter>();

        public TestTextWriter()
        {
            localWriter.Value = new StringWriter();
            AttachToConsoleOut(this);
        }

        public override Encoding Encoding => Encoding.Unicode;

        public override void Write(char value)
        {
            localWriter.Value?.Write(value);
        }

        public override void Write(char[] buffer, int index, int count)
        {
            localWriter.Value?.Write(buffer, index, count);
        }

        public override string ToString()
        {
            return GetOutput();
        }
    }
}
