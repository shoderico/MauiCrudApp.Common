using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using FluentAssertions;
using MauiCrudApp.Common.Utilities;
using Xunit;

namespace MauiCrudApp.Common.Tests.Utilities
{
    // Model class for testing with trackable properties
    public class TestModel : ObservableObject
    {
        private string _name;
        private int _value;
        private TestNestedModel _nested;
        private ObservableDictionary<string, int> _items = new();
        private ObservableCollection<TestNestedModel> _nestedList = new();

        [TrackChanges]
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public int UntrackedValue
        {
            get => _value;
            set => SetProperty(ref _value, value);
        }

        [TrackChanges]
        public TestNestedModel Nested
        {
            get => _nested;
            set => SetProperty(ref _nested, value);
        }

        [TrackChanges]
        public ObservableDictionary<string, int> Items
        {
            get => _items;
            set => SetProperty(ref _items, value);
        }

        [TrackChanges]
        public ObservableCollection<TestNestedModel> NestedList
        {
            get => _nestedList;
            set => SetProperty(ref _nestedList, value);
        }
    }

    // Nested model class for testing nested object tracking
    public class TestNestedModel : ObservableObject
    {
        private int _value;

        [TrackChanges]
        public int Value
        {
            get => _value;
            set => SetProperty(ref _value, value);
        }
    }

    // Parent model class for testing property tracking
    public class TestParentModel : ObservableObject
    {
        private TestModel _child;

        [TrackChanges]
        public TestModel Child
        {
            get => _child;
            set => SetProperty(ref _child, value);
        }
    }

    public class ChangeTrackerTests
    {
        // Tests constructor with valid input
        [Fact]
        public void Constructor_ValidInput_InitializesWithoutError()
        {
            // Arrange
            var model = new TestModel();

            // Act
            var tracker = new ChangeTracker(typeof(TestModel), model);

            // Assert
            tracker.Should().NotBeNull();
        }

