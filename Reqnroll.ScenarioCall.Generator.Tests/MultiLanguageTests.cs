using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Moq;
using Reqnroll.Generator.UnitTestConverter;
using Reqnroll.Parser;
using Reqnroll.ScenarioCall.Generator;
using Xunit;

namespace Reqnroll.ScenarioCall.Generator.Tests;

[Collection("Sequential")]
public class MultiLanguageTests : IDisposable
{
    private readonly Mock<IFeatureGenerator> _mockBaseGenerator;
    private readonly ScenarioCallFeatureGenerator _generator;
    private string? _testTempDirectory;

    public MultiLanguageTests()
    {
        _mockBaseGenerator = new Mock<IFeatureGenerator>();
        _generator = new ScenarioCallFeatureGenerator(_mockBaseGenerator.Object, null!);
    }

    public void Dispose()
    {
        // Clean up the temporary directory
        if (_testTempDirectory != null && Directory.Exists(_testTempDirectory))
        {
            try
            {
                Directory.Delete(_testTempDirectory, true);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }

    [Fact]
    public void PreprocessFeatureContent_WithGermanFeature_ExpandsScenarioCall()
    {
        // Arrange
        var originalContent = @"# language: de
Funktionalität: Test Funktionalität
Szenario: Test Szenario
    Angenommen I call scenario ""Login"" from feature ""Authentifizierung""";

        SetupFeatureFileContent("Authentifizierung", @"# language: de
Funktionalität: Authentifizierung
Szenario: Login
    Angenommen ich bin auf der Login-Seite
    Wenn ich Anmeldedaten eingebe
    Dann sollte ich eingeloggt sein");

        // Act
        var result = _generator.PreprocessFeatureContent(originalContent);

        // Assert
        Assert.Contains("# Expanded from scenario call", result);
        Assert.Contains("Angenommen ich bin auf der Login-Seite", result);
        Assert.Contains("Wenn ich Anmeldedaten eingebe", result);
        Assert.Contains("Dann sollte ich eingeloggt sein", result);
    }

    [Fact]
    public void PreprocessFeatureContent_WithFrenchFeature_ExpandsScenarioCall()
    {
        // Arrange
        var originalContent = @"# language: fr
Fonctionnalité: Test Fonctionnalité
Scénario: Test Scénario
    Etant donné I call scenario ""Connexion"" from feature ""Authentification""";

        SetupFeatureFileContent("Authentification", @"# language: fr
Fonctionnalité: Authentification
Scénario: Connexion
    Etant donné je suis sur la page de connexion
    Quand je saisis mes identifiants
    Alors je devrais être connecté");

        // Act
        var result = _generator.PreprocessFeatureContent(originalContent);

        // Assert
        Assert.Contains("# Expanded from scenario call", result);
        Assert.Contains("Etant donné je suis sur la page de connexion", result);
        Assert.Contains("Quand je saisis mes identifiants", result);
        Assert.Contains("Alors je devrais être connecté", result);
    }

    [Fact]
    public void PreprocessFeatureContent_WithSpanishFeature_ExpandsScenarioCall()
    {
        // Arrange
        var originalContent = @"# language: es
Característica: Test Característica
Escenario: Test Escenario
    Dado I call scenario ""Inicio de sesión"" from feature ""Autenticación""";

        SetupFeatureFileContent("Autenticación", @"# language: es
Característica: Autenticación
Escenario: Inicio de sesión
    Dado estoy en la página de inicio de sesión
    Cuando ingreso mis credenciales
    Entonces debería estar conectado");

        // Act
        var result = _generator.PreprocessFeatureContent(originalContent);

        // Assert
        Assert.Contains("# Expanded from scenario call", result);
        Assert.Contains("Dado estoy en la página de inicio de sesión", result);
        Assert.Contains("Cuando ingreso mis credenciales", result);
        Assert.Contains("Entonces debería estar conectado", result);
    }

    [Fact]
    public void PreprocessFeatureContent_WithDutchFeature_ExpandsScenarioCall()
    {
        // Arrange
        var originalContent = @"# language: nl
Functionaliteit: Test Functionaliteit
Scenario: Test Scenario
    Gegeven I call scenario ""Inloggen"" from feature ""Authenticatie""";

        SetupFeatureFileContent("Authenticatie", @"# language: nl
Functionaliteit: Authenticatie
Scenario: Inloggen
    Gegeven ik ben op de inlogpagina
    Als ik mijn inloggegevens invoer
    Dan zou ik ingelogd moeten zijn");

        // Act
        var result = _generator.PreprocessFeatureContent(originalContent);

        // Assert
        Assert.Contains("# Expanded from scenario call", result);
        Assert.Contains("Gegeven ik ben op de inlogpagina", result);
        Assert.Contains("Als ik mijn inloggegevens invoer", result);
        Assert.Contains("Dan zou ik ingelogd moeten zijn", result);
    }

    [Fact]
    public void PreprocessFeatureContent_WithDutchFeature_WithDutchTranslation_ExpandsScenarioCall()
    {
        // Arrange
        var originalContent = @"# language: nl
Functionaliteit: Test Functionaliteit
Scenario: Test Scenario
    Gegeven ik roep scenario aan ""Inloggen"" uit functionaliteit ""Authenticatie""";

        SetupFeatureFileContent("Authenticatie", @"# language: nl
Functionaliteit: Authenticatie
Scenario: Inloggen
    Gegeven ik ben op de inlogpagina
    Als ik mijn inloggegevens invoer
    Dan zou ik ingelogd moeten zijn");

        // Act
        var result = _generator.PreprocessFeatureContent(originalContent);

        // Assert
        Assert.Contains("# Expanded from scenario call", result);
        Assert.Contains("Gegeven ik ben op de inlogpagina", result);
        Assert.Contains("Als ik mijn inloggegevens invoer", result);
        Assert.Contains("Dan zou ik ingelogd moeten zijn", result);
    }

    [Fact]
    public void PreprocessFeatureContent_MixedLanguages_ExpandsCorrectly()
    {
        // Arrange - English feature calling German scenario
        var originalContent = @"Feature: Test Feature
Scenario: Test Scenario
    Given I call scenario ""Login"" from feature ""Authentifizierung""";

        SetupFeatureFileContent("Authentifizierung", @"# language: de
Funktionalität: Authentifizierung
Szenario: Login
    Angenommen ich bin auf der Login-Seite
    Wenn ich Anmeldedaten eingebe
    Dann sollte ich eingeloggt sein");

        // Act
        var result = _generator.PreprocessFeatureContent(originalContent);

        // Assert
        Assert.Contains("# Expanded from scenario call", result);
        Assert.Contains("Angenommen ich bin auf der Login-Seite", result);
        Assert.Contains("Wenn ich Anmeldedaten eingebe", result);
        Assert.Contains("Dann sollte ich eingeloggt sein", result);
    }

    [Fact]
    public void PreprocessFeatureContent_WithoutLanguageDirective_DefaultsToEnglish()
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
    public void DetectLanguage_WithGermanDirective_ReturnsGerman()
    {
        // Arrange
        var content = @"# language: de
Funktionalität: Test";

        // Act
        var language = CallPrivateStaticMethod<string>(typeof(ScenarioCallFeatureGenerator), "DetectLanguage", content);

        // Assert
        Assert.Equal("de", language);
    }

    [Fact]
    public void DetectLanguage_WithFrenchDirective_ReturnsFrench()
    {
        // Arrange
        var content = @"# language: fr
Fonctionnalité: Test";

        // Act
        var language = CallPrivateStaticMethod<string>(typeof(ScenarioCallFeatureGenerator), "DetectLanguage", content);

        // Assert
        Assert.Equal("fr", language);
    }

    [Fact]
    public void DetectLanguage_WithoutDirective_ReturnsEnglish()
    {
        // Arrange
        var content = @"Feature: Test";

        // Act
        var language = CallPrivateStaticMethod<string>(typeof(ScenarioCallFeatureGenerator), "DetectLanguage", content);

        // Assert
        Assert.Equal("en", language);
    }

    [Fact]
    public void PreprocessFeatureContent_WithGermanStepKeywords_RecognizesCorrectly()
    {
        // Arrange - German feature using German step keywords
        var originalContent = @"# language: de
Funktionalität: Benutzerverwaltung
Szenario: Neuen Benutzer erstellen
    Angenommen I call scenario ""Login"" from feature ""Authentifizierung""
    Wenn ich zum Benutzerverwaltungsbereich navigiere
    Dann sollte ich die Benutzerliste sehen";

        SetupFeatureFileContent("Authentifizierung", @"# language: de
Funktionalität: Authentifizierung
Szenario: Login
    Angenommen ich bin auf der Login-Seite
    Und ich gebe meinen Benutzernamen ein
    Und ich gebe mein Passwort ein");

        // Act
        var result = _generator.PreprocessFeatureContent(originalContent);

        // Assert
        Assert.Contains("# Expanded from scenario call", result);
        Assert.Contains("Angenommen ich bin auf der Login-Seite", result);
        Assert.Contains("Und ich gebe meinen Benutzernamen ein", result);
        Assert.Contains("Und ich gebe mein Passwort ein", result);
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

    private T CallPrivateStaticMethod<T>(Type type, string methodName, params object[] parameters)
    {
        var methods = type.GetMethods(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
            .Where(m => m.Name == methodName);
            
        var method = methods.FirstOrDefault(m => m.GetParameters().Length == parameters.Length);
        
        if (method == null)
        {
            throw new ArgumentException($"Static method {methodName} with {parameters.Length} parameters not found");
        }
        
        return (T)method.Invoke(null, parameters)!;
    }
}
