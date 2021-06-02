// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

#nullable disable

using System.Collections;
using System.IO;
using Scriban.Runtime;
using Scriban.Helpers;
using System.Collections.Generic;

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
        private ScriptExpression _target;
        private ScriptToken _openBracket;
        private ScriptExpression _index;
        private ScriptToken _closeBracket;

        public ScriptIndexerExpression()
        {
            OpenBracket = ScriptToken.OpenBracket();
            CloseBracket = ScriptToken.CloseBracket();
        }

        public ScriptExpression Target
        {
            get => _target;
            set => ParentToThis(ref _target, value);
        }

        public ScriptToken OpenBracket
        {
            get => _openBracket;
            set => ParentToThis(ref _openBracket, value);
        }

        public ScriptExpression Index
        {
            get => _index;
            set => ParentToThis(ref _index, value);
        }

        public ScriptToken CloseBracket
        {
            get => _closeBracket;
            set => ParentToThis(ref _closeBracket, value);
        }

        public override object Evaluate(TemplateContext context)
        {
            return context.GetValue(this);
        }

        public override bool CanHaveLeadingTrivia()
        {
            return false;
        }

        public override void PrintTo(ScriptPrinter printer)
        {
            printer.Write(Target);
            var isSpecialArgumentsArray = Equals(Target, ScriptVariable.Arguments) && Index is ScriptLiteral &&
                                          ((ScriptLiteral) Index).IsPositiveInteger();
            if (!isSpecialArgumentsArray)
            {
                printer.Write(OpenBracket);
            }
            printer.Write(Index);
            if (!isSpecialArgumentsArray)
            {
                printer.Write(CloseBracket);
            }
        }
        public object GetValue(TemplateContext context)
        {
            return GetOrSetValue(context, null, false);
        }

        public void SetValue(TemplateContext context, object valueToSet)
        {
            GetOrSetValue(context, valueToSet, true);
        }

        public string GetFirstPath()
        {
            return (Target as IScriptVariablePath)?.GetFirstPath();
        }

        private object GetOrSetValue(TemplateContext context, object valueToSet, bool setter)
        {
            object value = null;

            var targetObject = context.GetValue(Target);
            if (targetObject == null)
            {
                if (context.EnableRelaxedTargetAccess)
                {
                    return null;
                }
                else
                {
                    throw new ScriptRuntimeException(Target.Span, $"Object `{Target}` is null. Cannot access indexer: {this}"); // unit test: 130-indexer-accessor-error1.txt
                }
            }

            var index = context.Evaluate(Index);
            if (index == null)
            {
                if (context.EnableNullIndexer)
                {
                    return null;
                }
                else
                {
                    throw new ScriptRuntimeException(Index.Span,  $"Cannot access target `{Target}` with a null indexer: {this}"); // unit test: 130-indexer-accessor-error2.txt
                }
            }

            var listAccessor = context.GetListAccessor(targetObject);
            if (targetObject is IDictionary || (targetObject is IScriptObject && (listAccessor == null || index is string)) || listAccessor == null)
            {
                var accessor = context.GetMemberAccessor(targetObject);

                if (accessor.HasIndexer)
                {
                    var itemIndex = context.ToObject(Index.Span, index, accessor.IndexType);
                    if (setter)
                    {
                        if (!accessor.TrySetItem(context, Index.Span, targetObject, itemIndex, valueToSet))
                        {
                            throw new ScriptRuntimeException(Index.Span, $"Cannot set a value for the readonly member `{itemIndex}` in the indexer: {Target}['{itemIndex}']");

                        }
                    }
                    else
                    {
                        var result = accessor.TryGetItem(context, Index.Span, targetObject, itemIndex, out value);
                        if (!context.EnableRelaxedMemberAccess && !result)
                        {
                            throw new ScriptRuntimeException(Index.Span, $"Cannot access target `{Target}` with an indexer: {Index}");
                        }
                    }
                }
                else
                {
                    var indexAsString = context.ObjectToString(index);

                    if (setter)
                    {
                        if (!accessor.TrySetValue(context, Index.Span, targetObject, indexAsString, valueToSet))
                        {
                            throw new ScriptRuntimeException(Index.Span, $"Cannot set a value for the readonly member `{indexAsString}` in the indexer: {Target}['{indexAsString}']"); // unit test: 130-indexer-accessor-error3.txt
                        }
                    }
                    else
                    {
                        if (!accessor.TryGetValue(context, Index.Span, targetObject, indexAsString, out value))
                        {
                            var result = context.TryGetMember?.Invoke(context, Index.Span, targetObject, indexAsString, out value) ?? false;
                            if (!context.EnableRelaxedMemberAccess && !result)
                            {
                                throw new ScriptRuntimeException(Index.Span, $"Cannot access target `{Target}` with an indexer: {Index}");
                            }
                        }
                    }
                }
            }
            else
            {
                int i = context.ToInt(Index.Span, index);

                var length = listAccessor.GetLength(context, Target.Span, targetObject);
                // Allow negative index from the end of the array
                if (i < 0)
                {
                    i = length + i;
                }

                if (!context.EnableRelaxedIndexerAccess && (i < 0 || i >= length))
                {
                    throw new ScriptRuntimeException(Index.Span, $"The index {i} is out of bounds [0, {length}] on the `{Target}` with the indexer: {Index}");
                }

                if (i >= 0)
                {
                    if (setter)
                    {
                        listAccessor.SetValue(context, Index.Span, targetObject, i, valueToSet);
                    }
                    else
                    {
                        value = listAccessor.GetValue(context, Index.Span, targetObject, i);
                    }
                }
            }
            return value;
        }
    }
}