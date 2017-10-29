// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.
namespace Scriban.Syntax
{
    public abstract class ScriptVariablePath : ScriptExpression
    {
        public abstract object GetValue(TemplateContext context);

        public abstract void SetValue(TemplateContext context, object valueToSet);
    }
}