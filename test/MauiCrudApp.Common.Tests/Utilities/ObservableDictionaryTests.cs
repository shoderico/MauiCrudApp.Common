using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using FluentAssertions;
using MauiCrudApp.Common.Utilities;
using Xunit;

namespace MauiCrudApp.Common.Tests.Utilities
{
    public class ObservableDictionaryTests
    {
        // コンストラクタ
        [Fact]
        public void Constructor_Empty_CreatesEmptyDictionary()
        {
            // Arrange & Act
            var dict = new ObservableDictionary<string, int>();

            // Assert
            dict.Should().BeEmpty();
            dict.Count.Should().Be(0);
        }

        [Fact]
        public void Constructor_WithDictionary_CopiesItems()
        {
            // Arrange
            var initial = new Dictionary<string, int> { { "A", 1 }, { "B", 2 } };

            // Act
            var dict = new ObservableDictionary<string, int>(initial);

            // Assert
            dict.Should().HaveCount(2);
            dict["A"].Should().Be(1);
            dict["B"].Should().Be(2);
        }

        [Fact]
        public void Constructor_NullDictionary_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new ObservableDictionary<string, int>(null));
        }

        // プロパティとインデクサ
        [Fact]
        public void Indexer_Get_ExistingKey_ReturnsValue()
        {
            // Arrange
            var dict = new ObservableDictionary<string, int> { { "Key1", 42 } };

            // Act
            var value = dict["Key1"];

            // Assert
            value.Should().Be(42);
        }

        [Fact]
        public void Indexer_Get_NonExistingKey_ThrowsKeyNotFoundException()
        {
            // Arrange
            var dict = new ObservableDictionary<string, int>();

            // Act & Assert
            Assert.Throws<KeyNotFoundException>(() => dict["NonExistent"]);
        }

        [Fact]
        public void Indexer_Set_NewKey_TriggersAdd()
        {
            // Arrange
            var dict = new ObservableDictionary<string, int>();
            NotifyCollectionChangedEventArgs? eventArgs = null;
            dict.CollectionChanged += (s, e) => eventArgs = e;

            // Act
            dict["Key1"] = 42;

            // Assert
            dict["Key1"].Should().Be(42);
            eventArgs.Should().NotBeNull();
            eventArgs!.Action.Should().Be(NotifyCollectionChangedAction.Add);
            var newItem = (KeyValuePair<string, int>)eventArgs.NewItems![0];
            newItem.Should().Be(new KeyValuePair<string, int>("Key1", 42));
        }

        [Fact]
        public void Indexer_Set_ExistingKey_TriggersReplace()
        {
            // Arrange
            var dict = new ObservableDictionary<string, int> { { "Key1", 42 } };
            NotifyCollectionChangedEventArgs? eventArgs = null;
            dict.CollectionChanged += (s, e) => eventArgs = e;

            // Act
            dict["Key1"] = 100;

            // Assert
            dict["Key1"].Should().Be(100);
            eventArgs.Should().NotBeNull();
            eventArgs!.Action.Should().Be(NotifyCollectionChangedAction.Replace);
            var newItem = (KeyValuePair<string, int>)eventArgs.NewItems![0];
            var oldItem = (KeyValuePair<string, int>)eventArgs.OldItems![0];
            newItem.Should().Be(new KeyValuePair<string, int>("Key1", 100));
            oldItem.Should().Be(new KeyValuePair<string, int>("Key1", 42));
        }

        [Fact]
        public void Count_ReflectsChanges()
        {
            // Arrange
            var dict = new ObservableDictionary<string, int>();

            // Act
            dict.Add("Key1", 1);
            dict.Add("Key2", 2);
            dict.Remove("Key1");

            // Assert
            dict.Count.Should().Be(1);
        }

        [Fact]
        public void Keys_ReflectsChanges()
        {
            // Arrange
            var dict = new ObservableDictionary<string, int> { { "Key1", 1 }, { "Key2", 2 } };

            // Act
            dict.Remove("Key1");

            // Assert
            dict.Keys.Should().BeEquivalentTo(new[] { "Key2" });
        }

        [Fact]
        public void Values_ReflectsChanges()
        {
            // Arrange
            var dict = new ObservableDictionary<string, int> { { "Key1", 1 }, { "Key2", 2 } };

            // Act
            dict.Remove("Key1");

            // Assert
            dict.Values.Should().BeEquivalentTo(new[] { 2 });
        }

        [Fact]
        public void IsReadOnly_ReturnsFalse()
        {
            // Arrange
            var dict = new ObservableDictionary<string, int>();

            // Act & Assert
            dict.IsReadOnly.Should().BeFalse();
        }

        // コレクション操作
        [Fact]
        public void Add_NewKey_TriggersCollectionChanged()
        {
            // Arrange
            var dict = new ObservableDictionary<string, int>();
            NotifyCollectionChangedEventArgs? eventArgs = null;
            dict.CollectionChanged += (s, e) => eventArgs = e;

            // Act
            dict.Add("Key1", 42);

            // Assert
            dict.Should().Contain("Key1", 42);
            eventArgs.Should().NotBeNull();
            eventArgs!.Action.Should().Be(NotifyCollectionChangedAction.Add);
            var newItem = (KeyValuePair<string, int>)eventArgs.NewItems![0];
            newItem.Should().Be(new KeyValuePair<string, int>("Key1", 42));
        }

        [Fact]
        public void Add_DuplicateKey_ThrowsArgumentException()
        {
            // Arrange
            var dict = new ObservableDictionary<string, int> { { "Key1", 42 } };

            // Act & Assert
            Assert.Throws<ArgumentException>(() => dict.Add("Key1", 100));
        }

        [Fact]
        public void Add_KeyValuePair_TriggersCollectionChanged()
        {
            // Arrange
            var dict = new ObservableDictionary<string, int>();
            NotifyCollectionChangedEventArgs? eventArgs = null;
            dict.CollectionChanged += (s, e) => eventArgs = e;

            // Act
            dict.Add(new KeyValuePair<string, int>("Key1", 42));

            // Assert
            dict.Should().Contain("Key1", 42);
            eventArgs.Should().NotBeNull();
            eventArgs!.Action.Should().Be(NotifyCollectionChangedAction.Add);
            var newItem = (KeyValuePair<string, int>)eventArgs.NewItems![0];
            newItem.Should().Be(new KeyValuePair<string, int>("Key1", 42));
        }

        [Fact]
        public void Remove_ExistingKey_TriggersCollectionChanged()
        {
            // Arrange
            var dict = new ObservableDictionary<string, int> { { "Key1", 42 } };
            NotifyCollectionChangedEventArgs? eventArgs = null;
            dict.CollectionChanged += (s, e) => eventArgs = e;

            // Act
            var result = dict.Remove("Key1");

            // Assert
            result.Should().BeTrue();
            dict.Should().BeEmpty();
            eventArgs.Should().NotBeNull();
            eventArgs!.Action.Should().Be(NotifyCollectionChangedAction.Remove);
            var oldItem = (KeyValuePair<string, int>)eventArgs.OldItems![0];
            oldItem.Should().Be(new KeyValuePair<string, int>("Key1", 42));
        }

        [Fact]
        public void Remove_NonExistingKey_ReturnsFalse()
        {
            // Arrange
            var dict = new ObservableDictionary<string, int>();
            NotifyCollectionChangedEventArgs? eventArgs = null;
            dict.CollectionChanged += (s, e) => eventArgs = e;

            // Act
            var result = dict.Remove("NonExistent");

            // Assert
            result.Should().BeFalse();
            eventArgs.Should().BeNull();
        }

        [Fact]
        public void Remove_KeyValuePair_TriggersCollectionChanged()
        {
            // Arrange
            var dict = new ObservableDictionary<string, int> { { "Key1", 42 } };
            NotifyCollectionChangedEventArgs? eventArgs = null;
            dict.CollectionChanged += (s, e) => eventArgs = e;

            // Act
            var result = dict.Remove(new KeyValuePair<string, int>("Key1", 42));

            // Assert
            result.Should().BeTrue();
            dict.Should().BeEmpty();
            eventArgs.Should().NotBeNull();
            eventArgs!.Action.Should().Be(NotifyCollectionChangedAction.Remove);
            var oldItem = (KeyValuePair<string, int>)eventArgs.OldItems![0];
            oldItem.Should().Be(new KeyValuePair<string, int>("Key1", 42));
        }

        [Fact]
        public void Clear_NonEmptyDictionary_TriggersReset()
        {
            // Arrange
            var dict = new ObservableDictionary<string, int> { { "Key1", 42 }, { "Key2", 43 } };
            NotifyCollectionChangedEventArgs? eventArgs = null;
            dict.CollectionChanged += (s, e) => eventArgs = e;

            // Act
            dict.Clear();

            // Assert
            dict.Should().BeEmpty();
            eventArgs.Should().NotBeNull();
            eventArgs!.Action.Should().Be(NotifyCollectionChangedAction.Reset);
        }

        [Fact]
        public void Clear_EmptyDictionary_NoEvent()
        {
            // Arrange
            var dict = new ObservableDictionary<string, int>();
            NotifyCollectionChangedEventArgs? eventArgs = null;
            dict.CollectionChanged += (s, e) => eventArgs = e;

            // Act
            dict.Clear();

            // Assert
            dict.Should().BeEmpty();
            eventArgs.Should().BeNull();
        }

        [Fact]
        public void ContainsKey_ExistingKey_ReturnsTrue()
        {
            // Arrange
            var dict = new ObservableDictionary<string, int> { { "Key1", 42 } };

            // Act
            var result = dict.ContainsKey("Key1");

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void ContainsKey_NonExistingKey_ReturnsFalse()
        {
            // Arrange
            var dict = new ObservableDictionary<string, int>();

            // Act
            var result = dict.ContainsKey("NonExistent");

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void Contains_ExistingPair_ReturnsTrue()
        {
            // Arrange
            var dict = new ObservableDictionary<string, int> { { "Key1", 42 } };

            // Act
            var result = dict.Contains(new KeyValuePair<string, int>("Key1", 42));

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void Contains_NonExistingPair_ReturnsFalse()
        {
            // Arrange
            var dict = new ObservableDictionary<string, int> { { "Key1", 42 } };

            // Act
            var result = dict.Contains(new KeyValuePair<string, int>("Key1", 43));

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void CopyTo_ValidArray_CopiesItems()
        {
            // Arrange
            var dict = new ObservableDictionary<string, int> { { "Key1", 1 }, { "Key2", 2 } };
            var array = new KeyValuePair<string, int>[3];

            // Act
            dict.CopyTo(array, 1);

            // Assert
            array[1].Should().Be(new KeyValuePair<string, int>("Key1", 1));
            array[2].Should().Be(new KeyValuePair<string, int>("Key2", 2));
        }

        [Fact]
        public void CopyTo_InvalidIndex_ThrowsArgumentException()
        {
            // Arrange
            var dict = new ObservableDictionary<string, int> { { "Key1", 1 } };
            var array = new KeyValuePair<string, int>[1];

            // Act & Assert
            //Assert.Throws<ArgumentException>(() => dict.CopyTo(array, 2));
            Assert.Throws<ArgumentOutOfRangeException>(() => dict.CopyTo(array, 2));
        }

        // イベント通知
        [Fact]
        public void Add_TriggersPropertyChanged()
        {
            // Arrange
            var dict = new ObservableDictionary<string, int>();
            var propertyChangedNames = new List<string>();
            dict.PropertyChanged += (s, e) => propertyChangedNames.Add(e.PropertyName);

            // Act
            dict.Add("Key1", 42);

            // Assert
            propertyChangedNames.Should().Contain("Count");
            propertyChangedNames.Should().Contain("Keys");
            propertyChangedNames.Should().Contain("Values");
            propertyChangedNames.Should().HaveCount(3);
        }

        [Fact]
        public void Remove_TriggersPropertyChanged()
        {
            // Arrange
            var dict = new ObservableDictionary<string, int> { { "Key1", 42 } };
            var propertyChangedNames = new List<string>();
            dict.PropertyChanged += (s, e) => propertyChangedNames.Add(e.PropertyName);

            // Act
            dict.Remove("Key1");

            // Assert
            propertyChangedNames.Should().Contain("Count");
            propertyChangedNames.Should().Contain("Keys");
            propertyChangedNames.Should().Contain("Values");
            propertyChangedNames.Should().HaveCount(3);
        }

        [Fact]
        public void Clear_TriggersPropertyChanged()
        {
            // Arrange
            var dict = new ObservableDictionary<string, int> { { "Key1", 42 } };
            var propertyChangedNames = new List<string>();
            dict.PropertyChanged += (s, e) => propertyChangedNames.Add(e.PropertyName);

            // Act
            dict.Clear();

            // Assert
            propertyChangedNames.Should().Contain("Count");
            propertyChangedNames.Should().Contain("Keys");
            propertyChangedNames.Should().Contain("Values");
            propertyChangedNames.Should().HaveCount(3);
        }

        [Fact]
        public void MultipleOperations_TriggersMultipleEvents()
        {
            // Arrange
            var dict = new ObservableDictionary<string, int>();
            var collectionChangedCount = 0;
            var propertyChangedCount = 0;
            dict.CollectionChanged += (s, e) => collectionChangedCount++;
            dict.PropertyChanged += (s, e) => propertyChangedCount++;

            // Act
            dict.Add("Key1", 1);
            dict["Key2"] = 2;
            dict.Remove("Key1");

            // Assert
            collectionChangedCount.Should().Be(3); // Add, Add, Remove
            propertyChangedCount.Should().Be(9); // 3 properties * 3 operations
        }

        [Fact]
        public void UnsubscribedEvent_DoesNotTrigger()
        {
            // Arrange
            var dict = new ObservableDictionary<string, int>();
            var collectionChangedCount = 0;
            var propertyChangedCount = 0;
            void CollectionHandler(object s, NotifyCollectionChangedEventArgs e) => collectionChangedCount++;
            void PropertyHandler(object s, PropertyChangedEventArgs e) => propertyChangedCount++;
            dict.CollectionChanged += CollectionHandler;
            dict.PropertyChanged += PropertyHandler;
            dict.CollectionChanged -= CollectionHandler;
            dict.PropertyChanged -= PropertyHandler;

            // Act
            dict.Add("Key1", 42);

            // Assert
            collectionChangedCount.Should().Be(0);
            propertyChangedCount.Should().Be(0);
        }

        // 列挙
        [Fact]
        public void GetEnumerator_EnumeratesAllItems()
        {
            // Arrange
            var dict = new ObservableDictionary<string, int> { { "Key1", 1 }, { "Key2", 2 } };
            var items = new List<KeyValuePair<string, int>>();

            // Act
            foreach (var item in dict)
            {
                items.Add(item);
            }

            // Assert
            items.Should().HaveCount(2);
            items.Should().Contain(new KeyValuePair<string, int>("Key1", 1));
            items.Should().Contain(new KeyValuePair<string, int>("Key2", 2));
        }

        [Fact]
        public void NonGenericGetEnumerator_EnumeratesAllItems()
        {
            // Arrange
            var dict = new ObservableDictionary<string, int> { { "Key1", 1 }, { "Key2", 2 } };
            var items = new List<KeyValuePair<string, int>>();
            var enumerable = (System.Collections.IEnumerable)dict;

            // Act
            foreach (var item in enumerable)
            {
                items.Add((KeyValuePair<string, int>)item);
            }

            // Assert
            items.Should().HaveCount(2);
            items.Should().Contain(new KeyValuePair<string, int>("Key1", 1));
            items.Should().Contain(new KeyValuePair<string, int>("Key2", 2));
        }

        // エッジケース
        [Fact]
        public void Add_NullKey_ThrowsArgumentNullException()
        {
            // Arrange
            var dict = new ObservableDictionary<string, int>();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => dict.Add(null, 42));
        }

        [Fact]
        public void Add_NullValue_HandlesGracefully()
        {
            // Arrange
            var dict = new ObservableDictionary<string, string>();

            // Act
            dict.Add("Key1", null);

            // Assert
            dict["Key1"].Should().BeNull();
        }

        [Fact]
        public void TryGetValue_NonExistingKey_ReturnsFalse()
        {
            // Arrange
            var dict = new ObservableDictionary<string, int>();

            // Act
            var result = dict.TryGetValue("NonExistent", out var value);

            // Assert
            result.Should().BeFalse();
            value.Should().Be(default(int));
        }

        [Fact]
        public void LargeNumberOfItems_HandlesCorrectly()
        {
            // Arrange
            var dict = new ObservableDictionary<int, int>();
            const int itemCount = 1000;

            // Act
            for (int i = 0; i < itemCount; i++)
            {
                dict.Add(i, i);
            }

            // Assert
            dict.Count.Should().Be(itemCount);
            dict[500].Should().Be(500);
        }

        [Fact]
        public void EmptyDictionary_Operations_NoError()
        {
            // Arrange
            var dict = new ObservableDictionary<string, int>();

            // Act
            var contains = dict.ContainsKey("NonExistent");
            var tryGet = dict.TryGetValue("NonExistent", out var value);
            var remove = dict.Remove("NonExistent");

            // Assert
            contains.Should().BeFalse();
            tryGet.Should().BeFalse();
            value.Should().Be(default(int));
            remove.Should().BeFalse();
        }
    }
}
/*
using System.Collections.Specialized;
using System.ComponentModel;
using FluentAssertions;
using MauiCrudApp.Common.Utilities;
using Xunit;

namespace MauiCrudApp.Common.Tests.Utilities
{
    public class ObservableDictionaryTests
    {
        [Fact]
        public void Constructor_Empty_CreatesEmptyDictionary()
        {
            // Arrange & Act
            var dict = new ObservableDictionary<string, int>();

            // Assert
            dict.Should().BeEmpty();
            dict.Count.Should().Be(0);
        }

        [Fact]
        public void Constructor_WithDictionary_CopiesItems()
        {
            // Arrange
            var initial = new Dictionary<string, int> { { "A", 1 }, { "B", 2 } };

            // Act
            var dict = new ObservableDictionary<string, int>(initial);

            // Assert
            dict.Should().HaveCount(2);
            dict["A"].Should().Be(1);
            dict["B"].Should().Be(2);
        }

        [Fact]
        public void Add_NewKey_TriggersCollectionChanged()
        {
            // Arrange
            var dict = new ObservableDictionary<string, int>();
            NotifyCollectionChangedEventArgs? eventArgs = null;
            dict.CollectionChanged += (s, e) => eventArgs = e;

            // Act
            dict.Add("Key1", 42);

            // Assert
            dict.Should().Contain("Key1", 42);
            eventArgs.Should().NotBeNull();
            eventArgs!.Action.Should().Be(NotifyCollectionChangedAction.Add);
            var newItem = (KeyValuePair<string, int>)eventArgs.NewItems![0];
            newItem.Should().Be(new KeyValuePair<string, int>("Key1", 42));
        }

        [Fact]
        public void Add_NewKey_TriggersPropertyChanged()
        {
            // Arrange
            var dict = new ObservableDictionary<string, int>();
            var propertyChangedNames = new List<string>();
            dict.PropertyChanged += (s, e) => propertyChangedNames.Add(e.PropertyName);

            // Act
            dict.Add("Key1", 42);

            // Assert
            propertyChangedNames.Should().Contain("Count");
            propertyChangedNames.Should().Contain("Keys");
            propertyChangedNames.Should().Contain("Values");
        }

        [Fact]
        public void Remove_ExistingKey_TriggersCollectionChanged()
        {
            // Arrange
            var dict = new ObservableDictionary<string, int> { { "Key1", 42 } };
            NotifyCollectionChangedEventArgs? eventArgs = null;
            dict.CollectionChanged += (s, e) => eventArgs = e;

            // Act
            var result = dict.Remove("Key1");

            // Assert
            result.Should().BeTrue();
            dict.Should().BeEmpty();
            eventArgs.Should().NotBeNull();
            eventArgs!.Action.Should().Be(NotifyCollectionChangedAction.Remove);
            var oldItem = (KeyValuePair<string, int>)eventArgs.OldItems![0];
            oldItem.Should().Be(new KeyValuePair<string, int>("Key1", 42));
        }

        [Fact]
        public void Indexer_SetExistingKey_TriggersReplace()
        {
            // Arrange
            var dict = new ObservableDictionary<string, int> { { "Key1", 42 } };
            NotifyCollectionChangedEventArgs? eventArgs = null;
            dict.CollectionChanged += (s, e) => eventArgs = e;

            // Act
            dict["Key1"] = 100;

            // Assert
            dict["Key1"].Should().Be(100);
            eventArgs.Should().NotBeNull();
            eventArgs!.Action.Should().Be(NotifyCollectionChangedAction.Replace);
            var newItem = (KeyValuePair<string, int>)eventArgs.NewItems![0];
            var oldItem = (KeyValuePair<string, int>)eventArgs.OldItems![0];
            newItem.Should().Be(new KeyValuePair<string, int>("Key1", 100));
            oldItem.Should().Be(new KeyValuePair<string, int>("Key1", 42));
        }

        [Fact]
        public void Clear_NonEmptyDictionary_TriggersReset()
        {
            // Arrange
            var dict = new ObservableDictionary<string, int> { { "Key1", 42 }, { "Key2", 43 } };
            NotifyCollectionChangedEventArgs? eventArgs = null;
            dict.CollectionChanged += (s, e) => eventArgs = e;

            // Act
            dict.Clear();

            // Assert
            dict.Should().BeEmpty();
            eventArgs.Should().NotBeNull();
            eventArgs!.Action.Should().Be(NotifyCollectionChangedAction.Reset);
        }

        [Fact]
        public void TryGetValue_NonExistingKey_ReturnsFalse()
        {
            // Arrange
            var dict = new ObservableDictionary<string, int>();

            // Act
            var result = dict.TryGetValue("NonExistent", out var value);

            // Assert
            result.Should().BeFalse();
            value.Should().Be(default(int));
        }
    }
}
*/