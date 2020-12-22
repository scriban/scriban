// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

namespace Scriban.Parsing
{
    /// <summary>
    /// Defines the options used when parsing a template.
    /// </summary>
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    struct ParserOptions
    {
        /// <summary>
        /// Sets the depth limit of nested statements (e.g nested if/else) to disallow deep/potential stack-overflow exploits. Default is null, so there is no limit.
        /// </summary>
        public int? ExpressionDepthLimit { get; set; }

        /// <summary>
        /// <c>true</c> to convert liquid builtin function calls to scriban function calls (e.g abs = math.abs, downcase = string.downcase)
        /// </summary>
        public bool LiquidFunctionsToScriban { get; set; }

        /// <summary>
        /// Parse float as <see cref="decimal"/> instead of <see cref="double"/>.
        /// If the number cannot be represented to a decimal, it will fall back to a double.
        /// </summary>
        public bool ParseFloatAsDecimal { get; set; }
    }
}