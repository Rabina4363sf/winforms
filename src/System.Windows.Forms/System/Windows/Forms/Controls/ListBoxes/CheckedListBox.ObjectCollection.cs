// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Windows.Forms;

public partial class CheckedListBox
{
    public new class ObjectCollection : ListBox.ObjectCollection
    {
        private readonly CheckedListBox _owner;
        private readonly List<(WeakReference<object> Item, CheckState CheckState)> _clearedItemStates = [];

        public ObjectCollection(CheckedListBox owner) : base(owner)
        {
            _owner = owner;
        }

        internal override void OnItemsClearing()
        {
            RemoveCollectedItemStates();

            List<(WeakReference<object> Item, CheckState CheckState)> itemStates = new(Count);
            for (int i = 0; i < Count; i++)
            {
                itemStates.Add((new(this[i]), _owner.GetItemCheckState(i)));
            }

            _clearedItemStates.InsertRange(0, itemStates);
        }

        internal override int AddInternal(object item)
        {
            int index = base.AddInternal(item);

            for (int i = 0; i < _clearedItemStates.Count; i++)
            {
                var (clearedItemReference, checkState) = _clearedItemStates[i];
                if (!clearedItemReference.TryGetTarget(out object? clearedItem))
                {
                    _clearedItemStates.RemoveAt(i--);
                }
                else if (Equals(clearedItem, item))
                {
                    _clearedItemStates.RemoveAt(i);
                    _owner.SetItemCheckState(index, checkState);
                    break;
                }
            }

            return index;
        }

        private void RemoveCollectedItemStates()
        {
            for (int i = _clearedItemStates.Count - 1; i >= 0; i--)
            {
                if (!_clearedItemStates[i].Item.TryGetTarget(out _))
                {
                    _clearedItemStates.RemoveAt(i);
                }
            }
        }

        /// <summary>
        ///  Lets the user add an item to the listbox with the given initial value
        ///  for the Checked portion of the item.
        /// </summary>
        public int Add(object item, bool isChecked)
        {
            return Add(item, isChecked ? CheckState.Checked : CheckState.Unchecked);
        }

        /// <summary>
        ///  Lets the user add an item to the listbox with the given initial value
        ///  for the Checked portion of the item.
        /// </summary>
        public int Add(object item, CheckState check)
        {
            // validate the enum that's passed in here
            //
            // Valid values are 0-2 inclusive.
            SourceGenerated.EnumValidator.Validate(check, nameof(check));

            int index = Add(item);
            _owner.SetItemCheckState(index, check);

            return index;
        }
    }
}
