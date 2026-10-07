// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

namespace System.Windows.Forms.Tests;

public class Form_ControlCollection
{
    [WinFormsFact]
    public void ControlCollection_GenericIList_UsesTypedMembers()
    {
        using Form owner = new();
        IList<Control> collection = owner.Controls;
        using Button button = new();

        collection.Add(button);

        Assert.Same(button, collection[0]);
        Assert.Same(button, collection.Single());
        Assert.True(collection.Remove(button));
        Assert.Empty(collection);
    }

    [WinFormsFact]
    public void ControlCollection_Ctor_Control()
    {
        using Form owner = new();
        Form.ControlCollection collection = new(owner);

        Assert.Empty(collection);
        Assert.False(collection.IsReadOnly);
        Assert.Same(owner, collection.Owner);
    }

    [WinFormsFact]
    public void ControlCollection_Ctor_NullOwner_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>("owner", () => new Form.ControlCollection(null));
    }
}
