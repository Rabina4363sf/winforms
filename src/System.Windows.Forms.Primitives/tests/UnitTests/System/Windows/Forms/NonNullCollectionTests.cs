// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;

namespace System.Windows.Forms.Tests;

public class NonNullCollectionTests
{
    [Fact]
    public void NonNullCollection_IsPublic()
    {
        Assert.True(typeof(NonNullCollection<>).IsPublic);
    }

    [Fact]
    public void NonNullCollection_IsForwardedFromWindowsForms()
    {
        Assert.Contains(typeof(NonNullCollection<>), typeof(Control).Assembly.GetForwardedTypes());
    }

    [Fact]
    public void NonNullCollection_AddRange_EnumeratesOnce()
    {
        TestCollection collection = new();
        object item = new();
        int enumerations = 0;

        collection.AddRange(GetItems());

        Assert.Equal(1, enumerations);
        Assert.Same(item, Assert.Single(collection));
        Assert.Equal(1, collection.AddCount);

        IEnumerable<object> GetItems()
        {
            enumerations++;
            yield return item;
        }
    }

    [Fact]
    public void NonNullCollection_AddRange_WithSelf()
    {
        object item = new();
        TestCollection collection = new([item]);

        collection.AddRange(collection);

        Assert.Equal(new[] { item, item }, collection);
        Assert.Equal(2, collection.AddCount);
    }

    [Fact]
    public void NonNullCollection_AddRange_WithNull_DoesNotAddItems()
    {
        object original = new();
        TestCollection collection = new([original]);

        Assert.Throws<ArgumentNullException>("items", () => collection.AddRange([new(), null!]));

        Assert.Same(original, Assert.Single(collection));
        Assert.Equal(1, collection.AddCount);
    }

    [Fact]
    public void NonNullCollection_Constructor_ThrowsWithNull()
    {
        Assert.Throws<ArgumentNullException>("items", () => new TestCollection(null!));
    }

    [Fact]
    public void NonNullCollection_Constructor_ThrowsWithNullInCollection()
    {
        Assert.Throws<ArgumentNullException>("items", () => new TestCollection(new object[] { null! }));
    }

    [Fact]
    public void NonNullCollection_Add_ThrowsWithNull()
    {
        TestCollection collection = new();
        Assert.Throws<ArgumentNullException>("item", () => collection.Add(null!));
    }

    [Fact]
    public void NonNullCollection_Add_CallsItemAdded()
    {
        TestCollection collection = new();
        object item = new();
        collection.Add(item);
        Assert.Same(item, collection.LastAdded);
        Assert.Equal(1, collection.AddCount);
    }

    [Fact]
    public void NonNullCollection_IListAdd_ThrowsWithNull()
    {
        TestCollection collection = new();
        Assert.Throws<ArgumentNullException>("value", () => ((IList)collection).Add(null));
    }

    [Fact]
    public void NonNullCollection_IListAdd_CallsItemAdded()
    {
        TestCollection collection = new();
        object item = new();
        ((IList)collection).Add(item);
        Assert.Same(item, collection.LastAdded);
        Assert.Equal(1, collection.AddCount);
    }

    [Fact]
    public void NonNullCollection_Indexer_ThrowsWithNull()
    {
        TestCollection collection = new() { new() };
        Assert.Throws<ArgumentNullException>("value", () => collection[0] = null!);
    }

    [Fact]
    public void NonNullCollection_Indexer_CallsItemAdded()
    {
        TestCollection collection = new(new object[] { new() });
        object item = new();
        collection[0] = item;
        Assert.Same(item, collection.LastAdded);
        Assert.Equal(2, collection.AddCount);
    }

    [Fact]
    public void NonNullCollection_IListIndexer_ThrowsWithNull()
    {
        TestCollection collection = new() { new() };
        Assert.Throws<ArgumentNullException>("value", () => ((IList)collection)[0] = null);
    }

    [Fact]
    public void NonNullCollection_IListIndexer_CallsItemAdded()
    {
        TestCollection collection = new(new object[] { new() });
        object item = new();
        ((IList)collection)[0] = item;
        Assert.Same(item, collection.LastAdded);
        Assert.Equal(2, collection.AddCount);
    }

    [Fact]
    public void NonNullCollection_Insert_ThrowsWithNull()
    {
        TestCollection collection = new();
        Assert.Throws<ArgumentNullException>("item", () => collection.Insert(0, null!));
    }

