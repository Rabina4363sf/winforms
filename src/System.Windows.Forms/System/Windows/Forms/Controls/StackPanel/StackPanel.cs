// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.ComponentModel;

namespace System.Windows.Forms;

/// <summary>
///  Displays one of several overlapping pages at a time.
/// </summary>
[DefaultProperty(nameof(Pages))]
[DefaultEvent(nameof(SelectedIndexChanged))]
[Designer($"System.Windows.Forms.Design.StackPanelDesigner, {Assemblies.SystemDesign}")]
public class StackPanel : Control, ISupportInitialize
{
    private static readonly object s_selectedIndexChangedEvent = new();

    private readonly StackPageCollection _pages;
    private int _selectedIndex = -1;
    private bool _initializing;

    /// <summary>
    ///  Initializes a new instance of the <see cref="StackPanel"/> class.
    /// </summary>
    public StackPanel()
    {
        _pages = new StackPageCollection(this);
        SetStyle(ControlStyles.ContainerControl, true);
    }

    /// <summary>
    ///  Gets the pages contained by the control.
    /// </summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    public StackPageCollection Pages => _pages;

    /// <summary>
    ///  Gets the number of pages contained by the control.
    /// </summary>
    [Browsable(false)]
    public int PageCount => _pages.Count;

    /// <summary>
    ///  Gets or sets the zero-based index of the page that is displayed.
    /// </summary>
    [DefaultValue(-1)]
    public int SelectedIndex
    {
        get => _selectedIndex;
        set
        {
            if (value < -1 || value >= PageCount)
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }

            if (_selectedIndex == value)
            {
                return;
            }

            SelectPage(value);
        }
    }

    /// <summary>
    ///  Gets or sets the page that is displayed.
    /// </summary>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public StackPage? SelectedPage
    {
        get => _selectedIndex >= 0 ? _pages[_selectedIndex] : null;
        set
        {
            if (value is null)
            {
                SelectedIndex = -1;
                return;
            }

            int index = _pages.IndexOf(value);
            if (index < 0)
            {
                throw new ArgumentException("The page must belong to this control.", nameof(value));
            }

            SelectedIndex = index;
        }
    }

    /// <summary>
    ///  Occurs when the selected page changes.
    /// </summary>
    public event EventHandler? SelectedIndexChanged
    {
        add => Events.AddHandler(s_selectedIndexChangedEvent, value);
        remove => Events.RemoveHandler(s_selectedIndexChangedEvent, value);
    }

    /// <summary>
    ///  Raises the <see cref="SelectedIndexChanged"/> event.
    /// </summary>
    /// <param name="e">The event data.</param>
    protected virtual void OnSelectedIndexChanged(EventArgs e)
        => (Events[s_selectedIndexChangedEvent] as EventHandler)?.Invoke(this, e);

    /// <summary>
    ///  Begins initialization of the control.
    /// </summary>
    public void BeginInit() => _initializing = true;

    /// <summary>
    ///  Completes initialization of the control.
    /// </summary>
    public void EndInit()
    {
        _initializing = false;
        if (_selectedIndex < 0 && PageCount > 0)
        {
            SelectPage(0);
        }
    }

    /// <inheritdoc />
    protected override ControlCollection CreateControlsInstance()
        => new StackPanelControlCollection(this);

    /// <inheritdoc />
    protected override void OnLayout(LayoutEventArgs levent)
    {
        base.OnLayout(levent);

        foreach (StackPage page in _pages)
        {
            page.Bounds = ClientRectangle;
        }
    }

    private void AddPage(StackPage page)
    {
        ArgumentNullException.ThrowIfNull(page);

        Controls.Add(page);
    }

    private void RemovePage(StackPage page)
    {
        Controls.Remove(page);
    }

    private void ConfigureAddedPage(StackPage page)
    {
        page.Bounds = ClientRectangle;
        page.Visible = false;

        if (!_initializing && _selectedIndex < 0)
        {
            SelectPage(0);
        }
    }

    private void ConfigureRemovedPage(int removedIndex, bool wasSelected)
    {
        if (PageCount == 0)
        {
            _selectedIndex = -1;
            OnSelectedIndexChanged(EventArgs.Empty);
        }
        else if (wasSelected)
        {
            _selectedIndex = -1;
            SelectPage(Math.Min(removedIndex, PageCount - 1));
        }
        else if (removedIndex < _selectedIndex)
        {
            _selectedIndex--;
        }
    }

    private void SelectPage(int index)
    {
        if (_selectedIndex >= 0)
        {
            _pages[_selectedIndex].Visible = false;
        }

        _selectedIndex = index;

        if (_selectedIndex >= 0)
        {
            StackPage page = _pages[_selectedIndex];
            page.Bounds = ClientRectangle;
            page.Visible = true;
            page.BringToFront();
        }

        OnSelectedIndexChanged(EventArgs.Empty);
    }

    private sealed class StackPanelControlCollection : ControlCollection
    {
        /// <summary>
        ///  Initializes a new instance of the control collection.
        /// </summary>
        public StackPanelControlCollection(StackPanel owner) : base(owner)
        {
        }

        /// <inheritdoc />
        public override void Add(Control? value)
        {
            if (value is not StackPage page)
            {
                throw new ArgumentException("Only StackPage controls can be added.", nameof(value));
            }

            base.Add(value);
            ((StackPanel)Owner).ConfigureAddedPage(page);
        }

        /// <inheritdoc />
        public override void Remove(Control? value)
        {
            StackPanel owner = (StackPanel)Owner;
            int removedIndex = value is StackPage page ? owner._pages.IndexOf(page) : -1;
            bool wasSelected = removedIndex >= 0 && removedIndex == owner._selectedIndex;

            base.Remove(value);
            if (value is StackPage && removedIndex >= 0)
            {
                owner.ConfigureRemovedPage(removedIndex, wasSelected);
            }
        }
    }

    /// <summary>
    ///  Represents the collection of pages in a <see cref="StackPanel"/>.
    /// </summary>
    public sealed class StackPageCollection : IList
    {
        private readonly StackPanel _owner;

        internal StackPageCollection(StackPanel owner) => _owner = owner;

        /// <summary>
        ///  Gets the page at the specified index.
        /// </summary>
        public StackPage this[int index]
        {
            get => (StackPage)_owner.Controls[index];
            set
            {
                ArgumentNullException.ThrowIfNull(value);
                RemoveAt(index);
                Insert(index, value);
            }
        }

        object? IList.this[int index]
        {
            get => this[index];
            set
            {
                if (value is not StackPage page)
                {
                    throw new ArgumentException(null, nameof(value));
                }

                this[index] = page;
            }
        }

        /// <summary>
        ///  Gets the number of pages.
        /// </summary>
        [Browsable(false)]
        public int Count => _owner.Controls.Count;

        bool IList.IsReadOnly => false;
        bool IList.IsFixedSize => false;
        bool ICollection.IsSynchronized => false;
        object ICollection.SyncRoot => this;

        /// <summary>
        ///  Adds a page to the collection.
        /// </summary>
        public void Add(StackPage page) => _owner.AddPage(page);

        int IList.Add(object? value)
        {
            if (value is not StackPage page)
            {
                throw new ArgumentException(null, nameof(value));
            }

            Add(page);
            return Count - 1;
        }

        /// <summary>
        ///  Adds multiple pages to the collection.
        /// </summary>
        public void AddRange(params StackPage[] pages)
        {
            ArgumentNullException.ThrowIfNull(pages);

            foreach (StackPage page in pages)
            {
                Add(page);
            }
        }

        /// <summary>
        ///  Removes all pages from the collection.
        /// </summary>
        public void Clear()
        {
            while (Count > 0)
            {
                RemoveAt(Count - 1);
            }
        }

        /// <summary>
        ///  Gets whether the collection contains the specified page.
        /// </summary>
        public bool Contains(StackPage page) => IndexOf(page) >= 0;

        bool IList.Contains(object? value) => value is StackPage page && Contains(page);

        /// <summary>
        ///  Gets the index of the specified page.
        /// </summary>
        public int IndexOf(StackPage page)
        {
            ArgumentNullException.ThrowIfNull(page);
            return _owner.Controls.IndexOf(page);
        }

        int IList.IndexOf(object? value) => value is StackPage page ? IndexOf(page) : -1;

        /// <summary>
        ///  Inserts a page at the specified index.
        /// </summary>
        public void Insert(int index, StackPage page)
        {
            ArgumentNullException.ThrowIfNull(page);
            int selectedIndex = _owner._selectedIndex;

            _owner.Controls.Add(page);
            _owner.Controls.SetChildIndex(page, index);

            if (selectedIndex >= index)
            {
                _owner._selectedIndex++;
            }
        }

        void IList.Insert(int index, object? value)
        {
            if (value is not StackPage page)
            {
                throw new ArgumentException(null, nameof(value));
            }

            Insert(index, page);
        }

        /// <summary>
        ///  Removes the specified page.
        /// </summary>
        public void Remove(StackPage page)
        {
            ArgumentNullException.ThrowIfNull(page);
            _owner.RemovePage(page);
        }

        void IList.Remove(object? value)
        {
            if (value is StackPage page)
            {
                Remove(page);
            }
        }

        /// <summary>
        ///  Removes the page at the specified index.
        /// </summary>
        public void RemoveAt(int index) => Remove(this[index]);

        void ICollection.CopyTo(Array array, int index)
            => ((ICollection)_owner.Controls).CopyTo(array, index);

        public IEnumerator GetEnumerator() => _owner.Controls.GetEnumerator();
    }
}
