// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

namespace Scriban.Syntax
{
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    enum ScriptTriviaType
    {
        Empty = 0,

        Whitespace,

        WhitespaceFull,

        Comment,

        Comma,

        CommentMulti,

        NewLine,

        SemiColon,
    }
}