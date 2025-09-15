using Moq;
using Reqnroll.Generator.Plugins;
using Reqnroll.Infrastructure;
using Reqnroll.ScenarioCall.Generator;
using Reqnroll.UnitTestProvider;
using Xunit;

namespace Reqnroll.ScenarioCall.Generator.Tests;

public class GeneratorPluginTests
{
    private readonly GeneratorPlugin _plugin;

    public GeneratorPluginTests()
    {
        _plugin = new GeneratorPlugin();
    }

    [Fact]
    public void Initialize_CanBeCalledWithoutException()
    {
        // Arrange
        var mockEvents = new Mock<GeneratorPluginEvents>();
        var mockParameters = new Mock<GeneratorPluginParameters>();
        var mockUnitTestProviderConfiguration = new Mock<UnitTestProviderConfiguration>();

        // Act & Assert
        // Just verify that Initialize doesn't throw an exception
        _plugin.Initialize(mockEvents.Object, mockParameters.Object, mockUnitTestProviderConfiguration.Object);
    }

    [Fact]
    public void GeneratorPlugin_ImplementsIGeneratorPlugin()
    {
        // Assert
        Assert.IsAssignableFrom<IGeneratorPlugin>(_plugin);
    }

    [Fact]
    public void GeneratorPlugin_HasCorrectAssemblyAttribute()
    {
        // Arrange & Act
        var assembly = typeof(GeneratorPlugin).Assembly;
        var attributes = assembly.GetCustomAttributes(typeof(GeneratorPluginAttribute), false);

        // Assert
        Assert.Single(attributes);
        var attribute = (GeneratorPluginAttribute)attributes[0];
        Assert.Equal(typeof(GeneratorPlugin), attribute.PluginType);
    }
}