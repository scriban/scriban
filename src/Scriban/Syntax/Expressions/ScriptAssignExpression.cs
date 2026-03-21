// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using Scriban.Parsing;

namespace Scriban.Syntax
{
    [ScriptSyntax("assign expression", "<target_expression> = <value_expression>")]
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    partial class ScriptAssignExpression : ScriptExpression
    {
        private ScriptExpression? _target;
        private ScriptToken _equalToken = ScriptToken.Equal();
        private ScriptExpression? _value;
        public ScriptAssignExpression()
        {
            _equalToken.Parent = this;
        }

        public ScriptExpression? Target
        {
            get => _target;
            set => ParentToThisNullable(ref _target, value);
        }

        public ScriptToken EqualToken
        {
            get => _equalToken;
            set => ParentToThis(ref _equalToken, value);
        }

        public ScriptExpression? Value
        {
            get => _value;
            set => ParentToThisNullable(ref _value, value);
        }

        public override object? Evaluate(TemplateContext context)
        {
            if (Target is null || Value is null)
            {
                throw new ScriptRuntimeException(Span, "Invalid assignment expression. Target and value are required.");
            }

            var valueObject = EqualToken.TokenType == TokenType.Equal
                ? context.Evaluate(Value)
                : GetValueToSet(context);
            context.SetValue(Target, valueObject);
            return null;
        }

        private object? GetValueToSet(TemplateContext context)
        {
            if (Target is null || Value is null)
            {
                throw new ScriptRuntimeException(Span, "Invalid assignment expression. Target and value are required.");
            }

            var right = context.Evaluate(Value);
            var left = context.Evaluate(Target);
            var op = this.EqualToken.TokenType switch
            {
                TokenType.PlusEqual => ScriptBinaryOperator.Add,
                TokenType.MinusEqual => ScriptBinaryOperator.Subtract,
                TokenType.AsteriskEqual => ScriptBinaryOperator.Multiply,
                TokenType.DivideEqual => ScriptBinaryOperator.Divide,
                TokenType.DoubleDivideEqual => ScriptBinaryOperator.DivideRound,
                TokenType.PercentEqual => ScriptBinaryOperator.Modulus,
                _ => throw new ScriptRuntimeException(context.CurrentSpan, $"Operator {this.EqualToken} is not a valid compound assignment operator"),
            };
            return ScriptBinaryExpression.Evaluate(context, this.Span, op, left, right);
        }

        public override bool CanHaveLeadingTrivia()
        {
            return false;
        }

        public override void PrintTo(ScriptPrinter printer)
        {
            if (Target is not null)
            {
                printer.Write(Target);
            }
            printer.Write(EqualToken);
            if (Value is not null)
            {
                printer.Write(Value);
            }
        }
    }
}
