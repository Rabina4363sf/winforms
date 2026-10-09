// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Windows.Forms.Primitives.Resources;

namespace System.Windows.Forms;

/// <summary>
///  Collection that protects against inserting null objects.
/// </summary>
/// <typeparam name="T">The reference type of the elements in the collection.</typeparam>
/// <remarks>
///  <para>
///   Null elements are rejected before calling validation or mutation hooks, even when
///   <see cref="OnValidate"/> is overridden without calling the base implementation.
///  </para>
///  <para>
///   Mutation hooks follow the order used by <see cref="CollectionBase"/>. Insertion, removal,
///   and replacement are rolled back if a completion hook throws. Clearing is not rolled back
///   if <see cref="OnClearComplete"/> throws. Overrides should not modify this collection from a hook.
///  </para>
/// </remarks>
/// <example>
///  <code>
///   public sealed class NameCollection : NonNullCollection&lt;string&gt;
///   {
///   }
///
///   NameCollection names = new();
///   names.Add("First");
///  </code>
/// </example>
[DebuggerDisplay("{DebuggerDisplay,nq}")]
public abstract class NonNullCollection<T>
    : IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable, IList, ICollection, IReadOnlyList<T>, IReadOnlyCollection<T>
    where T : class
{
    private readonly List<T> _list = [];

    /// <summary>
    ///  Initializes an empty collection.
    /// </summary>
    public NonNullCollection() { }

    /// <summary>
    ///  Initializes a collection containing the specified elements.
    /// </summary>
    /// <param name="items">The elements to add to the collection.</param>
    /// <exception cref="ArgumentNullException">
    ///  <paramref name="items"/> is null or contains a null element.
    /// </exception>
    /// <remarks>
    ///  <para>Calls the validation and insertion hooks for each element during construction.</para>
    /// </remarks>
    public NonNullCollection(IEnumerable<T> items) => AddRange(items);

    /// <summary>
    ///  Gets or sets the element at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the element.</param>
    /// <exception cref="ArgumentNullException">The assigned value is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    ///  <paramref name="index"/> is outside the collection.
    /// </exception>
    public T this[int index]
    {
        get => _list[index];
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            T oldValue = _list[index];
            OnValidate(value);
            OnSet(index, oldValue, value);
            _list[index] = value;
            try
            {
                OnSetComplete(index, oldValue, value);
                ItemAdded(value);
            }
            catch
            {
                _list[index] = oldValue;
                throw;
            }
        }
    }

    /// <inheritdoc/>
    public int Count => _list.Count;

    /// <inheritdoc/>
    public bool IsReadOnly => ((ICollection<T>)_list).IsReadOnly;

    /// <summary>
    ///  Adds an element to the end of the collection.
    /// </summary>
    /// <param name="item">The element to add.</param>
    /// <exception cref="ArgumentNullException"><paramref name="item"/> is null.</exception>
    public void Add(T item) => Insert(Count, item);

    /// <summary>
    ///  Removes all elements from the collection.
    /// </summary>
    /// <remarks>
    ///  <para>If <see cref="OnClearComplete"/> throws, the collection remains empty.</para>
    /// </remarks>
    public void Clear()
    {
        OnClear();
        _list.Clear();
        OnClearComplete();
    }

    /// <inheritdoc/>
    public bool Contains(T item) => _list.Contains(item);

    /// <inheritdoc/>
    public void CopyTo(T[] array, int arrayIndex) => _list.CopyTo(array, arrayIndex);

    /// <inheritdoc/>
    public IEnumerator<T> GetEnumerator() => _list.GetEnumerator();

    /// <inheritdoc/>
    public int IndexOf(T item) => _list.IndexOf(item);

    /// <summary>
    ///  Inserts an element at the specified index.
    /// </summary>
    /// <param name="index">The zero-based insertion index.</param>
    /// <param name="item">The element to insert.</param>
    /// <exception cref="ArgumentNullException"><paramref name="item"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    ///  <paramref name="index"/> is less than zero or greater than <see cref="Count"/>.
    /// </exception>
    public void Insert(int index, T item)
    {
        ArgumentNullException.ThrowIfNull(item);
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(index, Count);
        OnValidate(item);
        OnInsert(index, item);
        _list.Insert(index, item);
        try
        {
            OnInsertComplete(index, item);
            ItemAdded(item);
        }
        catch
        {
            _list.RemoveAt(index);
            throw;
        }
    }

    /// <inheritdoc/>
    public bool Remove(T item)
    {
        int index = IndexOf(item);
        if (index < 0)
        {
            return false;
        }

        RemoveAt(index);
        return true;
    }

    /// <summary>
    ///  Removes the element at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the element to remove.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    ///  <paramref name="index"/> is outside the collection.
    /// </exception>
    public void RemoveAt(int index)
    {
        T item = _list[index];
        OnValidate(item);
        OnRemove(index, item);
        _list.RemoveAt(index);
        try
        {
            OnRemoveComplete(index, item);
        }
        catch
        {
            _list.Insert(index, item);
            throw;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_list).GetEnumerator();

    /// <summary>
    ///  Adds the items in <paramref name="items"/> to the collection.
    /// </summary>
    /// <param name="items">The elements to add.</param>
    /// <exception cref="ArgumentNullException">
    ///  <paramref name="items"/> is null or contains a null element.
    /// </exception>
    /// <remarks>
    ///  <para>
    ///   The input is enumerated once and checked for null elements before any items are added.
    ///   Each item is then added through <see cref="Add"/>, including its mutation hooks.
    ///   If a hook throws, previously added items remain in the collection.
    ///  </para>
    /// </remarks>
    public void AddRange(IEnumerable<T> items)
    {
        ArgumentNullException.ThrowIfNull(items);
        List<T> newItems = [.. items];
        foreach (T item in newItems)
        {
            ArgumentNullException.ThrowIfNull(item, nameof(items));
        }

        foreach (T item in newItems)
        {
            Add(item);
        }
    }

    int IList.Add(object? value)
    {
        T item = GetItem(value);
        int index = Count;
        Add(item);
        return index;
    }

    bool IList.Contains(object? value) => ((IList)_list).Contains(value);

    int IList.IndexOf(object? value) => ((IList)_list).IndexOf(value);

    void IList.Insert(int index, object? value) => Insert(index, GetItem(value));

    void IList.Remove(object? value)
    {
        if (value is T item)
        {
            Remove(item);
        }
    }

    object? IList.this[int index]
    {
        get => this[index];
        set => this[index] = GetItem(value);
    }

    void ICollection.CopyTo(Array array, int index) => ((IList)_list).CopyTo(array, index);

    private string DebuggerDisplay => $"Count: {Count}";

    bool IList.IsFixedSize => ((IList)_list).IsFixedSize;

    object ICollection.SyncRoot => ((ICollection)_list).SyncRoot;

    bool ICollection.IsSynchronized => ((ICollection)_list).IsSynchronized;

    /// <summary>
    ///  Performs additional validation before inserting, replacing, or removing an element.
    /// </summary>
    /// <param name="value">The element to validate.</param>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
    protected virtual void OnValidate(T value) => ArgumentNullException.ThrowIfNull(value);

    /// <summary>
    ///  Performs additional actions before clearing the collection.
    /// </summary>
    protected virtual void OnClear()
    {
    }

    /// <summary>
    ///  Performs additional actions after clearing the collection.
    /// </summary>
    protected virtual void OnClearComplete()
    {
    }

    /// <summary>
    ///  Performs additional actions before inserting an element.
    /// </summary>
    /// <param name="index">The zero-based insertion index.</param>
    /// <param name="value">The element to insert.</param>
    protected virtual void OnInsert(int index, T value)
    {
    }

    /// <summary>
    ///  Performs additional actions after inserting an element.
    /// </summary>
    /// <param name="index">The zero-based index of the inserted element.</param>
    /// <param name="value">The inserted element.</param>
    protected virtual void OnInsertComplete(int index, T value)
    {
    }

    /// <summary>
    ///  Performs additional actions before removing an element.
    /// </summary>
    /// <param name="index">The zero-based index of the element to remove.</param>
    /// <param name="value">The element to remove.</param>
    protected virtual void OnRemove(int index, T value)
    {
    }

    /// <summary>
    ///  Performs additional actions after removing an element.
    /// </summary>
    /// <param name="index">The former zero-based index of the removed element.</param>
    /// <param name="value">The removed element.</param>
    protected virtual void OnRemoveComplete(int index, T value)
    {
    }

    /// <summary>
    ///  Performs additional actions before replacing an element.
    /// </summary>
    /// <param name="index">The zero-based index of the element to replace.</param>
    /// <param name="oldValue">The element being replaced.</param>
    /// <param name="newValue">The replacement element.</param>
    protected virtual void OnSet(int index, T oldValue, T newValue)
    {
    }

    /// <summary>
    ///  Performs additional actions after replacing an element.
    /// </summary>
    /// <param name="index">The zero-based index of the replaced element.</param>
    /// <param name="oldValue">The replaced element.</param>
    /// <param name="newValue">The replacement element.</param>
    protected virtual void OnSetComplete(int index, T oldValue, T newValue)
    {
    }

    /// <summary>
    ///  Performs additional actions after inserting or replacing an element and its completion hook.
    /// </summary>
    /// <param name="item">The inserted or replacement element.</param>
    protected virtual void ItemAdded(T item)
    {
    }

    private static T GetItem(object? value)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (value is not T item)
        {
            throw new ArgumentException(string.Format(SR.NonNullCollectionInvalidType, value, typeof(T)), nameof(value));
        }

        return item;
    }
}
