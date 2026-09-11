// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;

namespace System.Windows.Forms.Design;

internal sealed class StackPanelDesigner : ParentControlDesigner
{
    public override bool CanParent(Control control)
        => control is StackPage && !Control.Contains(control);

    public override void InitializeNewComponent(IDictionary defaultValues)
    {
        base.InitializeNewComponent(defaultValues);

        if (Component is StackPanel stackPanel && stackPanel.PageCount == 0)
        {
            stackPanel.Pages.Add(new StackPage());
            stackPanel.SelectedIndex = 0;
        }
    }
}

internal sealed class StackPageDesigner : PanelDesigner
{
    public override bool CanBeParentedTo(IDesigner parentDesigner)
        => parentDesigner?.Component is StackPanel;

    public override SelectionRules SelectionRules
    {
        get
        {
            SelectionRules rules = base.SelectionRules;
            if (Control.Parent is StackPanel)
            {
                rules &= ~SelectionRules.AllSizeable;
            }

            return rules;
        }
    }
}
