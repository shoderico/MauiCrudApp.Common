// File: MauiCrudApp.Common.Tests/ViewModels/ViewModelBaseTests.cs
using MauiCrudApp.Common.Navigation;
using MauiCrudApp.Common.ViewModels;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace MauiCrudApp.Common.Tests.ViewModels
{
    public class ViewModelBaseTests
    {
        // Mock objects
        private readonly Mock<INavigationParameterStore> _parameterStoreMock;

        public ViewModelBaseTests()
        {
            _parameterStoreMock = new Mock<INavigationParameterStore>();
        }

        // Concrete implementation of ViewModelBase for testing
        public class TestViewModel : ViewModelBase<string>
        {
            public bool InitializeCalled { get; set; }
            public string ReceivedParameter { get; private set; }
            public bool ReceivedIsInitialized { get; private set; }
            public bool FinalizeCalled { get; set; }
            public bool ReceivedIsFinalized { get; private set; }

            public TestViewModel(INavigationParameterStore parameterStore)
                : base(parameterStore)
            {
            }

            public override Task InitializeAsync(string parameter, bool isInitialized)
            {
                InitializeCalled = true;
                ReceivedParameter = parameter;
                ReceivedIsInitialized = isInitialized;
                return Task.CompletedTask;
            }

            public override Task FinalizeAsync(bool isFinalized)
            {
                FinalizeCalled = true;
                ReceivedIsFinalized = isFinalized;
                return Task.CompletedTask;
            }
        }

        [Fact]
        public void Constructor_WhenParameterStoreIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            INavigationParameterStore parameterStore = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new TestViewModel(parameterStore));
        }

        [Fact]
        public async Task Constructor_WhenParameterStoreReturnsParameter_SetsParameterCorrectly()
        {
            // Arrange
            var expectedParameter = "TestParameter";
            _parameterStoreMock.Setup(ps => ps.HasParameter<string>()).Returns(true);
            _parameterStoreMock.Setup(ps => ps.PopParameter<string>()).Returns(expectedParameter);
            var viewModel = new TestViewModel(_parameterStoreMock.Object);

            // Act
            await viewModel.PerformInitializeAsync();

            // Assert
            Assert.True(viewModel.InitializeCalled);
            Assert.Equal(expectedParameter, viewModel.ReceivedParameter);
            Assert.False(viewModel.ReceivedIsInitialized); // Initial call should be false
            _parameterStoreMock.Verify(ps => ps.PopParameter<string>(), Times.Once());
        }

        [Fact]
        public async Task PerformInitializeAsync_CallsInitializeAsyncWithCorrectParameter()
        {
            // Arrange
            var expectedParameter = "TestParameter";
            _parameterStoreMock.Setup(ps => ps.HasParameter<string>()).Returns(true);
            _parameterStoreMock.Setup(ps => ps.PopParameter<string>()).Returns(expectedParameter);
            var viewModel = new TestViewModel(_parameterStoreMock.Object);

            // Act
            await viewModel.PerformInitializeAsync();

            // Assert
            Assert.True(viewModel.InitializeCalled);
            Assert.Equal(expectedParameter, viewModel.ReceivedParameter);
            Assert.False(viewModel.ReceivedIsInitialized); // Initial call should be false
        }

        [Fact]
        public async Task PerformInitializeAsync_WhenInitializeAsyncThrowsException_PropagatesException()
        {
            // Arrange
            var expectedException = new System.InvalidOperationException("Test exception");
            _parameterStoreMock.Setup(ps => ps.HasParameter<string>()).Returns(true);
            _parameterStoreMock.Setup(ps => ps.PopParameter<string>()).Returns("TestParameter");
            var errorViewModel = new Mock<TestViewModel>(_parameterStoreMock.Object);
            errorViewModel.Setup(vm => vm.InitializeAsync(It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<System.InvalidOperationException>(() =>
                errorViewModel.Object.PerformInitializeAsync());
            Assert.Equal(expectedException.Message, exception.Message);
        }

        [Fact]
        public async Task InitializeAsync_WhenParameterIsNull_CanHandleNullParameter()
        {
            // Arrange
            _parameterStoreMock.Setup(ps => ps.HasParameter<string>()).Returns(true);
            _parameterStoreMock.Setup(ps => ps.PopParameter<string>()).Returns((string)null);
            var viewModel = new TestViewModel(_parameterStoreMock.Object);

            // Act
            await viewModel.PerformInitializeAsync();

            // Assert
            Assert.True(viewModel.InitializeCalled);
            Assert.Null(viewModel.ReceivedParameter);
            Assert.False(viewModel.ReceivedIsInitialized); // Initial call should be false
        }

        [Fact]
        public void PopParameter_WhenNoParameterExists_ThrowsInvalidOperationException()
        {
            // Arrange
            _parameterStoreMock.Setup(ps => ps.HasParameter<string>()).Returns(true);
            _parameterStoreMock.Setup(ps => ps.PopParameter<string>())
                .Throws(new InvalidOperationException("No parameter found"));

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => new TestViewModel(_parameterStoreMock.Object));
        }

        [Fact]
        public async Task PerformInitializeAsync_MultipleCalls_InitializeAsyncWithCorrectIsInitialized()
        {
            // Arrange
            var parameter = "TestParameter";
            _parameterStoreMock.Setup(ps => ps.HasParameter<string>()).Returns(true);
            _parameterStoreMock.Setup(ps => ps.PopParameter<string>()).Returns(parameter);
            var viewModel = new TestViewModel(_parameterStoreMock.Object);

            // Act: First call
            await viewModel.PerformInitializeAsync();

            // Assert: First call
            Assert.True(viewModel.InitializeCalled);
            Assert.Equal(parameter, viewModel.ReceivedParameter);
            Assert.False(viewModel.ReceivedIsInitialized); // First call should be false

            // Act: Second call
            viewModel.InitializeCalled = false; // Reset for clarity
            await viewModel.PerformInitializeAsync();

            // Assert: Second call
            Assert.True(viewModel.InitializeCalled);
            Assert.True(viewModel.ReceivedIsInitialized); // Second call should be true
            _parameterStoreMock.Verify(ps => ps.PopParameter<string>(), Times.Once());
        }

        [Fact]
        public async Task PerformFinalizeAsync_CallsFinalizeAsyncWithCorrectParameter()
        {
            // Arrange
            var expectedParameter = "TestParameter";
            _parameterStoreMock.Setup(ps => ps.HasParameter<string>()).Returns(true);
            _parameterStoreMock.Setup(ps => ps.PopParameter<string>()).Returns(expectedParameter);
            var viewModel = new TestViewModel(_parameterStoreMock.Object);

            // Act
            await viewModel.PerformFinalizeAsync();

            // Assert
            Assert.True(viewModel.FinalizeCalled);
            Assert.False(viewModel.ReceivedIsFinalized); // Initial call should be false
        }

        [Fact]
        public async Task PerformFinalizeAsync_WhenFinalizeAsyncThrowsException_PropagatesException()
        {
            // Arrange
            var expectedException = new System.InvalidOperationException("Test exception");
            _parameterStoreMock.Setup(ps => ps.HasParameter<string>()).Returns(true);
            _parameterStoreMock.Setup(ps => ps.PopParameter<string>()).Returns("TestParameter");
            var errorViewModel = new Mock<TestViewModel>(_parameterStoreMock.Object);
            errorViewModel.Setup(vm => vm.FinalizeAsync(It.IsAny<bool>())).ThrowsAsync(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<System.InvalidOperationException>(() =>
                errorViewModel.Object.PerformFinalizeAsync());
            Assert.Equal(expectedException.Message, exception.Message);
        }

        [Fact]
        public async Task PerformFinalizeAsync_MultipleCalls_FinalizeAsyncWithCorrectIsFinalized()
        {
            // Arrange
            var parameter = "TestParameter";
            _parameterStoreMock.Setup(ps => ps.HasParameter<string>()).Returns(true);
            _parameterStoreMock.Setup(ps => ps.PopParameter<string>()).Returns(parameter);
            var viewModel = new TestViewModel(_parameterStoreMock.Object);

            // Act: First call
            await viewModel.PerformFinalizeAsync();

            // Assert: First call
            Assert.True(viewModel.FinalizeCalled);
            Assert.False(viewModel.ReceivedIsFinalized); // First call should be false

            // Act: Second call
            viewModel.FinalizeCalled = false; // Reset for clarity
            await viewModel.PerformFinalizeAsync();

            // Assert: Second call
            Assert.True(viewModel.FinalizeCalled);
            Assert.True(viewModel.ReceivedIsFinalized); // Second call should be true
        }
    }
}