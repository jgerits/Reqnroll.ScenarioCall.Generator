using System;
using System.Linq;
using Reqnroll.ScenarioCall.Generator;
using Xunit;

namespace Reqnroll.ScenarioCall.Generator.Tests;

public class LanguageHelperTests
{
    private readonly LanguageHelper _languageHelper = new();

    [Theory]
    [InlineData("#language: en", "en")]
    [InlineData("#language: de-DE", "de-DE")]
    [InlineData("#language: fr-FR", "fr-FR")]
    [InlineData("#language: es", "es")]
    [InlineData("# language: it-IT", "it-IT")]
    [InlineData("#Language: nl-NL", "nl-NL")]
    [InlineData("", "en")]
    [InlineData("Feature: Test", "en")]
    public void ExtractLanguage_ExtractsCorrectLanguage(string featureContent, string expectedLanguage)
    {
        // Act
        var result = _languageHelper.ExtractLanguage(featureContent);

        // Assert
        Assert.Equal(expectedLanguage, result);
    }

    [Fact]
    public void ExtractLanguage_WithMultilineFeature_ExtractsLanguageFromHeader()
    {
        // Arrange
        var featureContent = @"#language: de-DE
# This is a comment
Feature: Test Feature
Scenario: Test Scenario";

        // Act
        var result = _languageHelper.ExtractLanguage(featureContent);

        // Assert
        Assert.Equal("de-DE", result);
    }

    [Theory]
    [InlineData("Given I have something", "en", true)]
    [InlineData("When I do something", "en", true)]
    [InlineData("Then I should see something", "en", true)]
    [InlineData("And I also do this", "en", true)]
    [InlineData("But I should not see that", "en", true)]
    [InlineData("* I can use asterisk", "en", true)]
    [InlineData("Angenommen ich habe etwas", "de", true)]
    [InlineData("Wenn ich etwas mache", "de", true)]
    [InlineData("Dann sehe ich etwas", "de", true)]
    [InlineData("Und ich mache auch das", "de", true)]
    [InlineData("Aber ich sehe das nicht", "de", true)]
    [InlineData("Soit je ai quelque chose", "fr", true)]
    [InlineData("Quand je fais quelque chose", "fr", true)]
    [InlineData("Alors je vois quelque chose", "fr", true)]
    [InlineData("Et je fais aussi cela", "fr", true)]
    [InlineData("# This is a comment", "en", false)]
    [InlineData("Feature: Test", "en", false)]
    [InlineData("Scenario: Test", "en", false)]
    [InlineData("", "en", false)]
    [InlineData("  ", "en", false)]
    public void IsStepLine_IdentifiesStepLinesInDifferentLanguages(string line, string language, bool expected)
    {
        // Act
        var result = _languageHelper.IsStepLine(line, language);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(@"Given I call scenario ""Login"" from feature ""Auth""", "en", true)]
    [InlineData(@"When I call scenario ""Logout"" from feature ""Auth""", "en", true)]
    [InlineData(@"Then I call scenario ""Setup"" from feature ""Test""", "en", true)]
    [InlineData(@"And I call scenario ""Cleanup"" from feature ""Test""", "en", true)]
    [InlineData(@"But I call scenario ""Reset"" from feature ""Test""", "en", true)]
    [InlineData(@"Angenommen I call scenario ""Login"" from feature ""Auth""", "de", true)]
    [InlineData(@"Wenn I call scenario ""Login"" from feature ""Auth""", "de", true)]
    [InlineData(@"Dann I call scenario ""Login"" from feature ""Auth""", "de", true)]
    [InlineData(@"Und I call scenario ""Login"" from feature ""Auth""", "de", true)]
    [InlineData(@"Aber I call scenario ""Login"" from feature ""Auth""", "de", true)]
    [InlineData(@"Soit I call scenario ""Login"" from feature ""Auth""", "fr", true)]
    [InlineData(@"Quand I call scenario ""Login"" from feature ""Auth""", "fr", true)]
    [InlineData(@"Alors I call scenario ""Login"" from feature ""Auth""", "fr", true)]
    [InlineData(@"Et I call scenario ""Login"" from feature ""Auth""", "fr", true)]
    [InlineData(@"Given I have some data", "en", false)]
    [InlineData(@"When I perform an action", "en", false)]
    [InlineData(@"I call scenario ""Test"" from feature ""Test""", "en", false)]
    [InlineData(@"Given I call scenario Login from feature Auth", "en", false)]
    public void IsScenarioCallStep_DetectsScenarioCallStepsInDifferentLanguages(string stepText, string language, bool expected)
    {
        // Act
        var result = _languageHelper.IsScenarioCallStep(stepText, language);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(@"Given I call scenario ""Login"" from feature ""Authentication""", "en", "Login", "Authentication")]
    [InlineData(@"When I call scenario ""Logout"" from feature ""User Management""", "en", "Logout", "User Management")]
    [InlineData(@"Angenommen I call scenario ""Anmeldung"" from feature ""Authentifizierung""", "de", "Anmeldung", "Authentifizierung")]
    [InlineData(@"Quand I call scenario ""Connexion"" from feature ""Authentification""", "fr", "Connexion", "Authentification")]
    public void ExtractScenarioCall_ExtractsCorrectInformation(string stepText, string language, string expectedScenario, string expectedFeature)
    {
        // Act
        var result = _languageHelper.ExtractScenarioCall(stepText, language);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedScenario, result.Value.scenarioName);
        Assert.Equal(expectedFeature, result.Value.featureName);
    }

    [Theory]
    [InlineData(@"Given I have some data", "en")]
    [InlineData(@"When I perform an action", "en")]
    [InlineData(@"I call scenario ""Test"" from feature ""Test""", "en")]
    [InlineData(@"Given I call scenario Login from feature Auth", "en")]
    public void ExtractScenarioCall_ReturnsNullForInvalidSteps(string stepText, string language)
    {
        // Act
        var result = _languageHelper.ExtractScenarioCall(stepText, language);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetSupportedLanguages_ReturnsListOfLanguages()
    {
        // Act
        var result = _languageHelper.GetSupportedLanguages().ToList();

        // Assert
        Assert.NotEmpty(result);
        Assert.Contains("en", result);
        Assert.Contains("de-DE", result);
        Assert.Contains("fr-FR", result);
        Assert.Contains("es-ES", result);
    }

    [Theory]
    [InlineData("en")]
    [InlineData("de-DE")]
    [InlineData("fr-FR")]
    [InlineData("es-ES")]
    [InlineData("invalid-language")]
    public void GetDialect_ReturnsDialectForLanguage(string language)
    {
        // Act
        var dialect = _languageHelper.GetDialect(language);

        // Assert
        Assert.NotNull(dialect);
        Assert.NotEmpty(dialect.GivenStepKeywords);
        Assert.NotEmpty(dialect.WhenStepKeywords);
        Assert.NotEmpty(dialect.ThenStepKeywords);
    }

    [Fact]
    public void GetDialect_CachesDialects()
    {
        // Act
        var dialect1 = _languageHelper.GetDialect("en");
        var dialect2 = _languageHelper.GetDialect("en");

        // Assert
        Assert.Same(dialect1, dialect2);
    }
}