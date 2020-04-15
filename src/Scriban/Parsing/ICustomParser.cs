// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

namespace Scriban.Parsing
{
    public interface ICustomParser
    {
        bool IsCustomUnaryOperator(TokenType type, string operatorText, out int precedence);

        bool IsCustomBinaryOperator(TokenType type, string operatorText, out int precedence);



    }
}