    [Fact]
    public void NonNullCollection_Insert_CallsItemAdded()
    {
        TestCollection collection = new();
        object item = new();
        collection.Insert(0, item);
        Assert.Same(item, collection.LastAdded);
        Assert.Equal(1, collection.AddCount);
    }

    [Fact]
    public void NonNullCollection_IListInsert_ThrowsWithNull()
    {
        TestCollection collection = new();
        Assert.Throws<ArgumentNullException>("value", () => ((IList)collection).Insert(0, null));
    }

    [Fact]
    public void NonNullCollection_IListInsert_CallsItemAdded()
    {
        TestCollection collection = new();
        object item = new();
        ((IList)collection).Insert(0, item);
        Assert.Same(item, collection.LastAdded);
        Assert.Equal(1, collection.AddCount);
    }

    [Fact]
    public void NonNullCollection_AddRange_ThrowsWithNull()
    {
        TestCollection collection = new();
        Assert.Throws<ArgumentNullException>("items", () => collection.AddRange(null!));
    }

    [Fact]
    public void NonNullCollection_AddRange_ThrowsWithNullInCollection()
    {
        TestCollection collection = new();
        Assert.Throws<ArgumentNullException>("items", () => collection.AddRange(new object[] { null! }));
    }

    [Fact]
    public void NonNullCollection_AddRange_CallsItemAdded()
    {
        TestCollection collection = new();
        object item = new();
        collection.AddRange(new object[] { item });
        Assert.Same(item, collection.LastAdded);
        Assert.Equal(1, collection.AddCount);

        collection.AddRange(new object[] { new(), new(), new() });
        Assert.Equal(4, collection.AddCount);
    }

    [Theory]
    [InlineData("Add", "Validate,Insert,InsertComplete,ItemAdded")]
    [InlineData("Insert", "Validate,Insert,InsertComplete,ItemAdded")]
    [InlineData("Set", "Validate,Set,SetComplete,ItemAdded")]
    [InlineData("Remove", "Validate,Remove,RemoveComplete")]
    [InlineData("RemoveAt", "Validate,Remove,RemoveComplete")]
    [InlineData("Clear", "Clear,ClearComplete")]
    public void NonNullCollection_Mutation_CallsHooks(string operation, string expectedHooks)
    {
        foreach (bool nonGeneric in new[] { false, true })
        {
            HookCollection collection = new() { "old" };
            collection.Calls.Clear();

            ApplyMutation(collection, operation, nonGeneric);

            Assert.Equal(expectedHooks.Split(','), collection.Calls.Select(call => call.Name));
            int expectedCount = operation switch
            {
                "Add" or "Insert" => 2,
                "Set" => 1,
                _ => 0
            };
            Assert.Equal(expectedCount, collection.Count);

            foreach (var call in collection.Calls)
            {
                Assert.Equal(call.Name.EndsWith("Complete", StringComparison.Ordinal) || call.Name == "ItemAdded" ? expectedCount : 1, call.Count);
                if (call.Name is "Validate" or "ItemAdded" or "Clear" or "ClearComplete")
                {
                    continue;
                }

                Assert.Equal(operation == "Add" ? 1 : 0, call.Index);
                Assert.Equal(operation is "Set" or "Remove" or "RemoveAt" ? "old" : null, call.OldValue);
                Assert.Equal(operation is "Add" or "Insert" or "Set" ? "new" : null, call.NewValue);
            }
        }
    }

    [Theory]
    [InlineData("Add", "Validate")]
    [InlineData("Add", "Insert")]
    [InlineData("Add", "InsertComplete")]
    [InlineData("Add", "ItemAdded")]
    [InlineData("Insert", "Insert")]
    [InlineData("Insert", "InsertComplete")]
    [InlineData("Set", "Validate")]
    [InlineData("Set", "Set")]
    [InlineData("Set", "SetComplete")]
    [InlineData("Set", "ItemAdded")]
    [InlineData("Remove", "Validate")]
    [InlineData("Remove", "Remove")]
    [InlineData("Remove", "RemoveComplete")]
    [InlineData("RemoveAt", "RemoveComplete")]
    [InlineData("Clear", "Clear")]
    public void NonNullCollection_Mutation_HookThrows_PreservesItems(string operation, string failingHook)
    {
        foreach (bool nonGeneric in new[] { false, true })
        {
            HookCollection collection = new() { "old" };
            InvalidOperationException exception = new();
            collection.Callback = name =>
            {
                if (name == failingHook)
                {
                    throw exception;
                }
            };

            Assert.Same(exception, Assert.Throws<InvalidOperationException>(() => ApplyMutation(collection, operation, nonGeneric)));
            Assert.Equal("old", Assert.Single(collection));
        }
    }

