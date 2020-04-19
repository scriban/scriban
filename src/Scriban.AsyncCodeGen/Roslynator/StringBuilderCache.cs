// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Text;

namespace Roslynator.Text
{
    internal static class StringBuilderCache
    {
        private const int MaxSize = 256;
        private const int DefaultCapacity = 16;

        [ThreadStatic]
        private static StringBuilder _cachedInstance;

        public static StringBuilder GetInstance(int capacity = DefaultCapacity)
        {
            if (capacity <= MaxSize)
            {
                StringBuilder sb = _cachedInstance;

                if (sb != null
                    && capacity <= sb.Capacity)
                {
                    _cachedInstance = null;
                    sb.Clear();
                    return sb;
                }
            }

            return new StringBuilder(capacity);
        }

        public static void Free(StringBuilder sb)
        {
            if (sb.Capacity <= MaxSize)
                _cachedInstance = sb;
        }

        public static string GetStringAndFree(StringBuilder sb)
        {
            string s = sb.ToString();
            Free(sb);
            return s;
        }
    }
}