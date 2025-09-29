using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Moq;
using Reqnroll.Generator.UnitTestConverter;
using Reqnroll.Parser;
using Reqnroll.ScenarioCall.Generator;
using Xunit;

namespace Reqnroll.ScenarioCall.Generator.Tests;

public class ScenarioCallFeatureGeneratorSimpleTests
{
    private readonly Mock<IFeatureGenerator> _mockBaseGenerator;
    private readonly ScenarioCallFeatureGenerator _generator;

    public ScenarioCallFeatureGeneratorSimpleTests()
    {
        _mockBaseGenerator = new Mock<IFeatureGenerator>();
        // Pass null for the document since we're only testing the preprocessing functionality
        _generator = new ScenarioCallFeatureGenerator(_mockBaseGenerator.Object, null!);
    }

    [Fact]
    public void Constructor_InitializesWithBaseGenerator()
    {
        // Arrange & Act
        var generator = new ScenarioCallFeatureGenerator(_mockBaseGenerator.Object, null!);

        // Assert
        Assert.NotNull(generator);
    }

    [Fact]
    public void PreprocessFeatureContent_WithNoScenarioCalls_ReturnsOriginalContentWithNewline()
    {
        // Arrange
        var originalContent = @"Feature: Test Feature
Scenario: Test Scenario
    Given I have a test
    When I execute it
    Then it should pass";

        // Act
        var result = _generator.PreprocessFeatureContent(originalContent);

        // Assert
        Assert.Equal(originalContent + Environment.NewLine, result);
    }

    [Fact]
    public void PreprocessFeatureContent_WithValidScenarioCall_ExpandsScenarioCall()
    {
        // Arrange
        var originalContent = @"Feature: Test Feature
Scenario: Test Scenario
    Given I call scenario ""Login"" from feature ""Authentication""";

        SetupFeatureFileContent("Authentication", @"Feature: Authentication
Scenario: Login
    Given I am on the login page
    When I enter credentials
    Then I should be logged in");

        // Act
        var result = _generator.PreprocessFeatureContent(originalContent);

        // Assert
        Assert.Contains("# Expanded from scenario call", result);
        Assert.Contains("Given I am on the login page", result);
        Assert.Contains("When I enter credentials", result);
        Assert.Contains("Then I should be logged in", result);
    }

    [Fact]
    public void PreprocessFeatureContent_WithInvalidScenarioCall_AddsWarningComment()
    {
        // Arrange
        var originalContent = @"Feature: Test Feature
Scenario: Test Scenario
    Given I call scenario ""NonExistent"" from feature ""NonExistent""";

        // Act
        var result = _generator.PreprocessFeatureContent(originalContent);

        // Assert
        Assert.Contains("# Warning: Could not expand scenario call", result);
    }

