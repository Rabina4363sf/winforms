// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;

namespace System.Windows.Forms;

public partial class Form
{
    private static readonly int s_propTaskbarItemInfo = PropertyStore.CreateKey();

    /// <summary>
    ///  Gets the information used to display a progress indicator in this form's taskbar button.
    /// </summary>
    /// <value>
    ///  A <see cref="TaskbarItemInfo"/> associated with this form.
    /// </value>
    /// <remarks>
    ///  The returned object is created on first access and remains associated with this
    ///  form for its lifetime.
    /// </remarks>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public TaskbarItemInfo TaskbarItemInfo
        => Properties.GetValueOrDefault<TaskbarItemInfo>(s_propTaskbarItemInfo)
            ?? Properties.AddValue(s_propTaskbarItemInfo, new TaskbarItemInfo(this));

    private void OnTaskbarItemInfoHandleCreated()
        => Properties.GetValueOrDefault<TaskbarItemInfo>(s_propTaskbarItemInfo)?.OnHandleCreated();

    private void DisposeTaskbarItemInfo()
    {
        if (Properties.TryGetValue(s_propTaskbarItemInfo, out TaskbarItemInfo? taskbarItemInfo))
        {
            taskbarItemInfo.Dispose();
            Properties.RemoveValue(s_propTaskbarItemInfo);
        }
    }
}
