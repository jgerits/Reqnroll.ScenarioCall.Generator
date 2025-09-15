using Reqnroll.Generator.Plugins;
using Reqnroll.Infrastructure;
using Reqnroll.UnitTestProvider;

[assembly: GeneratorPlugin(typeof(Reqnroll.ScenarioCall.Generator.GeneratorPlugin))]

namespace Reqnroll.ScenarioCall.Generator;

public class GeneratorPlugin : IGeneratorPlugin
{
    public void Initialize(GeneratorPluginEvents generatorPluginEvents, GeneratorPluginParameters generatorPluginParameters, UnitTestProviderConfiguration unitTestProviderConfiguration)
    {
        generatorPluginEvents.RegisterDependencies += (sender, args) =>
        {
            // Register our custom test generator that preprocesses scenario calls
            args.ObjectContainer.RegisterTypeAs<ScenarioCallTestGenerator, Reqnroll.Generator.Interfaces.ITestGenerator>();
        };
    }
}