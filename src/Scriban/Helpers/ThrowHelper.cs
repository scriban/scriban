// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Scriban.Helpers
{
    internal static class ThrowHelper
    {
        public static void ThrowArgumentOutOfRangeException(ExceptionArgument ex)
        {
            throw GetArgumentOutOfRangeException(ex);
        }

        private static ArgumentOutOfRangeException GetArgumentOutOfRangeException(ExceptionArgument argument)
        {
            return new ArgumentOutOfRangeException(GetArgumentName(argument));
        }

        public static ArgumentOutOfRangeException GetIndexNegativeArgumentOutOfRangeException()
        {
            return new ArgumentOutOfRangeException("index", "Index must be positive");
        }
        public static ArgumentOutOfRangeException GetIndexArgumentOutOfRangeException(int maxValue)
        {
            return new ArgumentOutOfRangeException("index", $"Index must be less than {maxValue}");
        }
        public static InvalidOperationException GetExpectingNoParentException()
        {
            return new InvalidOperationException("The node is already attached to another parent");
        }

        // This function will convert an ExceptionArgument enum value to the argument name string.
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static string GetArgumentName(ExceptionArgument argument)
        {
            Debug.Assert(Enum.IsDefined(typeof(ExceptionArgument), argument), "The enum value is not defined, please check the ExceptionArgument Enum.");
            return argument.ToString();
        }
    }

    internal enum ExceptionArgument
    {
        parent,
        index,
        element,
        item,
        array,
        value,
        builder,
        capacity,
        dictionary,
        collection,
        Arg_ArrayPlusOffTooSmall,
        key,
        min,
        handle
    }
}