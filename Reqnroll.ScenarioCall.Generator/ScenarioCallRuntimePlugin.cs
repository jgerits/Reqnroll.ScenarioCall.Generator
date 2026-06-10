using System;
using Reqnroll.Plugins;
using Reqnroll.UnitTestProvider;

namespace Reqnroll.ScenarioCall.Generator;

public class ScenarioCallRuntimePlugin : IRuntimePlugin
{
    private static readonly string AssemblyName = typeof(ScenarioCallRuntimePlugin).Assembly.GetName().Name;

    public void Initialize(RuntimePluginEvents runtimePluginEvents, RuntimePluginParameters runtimePluginParameters, UnitTestProviderConfiguration unitTestProviderConfiguration)
    {
        runtimePluginEvents.ConfigurationDefaults += (_, args) =>
        {
            if (!args.ReqnrollConfiguration.AdditionalStepAssemblies.Contains(AssemblyName))
            {
                args.ReqnrollConfiguration.AdditionalStepAssemblies.Add(AssemblyName);
            }
        };
    }
}