    [Fact]
    public void NonNullCollection_Clear_CompleteThrows_RemainsEmpty()
    {
        HookCollection collection = new() { "old" };
        collection.Callback = name =>
        {
            if (name == "ClearComplete")
            {
                throw new InvalidOperationException();
            }
        };

        Assert.Throws<InvalidOperationException>(collection.Clear);

        Assert.Empty(collection);
    }

    [Theory]
    [InlineData("Add")]
    [InlineData("Insert")]
    [InlineData("Set")]
    public void NonNullCollection_Mutation_NullCannotBypassValidation(string operation)
    {
        foreach (bool nonGeneric in new[] { false, true })
        {
            HookCollection collection = new() { "old" };
            collection.Calls.Clear();

            Assert.Throws<ArgumentNullException>(() => ApplyMutation(collection, operation, nonGeneric, null));

            Assert.Equal("old", Assert.Single(collection));
            Assert.Empty(collection.Calls);
        }
    }

    [Theory]
    [InlineData("Add")]
    [InlineData("Insert")]
    [InlineData("Set")]
    public void NonNullCollection_IListMutation_InvalidType_DoesNotCallHooks(string operation)
    {
        HookCollection collection = new() { "old" };
        collection.Calls.Clear();
        IList list = collection;

        Assert.Throws<ArgumentException>("value", () =>
        {
            switch (operation)
            {
                case "Add":
                    list.Add(new object());
                    break;
                case "Insert":
                    list.Insert(0, new object());
                    break;
                case "Set":
                    list[0] = new object();
                    break;
            }
        });

        Assert.Equal("old", Assert.Single(collection));
        Assert.Empty(collection.Calls);
    }

    [Theory]
    [InlineData("Insert", -1)]
    [InlineData("Insert", 2)]
    [InlineData("Set", -1)]
    [InlineData("Set", 1)]
    [InlineData("RemoveAt", -1)]
    [InlineData("RemoveAt", 1)]
    public void NonNullCollection_Mutation_InvalidIndex_DoesNotCallHooks(string operation, int index)
    {
        foreach (bool nonGeneric in new[] { false, true })
        {
            HookCollection collection = new() { "old" };
            collection.Calls.Clear();

            Assert.Throws<ArgumentOutOfRangeException>(() => ApplyMutation(collection, operation, nonGeneric, index: index));

            Assert.Equal("old", Assert.Single(collection));
            Assert.Empty(collection.Calls);
        }
    }

    [Fact]
    public void NonNullCollection_Remove_MissingItem_DoesNotCallHooks()
    {
        HookCollection collection = new() { "old" };
        collection.Calls.Clear();

        Assert.False(collection.Remove("missing"));
        Assert.False(collection.Remove(null!));
        ((IList)collection).Remove("missing");
        ((IList)collection).Remove(new object());
        ((IList)collection).Remove(null);

        Assert.Equal("old", Assert.Single(collection));
        Assert.Empty(collection.Calls);
    }

    [Fact]
    public void NonNullCollection_AddRange_CallsHooksForEachItem()
    {
        HookCollection collection = new();

        collection.AddRange(["first", "second"]);

        Assert.Equal(new[] { "first", "second" }, collection);
        Assert.Equal(
            new[] { "Validate", "Insert", "InsertComplete", "ItemAdded", "Validate", "Insert", "InsertComplete", "ItemAdded" },
            collection.Calls.Select(call => call.Name));
    }

    [Fact]
    public void NonNullCollection_AddRange_Null_DoesNotCallHooks()
    {
        HookCollection collection = new();

        Assert.Throws<ArgumentNullException>("items", () => collection.AddRange(["first", null!]));

        Assert.Empty(collection);
        Assert.Empty(collection.Calls);
    }

    [Fact]
    public void NonNullCollection_AddRange_EnumerationThrows_DoesNotAddItems()
    {
        HookCollection collection = new();
        InvalidOperationException exception = new();

        Assert.Same(exception, Assert.Throws<InvalidOperationException>(() => collection.AddRange(GetItems())));

        Assert.Empty(collection);
        Assert.Empty(collection.Calls);

        IEnumerable<string> GetItems()
        {
            yield return "first";
            throw exception;
        }
    }

