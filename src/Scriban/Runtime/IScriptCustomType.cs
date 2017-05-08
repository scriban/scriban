// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.
using System;
using Scriban.Model;

namespace Scriban.Runtime
{
    public interface IScriptCustomType
    {
        bool TryConvertTo(Type destinationType, out object value);

        object EvaluateUnaryExpression(ScriptUnaryExpression expression);

        object EvaluateBinaryExpression(ScriptBinaryExpression expression, object left, object right);
    }
}