// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms.Design;
using Moq;

namespace System.Windows.Forms.PropertyGridInternal.Tests;

public partial class PropertyGridViewTests
{
    [WinFormsFact]
    public void PropertyGridView_Ctor_Default()
    {
        using PropertyGrid propertyGrid = new();
        PropertyGridView propertyGridView = propertyGrid.TestAccessor.GridView;

        // TODO: validate properties

        Assert.NotNull(propertyGridView);
    }

    [WinFormsTheory]
    [InlineData(DialogResult.OK, true, false, false, 1)]
    [InlineData(DialogResult.OK, false, false, false, 0)]
    [InlineData(DialogResult.Cancel, true, false, false, 0)]
    [InlineData(DialogResult.Abort, true, false, false, 0)]
    [InlineData(DialogResult.OK, true, true, false, 1)]
    [InlineData(DialogResult.OK, true, false, true, 1)]
    [InlineData(DialogResult.Cancel, true, false, true, 0)]
    public void PropertyGridView_EditPropertyValue_CollectionChanges(
        DialogResult dialogResult,
        bool changed,
        bool replaceValue,
        bool readOnly,
        int expectedNotifications)
    {
        CollectionOwner owner = new();
        using PropertyGrid propertyGrid = new()
        {
            PropertySort = PropertySort.NoSort,
            SelectedObject = owner
        };
        PropertyGridView gridView = propertyGrid.TestAccessor.GridView;
        string propertyName = readOnly ? nameof(CollectionOwner.ReadOnlyItems) : nameof(CollectionOwner.Items);
        GridEntry entry = gridView.TopLevelGridEntries.Single(e => e.PropertyName == propertyName);
        propertyGrid.SelectedGridItem = entry;
        var editor = Assert.IsType<InPlaceCollectionEditor>(entry.UITypeEditor);
        BindingList<string> originalValue = owner.Items;
        int notificationCount = 0;
        propertyGrid.PropertyValueChanged += (sender, e) =>
        {
            Assert.Same(propertyGrid, sender);
            Assert.Same(entry, e.ChangedItem);
            Assert.Same(originalValue, e.OldValue);
            notificationCount++;
        };

        Mock<IUIService> uiService = new();
        Mock<ISite> services = new();
        services.Setup(s => s.GetService(typeof(IUIService))).Returns(uiService.Object);
        gridView.ServiceProvider = services.Object;
        gridView.Site = services.Object;

        editor.Edit = (context, provider, value) =>
        {
            uiService.Setup(s => s.ShowDialog(It.IsAny<Form>())).Returns(() =>
            {
                if (changed)
                {
                    originalValue.Add("added");
                    context.OnComponentChanged();
                    context.OnComponentChanged();
                }

                Assert.Equal(0, notificationCount);
                if (dialogResult != DialogResult.OK)
                {
                    originalValue.Remove("added");
                    context.OnComponentChanged();
                }

                return dialogResult;
            });

            using Form dialog = new();
            var editorService = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
            Assert.Equal(dialogResult, editorService.ShowDialog(dialog));
            Assert.Equal(0, notificationCount);
            return replaceValue ? new BindingList<string>(["replacement"]) : value;
        };

        entry.EditPropertyValue(gridView);

        Assert.Equal(expectedNotifications, notificationCount);
        Assert.Equal(replaceValue ? 1 : 0, owner.SetterCallCount);
        Assert.Equal(dialogResult == DialogResult.OK && changed ? 2 : 1, originalValue.Count);

        // A subsequent unchanged edit must not reuse the preceding edit's notifications.
        editor.Edit = (context, provider, value) => value;
        entry.EditPropertyValue(gridView);
        Assert.Equal(expectedNotifications, notificationCount);
        uiService.Verify(s => s.ShowError(It.IsAny<Exception>()), Times.Never());
    }

    /// <summary>
    ///  Provides editable and getter-only collections for in-place editor tests.
    /// </summary>
    private class CollectionOwner
    {
        private BindingList<string> _items = new(["original"]);

        [Editor(typeof(InPlaceCollectionEditor), typeof(UITypeEditor))]
        public BindingList<string> Items
        {
            get => _items;
            set
            {
                SetterCallCount++;
                _items = value;
            }
        }

        [Editor(typeof(InPlaceCollectionEditor), typeof(UITypeEditor))]
        public BindingList<string> ReadOnlyItems => _items;

        [Browsable(false)]
        public int SetterCallCount { get; private set; }
    }

    /// <summary>
    ///  Reports collection mutations through the editor context without replacing the value.
    /// </summary>
    private class InPlaceCollectionEditor : UITypeEditor
    {
        public Func<ITypeDescriptorContext, IServiceProvider, object, object> Edit { get; set; }

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
            => UITypeEditorEditStyle.Modal;

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
            => Edit(context, provider, value);
    }
}
