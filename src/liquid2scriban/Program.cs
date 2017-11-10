// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.IO;
using Scriban.Parsing;

namespace Scriban.Liquid2Scriban
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("liquid2scriban.exe [--relaxed-include] <files...> ");
                Console.WriteLine(" Converts input Liquid files to Scriban files. Write result to input file + `.sbnXXX` extension where XXX is the original extension");
                Console.WriteLine();
                Console.WriteLine("    <files> can be a wildcard **.htm (for recursive, or *.htm) for not recursive");
                Console.WriteLine();
                Console.WriteLine("    --relaxed-include         Parse liquid include parameter as a string without quotes");
                Environment.Exit(1);
                return;
            }

            var files = new List<string>();
            bool relaxedInclude = false;
            for (var i = 0; i < args.Length; i++)
            {
                var arg = args[i];
                if (arg == "--relaxed-include")
                {
                    relaxedInclude = true;
                }
                else if (arg.IndexOf("**") >= 0)
                {
                    var index = arg.IndexOf("**");
                    var previousdir = arg.Substring(0, index);
                    var pattern = arg.Substring(index + 1);
                    if (pattern.Contains("/") || pattern.Contains("\\"))
                    {
                        Console.WriteLine($"Error the pattern: `{pattern}` cannot contain /, \\");
                        Environment.Exit(1);
                        return;
                    }
                    var rootDir = Path.Combine(Environment.CurrentDirectory, previousdir);
                    foreach (var file in Directory.GetFiles(rootDir, pattern, SearchOption.AllDirectories))
                    {
                        files.Add(Path.Combine(rootDir, file));
                    }
                }
                else if (arg.IndexOf("*") >= 0)
                {
                    var index = arg.IndexOf("*");
                    var previousdir = arg.Substring(0, index);
                    var pattern = arg.Substring(index);
                    if (pattern.Contains("/") || pattern.Contains("\\"))
                    {
                        Console.WriteLine($"Error the pattern: `{pattern}` cannot contain /, \\");
                        Environment.Exit(1);
                        return;
                    }
                    var rootDir = Path.Combine(Environment.CurrentDirectory, previousdir);
                    foreach (var file in Directory.GetFiles(rootDir, pattern))
                    {
                        files.Add(Path.Combine(rootDir, file));
                    }
                }
                else
                {
                    files.Add(Path.Combine(Environment.CurrentDirectory, arg));
                }
            }

            foreach (var file in files)
            {
                var text = File.ReadAllText(file);

                var template = Template.ParseLiquid(text, file, lexerOptions: new LexerOptions() {Mode = ScriptMode.Liquid, KeepTrivia = true, EnableIncludeImplicitString = relaxedInclude });

                if (template.HasErrors)
                {
                    DumpMessages(template, "parsing the liquid template", file);
                }
                else
                {
                    var scriban = template.ToText();
                    var extension = Path.GetExtension(file);
                    if (!string.IsNullOrEmpty(extension))
                    {
                        extension = ".sbn" + extension.Substring(1);
                    }
                    else
                    {
                        extension = ".sbn";
                    }
                    var outputFile = Path.ChangeExtension(file, extension);
                    File.WriteAllText(outputFile, scriban);

                    var color = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"Scriban file generated: {outputFile}");
                    Console.ForegroundColor = color;

                    // Try to reparse the generated template to verify that we don't have any errors
                    var newTemplate = Template.Parse(scriban, outputFile);
                    if (newTemplate.HasErrors)
                    {
                        DumpMessages(template, "verifying the generated scriban template", outputFile);
                    }
                }
            }
        }

        private static void DumpMessages(Template template, string parsingContext, string file)
        {
            if (template.HasErrors)
            {
                Console.WriteLine($"Error while {parsingContext}: {file}");
                foreach (var message in template.Messages)
                {
                    var color = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("  " + message);
                    Console.ForegroundColor = color;
                }
            }
        }
    }
}
