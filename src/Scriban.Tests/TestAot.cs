// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. See license.txt file in the project root for full license information.

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Scriban.Tests
{
    [TestFixture]
    public class TestAot
    {
        [Test]
        public void AotPublishProducesNoWarnings()
        {
            // Find the Scriban.Tests.Aot project relative to the test assembly location
            var testDir = TestContext.CurrentContext.TestDirectory;
            var srcDir = Path.GetFullPath(Path.Combine(testDir, "..", "..", "..", ".."));
            var aotProjectDir = Path.Combine(srcDir, "Scriban.Tests.Aot");
            var aotProjectFile = Path.Combine(aotProjectDir, "Scriban.Tests.Aot.csproj");

            if (!File.Exists(aotProjectFile))
            {
                Assert.Ignore($"Scriban.Tests.Aot project not found at {aotProjectFile}");
                return;
            }

            // Run dotnet publish with AOT — TreatWarningsAsErrors is enabled in the project
            var psi = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"publish \"{aotProjectFile}\" -c Release",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            var output = new StringBuilder();
            var errors = new StringBuilder();

            using var process = Process.Start(psi);
            process.OutputDataReceived += (_, e) => { if (e.Data != null) output.AppendLine(e.Data); };
            process.ErrorDataReceived += (_, e) => { if (e.Data != null) errors.AppendLine(e.Data); };
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            var exited = process.WaitForExit(5 * 60 * 1000); // 5 minute timeout
            if (!exited)
            {
                process.Kill();
                Assert.Fail("dotnet publish timed out after 5 minutes");
            }

            var allOutput = output.ToString() + errors.ToString();

            // Check for any IL trimming/AOT warnings in the output
            var warningLines = allOutput
                .Split('\n')
                .Where(line => line.Contains("warning IL") || line.Contains("warning AOT"))
                .ToArray();

            if (warningLines.Length > 0)
            {
                Assert.Fail($"AOT publish produced {warningLines.Length} warning(s):\n{string.Join("\n", warningLines)}");
            }

            Assert.That(process.ExitCode, Is.EqualTo(0), $"dotnet publish failed:\n{allOutput}");
        }
    }
}
