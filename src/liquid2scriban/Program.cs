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
                Console.WriteLine("liquid2scriban.exe <files...> ");
                Console.WriteLine(" Converts input Liquid files to Scriban files. Write result to input file + `.sbnXXX` extension where XXX is the original extension");
                Console.WriteLine(" <files> can be a wildcard **.htm (for recursive, or *.htm) for not recursive");
                Environment.Exit(1);
                return;
            }

            var outputExtension = "scriban";

            var files = new List<string>();
            for (var i = 0; i < args.Length; i++)
            {
                var arg = args[i];
                if (arg.IndexOf("**") >= 0)
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

                var template = Template.ParseLiquid(text, file, lexerOptions: new LexerOptions() {Mode = ScriptMode.Liquid, KeepTrivia = true});

                if (template.HasErrors)
                {
                    Console.WriteLine($"Error while parsing the template: {file}");
                    foreach (var message in template.Messages)
                    {
                        var color = Console.ForegroundColor;
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("  " + message);
                        Console.ForegroundColor = color;
                    }
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
                }
            }
        }
    }
}
