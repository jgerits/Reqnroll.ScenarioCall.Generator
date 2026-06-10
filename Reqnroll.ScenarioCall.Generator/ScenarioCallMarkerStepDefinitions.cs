using Reqnroll;

namespace Reqnroll.ScenarioCall.Generator;

[Binding]
public sealed class ScenarioCallMarkerStepDefinitions
{
    [StepDefinition(@"scenario call: ""([^""]+)"" from feature ""([^""]+)""(?: with background)?")]
    public void ScenarioCallMarker(string scenarioName, string featureName)
    {
        // Marker step only: keeps scenario-call boundaries visible in generated test output.
    }
}
