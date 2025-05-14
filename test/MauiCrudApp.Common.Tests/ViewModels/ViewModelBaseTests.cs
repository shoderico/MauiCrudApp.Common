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
        public class TestViewModel : ViewModelBase<string> // Changed from private to public
        {
            public bool InitializeCalled { get; private set; }
            public string ReceivedParameter { get; private set; }

            public TestViewModel(INavigationParameterStore parameterStore)
                : base(parameterStore)
            {
            }

            public override Task InitializeAsync(string parameter)
            {
                InitializeCalled = true;
                ReceivedParameter = parameter;
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
            _parameterStoreMock.Setup(ps => ps.PopParameter<string>()).Returns(expectedParameter);
            var viewModel = new TestViewModel(_parameterStoreMock.Object);

            // Act
            await viewModel.PerformInitializeAsync();

            // Assert
            Assert.True(viewModel.InitializeCalled);
            Assert.Equal(expectedParameter, viewModel.ReceivedParameter);
            _parameterStoreMock.Verify(ps => ps.PopParameter<string>(), Times.Once());
        }

        [Fact]
        public async Task PerformInitializeAsync_CallsInitializeAsyncWithCorrectParameter()
        {
            // Arrange
            var expectedParameter = "TestParameter";
            _parameterStoreMock.Setup(ps => ps.PopParameter<string>()).Returns(expectedParameter);
            var viewModel = new TestViewModel(_parameterStoreMock.Object);

            // Act
            await viewModel.PerformInitializeAsync();

            // Assert
            Assert.True(viewModel.InitializeCalled);
            Assert.Equal(expectedParameter, viewModel.ReceivedParameter);
        }

        [Fact]
        public async Task PerformInitializeAsync_WhenInitializeAsyncThrowsException_PropagatesException()
        {
            // Arrange
            var expectedException = new System.InvalidOperationException("Test exception");
            _parameterStoreMock.Setup(ps => ps.PopParameter<string>()).Returns("TestParameter");
            var errorViewModel = new Mock<TestViewModel>(_parameterStoreMock.Object);
            errorViewModel.Setup(vm => vm.InitializeAsync(It.IsAny<string>())).ThrowsAsync(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<System.InvalidOperationException>(() =>
                errorViewModel.Object.PerformInitializeAsync());
            Assert.Equal(expectedException.Message, exception.Message);
        }

        [Fact]
        public async Task InitializeAsync_WhenParameterIsNull_CanHandleNullParameter()
        {
            // Arrange
            _parameterStoreMock.Setup(ps => ps.PopParameter<string>()).Returns((string)null);
            var viewModel = new TestViewModel(_parameterStoreMock.Object);

            // Act
            await viewModel.PerformInitializeAsync();

            // Assert
            Assert.True(viewModel.InitializeCalled);
            Assert.Null(viewModel.ReceivedParameter);
        }

        [Fact]
        public void PopParameter_WhenNoParameterExists_ThrowsInvalidOperationException()
        {
            // Arrange
            _parameterStoreMock.Setup(ps => ps.PopParameter<string>())
                .Throws(new InvalidOperationException("No parameter found"));

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => new TestViewModel(_parameterStoreMock.Object));
        }

        [Fact]
        public async Task PerformInitializeAsync_MultipleCalls_InitializeAsyncCalledOnce()
        {
            // Arrange
            var parameter = "TestParameter";
            _parameterStoreMock.Setup(ps => ps.PopParameter<string>()).Returns(parameter);
            var viewModel = new TestViewModel(_parameterStoreMock.Object);

            // Act
            await viewModel.PerformInitializeAsync();
            await viewModel.PerformInitializeAsync();

            // Assert
            Assert.True(viewModel.InitializeCalled);
            Assert.Equal(parameter, viewModel.ReceivedParameter);
            _parameterStoreMock.Verify(ps => ps.PopParameter<string>(), Times.Once());
        }
    }
}
