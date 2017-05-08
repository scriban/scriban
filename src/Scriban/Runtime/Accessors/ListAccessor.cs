// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.
using System.Collections;

namespace Scriban.Runtime.Accessors
{
    public class ListAccessor : IListAccessor
    {
        public static ListAccessor Default = new ListAccessor();

        private ListAccessor()
        {
        }

        public int GetLength(object target)
        {
            return ((IList) target).Count;
        }

        public object GetValue(object target, int index)
        {
            var list = ((IList)target);
            if (index < 0 || index >= list.Count)
            {
                return null;
            }

            return list[index];
        }

        public void SetValue(object target, int index, object value)
        {
            var list = ((IList) target);
            if (index < 0)
            {
                return;
            }
            // Auto-expand the array in case of accessing a range outside the current value
            for (int i = list.Count; i <= index; i++)
            {
                // TODO: If the array doesn't support null value, we shoud add a default value or throw an error?
                list.Add(null);
            }

            list[index] = value;
        }
    }
}