    [Fact]
    public void NonNullCollection_AddRange_CompletionThrows_PreservesEarlierItems()
    {
        HookCollection collection = new();
        collection.Callback = name =>
        {
            if (name == "InsertComplete" && collection.Count == 2)
            {
                throw new InvalidOperationException();
            }
        };

        Assert.Throws<InvalidOperationException>(() => collection.AddRange(["first", "second"]));

        Assert.Equal("first", Assert.Single(collection));
    }

    [Fact]
    public void NonNullCollection_GenericInterfaces_UseCollectionStorageAndHooks()
    {
        HookCollection collection = new();
        ICollection<string> items = collection;
        IList<string> list = collection;
        IReadOnlyList<string> readOnlyList = collection;

        items.Add("first");
        list.Insert(0, "second");
        list[1] = "third";

        Assert.Equal(new[] { "second", "third" }, collection);
        Assert.Equal("second", readOnlyList[0]);
        Assert.Equal(2, ((IReadOnlyCollection<string>)collection).Count);
        Assert.False(items.IsReadOnly);
        Assert.True(items.Contains("third"));
        Assert.Equal(1, list.IndexOf("third"));
        string[] copy = new string[2];
        items.CopyTo(copy, 0);
        Assert.Equal(new[] { "second", "third" }, copy);
        Assert.Equal(3, collection.Calls.Count(call => call.Name == "ItemAdded"));

        Assert.True(items.Remove("second"));
        list.RemoveAt(0);
        items.Clear();

        Assert.Empty(collection);
    }

    [Fact]
    public void NonNullCollection_IListAdd_ReturnsInsertedIndex()
    {
        HookCollection collection = new() { "old" };

        Assert.Equal(1, ((IList)collection).Add("new"));
        Assert.Equal(new[] { "old", "new" }, collection);
    }

    private static void ApplyMutation(HookCollection collection, string operation, bool nonGeneric, string? value = "new", int index = 0)
    {
        if (nonGeneric)
        {
            IList list = collection;
            switch (operation)
            {
                case "Add":
                    list.Add(value);
                    break;
                case "Insert":
                    list.Insert(index, value);
                    break;
                case "Set":
                    list[index] = value;
                    break;
                case "Remove":
                    list.Remove("old");
                    break;
                case "RemoveAt":
                    list.RemoveAt(index);
                    break;
                case "Clear":
                    list.Clear();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(operation));
            }
        }
        else
        {
            switch (operation)
            {
                case "Add":
                    collection.Add(value!);
                    break;
                case "Insert":
                    collection.Insert(index, value!);
                    break;
                case "Set":
                    collection[index] = value!;
                    break;
                case "Remove":
                    collection.Remove("old");
                    break;
                case "RemoveAt":
                    collection.RemoveAt(index);
                    break;
                case "Clear":
                    collection.Clear();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(operation));
            }
        }
    }

    /// <summary>
    ///  Records mutation hooks without relying on the default validation implementation.
    /// </summary>
    private class HookCollection : NonNullCollection<string>
    {
        public List<(string Name, int Index, string? OldValue, string? NewValue, int Count)> Calls { get; } = [];

        public Action<string>? Callback { get; set; }

        protected override void OnValidate(string value) => Record("Validate", newValue: value);

        protected override void OnClear() => Record("Clear");

        protected override void OnClearComplete() => Record("ClearComplete");

        protected override void OnInsert(int index, string value) => Record("Insert", index, newValue: value);

        protected override void OnInsertComplete(int index, string value) => Record("InsertComplete", index, newValue: value);

        protected override void OnRemove(int index, string value) => Record("Remove", index, oldValue: value);

        protected override void OnRemoveComplete(int index, string value) => Record("RemoveComplete", index, oldValue: value);

        protected override void OnSet(int index, string oldValue, string newValue) => Record("Set", index, oldValue, newValue);

        protected override void OnSetComplete(int index, string oldValue, string newValue) => Record("SetComplete", index, oldValue, newValue);

        protected override void ItemAdded(string item) => Record("ItemAdded", newValue: item);

        private void Record(string name, int index = -1, string? oldValue = null, string? newValue = null)
        {
            Calls.Add((name, index, oldValue, newValue, Count));
            Callback?.Invoke(name);
        }
    }

    /// <summary>
    ///  Tracks the existing item-added notification.
    /// </summary>
    private class TestCollection : NonNullCollection<object>
    {
        public object? LastAdded { get; set; }

        public int AddCount { get; set; }

        public TestCollection()
        {
        }

        public TestCollection(IEnumerable<object> items) : base(items)
        {
        }

        protected override void ItemAdded(object item)
        {
            LastAdded = item;
            AddCount++;
            base.ItemAdded(item);
        }
    }
}
