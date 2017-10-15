// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.
using System;
using Scriban.Parsing;

namespace Scriban.Runtime.Accessors
{
    public class ArrayAccessor : IListAccessor
    {
        public static ArrayAccessor Default = new ArrayAccessor();

        private ArrayAccessor()
        {
        }

        public int GetLength(TemplateContext context, SourceSpan span, object target)
        {
            return ((Array) target).Length;
        }

        public object GetValue(TemplateContext context, SourceSpan span, object target, int index)
        {
            return ((Array)target).GetValue(index);
        }

        public void SetValue(TemplateContext context, SourceSpan span, object target, int index, object value)
        {
            ((Array)target).SetValue(value, index);
        }
    }
}