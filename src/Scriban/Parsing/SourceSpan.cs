// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.
namespace Scriban.Parsing
{
    /// <summary>
    /// Defines the precise source location.
    /// </summary>
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    struct SourceSpan
    {
        public SourceSpan(string fileName, TextPosition start, TextPosition end)
        {
            FileName = fileName;
            Start = start;
            End = end;
        }

        public string FileName { get; set; }

        public bool IsEmpty => FileName == null;

        public TextPosition Start { get; set; }

        public TextPosition End { get; set; }

        public int Length => End.Offset - Start.Offset + 1;

        public override string ToString()
        {
            return $"{FileName}({Start})-({End})";
        }

        public string ToStringSimple()
        {
            return $"{FileName}({Start.ToStringSimple()})";
        }
    }
}