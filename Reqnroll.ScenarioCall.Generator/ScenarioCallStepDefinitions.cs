using Reqnroll;

namespace Reqnroll.ScenarioCall.Generator;

/// <summary>
/// Step definitions for scenario call syntax.
/// These are no-op implementations that exist solely for IDE support (Visual Studio extension).
/// The actual scenario expansion happens at build time via the generator plugin.
/// </summary>
[Binding]
public class ScenarioCallStepDefinitions
{
    /// <summary>
    /// English: Given/When/Then/And/But I call scenario "ScenarioName" from feature "FeatureName"
    /// </summary>
    [Given(@"I call scenario ""([^""]+)"" from feature ""([^""]+)""")]
    [When(@"I call scenario ""([^""]+)"" from feature ""([^""]+)""")]
    [Then(@"I call scenario ""([^""]+)"" from feature ""([^""]+)""")]
    public void CallScenarioFromFeature(string scenarioName, string featureName)
    {
        // No-op: This step is expanded at build time by the generator plugin.
        // This binding exists only to satisfy the Visual Studio extension's step definition discovery.
    }

    /// <summary>
    /// Dutch: ik roep scenario "..." aan uit functionaliteit "..."
    /// </summary>
    [Given(@"ik roep scenario ""([^""]+)"" aan uit functionaliteit ""([^""]+)""")]
    [When(@"ik roep scenario ""([^""]+)"" aan uit functionaliteit ""([^""]+)""")]
    [Then(@"ik roep scenario ""([^""]+)"" aan uit functionaliteit ""([^""]+)""")]
    public void RoepScenarioAanUitFunctionaliteit(string scenarioNaam, string functionaliteitNaam)
    {
        // No-op: Deze stap wordt tijdens build-time uitgebreid door de generator plugin.
    }

    /// <summary>
    /// Dutch: ik roep scenario "..." aan van functionaliteit "..."
    /// </summary>
    [Given(@"ik roep scenario ""([^""]+)"" aan van functionaliteit ""([^""]+)""")]
    [When(@"ik roep scenario ""([^""]+)"" aan van functionaliteit ""([^""]+)""")]
    [Then(@"ik roep scenario ""([^""]+)"" aan van functionaliteit ""([^""]+)""")]
    public void RoepScenarioAanVanFunctionaliteit(string scenarioNaam, string functionaliteitNaam)
    {
        // No-op: Deze stap wordt tijdens build-time uitgebreid door de generator plugin.
    }

    /// <summary>
    /// German: ich rufe Szenario "..." auf aus Funktionalität "..."
    /// </summary>
    [Given(@"ich rufe Szenario ""([^""]+)"" auf aus Funktionalität ""([^""]+)""")]
    [When(@"ich rufe Szenario ""([^""]+)"" auf aus Funktionalität ""([^""]+)""")]
    [Then(@"ich rufe Szenario ""([^""]+)"" auf aus Funktionalität ""([^""]+)""")]
    public void RufeSzenarioAufAusFunktionalitaet(string szenarioName, string funktionalitaetName)
    {
        // No-op: Dieser Schritt wird zur Build-Zeit vom Generator-Plugin erweitert.
    }

    /// <summary>
    /// German: ich rufe Szenario "..." auf von Funktionalität "..."
    /// </summary>
    [Given(@"ich rufe Szenario ""([^""]+)"" auf von Funktionalität ""([^""]+)""")]
    [When(@"ich rufe Szenario ""([^""]+)"" auf von Funktionalität ""([^""]+)""")]
    [Then(@"ich rufe Szenario ""([^""]+)"" auf von Funktionalität ""([^""]+)""")]
    public void RufeSzenarioAufVonFunktionalitaet(string szenarioName, string funktionalitaetName)
    {
        // No-op: Dieser Schritt wird zur Build-Zeit vom Generator-Plugin erweitert.
    }

    /// <summary>
    /// French: j'appelle le scénario "..." de la fonctionnalité "..."
    /// </summary>
    [Given(@"j'appelle le scénario ""([^""]+)"" de la fonctionnalité ""([^""]+)""")]
    [When(@"j'appelle le scénario ""([^""]+)"" de la fonctionnalité ""([^""]+)""")]
    [Then(@"j'appelle le scénario ""([^""]+)"" de la fonctionnalité ""([^""]+)""")]
    public void AppelleScenarioDeLaFonctionnalite(string nomScenario, string nomFonctionnalite)
    {
        // No-op: Cette étape est développée au moment de la construction par le plugin générateur.
    }

    /// <summary>
    /// Spanish: llamo al escenario "..." de la característica "..."
    /// </summary>
    [Given(@"llamo al escenario ""([^""]+)"" de la característica ""([^""]+)""")]
    [When(@"llamo al escenario ""([^""]+)"" de la característica ""([^""]+)""")]
    [Then(@"llamo al escenario ""([^""]+)"" de la característica ""([^""]+)""")]
    public void LlamoAlEscenarioDeLaCaracteristica(string nombreEscenario, string nombreCaracteristica)
    {
        // No-op: Este paso se expande en tiempo de compilación mediante el complemento generador.
    }
}
