using Moq;
using Reqnroll.Generator.UnitTestConverter;
using Reqnroll.Parser;
using Reqnroll.ScenarioCall.Generator;
using Xunit;

namespace Reqnroll.ScenarioCall.Generator.Tests;

public class ScenarioCallFeatureGeneratorProviderSimpleTests
{
    private readonly Mock<UnitTestFeatureGeneratorProvider> _mockBaseProvider;
    private readonly Mock<IFeatureGenerator> _mockBaseGenerator;
    private readonly ScenarioCallFeatureGeneratorProvider _provider;

    public ScenarioCallFeatureGeneratorProviderSimpleTests()
    {
        _mockBaseProvider = new Mock<UnitTestFeatureGeneratorProvider>();
        _mockBaseGenerator = new Mock<IFeatureGenerator>();
        _provider = new ScenarioCallFeatureGeneratorProvider(_mockBaseProvider.Object);
    }

    [Fact]
    public void Constructor_InitializesWithBaseProvider()
    {
        // Arrange & Act
        var provider = new ScenarioCallFeatureGeneratorProvider(_mockBaseProvider.Object);

        // Assert
        Assert.NotNull(provider);
    }

    [Fact]
    public void Priority_ReturnsHighPriority()
    {
        // Act
        var priority = _provider.Priority;

        // Assert
        Assert.Equal(PriorityValues.High, priority);
    }

    [Fact]
    public void CanGenerate_DelegatesToBaseProvider_ReturnsTrue()
    {
        // Arrange
        var mockDocument = new Mock<ReqnrollDocument>(Mock.Of<ReqnrollFeature>(), null, Mock.Of<ReqnrollDocumentLocation>());
        _mockBaseProvider.Setup(x => x.CanGenerate(It.IsAny<ReqnrollDocument>())).Returns(true);

        // Act
        var result = _provider.CanGenerate(mockDocument.Object);

        // Assert
        Assert.True(result);
        _mockBaseProvider.Verify(x => x.CanGenerate(It.IsAny<ReqnrollDocument>()), Times.Once);
    }

    [Fact]
    public void CanGenerate_DelegatesToBaseProvider_ReturnsFalse()
    {
        // Arrange
        var mockDocument = new Mock<ReqnrollDocument>(Mock.Of<ReqnrollFeature>(), null, Mock.Of<ReqnrollDocumentLocation>());
        _mockBaseProvider.Setup(x => x.CanGenerate(It.IsAny<ReqnrollDocument>())).Returns(false);

        // Act
        var result = _provider.CanGenerate(mockDocument.Object);

        // Assert
        Assert.False(result);
        _mockBaseProvider.Verify(x => x.CanGenerate(It.IsAny<ReqnrollDocument>()), Times.Once);
    }

    [Fact]
    public void CreateGenerator_ReturnsScenarioCallFeatureGenerator()
    {
        // Arrange
        var mockDocument = new Mock<ReqnrollDocument>(Mock.Of<ReqnrollFeature>(), null, Mock.Of<ReqnrollDocumentLocation>());
        _mockBaseProvider.Setup(x => x.CreateGenerator(It.IsAny<ReqnrollDocument>())).Returns(_mockBaseGenerator.Object);

        // Act
        var result = _provider.CreateGenerator(mockDocument.Object);

        // Assert
        Assert.IsType<ScenarioCallFeatureGenerator>(result);
        _mockBaseProvider.Verify(x => x.CreateGenerator(It.IsAny<ReqnrollDocument>()), Times.Once);
    }
}