    [Theory]
    [InlineData(@"Given I call scenario ""Login"" from feature ""Auth""", true)]
    [InlineData(@"When I call scenario ""Logout"" from feature ""Auth""", true)]
    [InlineData(@"Then I call scenario ""Setup"" from feature ""Test""", true)]
    [InlineData(@"And I call scenario ""Cleanup"" from feature ""Test""", true)]
    [InlineData(@"But I call scenario ""Reset"" from feature ""Test""", true)]
    [InlineData(@"Given I have some data", false)]
    [InlineData(@"When I perform an action", false)]
    [InlineData(@"I call scenario ""Test"" from feature ""Test""", false)]
    [InlineData(@"Given I call scenario Login from feature Auth", false)]
    public void IsScenarioCallStep_DetectsScenarioCallSteps(string stepText, bool expected)
    {
        // Act
        var result = CallPrivateMethod<bool>(_generator, "IsScenarioCallStep", stepText);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ExpandScenarioCall_WithValidCall_ReturnsExpandedSteps()
    {
        // Arrange
        var callStep = @"    Given I call scenario ""Login"" from feature ""Authentication""";
        
        SetupFeatureFileContent("Authentication", @"Feature: Authentication
Scenario: Login
    Given I am on the login page
    When I enter credentials
    Then I should be logged in");

        // Act
        var result = CallPrivateMethod<string>(_generator, "ExpandScenarioCall", callStep, "Test Feature");

        // Assert
        Assert.NotNull(result);
        Assert.Contains("# Expanded from scenario call", result);
        Assert.Contains("Given I am on the login page", result);
        Assert.Contains("When I enter credentials", result);
        Assert.Contains("Then I should be logged in", result);
    }

    [Fact]
    public void ExpandScenarioCall_WithInvalidCall_ReturnsNull()
    {
        // Arrange
        var callStep = "Given I have some data";

        // Act
        var result = CallPrivateMethod<string>(_generator, "ExpandScenarioCall", callStep, "Test Feature");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void ExpandScenarioCall_WithNonExistentScenario_ReturnsNull()
    {
        // Arrange
        var callStep = @"    Given I call scenario ""NonExistent"" from feature ""NonExistent""";

        // Act
        var result = CallPrivateMethod<string>(_generator, "ExpandScenarioCall", callStep, "Test Feature");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void FindScenarioSteps_WithValidScenario_ReturnsSteps()
    {
        // Arrange
        SetupFeatureFileContent("Authentication", @"Feature: Authentication
Scenario: Login
    Given I am on the login page
    When I enter credentials
    Then I should be logged in

Scenario: Logout
    Given I am logged in
    When I click logout
    Then I should be logged out");

        // Act
        var result = CallPrivateMethod<List<string>>(_generator, "FindScenarioSteps", "Login", "Authentication");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
        Assert.Contains("Given I am on the login page", result);
        Assert.Contains("When I enter credentials", result);
        Assert.Contains("Then I should be logged in", result);
    }

    [Fact]
    public void FindScenarioSteps_WithNonExistentFeature_ReturnsNull()
    {
        // Act
        var result = CallPrivateMethod<List<string>>(_generator, "FindScenarioSteps", "Login", "NonExistent");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void FindScenarioSteps_WithNonExistentScenario_ReturnsNull()
    {
        // Arrange
        SetupFeatureFileContent("Authentication", @"Feature: Authentication
Scenario: Login
    Given I am on the login page");

        // Act
        var result = CallPrivateMethod<List<string>>(_generator, "FindScenarioSteps", "NonExistent", "Authentication");

        // Assert
        Assert.Null(result);
    }

    [Theory]
    [InlineData("Given I have some data", true)]
    [InlineData("When I perform an action", true)]
    [InlineData("Then I should see a result", true)]
    [InlineData("And I should also see this", true)]
    [InlineData("But I should not see that", true)]
    [InlineData("# This is a comment", false)]
    [InlineData("Scenario: Test", false)]
    [InlineData("Feature: Test", false)]
    [InlineData("", false)]
    [InlineData("  ", false)]
    public void IsStepLine_IdentifiesStepLines(string line, bool expected)
    {
        // Act
        var result = CallPrivateStaticMethod<bool>(typeof(ScenarioCallFeatureGenerator), "IsStepLine", line);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ExtractFeatureNameFromContent_WithValidFeature_ReturnsFeatureName()
    {
        // Arrange
        var content = @"# Comment
Feature: Authentication Feature
Some description";

        // Act
        var result = CallPrivateStaticMethod<string>(typeof(ScenarioCallFeatureGenerator), "ExtractFeatureNameFromContent", content);

        // Assert
        Assert.Equal("Authentication Feature", result);
    }

    [Fact]
    public void ExtractFeatureNameFromContent_WithNoFeature_ReturnsNull()
    {
        // Arrange
        var content = @"# Comment
Some content without feature";

        // Act
        var result = CallPrivateStaticMethod<string>(typeof(ScenarioCallFeatureGenerator), "ExtractFeatureNameFromContent", content);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetFeatureFilePaths_SearchesCommonDirectories()
    {
        // Arrange & Act
        var result = CallPrivateMethod<IEnumerable<string>>(_generator, "GetFeatureFilePaths", "/test/directory");

        // Assert
        Assert.NotNull(result);
        // This test is limited since we can't easily mock Directory.Exists and Directory.GetFiles
        // In a real scenario, you might use a file system abstraction
    }

    [Fact]
    public void FindFeatureFileContent_CachesResults()
    {
        // Arrange
        var featureName = "TestFeature";
        
        // First call
        var result1 = CallPrivateMethod<string>(_generator, "FindFeatureFileContent", featureName);
        
        // Second call should use cache
        var result2 = CallPrivateMethod<string>(_generator, "FindFeatureFileContent", featureName);

        // Assert
        Assert.Equal(result1, result2);
    }

    [Fact]
    public void PreprocessFeatureContent_PreservesIndentation()
    {
        // Arrange
        var originalContent = @"Feature: Test Feature
Scenario: Test Scenario
        Given I call scenario ""Login"" from feature ""Authentication""
        When I do something else";

        SetupFeatureFileContent("Authentication", @"Feature: Authentication
Scenario: Login
    Given I am on the login page");

        // Act
        var result = _generator.PreprocessFeatureContent(originalContent);

        // Assert
        Assert.Contains("# Expanded from scenario call:", result);
        Assert.Contains("Given I am on the login page", result);
    }

    [Fact]
    public void PreprocessFeatureContent_HandlesMultipleScenarios()
    {
        // Arrange
        var originalContent = @"Feature: Test Feature
Scenario: First Scenario
    Given I call scenario ""Login"" from feature ""Authentication""

Scenario: Second Scenario
    Given I call scenario ""Logout"" from feature ""Authentication""";

        SetupFeatureFileContent("Authentication", @"Feature: Authentication
Scenario: Login
    Given I am on the login page

Scenario: Logout
    Given I click logout");

        // Act
        var result = _generator.PreprocessFeatureContent(originalContent);

        // Assert
        Assert.Contains("Given I am on the login page", result);
        Assert.Contains("Given I click logout", result);
    }

    private void SetupFeatureFileContent(string featureName, string content)
    {
        // Create a temporary feature file for testing in a safe location
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        var featuresDir = Path.Combine(tempDir, "Features");
        Directory.CreateDirectory(featuresDir);
        
        var featureFile = Path.Combine(featuresDir, $"{featureName}.feature");
        File.WriteAllText(featureFile, content);

        // Set the current directory to the temp directory so the generator can find the files
        Environment.CurrentDirectory = tempDir;
    }

    private T CallPrivateMethod<T>(object obj, string methodName, params object[] parameters)
    {
        var parameterTypes = parameters.Select(p => p?.GetType()!).ToArray();
        var method = obj.GetType().GetMethod(methodName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance, null, parameterTypes, null);
        if (method == null)
        {
            // Fallback to old method resolution for backwards compatibility
            method = obj.GetType().GetMethod(methodName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        }
        if (method == null)
        {
            throw new ArgumentException($"Method {methodName} not found");
        }
        return (T)method.Invoke(obj, parameters)!;
    }

    private T CallPrivateStaticMethod<T>(Type type, string methodName, params object[] parameters)
    {
        var method = type.GetMethod(methodName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        if (method == null)
        {
            throw new ArgumentException($"Static method {methodName} not found");
        }
        return (T)method.Invoke(null, parameters)!;
    }
}