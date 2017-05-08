// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.
namespace Scriban.Runtime
{
    public interface IListAccessor
    {
        int GetLength(object target);

        object GetValue(object target, int index);

        void SetValue(object target, int index, object value);
    }
}