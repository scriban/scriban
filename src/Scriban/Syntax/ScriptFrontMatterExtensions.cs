// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

namespace Scriban.Syntax
{
    using System.Collections.Generic;
    using System.Linq;

    public static class ScriptFrontMatterExtensions
    {
        public static Dictionary<string, string> GetValues(this ScriptFrontMatter frontMatter) =>
            frontMatter.Statements.Statements.Cast<ScriptExpressionStatement>()
                       .Aggregate(new Dictionary<string, string>(), (dictionary, statement) =>
                           dictionary.Concat(statement.Expression switch
                           {
                               ScriptAssignExpression x => x.GetValues(),
                               ScriptNamedArgument x => x.GetValues(),
                               _ => new Dictionary<string, string>(),
                           }).ToDictionary(x => x.Key, x => x.Value));

        private static IEnumerable<KeyValuePair<string, string>> GetValues(this ScriptAssignExpression expression)
        {
            var key = ((ScriptVariable)expression.Target).Name;
            var value = ((ScriptLiteral)expression.Value).Value.ToString() ?? string.Empty;
            yield return new KeyValuePair<string, string>(key, value);
        }

        private static IEnumerable<KeyValuePair<string, string>> GetValues(this ScriptNamedArgument expression)
        {
            var key = expression.Name.Name;
            var value = ((ScriptLiteral)expression.Value).Value.ToString() ?? string.Empty;
            yield return new KeyValuePair<string, string>(key, value);
        }
    }
}