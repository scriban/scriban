// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.
using System;

namespace Scriban.Runtime
{
    public sealed class DelegateMemberRenamer : IMemberRenamer
    {
        public delegate string RenameDelegate(string name);

        public DelegateMemberRenamer(RenameDelegate rename)
        {
            Rename = rename ?? throw new ArgumentNullException(nameof(rename));
        }

        public RenameDelegate Rename { get; }

        public string GetName(string member)
        {
            return Rename(member);
        }
    }
}