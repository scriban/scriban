// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. See license.txt file in the project root for full license information.

using System;
using System.Collections;
using System.IO;
using System.Linq;

namespace Scriban.Tests
{
    static class TestFilesHelper
    {
        public const string InputFilePattern = "*.txt";
        public const string OutputEndFileExtension = ".out.txt";

        public static string BaseDirectory => Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "TestFiles"));

        public static IEnumerable ListTestFilesInFolder(string folder)
        {
            var baseDir = BaseDirectory;
            foreach (var file in
                Directory.GetFiles(Path.Combine(baseDir, folder), InputFilePattern, SearchOption.AllDirectories)
                    .Where(f => !f.EndsWith(OutputEndFileExtension))
                    .Select(f => f.StartsWith(baseDir) ? f.Substring(baseDir.Length + 1) : f)
                    .OrderBy(f => f))
            {
                var caseData = new TestCaseData(file);
                //var category = Path.GetDirectoryName(file);
                //caseData.TestName = category + "/" + Path.GetFileNameWithoutExtension(file);
                //caseData.SetCategory(category);
                yield return caseData;
            }
        }

        public static IEnumerable ListAllTestFiles()
        {
            var folders = new[]
            {
                "000-basic",
                "010-literals",
                "020-interpolation",
                "100-expressions",
                "200-statements",
                "300-functions",
                "400-builtins",
                "500-liquid"
            };

            foreach (var folder in folders)
            {
                foreach (var testCaseData in ListTestFilesInFolder(folder))
                {
                    yield return testCaseData;
                }
            }
        }

        public static string LoadTestFile(string inputName)
        {
            var baseDir = BaseDirectory;
            var inputFile = Path.Combine(baseDir, inputName);
            if (!File.Exists(inputFile))
                return null;
            var templateSource = File.ReadAllText(inputFile);
            return templateSource;
        }
    }
}