        // Tests constructor with null type
        [Fact]
        public void Constructor_NullType_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new ChangeTracker(null, new TestModel()));
        }

        // Tests constructor with null instance
        [Fact]
        public void Constructor_NullInstance_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new ChangeTracker(typeof(TestModel), null));
        }

        // Tests tracking of a property change
        [Fact]
        public void Track_PropertyChange_DetectsChange()
        {
            // Arrange
            var model = new TestModel();
            var tracker = new ChangeTracker(typeof(TestModel), model);
            var changeCount = 0;

            // Act
            tracker.Track(() => changeCount++);
            model.Name = "NewName";

            // Assert
            changeCount.Should().Be(1);
            tracker.HasChanges.Should().BeTrue();
        }

        // Tests tracking without onChanged callback
        [Fact]
        public void Track_WithoutOnChanged_DetectsHasChanges()
        {
            // Arrange
            var model = new TestModel();
            var tracker = new ChangeTracker(typeof(TestModel), model);

            // Act
            tracker.Track();
            model.Name = "NewName";

            // Assert
            tracker.HasChanges.Should().BeTrue();
        }

        // Tests ignoring changes to untracked properties
        [Fact]
        public void Track_UnmarkedPropertyChange_IgnoresChange()
        {
            // Arrange
            var model = new TestModel();
            var tracker = new ChangeTracker(typeof(TestModel), model);
            var changeCount = 0;

            // Act
            tracker.Track(() => changeCount++);
            model.UntrackedValue = 42;

            // Assert
            changeCount.Should().Be(0);
            tracker.HasChanges.Should().BeFalse();
        }

        // Tests tracking of nested object property changes
        [Fact]
        public void Track_NestedObjectChange_DetectsChange()
        {
            // Arrange
            var model = new TestModel();
            var tracker = new ChangeTracker(typeof(TestModel), model);
            var changeCount = 0;

            // Act
            tracker.Track(() => changeCount++);
            model.Nested = new TestNestedModel();
            model.Nested.Value = 42;

            // Assert
            changeCount.Should().Be(2); // Nested set + Value change
            tracker.HasChanges.Should().BeTrue();
        }

        // Tests tracking of nested object replacement
        [Fact]
        public void Track_NestedObjectSet_DetectsChange()
        {
            // Arrange
            var model = new TestModel { Nested = new TestNestedModel() };
            var tracker = new ChangeTracker(typeof(TestModel), model);
            var changeCount = 0;

            // Act
            tracker.Track(() => changeCount++);
            model.Nested = new TestNestedModel { Value = 100 };

            // Assert
            changeCount.Should().Be(1); // Nested set
            tracker.HasChanges.Should().BeTrue();
        }

        // Tests tracking of ObservableDictionary item addition
        [Fact]
        public void Track_ObservableDictionaryAdd_DetectsChange()
        {
            // Arrange
            var model = new TestModel();
            var tracker = new ChangeTracker(typeof(TestModel), model);
            var changeCount = 0;

            // Act
            tracker.Track(() => changeCount++);
            model.Items.Add("Key1", 42);

            // Assert
            changeCount.Should().BeGreaterThanOrEqualTo(1);
            tracker.HasChanges.Should().BeTrue();
        }

        // Tests tracking of ObservableDictionary item removal
        [Fact]
        public void Track_ObservableDictionaryRemove_DetectsChange()
        {
            // Arrange
            var model = new TestModel();
            var tracker = new ChangeTracker(typeof(TestModel), model);
            var changeCount = 0;

            // Act
            tracker.Track(() => changeCount++);
            model.Items.Add("Key1", 42);
            model.Items.Remove("Key1");

            // Assert
            changeCount.Should().BeGreaterThanOrEqualTo(2); // Add + Remove
            tracker.HasChanges.Should().BeTrue();
        }

        // Tests tracking of ObservableDictionary item replacement
        [Fact]
        public void Track_ObservableDictionaryReplace_DetectsChange()
        {
            // Arrange
            var model = new TestModel();
            var tracker = new ChangeTracker(typeof(TestModel), model);
            var changeCount = 0;

            // Act
            tracker.Track(() => changeCount++);
            model.Items.Add("Key1", 42);
            model.Items["Key1"] = 100;

            // Assert
            changeCount.Should().BeGreaterThanOrEqualTo(2); // Add + Replace
            tracker.HasChanges.Should().BeTrue();
        }

        // Tests tracking of ObservableCollection item addition
        [Fact]
        public void Track_ListAdd_DetectsChange()
        {
            // Arrange
            var model = new TestModel();
            var tracker = new ChangeTracker(typeof(TestModel), model);
            var changeCount = 0;

            // Act
            tracker.Track(() => changeCount++);
            model.NestedList.Add(new TestNestedModel());

            // Assert
            changeCount.Should().Be(1);
            tracker.HasChanges.Should().BeTrue();
        }

        // Tests tracking of ObservableCollection item property changes
        [Fact]
        public void Track_NestedListItemChange_DetectsChange()
        {
            // Arrange
            var model = new TestModel();
            var tracker = new ChangeTracker(typeof(TestModel), model);
            var changeCount = 0;

            // Act
            tracker.Track(() => changeCount++);
            model.NestedList.Add(new TestNestedModel());
            model.NestedList[0].Value = 42;

            // Assert
            changeCount.Should().Be(2); // Add + Value change
            tracker.HasChanges.Should().BeTrue();
        }

        // Tests tracking of property changes through TrackProperty
        [Fact]
        public void TrackProperty_PropertyChange_DetectsChange()
        {
            // Arrange
            var parent = new TestParentModel { Child = new TestModel() };
            var tracker = new ChangeTracker(typeof(TestModel), parent.Child);
            var changeCount = 0;

            // Act
            tracker.TrackProperty(parent, nameof(TestParentModel.Child), () => changeCount++);
            parent.Child.Name = "NewName";

            // Assert
            changeCount.Should().Be(1);
            tracker.HasChanges.Should().BeTrue();
        }

        // Tests tracking property without onChanged callback
        [Fact]
        public void TrackProperty_WithoutOnChanged_DetectsHasChanges()
        {
            // Arrange
            var parent = new TestParentModel { Child = new TestModel() };
            var tracker = new ChangeTracker(typeof(TestModel), parent.Child);

            // Act
            tracker.TrackProperty(parent, nameof(TestParentModel.Child));
            parent.Child.Name = "NewName";

            // Assert
            tracker.HasChanges.Should().BeTrue();
        }

        // Tests tracking of property replacement through TrackProperty
        [Fact]
        public void TrackProperty_NewInstance_DetectsChange()
        {
            // Arrange
            var parent = new TestParentModel { Child = new TestModel() };
            var tracker = new ChangeTracker(typeof(TestModel), parent.Child);
            var changeCount = 0;

            // Act
            tracker.TrackProperty(parent, nameof(TestParentModel.Child), () => changeCount++);
            parent.Child = new TestModel { Name = "NewInstance" };

            // Assert
            changeCount.Should().Be(1);
            tracker.HasChanges.Should().BeTrue();
        }

        // Tests TrackProperty with null parent
        [Fact]
        public void TrackProperty_NullParent_ThrowsArgumentNullException()
        {
            // Arrange
            var model = new TestModel();
            var tracker = new ChangeTracker(typeof(TestModel), model);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => tracker.TrackProperty(null, nameof(TestParentModel.Child), null));
        }

        // Tests TrackProperty with null property name
        [Fact]
        public void TrackProperty_NullPropertyName_ThrowsArgumentNullException()
        {
            // Arrange
            var parent = new TestParentModel();
            var model = new TestModel();
            var tracker = new ChangeTracker(typeof(TestModel), model);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => tracker.TrackProperty(parent, null, null));
        }

        // Tests saving does not throw and resets HasChanges
        [Fact]
        public void Save_ResetsHasChanges()
        {
            // Arrange
            var model = new TestModel();
            var tracker = new ChangeTracker(typeof(TestModel), model);
            tracker.Track();
            model.Name = "NewName";

            // Act
            tracker.Save();

            // Assert
            tracker.HasChanges.Should().BeFalse();
            tracker.Invoking(t => t.Save()).Should().NotThrow();
        }

        // Tests HasChanges after multiple changes and save
        [Fact]
        public void Track_MultipleChangesAndSave_ResetsHasChanges()
        {
            // Arrange
            var model = new TestModel();
            var tracker = new ChangeTracker(typeof(TestModel), model);
            var changeCount = 0;

            // Act
            tracker.Track(() => changeCount++);
            model.Name = "NewName";
            model.Nested = new TestNestedModel { Value = 42 };
            model.Items.Add("Key1", 100);

            // Assert
            changeCount.Should().BeGreaterThanOrEqualTo(3);
            tracker.HasChanges.Should().BeTrue();

            // Act
            tracker.Save();

            // Assert
            tracker.HasChanges.Should().BeFalse();

            // Act
            model.Name = "AnotherName";

            // Assert
            tracker.HasChanges.Should().BeTrue();
        }

        // Tests handling null values in properties
        [Fact]
        public void Track_NullValueInProperty_HandlesGracefully()
        {
            // Arrange
            var model = new TestModel { Nested = null };
            var tracker = new ChangeTracker(typeof(TestModel), model);
            var changeCount = 0;

            // Act
            tracker.Track(() => changeCount++);
            model.Nested = new TestNestedModel();

            // Assert
            changeCount.Should().Be(1);
            tracker.HasChanges.Should().BeTrue();
        }

        // Tests handling empty collections
        [Fact]
        public void Track_EmptyCollection_HandlesGracefully()
        {
            // Arrange
            var model = new TestModel();
            var tracker = new ChangeTracker(typeof(TestModel), model);
            var changeCount = 0;

            // Act
            tracker.Track(() => changeCount++);
            model.Items.Clear();

            // Assert
            changeCount.Should().Be(0);
            tracker.HasChanges.Should().BeFalse();
        }

        // Tests tracking non-ObservableObject instances
        [Fact]
        public void Track_NonObservableObject_DoesNotThrow()
        {
            // Arrange
            var obj = new object();
            var tracker = new ChangeTracker(typeof(object), obj);

            // Act
            tracker.Track();

            // Assert
            tracker.Should().NotBeNull();
        }

        // Tests tracking with null callback
        [Fact]
        public void Track_NullCallback_DoesNotThrow()
        {
            // Arrange
            var model = new TestModel();
            var tracker = new ChangeTracker(typeof(TestModel), model);

            // Act
            tracker.Track(null);

            // Assert
            tracker.Should().NotBeNull();
        }

        // Tests Dispose unsubscribes events
        [Fact]
        public void Dispose_UnsubscribesEvents()
        {
            // Arrange
            var model = new TestModel();
            var tracker = new ChangeTracker(typeof(TestModel), model);
            var changeCount = 0;

            // Act
            tracker.Track(() => changeCount++);
            tracker.Dispose();
            model.Name = "NewName";

            // Assert
            changeCount.Should().Be(0);
            tracker.HasChanges.Should().BeFalse();
        }

        // Tests Dispose unsubscribes nested events
        [Fact]
        public void Dispose_UnsubscribesNestedEvents()
        {
            // Arrange
            var model = new TestModel { Nested = new TestNestedModel() };
            var tracker = new ChangeTracker(typeof(TestModel), model);
            var changeCount = 0;

            // Act
            tracker.Track(() => changeCount++);
            tracker.Dispose();
            model.Nested.Value = 42;

            // Assert
            changeCount.Should().Be(0);
            tracker.HasChanges.Should().BeFalse();
        }

        // Tests Dispose unsubscribes collection events
        [Fact]
        public void Dispose_UnsubscribesCollectionEvents()
        {
            // Arrange
            var model = new TestModel();
            var tracker = new ChangeTracker(typeof(TestModel), model);
            var changeCount = 0;

            // Act
            tracker.Track(() => changeCount++);
            tracker.Dispose();
            model.Items.Add("Key1", 42);

            // Assert
            changeCount.Should().Be(0);
            tracker.HasChanges.Should().BeFalse();
        }

        // Tests Dispose unsubscribes property events
        [Fact]
        public void Dispose_UnsubscribesPropertyEvents()
        {
            // Arrange
            var parent = new TestParentModel { Child = new TestModel() };
            var tracker = new ChangeTracker(typeof(TestModel), parent.Child);
            var changeCount = 0;

            // Act
            tracker.TrackProperty(parent, nameof(TestParentModel.Child), () => changeCount++);
            tracker.Dispose();
            parent.Child = new TestModel { Name = "NewInstance" };

            // Assert
            changeCount.Should().Be(0);
            tracker.HasChanges.Should().BeFalse();
        }

        // Tests multiple Dispose calls do not throw errors
        [Fact]
        public void Dispose_MultipleCalls_NoError()
        {
            // Arrange
            var model = new TestModel();
            var tracker = new ChangeTracker(typeof(TestModel), model);

            // Act
            tracker.Dispose();
            tracker.Dispose();

            // Assert
            tracker.Should().NotBeNull();
        }

        // Tests Track after Dispose
        [Fact]
        public void Track_Disposed_ThrowsObjectDisposedException()
        {
            // Arrange
            var model = new TestModel();
            var tracker = new ChangeTracker(typeof(TestModel), model);

            // Act
            tracker.Dispose();

            // Assert
            Assert.Throws<ObjectDisposedException>(() => tracker.Track());
        }

        // Tests TrackProperty after Dispose
        [Fact]
        public void TrackProperty_Disposed_ThrowsObjectDisposedException()
        {
            // Arrange
            var parent = new TestParentModel();
            var model = new TestModel();
            var tracker = new ChangeTracker(typeof(TestModel), model);

            // Act
            tracker.Dispose();

            // Assert
            Assert.Throws<ObjectDisposedException>(() => tracker.TrackProperty(parent, nameof(TestParentModel.Child)));
        }

        // Tests Save after Dispose
        [Fact]
        public void Save_Disposed_ThrowsObjectDisposedException()
        {
            // Arrange
            var model = new TestModel();
            var tracker = new ChangeTracker(typeof(TestModel), model);

            // Act
            tracker.Dispose();

            // Assert
            Assert.Throws<ObjectDisposedException>(() => tracker.Save());
        }
    }
}
