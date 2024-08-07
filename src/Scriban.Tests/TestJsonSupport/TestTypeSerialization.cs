// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using Newtonsoft.Json.Linq;
using System;
using System.Globalization;
using System.Text.Json;

namespace Scriban.Tests.TestJsonSupport;

[TestFixture]
public class TestTypeSerialization {
    public static object[][] TestValues = [
        [(DateTime) new DateTime(2024, 08, 07, 15, 27, 0), "\"2024-08-07T15:27:00\""],
        [(TimeSpan) TimeSpan.FromTicks(123434563), "\"00:00:12.3434563\""],
        [(DateOnly) DateOnly.FromDateTime(new DateTime(2024, 08, 07)), "\"2024-08-07\""],
        [(DateTimeOffset) new DateTimeOffset(2024, 08, 07, 15, 27, 0, TimeSpan.FromHours(2)), "\"2024-08-07T15:27:00+02:00\""],
        [(byte) 255, "255"],
        [(char) 'R', "\"R\""],
        [(decimal) 99.99m, "99.99"],
        [(double) 99.99, "99.99"],
        [(float) 99.99f, "99.99"],
        [(Enum) StringSplitOptions.RemoveEmptyEntries, "1"],
        [(Guid) Guid.Parse("82865716-6e99-4ba9-9a12-3e8b2e3cd891"), "\"82865716-6e99-4ba9-9a12-3e8b2e3cd891\""],
        [(Uri) new Uri("/relative/url/?with=query", UriKind.Relative), "\"/relative/url/?with=query\""]
    ];

    [TestCaseSource(nameof(TestValues))]
    public void Can_serialize_special_types_and_structs_to_json(object modelValue, string expected)
    {
        var template = Template.Parse("{{ model | object.to_json }}");

        var model = new {
            model = modelValue
        };

        var result = template.Render(model);

        Console.WriteLine($"Result: {result}");
        Assert.AreEqual(expected, result);
    }

    [Test]
    public void JsonSerializer_produces_iso8601_with_trimmed_zeros()
    {
        var date = new DateTime(2024, 08, 07, 15, 27, 12, 234, DateTimeKind.Utc);

        var iso8601Result = date.ToString("O");
        Assert.AreEqual("2024-08-07T15:27:12.2340000Z", iso8601Result);

        var jsonResult = JsonSerializer.Serialize(date);
        Assert.AreEqual("\"2024-08-07T15:27:12.234Z\"", jsonResult);
    }
}