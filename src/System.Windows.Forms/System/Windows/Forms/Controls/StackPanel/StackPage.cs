// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;

namespace System.Windows.Forms;

/// <summary>
///  Represents a page displayed by a <see cref="StackPanel"/>.
/// </summary>
[ToolboxItem(false)]
[DesignTimeVisible(false)]
[Designer($"System.Windows.Forms.Design.StackPageDesigner, {Assemblies.SystemDesign}")]
public class StackPage : Panel
{
    /// <summary>
    ///  Initializes a new instance of the <see cref="StackPage"/> class.
    /// </summary>
    public StackPage()
    {
        Dock = DockStyle.Fill;
    }

    /// <summary>
    ///  Initializes a new instance of the <see cref="StackPage"/> class with the specified name.
    /// </summary>
    /// <param name="text">The name of the page.</param>
    public StackPage(string? text) : this()
    {
        Text = text;
    }

    /// <inheritdoc />
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public override string Text
    {
        get => base.Text;
        set => base.Text = value;
    }
}
