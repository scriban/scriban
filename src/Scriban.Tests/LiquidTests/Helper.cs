// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.

using System.Collections;
using Scriban.Runtime;
using Scriban.Runtime.Accessors;
using Scriban.Tests;

namespace DotLiquid.Tests.Tags
{
    public class Helper
    {
        public static void AssertTemplateResult(string result, string input, object model = null, bool hasForVariableNotSupportedByScriban = false)
        {
            TestParser.AssertTemplate(result, input, true, false, false, model, true, expectParsingErrorForRountrip: hasForVariableNotSupportedByScriban);
        }
    }

    public class Hash : ScriptObject
    {
        public static Hash FromAnonymousObject(object obj)
        {
            var hash = new Hash();
            hash.Import(obj);
            return hash;
        }

        public static Hash FromDictionary(object obj)
        {
            var hash = new Hash();
            hash.Import(obj);
           
            return hash;
        }
    }
}