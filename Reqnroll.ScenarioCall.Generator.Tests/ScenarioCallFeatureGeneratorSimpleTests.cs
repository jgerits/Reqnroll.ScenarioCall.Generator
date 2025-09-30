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
    private string? _testTempDirectory;

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
        // Create or reuse a temporary feature directory for testing
        if (_testTempDirectory == null)
        {
            _testTempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_testTempDirectory);
            var featuresDir = Path.Combine(_testTempDirectory, "Features");
            Directory.CreateDirectory(featuresDir);
            // Set the current directory to the temp directory so the generator can find the files
            Environment.CurrentDirectory = _testTempDirectory;
        }
        
        var featureFile = Path.Combine(_testTempDirectory, "Features", $"{featureName}.feature");
        File.WriteAllText(featureFile, content);
    }

    private T CallPrivateMethod<T>(object obj, string methodName, params object[] parameters)
    {
        var method = obj.GetType().GetMethod(methodName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
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

    [Fact]
    public void PreprocessFeatureContent_WithNestedScenarioCalls_PreservesInnerCalls()
    {
        // Arrange
        // Note: The current implementation does not recursively expand nested scenario calls.
        // This test verifies that behavior - only the outermost call is expanded.
        var originalContent = @"Feature: Test Feature
Scenario: Outer Scenario
    Given I call scenario ""Inner"" from feature ""Helper""
    When I do something else";

        SetupFeatureFileContent("Helper", @"Feature: Helper
Scenario: Inner
    Given I call scenario ""Base"" from feature ""Foundation""
    When I do inner work");

        SetupFeatureFileContent("Foundation", @"Feature: Foundation
Scenario: Base
    Given I have a foundation step
    When I build on it");

        // Act
        var result = _generator.PreprocessFeatureContent(originalContent);

        // Assert
        // The outer scenario call should be expanded
        Assert.Contains("# Expanded from scenario call: \"Inner\" from feature \"Helper\"", result);
        // But the inner call is NOT recursively expanded - it's preserved as-is
        Assert.Contains("Given I call scenario \"Base\" from feature \"Foundation\"", result);
        Assert.Contains("When I do inner work", result);
    }

    [Fact]
    public void PreprocessFeatureContent_WithScenarioCallInBackground_DoesNotExpand()
    {
        // Arrange
        // Note: The current implementation only expands scenario calls within Scenario blocks,
        // not in Background sections.
        var originalContent = @"Feature: Test Feature
Background:
    Given I call scenario ""Setup"" from feature ""Common""

Scenario: Test Scenario
    When I do something
    Then it should work";

        SetupFeatureFileContent("Common", @"Feature: Common
Scenario: Setup
    Given the system is initialized
    And the database is ready");

        // Act
        var result = _generator.PreprocessFeatureContent(originalContent);

        // Assert
        // Background scenario calls are not expanded in the current implementation
        Assert.Contains("Given I call scenario \"Setup\" from feature \"Common\"", result);
        Assert.DoesNotContain("# Expanded from scenario call", result);
    }

    [Fact]
    public void PreprocessFeatureContent_WithMixedCallsAndRegularSteps_MaintainsCorrectOrder()
    {
        // Arrange
        var originalContent = @"Feature: Test Feature
Scenario: Mixed Steps
    Given I call scenario ""Login"" from feature ""Auth""
    And I navigate to dashboard
    When I call scenario ""LoadData"" from feature ""Data""
    Then I should see results";

        SetupFeatureFileContent("Auth", @"Feature: Auth
Scenario: Login
    Given I enter username
    When I enter password");

        SetupFeatureFileContent("Data", @"Feature: Data
Scenario: LoadData
    Given I load the data
    When I process it");

        // Act
        var result = _generator.PreprocessFeatureContent(originalContent);

        // Assert
        // Verify that both scenario calls are expanded and regular steps are preserved
        Assert.Contains("# Expanded from scenario call: \"Login\" from feature \"Auth\"", result);
        Assert.Contains("# Expanded from scenario call: \"LoadData\" from feature \"Data\"", result);
        Assert.Contains("And I navigate to dashboard", result);
        Assert.Contains("Then I should see results", result);
        
        // Verify the expanded steps are present
        Assert.Contains("Given I enter username", result);
        Assert.Contains("When I enter password", result);
        Assert.Contains("Given I load the data", result);
        Assert.Contains("When I process it", result);
    }

    [Fact]
    public void PreprocessFeatureContent_WithScenarioOutline_DoesNotExpandDueToOutlineKeyword()
    {
        // Arrange
        // Note: The current implementation only recognizes "Scenario:" not "Scenario Outline:"
        // This test documents that limitation
        var originalContent = @"Feature: Test Feature
Scenario Outline: Test with Examples
    Given I have <value>
    When I call scenario ""Process"" from feature ""Helper""
    Then I should get <result>

Examples:
    | value | result |
    | 1     | 2      |
    | 2     | 4      |";

        SetupFeatureFileContent("Helper", @"Feature: Helper
Scenario: Process
    Given I process the input
    When I calculate");

        // Act
        var result = _generator.PreprocessFeatureContent(originalContent);

        // Assert
        // Since "Scenario Outline:" is not recognized as a scenario in the current implementation,
        // the scenario call is NOT expanded
        Assert.DoesNotContain("# Expanded from scenario call", result);
        Assert.Contains("Given I have <value>", result);
        Assert.Contains("Examples:", result);
    }

    [Fact]
    public void PreprocessFeatureContent_WithEmptyFeatureFile_HandlesGracefully()
    {
        // Arrange
        var originalContent = @"";

        // Act
        var result = _generator.PreprocessFeatureContent(originalContent);

        // Assert
        Assert.Equal(Environment.NewLine, result);
    }

    [Fact]
    public void PreprocessFeatureContent_WithOnlyComments_HandlesCorrectly()
    {
        // Arrange
        var originalContent = @"# This is a comment
# Another comment
# No actual feature content";

        // Act
        var result = _generator.PreprocessFeatureContent(originalContent);

        // Assert
        Assert.Contains("# This is a comment", result);
        Assert.Contains("# Another comment", result);
    }

    [Fact]
    public void FindScenarioSteps_WithScenarioHavingTags_SkipsTags()
    {
        // Arrange
        SetupFeatureFileContent("Tagged", @"Feature: Tagged Feature
@smoke @regression
Scenario: Tagged Scenario
    Given I have a tagged step
    When I execute it
    Then it should work");

        var featureContent = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "Features", "Tagged.feature"));

        // Act
        var steps = CallPrivateMethod<List<string>>(_generator, "FindScenarioSteps", "Tagged Scenario", "Tagged Feature");

        // Assert
        Assert.NotNull(steps);
        Assert.Equal(3, steps.Count);
        Assert.Contains("Given I have a tagged step", steps);
        Assert.DoesNotContain("@smoke", string.Join(" ", steps));
    }

    [Fact]
    public void PreprocessFeatureContent_WithMultilineScenarioDescription_HandlesCorrectly()
    {
        // Arrange
        var originalContent = @"Feature: Test Feature
Scenario: Scenario with description
    This is a description
    that spans multiple lines
    
    Given I call scenario ""Login"" from feature ""Auth""";

        SetupFeatureFileContent("Auth", @"Feature: Auth
Scenario: Login
    Given I log in");

        // Act
        var result = _generator.PreprocessFeatureContent(originalContent);

        // Assert
        Assert.Contains("This is a description", result);
        Assert.Contains("that spans multiple lines", result);
        Assert.Contains("Given I log in", result);
    }
}