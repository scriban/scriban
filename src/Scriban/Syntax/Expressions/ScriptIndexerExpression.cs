// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

#nullable enable

using System.Collections;
using System.IO;
using Scriban.Runtime;
using Scriban.Helpers;
using System.Collections.Generic;
using Scriban.Parsing;

namespace Scriban.Syntax
{
    [ScriptSyntax("indexer expression", "<expression>[<index_expression>]")]
#if SCRIBAN_PUBLIC
    public
#else
    internal
#endif
    partial class ScriptIndexerExpression : ScriptExpression, IScriptVariablePath
    {
        private ScriptExpression? _target;
        private ScriptToken _openBracket = ScriptToken.OpenBracket();
        private ScriptExpression? _index;
        private ScriptToken _closeBracket = ScriptToken.CloseBracket();
        public ScriptIndexerExpression()
        {
            _openBracket.Parent = this;
            _closeBracket.Parent = this;
        }

        public ScriptExpression? Target
        {
            get => _target;
            set => ParentToThisNullable(ref _target, value);
        }

        public ScriptToken OpenBracket
        {
            get => _openBracket;
            set => ParentToThis(ref _openBracket, value);
        }

        public ScriptExpression? Index
        {
            get => _index;
            set => ParentToThisNullable(ref _index, value);
        }

        public ScriptToken CloseBracket
        {
            get => _closeBracket;
            set => ParentToThis(ref _closeBracket, value);
        }

        public override object? Evaluate(TemplateContext context)
        {
            return context.GetValue(this);
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
            var isSpecialArgumentsArray = Equals(Target, ScriptVariable.Arguments) && Index is ScriptLiteral &&
                                          ((ScriptLiteral) Index).IsPositiveInteger();
            if (!isSpecialArgumentsArray)
            {
                printer.Write(OpenBracket);
            }
            if (Index is not null)
            {
                printer.Write(Index);
            }
            if (!isSpecialArgumentsArray)
            {
                printer.Write(CloseBracket);
            }
        }
        public object? GetValue(TemplateContext context)
        {
            return GetOrSetValue(context, null, false);
        }

        public void SetValue(TemplateContext context, object? valueToSet)
        {
            GetOrSetValue(context, valueToSet, true);
        }

        public string GetFirstPath()
        {
            return (Target as IScriptVariablePath)?.GetFirstPath() ?? string.Empty;
        }

        private object? GetOrSetValue(TemplateContext context, object? valueToSet, bool setter)
        {
            object? value = null;
            var target = Target;
            var indexExpression = Index;
            if (target is null || indexExpression is null)
            {
                throw new ScriptRuntimeException(Span, "Invalid indexer expression. Target and index are required.");
            }

            var targetObject = context.GetValue(target);
            if (targetObject is null)
            {
                if (!setter && (context.EnableRelaxedTargetAccess || HasNullConditionalTarget(target)))
                {
                    return null;
                }
                else
                {
                    throw new ScriptRuntimeException(target.Span, $"Object `{target}` is null. Cannot access indexer: {this}"); // unit test: 130-indexer-accessor-error1.txt
                }
            }

            var index = context.Evaluate(indexExpression);
            if (index is null)
            {
                if (context.EnableNullIndexer)
                {
                    return null;
                }
                else
                {
                    throw new ScriptRuntimeException(indexExpression.Span,  $"Cannot access target `{target}` with a null indexer: {this}"); // unit test: 130-indexer-accessor-error2.txt
                }
            }

            var listAccessor = context.TryGetListAccessor(targetObject);
            if (targetObject is IDictionary || (targetObject is IScriptObject && (listAccessor is null || index is string)) || listAccessor is null)
            {
                var accessor = context.GetMemberAccessor(targetObject);

                if (accessor.HasIndexer)
                {
                        var indexType = accessor.IndexType;
                        if (indexType is null)
                        {
                            throw new ScriptRuntimeException(indexExpression.Span, $"Cannot access target `{target}` with an untyped indexer: {this}");
                        }
                        var itemIndex = context.ToObject(indexExpression.Span, index, indexType);
                        if (itemIndex is null)
                        {
                            if (context.EnableNullIndexer)
                            {
                                return null;
                            }

                            throw new ScriptRuntimeException(indexExpression.Span, $"Cannot access target `{target}` with a null indexer: {this}");
                        }
                        if (setter)
                        {
                            if (!accessor.TrySetItem(context, indexExpression.Span, targetObject, itemIndex, valueToSet))
                        {
                            throw new ScriptRuntimeException(indexExpression.Span, $"Cannot set a value for the readonly member `{itemIndex}` in the indexer: {target}['{itemIndex}']");

                        }
                    }
                    else
                    {
                        var result = accessor.TryGetItem(context, indexExpression.Span, targetObject, itemIndex, out value);
                        if (!context.EnableRelaxedMemberAccess && !result)
                        {
                            throw new ScriptRuntimeException(indexExpression.Span, $"Cannot access target `{target}` with an indexer: {indexExpression}");
                        }
                    }
                }
                else
                {
                    var indexAsString = context.ObjectToString(index) ?? string.Empty;

                    if (setter)
                    {
                        if (!accessor.TrySetValue(context, indexExpression.Span, targetObject, indexAsString, valueToSet))
                        {
                            throw new ScriptRuntimeException(indexExpression.Span, $"Cannot set a value for the readonly member `{indexAsString}` in the indexer: {target}['{indexAsString}']"); // unit test: 130-indexer-accessor-error3.txt
                        }
                    }
                    else
                    {
                        if (!accessor.TryGetValue(context, indexExpression.Span, targetObject, indexAsString, out value))
                        {
                            var result = context.TryGetMember?.Invoke(context, indexExpression.Span, targetObject, indexAsString, out value) ?? false;
                            if (!context.EnableRelaxedMemberAccess && !result)
                            {
                                throw new ScriptRuntimeException(indexExpression.Span, $"Cannot access target `{target}` with an indexer: {indexExpression}");
                            }
                        }
                    }
                }
            }
            else
            {
                int i = context.ToInt(indexExpression.Span, index);

                var length = listAccessor.GetLength(context, target.Span, targetObject);
                // Allow negative index from the end of the array
                if (i < 0)
                {
                    i = length + i;
                }

                if (!context.EnableRelaxedIndexerAccess && (i < 0 || i >= length))
                {
                    throw new ScriptRuntimeException(indexExpression.Span, $"The index {i} is out of bounds [0, {length}] on the `{target}` with the indexer: {indexExpression}");
                }

                if (i >= 0)
                {
                    if (setter)
                    {
                        listAccessor.SetValue(context, indexExpression.Span, targetObject, i, valueToSet);
                    }
                    else
                    {
                        value = listAccessor.GetValue(context, indexExpression.Span, targetObject, i);
                    }
                }
            }
            return value;
        }

        private static bool HasNullConditionalTarget(ScriptExpression? expression)
        {
            switch (expression)
            {
                case ScriptMemberExpression memberExpression:
                    return memberExpression.DotToken.TokenType == TokenType.QuestionDot || HasNullConditionalTarget(memberExpression.Target);
                case ScriptIndexerExpression indexerExpression:
                    return HasNullConditionalTarget(indexerExpression.Target);
                case ScriptNestedExpression nestedExpression:
                    return HasNullConditionalTarget(nestedExpression.Expression);
                default:
                    return false;
            }
        }
    }
}
