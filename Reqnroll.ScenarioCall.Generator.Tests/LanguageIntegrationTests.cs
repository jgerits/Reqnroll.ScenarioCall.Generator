using System;
using System.IO;
using Moq;
using Reqnroll.Generator.UnitTestConverter;
using Reqnroll.ScenarioCall.Generator;
using Xunit;

namespace Reqnroll.ScenarioCall.Generator.Tests;

public class LanguageIntegrationTests
{
    private readonly Mock<IFeatureGenerator> _mockBaseGenerator;
    private readonly ScenarioCallFeatureGenerator _generator;

    public LanguageIntegrationTests()
    {
        _mockBaseGenerator = new Mock<IFeatureGenerator>();
        _generator = new ScenarioCallFeatureGenerator(_mockBaseGenerator.Object, null!);
    }

    [Fact]
    public void PreprocessFeatureContent_WithGermanLanguage_ExpandsScenarioCall()
    {
        // Arrange - German keywords and localized scenario call phrase
        var originalContent = @"#language: de-DE
Feature: Test Feature
Scenario: Test Scenario
    Angenommen ich rufe Szenario ""Anmeldung"" aus Feature ""Authentifizierung""";

        SetupFeatureFileContent("Authentifizierung", @"#language: de-DE
Feature: Authentifizierung
Scenario: Anmeldung
    Angenommen ich bin auf der Anmeldeseite
    Wenn ich die Anmeldedaten eingebe
    Dann sollte ich angemeldet sein");

        // Act
        var result = _generator.PreprocessFeatureContent(originalContent);

        // Assert
        Assert.Contains("# Expanded from scenario call", result);
        Assert.Contains("Angenommen ich bin auf der Anmeldeseite", result);
        Assert.Contains("Wenn ich die Anmeldedaten eingebe", result);
        Assert.Contains("Dann sollte ich angemeldet sein", result);
    }

    [Fact]
    public void PreprocessFeatureContent_WithFrenchLanguage_ExpandsScenarioCall()
    {
        // Arrange - French keywords and localized scenario call phrase
        var originalContent = @"#language: fr-FR
Feature: Fonctionnalité de test
Scenario: Scénario de test
    Soit j'appelle le scénario ""Connexion"" de la fonctionnalité ""Authentification""";

        SetupFeatureFileContent("Authentification", @"#language: fr-FR
Feature: Authentification
Scenario: Connexion
    Soit que je suis sur la page de connexion
    Quand je saisis mes identifiants
    Alors je devrais être connecté");

        // Act
        var result = _generator.PreprocessFeatureContent(originalContent);

        // Assert
        Assert.Contains("# Expanded from scenario call", result);
        Assert.Contains("Soit que je suis sur la page de connexion", result);
        Assert.Contains("Quand je saisis mes identifiants", result);
        Assert.Contains("Alors je devrais être connecté", result);
    }

    [Fact]
    public void PreprocessFeatureContent_WithSpanishLanguage_ExpandsScenarioCall()
    {
        // Arrange
        var originalContent = @"#language: es-ES
Feature: Funcionalidad de prueba
Scenario: Escenario de prueba
    Dado I call scenario ""Inicio de sesión"" from feature ""Autenticación""";

        SetupFeatureFileContent("Autenticación", @"#language: es-ES
Feature: Autenticación
Scenario: Inicio de sesión
    Dado que estoy en la página de inicio de sesión
    Cuando ingreso mis credenciales
    Entonces debería estar conectado");

        // Act
        var result = _generator.PreprocessFeatureContent(originalContent);

        // Assert
        Assert.Contains("# Expanded from scenario call", result);
        Assert.Contains("Dado que estoy en la página de inicio de sesión", result);
        Assert.Contains("Cuando ingreso mis credenciales", result);
        Assert.Contains("Entonces debería estar conectado", result);
    }

    [Fact]
    public void PreprocessFeatureContent_WithDutchLanguage_ExpandsScenarioCall()
    {
        // Arrange - Dutch keywords and localized scenario call phrase
        var originalContent = @"#language: nl-NL
Feature: Gebruikers authenticatie
Scenario: Succesvolle inloggen
    Gegeven ik roep scenario ""Inloggen"" van feature ""Authenticatie""";

        SetupFeatureFileContent("Authenticatie", @"#language: nl-NL
Feature: Authenticatie
Scenario: Inloggen
    Gegeven ik ben op de inlogpagina
    Als ik geldige inloggegevens invoer
    Dan zou ik succesvol ingelogd moeten zijn");

        // Act
        var result = _generator.PreprocessFeatureContent(originalContent);

        // Assert
        Assert.Contains("# Expanded from scenario call", result);
        Assert.Contains("Gegeven ik ben op de inlogpagina", result);
        Assert.Contains("Als ik geldige inloggegevens invoer", result);
        Assert.Contains("Dan zou ik succesvol ingelogd moeten zijn", result);
    }

    [Fact]
    public void PreprocessFeatureContent_WithMixedLanguages_HandlesCorrectly()
    {
        // Arrange - Main feature in English, calling German feature
        var originalContent = @"Feature: Mixed Language Test
Scenario: Test Scenario
    Given I call scenario ""Anmeldung"" from feature ""Authentifizierung""";

        SetupFeatureFileContent("Authentifizierung", @"#language: de-DE
Feature: Authentifizierung
Scenario: Anmeldung
    Gegeben sei ich bin auf der Anmeldeseite
    Wenn ich die Anmeldedaten eingebe");

        // Act
        var result = _generator.PreprocessFeatureContent(originalContent);

        // Assert
        Assert.Contains("# Expanded from scenario call", result);
        Assert.Contains("Gegeben sei ich bin auf der Anmeldeseite", result);
        Assert.Contains("Wenn ich die Anmeldedaten eingebe", result);
    }

    [Fact]
    public void PreprocessFeatureContent_WithPortugueseLanguage_ExpandsScenarioCall()
    {
        // Arrange
        var originalContent = @"#language: pt-BR
Feature: Funcionalidade de teste
Scenario: Cenário de teste
    Dado I call scenario ""Login"" from feature ""Autenticação""";

        SetupFeatureFileContent("Autenticação", @"#language: pt-BR
Feature: Autenticação
Scenario: Login
    Dado que estou na página de login
    Quando eu insiro minhas credenciais
    Então eu deveria estar logado");

        // Act
        var result = _generator.PreprocessFeatureContent(originalContent);

        // Assert
        Assert.Contains("# Expanded from scenario call", result);
        Assert.Contains("Dado que estou na página de login", result);
        Assert.Contains("Quando eu insiro minhas credenciais", result);
        Assert.Contains("Então eu deveria estar logado", result);
    }

    [Fact]
    public void PreprocessFeatureContent_WithItalianLanguage_ExpandsScenarioCall()
    {
        // Arrange
        var originalContent = @"#language: it-IT
Feature: Funzionalità di test
Scenario: Scenario di test
    Dato I call scenario ""Accesso"" from feature ""Autenticazione""";

        SetupFeatureFileContent("Autenticazione", @"#language: it-IT
Feature: Autenticazione
Scenario: Accesso
    Dato che sono sulla pagina di accesso
    Quando inserisco le mie credenziali
    Allora dovrei essere collegato");

        // Act
        var result = _generator.PreprocessFeatureContent(originalContent);

        // Assert
        Assert.Contains("# Expanded from scenario call", result);
        Assert.Contains("Dato che sono sulla pagina di accesso", result);
        Assert.Contains("Quando inserisco le mie credenziali", result);
        Assert.Contains("Allora dovrei essere collegato", result);
    }

    [Fact]
    public void PreprocessFeatureContent_WithMultipleScenarioCallsInDifferentLanguages_ExpandsAll()
    {
        // Arrange
        var originalContent = @"#language: de-DE
Feature: Multi-Call Test
Scenario: Erstes Szenario
    Gegeben sei I call scenario ""Setup"" from feature ""Common""
    Und I call scenario ""Login"" from feature ""Auth""";

        SetupFeatureFileContent("Common", @"Feature: Common
Scenario: Setup
    Given the system is initialized
    And the database is clean");

        SetupFeatureFileContent("Auth", @"Feature: Auth
Scenario: Login
    Given I am on the login page
    When I enter credentials");

        // Act
        var result = _generator.PreprocessFeatureContent(originalContent);

        // Assert
        Assert.Contains("Given the system is initialized", result);
        Assert.Contains("And the database is clean", result);
        Assert.Contains("Given I am on the login page", result);
        Assert.Contains("When I enter credentials", result);
    }

    [Fact]
    public void PreprocessFeatureContent_WithInvalidLanguageScenarioCall_AddsWarning()
    {
        // Arrange
        var originalContent = @"#language: de-DE
Feature: Test Feature
Scenario: Test Scenario
    Gegeben sei I call scenario ""NonExistent"" from feature ""NonExistent""";

        // Act
        var result = _generator.PreprocessFeatureContent(originalContent);

        // Assert
        Assert.Contains("# Warning: Could not expand scenario call", result);
    }

    [Fact]
    public void PreprocessFeatureContent_WithLanguageInMiddleOfFile_UsesDefaultEnglish()
    {
        // Arrange - Language directive not at the top
        var originalContent = @"Feature: Test Feature
#language: de-DE
Scenario: Test Scenario
    Given I call scenario ""Login"" from feature ""Auth""";

        SetupFeatureFileContent("Auth", @"Feature: Auth
Scenario: Login
    Given I am on the login page");

        // Act
        var result = _generator.PreprocessFeatureContent(originalContent);

        // Assert
        Assert.Contains("# Expanded from scenario call", result);
        Assert.Contains("Given I am on the login page", result);
    }

    [Fact]
    public void PreprocessFeatureContent_WithBackwardCompatibility_ExpandsEnglishPhrases()
    {
        // Arrange - Using English phrases with German keywords for backward compatibility
        var originalContent = @"#language: de-DE
Feature: Test Feature
Scenario: Test Scenario
    Angenommen I call scenario ""Login"" from feature ""Auth""";

        SetupFeatureFileContent("Auth", @"Feature: Auth
Scenario: Login
    Given I am on the login page");

        // Act
        var result = _generator.PreprocessFeatureContent(originalContent);

        // Assert
        Assert.Contains("# Expanded from scenario call", result);
        Assert.Contains("Given I am on the login page", result);
    }

    private string? _tempDir;

    private void SetupFeatureFileContent(string featureName, string content)
    {
        // Create a temporary feature file for testing in a safe location
        // Use the same temp directory for all calls during a test
        if (_tempDir == null)
        {
            _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_tempDir);
            var featuresDir = Path.Combine(_tempDir, "Features");
            Directory.CreateDirectory(featuresDir);
            
            // Set the current directory to the temp directory so the generator can find the files
            Environment.CurrentDirectory = _tempDir;
        }
        
        var featuresDirectory = Path.Combine(_tempDir, "Features");
        var featureFile = Path.Combine(featuresDirectory, $"{featureName}.feature");
        File.WriteAllText(featureFile, content);
    }
}