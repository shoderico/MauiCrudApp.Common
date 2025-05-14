using MauiCrudApp.Common.Navigation;
using System;
using Xunit;

namespace MauiCrudApp.Common.Tests.Navigation
{
    public class NavigationParameterStoreTests
    {
        private readonly NavigationParameterStore _store;

        public NavigationParameterStoreTests()
        {
            _store = new NavigationParameterStore();
        }

        // Test pushing and popping a single parameter of type string
        [Fact]
        public void PushParameter_And_PopParameter_WithString_ReturnsCorrectValue()
        {
            // Arrange
            string expected = "TestValue";

            // Act
            _store.PushParameter(expected);
            var result = _store.PopParameter<string>();

            // Assert
            Assert.Equal(expected, result);
        }

        // Test pushing and popping a single parameter of type int
        [Fact]
        public void PushParameter_And_PopParameter_WithInt_ReturnsCorrectValue()
        {
            // Arrange
            int expected = 42;

            // Act
            _store.PushParameter(expected);
            var result = _store.PopParameter<int>();

            // Assert
            Assert.Equal(expected, result);
        }

        // Test pushing and popping a complex object
        [Fact]
        public void PushParameter_And_PopParameter_WithComplexObject_ReturnsCorrectValue()
        {
            // Arrange
            var expected = new TestObject { Id = 1, Name = "Test" };

            // Act
            _store.PushParameter(expected);
            var result = _store.PopParameter<TestObject>();

            // Assert
            Assert.Equal(expected.Id, result.Id);
            Assert.Equal(expected.Name, result.Name);
        }

        // Test pushing multiple parameters of different types and popping them
        [Fact]
        public void PushParameter_MultipleTypes_PopParameter_ReturnsCorrectValues()
        {
            // Arrange
            string stringValue = "Hello";
            int intValue = 100;
            var objectValue = new TestObject { Id = 2, Name = "World" };

            // Act
            _store.PushParameter(stringValue);
            _store.PushParameter(intValue);
            _store.PushParameter(objectValue);

            var stringResult = _store.PopParameter<string>();
            var intResult = _store.PopParameter<int>();
            var objectResult = _store.PopParameter<TestObject>();

            // Assert
            Assert.Equal(stringValue, stringResult);
            Assert.Equal(intValue, intResult);
            Assert.Equal(objectValue.Id, objectResult.Id);
            Assert.Equal(objectValue.Name, objectResult.Name);
        }

        // Test overwriting a parameter of the same type
        [Fact]
        public void PushParameter_SameTypeMultipleTimes_OverwritesPreviousValue()
        {
            // Arrange
            string firstValue = "First";
            string secondValue = "Second";

            // Act
            _store.PushParameter(firstValue);
            _store.PushParameter(secondValue);
            var result = _store.PopParameter<string>();

            // Assert
            Assert.Equal(secondValue, result);
        }

        // Test popping a parameter that was never pushed throws InvalidOperationException
        [Fact]
        public void PopParameter_NonExistentType_ThrowsInvalidOperationException()
        {
            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => _store.PopParameter<string>());
        }

        // Test pushing and popping null value
        [Fact]
        public void PushParameter_NullValue_PopParameter_ReturnsNull()
        {
            // Arrange
            string? nullValue = null;

            // Act
            _store.PushParameter(nullValue);
            var result = _store.PopParameter<string>();

            // Assert
            Assert.Null(result);
        }

        // Test pushing and popping value type with default value
        [Fact]
        public void PushParameter_DefaultValueType_PopParameter_ReturnsDefault()
        {
            // Arrange
            int defaultValue = default;

            // Act
            _store.PushParameter(defaultValue);
            var result = _store.PopParameter<int>();

            // Assert
            Assert.Equal(defaultValue, result);
        }

        // Test popping after popping throws InvalidOperationException
        [Fact]
        public void PopParameter_AfterPopping_ThrowsInvalidOperationException()
        {
            // Arrange
            string value = "Test";
            _store.PushParameter(value);
            _store.PopParameter<string>(); // First pop

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => _store.PopParameter<string>());
        }

        // Test pushing and popping multiple times with same type
        [Fact]
        public void PushParameter_And_PopParameter_MultipleTimesWithSameType_WorksCorrectly()
        {
            // Arrange
            string firstValue = "First";
            string secondValue = "Second";

            // Act
            _store.PushParameter(firstValue);
            var firstResult = _store.PopParameter<string>();
            _store.PushParameter(secondValue);
            var secondResult = _store.PopParameter<string>();

            // Assert
            Assert.Equal(firstValue, firstResult);
            Assert.Equal(secondValue, secondResult);
        }

        // Test attempting to pop with a different type throws InvalidOperationException
        [Fact]
        public void PopParameter_WrongType_ThrowsInvalidOperationException()
        {
            // Arrange
            _store.PushParameter("TestString");

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => _store.PopParameter<int>());
        }

        // Helper class for testing complex objects
        private class TestObject
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
    }
}
