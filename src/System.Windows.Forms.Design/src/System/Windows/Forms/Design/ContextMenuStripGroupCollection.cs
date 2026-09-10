// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Windows.Forms.Design;

internal class ContextMenuStripGroupCollection : Dictionary<string, ContextMenuStripGroup>
{
    public new ContextMenuStripGroup this[string key]
    {
        get
        {
            if (!TryGetValue(key, out ContextMenuStripGroup group))
            {
                group = new ContextMenuStripGroup();
                Add(key, group);
            }

            return group;
        }
    }
}
