using Reqnroll.Plugins;
using Reqnroll.UnitTestProvider;

[assembly: RuntimePlugin(typeof(Reqnroll.ScenarioCall.Generator.RuntimePlugin))]

namespace Reqnroll.ScenarioCall.Generator;

/// <summary>
/// Runtime plugin that registers scenario call step definitions for IDE support.
/// The actual scenario expansion happens at build time via the generator plugin.
/// </summary>
public class RuntimePlugin : IRuntimePlugin
{
    public void Initialize(RuntimePluginEvents runtimePluginEvents, RuntimePluginParameters runtimePluginParameters, UnitTestProviderConfiguration unitTestProviderConfiguration)
    {
        // No additional runtime configuration needed.
        // Step definitions are automatically discovered via the [Binding] attribute.
    }
}
