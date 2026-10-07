// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.ComponentModel;

namespace System.Windows.Forms;

public partial class DomainUpDown
{
    /// <summary>
    ///  Encapsulates a collection of objects for use by the <see cref="DomainUpDown"/>
    ///  class.
    /// </summary>
    public class DomainUpDownItemCollection : ArrayList, IList<object>
    {
        private readonly DomainUpDown _owner;

        internal DomainUpDownItemCollection(DomainUpDown owner)
            : base()
        {
            _owner = owner;
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override object? this[int index]
        {
            get
            {
                return base[index];
            }

            set
            {
                base[index] = value;

                if (_owner.SelectedIndex == index)
                {
                    _owner.SelectIndex(index);
                }

                if (_owner.Sorted)
                {
                    _owner.SortDomainItems();
                }
            }
        }

        /// <summary>
        /// </summary>
        public override int Add(object? item)
        {
            // Overridden to perform sorting after adding an item

            int ret = base.Add(item);
            if (_owner.Sorted)
            {
                _owner.SortDomainItems();
            }

            return ret;
        }

        /// <summary>
        /// </summary>
        public override void Remove(object? item)
        {
            int index = IndexOf(item);

            if (index == -1)
            {
                throw new ArgumentOutOfRangeException(nameof(item), item, string.Format(SR.InvalidArgument, nameof(item), item));
            }
            else
            {
                RemoveAt(index);
            }
        }

        /// <summary>
        /// </summary>
        public override void RemoveAt(int item)
        {
            // Overridden to update the domain index if necessary
            base.RemoveAt(item);

            if (item < _owner._domainIndex)
            {
                // The item removed was before the currently selected item
                _owner.SelectIndex(_owner._domainIndex - 1);
            }
            else if (item == _owner._domainIndex)
            {
                // The currently selected item was removed
                _owner.SelectIndex(-1);
            }
        }

        /// <summary>
        /// </summary>
        public override void Insert(int index, object? item)
        {
            base.Insert(index, item);
            if (_owner.Sorted)
            {
                _owner.SortDomainItems();
            }
        }

        object IList<object>.this[int index]
        {
            get => this[index]!;
            set => this[index] = value;
        }

        int ICollection<object>.Count => Count;

        bool ICollection<object>.IsReadOnly => IsReadOnly;

        void ICollection<object>.Add(object item) => Add(item);

        void ICollection<object>.Clear() => Clear();

        bool ICollection<object>.Contains(object item) => Contains(item);

        void ICollection<object>.CopyTo(object[] array, int arrayIndex)
        {
            for (int i = 0; i < Count; i++)
            {
                array[arrayIndex + i] = this[i]!;
            }
        }

        bool ICollection<object>.Remove(object item)
        {
            int index = IndexOf(item);
            if (index < 0)
            {
                return false;
            }

            RemoveAt(index);
            return true;
        }

        IEnumerator<object> IEnumerable<object>.GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
            {
                yield return this[i]!;
            }
        }

        int IList<object>.IndexOf(object item) => IndexOf(item);

        void IList<object>.Insert(int index, object item) => Insert(index, item);

        void IList<object>.RemoveAt(int index) => RemoveAt(index);
    }
}
