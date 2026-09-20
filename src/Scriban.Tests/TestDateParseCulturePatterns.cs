// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using NUnit.Framework;

namespace Scriban.Tests;

public class TestDateParseCulturePatterns
{
    [TestCase("%g%x %X", "01/05/2016 21:22:23", "fr-FR")]
    [TestCase("%x %g%X", "01/05/2016 21:22:23", "fr-FR")]
    [TestCase("%x %X%g", "01/05/2016 21:22:23", "fr-FR")]
    [TestCase("%x %X", "05/01/2016 21:22:23", "fr-FR")]
    [TestCase("%x %X", "05.01.2016 21:22:23", "de-DE")]
    public void ParseCombinedStandardPatterns(string pattern, string text, string culture)
    {
        TestParser.AssertTemplate("2016-01-05 21:22:23",
            "{{ date.parse '" + text + "' '" + pattern + "' culture:'" + culture + "' | date.to_string '%Y-%m-%d %H:%M:%S' }}");
    }
}
