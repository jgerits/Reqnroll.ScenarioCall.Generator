using Reqnroll.Generator.UnitTestConverter;
using Reqnroll.Parser;

namespace Reqnroll.ScenarioCall.Generator;

public class ScenarioCallFeatureGeneratorProvider(UnitTestFeatureGeneratorProvider baseProvider)
    : IFeatureGeneratorProvider
{
    private readonly IFeatureGeneratorProvider _baseProvider = baseProvider;

    public int Priority => PriorityValues.High; // Higher priority than base provider

    public bool CanGenerate(ReqnrollDocument document)
    {
        return _baseProvider.CanGenerate(document);
    }

    public IFeatureGenerator CreateGenerator(ReqnrollDocument document)
    {
        var baseGenerator = _baseProvider.CreateGenerator(document);
        return new ScenarioCallFeatureGenerator(baseGenerator, document);
    }
}