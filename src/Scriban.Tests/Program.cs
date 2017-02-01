// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. See license.txt file in the project root for full license information.
using System;
using NUnit.Framework;
using Scriban.Parsing;
using Scriban.Runtime;

namespace Scriban.Tests
{
    class Program
    {
        static void Main(string[] args)
        {
            var lexer = new Lexer(@"Before{{ toto / = + - . <= < > >= ! && || | != == (){}[]
# Single line comment

## Multi line comment ##

## Multi line 
comment 2
##

a_b
_123
a123
""This is a string""
""This is a string with an escape \"" double quote and escape of \\ escape""

""This is a multiline
string
""
    
1235
1e5
1.
1.0
1.e1                                                                  
1.0e5
1.0e+5
1.0e-5

@}}
After {{ another block }}
{{}} 
");

            foreach (var token in lexer)
            {
                var text = token.GetText(lexer.Text);
                if (token.Type.HasText())
                {
                    Assert.AreEqual(token.Type.ToText(), text, $"Invalid text found for token [{token.Type}]");
                }
                Console.WriteLine($"{token} => {text}");
            }
        }
    }
}
