// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.
namespace Scriban.Model
{
    public enum ScriptBinaryOperator
    {
        /// <summary>
        /// The empty coalescing operator ??
        /// </summary>
        EmptyCoalescing, 

        Add,

        Substract,

        Divide,

        DivideRound,

        Multiply,

        Modulus,

        ShiftLeft,

        ShiftRight,

        RangeInclude,

        RangeExclude,

        CompareEqual,

        CompareNotEqual,

        CompareLessOrEqual,

        CompareGreaterOrEqual,

        CompareLess,

        CompareGreater,

        And,

        Or,
    }

    public static class ScriptBinaryOperatorExtensions
    {
        public static string ToText(this ScriptBinaryOperator op)
        {
            switch (op)
            {
                case ScriptBinaryOperator.Add:
                    return "+";
                case ScriptBinaryOperator.Substract:
                    return "-";
                case ScriptBinaryOperator.Divide:
                    return "/";
                case ScriptBinaryOperator.DivideRound:
                    return "//";
                case ScriptBinaryOperator.Multiply:
                    return "*";
                case ScriptBinaryOperator.Modulus:
                    return "%";
                case ScriptBinaryOperator.RangeInclude:
                    return "..";
                case ScriptBinaryOperator.RangeExclude:
                    return "...";
                case ScriptBinaryOperator.CompareEqual:
                    return "==";
                case ScriptBinaryOperator.CompareNotEqual:
                    return "!=";
                case ScriptBinaryOperator.CompareLessOrEqual:
                    return "<=";
                case ScriptBinaryOperator.CompareGreaterOrEqual:
                    return ">=";
                case ScriptBinaryOperator.CompareLess:
                    return "<";
                case ScriptBinaryOperator.CompareGreater:
                    return ">";
                case ScriptBinaryOperator.And:
                    return "&&";
                case ScriptBinaryOperator.Or:
                    return "||";
            }
            return op.ToString();
        }
    }
}