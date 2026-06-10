using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Reqnroll.Configuration;
using Reqnroll.Generator;
using Reqnroll.Generator.CodeDom;
using Reqnroll.Generator.Configuration;
using Reqnroll.Generator.Interfaces;
using Reqnroll.Generator.UnitTestConverter;
using Reqnroll.Parser;
using Gherkin;

namespace Reqnroll.ScenarioCall.Generator;

public class ScenarioCallTestGenerator : TestGenerator
{
    private const string ScenarioCallMarkerPrefix = "scenario call:";
    private const string ScenarioCallExpansionFailedPrefix = "scenario call expansion failed:";

    private readonly Dictionary<string, string> _featureFileCache = new();
    private readonly Dictionary<string, GherkinDialect> _dialectCache = new();
    private readonly ProjectSettings _projectSettings;

    public ScenarioCallTestGenerator(
        ReqnrollConfiguration reqnrollConfiguration,
        ProjectSettings projectSettings,
        IFeatureGeneratorRegistry featureGeneratorRegistry,
        CodeDomHelper codeDomHelper,
        IGherkinParserFactory gherkinParserFactory,
        GeneratorInfo generatorInfo)
        : base(reqnrollConfiguration, projectSettings, featureGeneratorRegistry, codeDomHelper, gherkinParserFactory, generatorInfo)
    {
        _projectSettings = projectSettings;
    }

    protected override ReqnrollDocument ParseContent(IGherkinParser parser, TextReader contentReader, ReqnrollDocumentLocation documentLocation)
    {
        var originalContent = contentReader.ReadToEnd();

        var expandedContent = PreprocessFeatureContent(originalContent);

        using var expandedReader = new StringReader(expandedContent);
        return parser.Parse(expandedReader, documentLocation);
    }

    private GherkinDialect GetDialect(string content)
    {
        var language = ScenarioCallParsing.DetectLanguage(content);
        
        if (!_dialectCache.TryGetValue(language, out var dialect))
        {
            var dialectProvider = new GherkinDialectProvider(language);
            dialect = dialectProvider.DefaultDialect;
            _dialectCache[language] = dialect;
        }
        
        return dialect;
    }

    private bool StartsWithAnyKeyword(string line, IEnumerable<string> keywords)
    {
        return ScenarioCallParsing.StartsWithAnyKeyword(line, keywords);
    }

    private string PreprocessFeatureContent(string originalContent)
    {
        var dialect = GetDialect(originalContent);
        var lines = originalContent.Split('\n');
        var result = new StringBuilder();
        var inScenario = false;
        var currentFeatureName = ExtractFeatureNameFromContent(originalContent, dialect);

        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();

            if (ScenarioCallParsing.IsScenarioOutlineLine(trimmedLine) || ScenarioCallParsing.IsExamplesLine(trimmedLine))
            {
                inScenario = false;
                result.AppendLine(line);
                continue;
            }
                
            if (StartsWithAnyKeyword(trimmedLine, dialect.ScenarioKeywords))
            {
                inScenario = true;
                result.AppendLine(line);
                continue;
            }
                
            if (StartsWithAnyKeyword(trimmedLine, dialect.FeatureKeywords) || 
                StartsWithAnyKeyword(trimmedLine, dialect.BackgroundKeywords))
            {
                inScenario = false;
                result.AppendLine(line);
                continue;
            }
                
            if (inScenario && IsScenarioCallStep(trimmedLine, dialect))
            {
                var expandedSteps = ExpandScenarioCall(line, currentFeatureName, dialect);
                if (expandedSteps != null)
                {
                    result.Append(expandedSteps);
                    continue; 
                }
                else
                {
                    var leadingWhitespace = line.Substring(0, line.Length - line.TrimStart().Length);
                    result.AppendLine($"{leadingWhitespace}# Warning: Could not expand scenario call");
                }
            }
                
            result.AppendLine(line);
        }

        return result.ToString();
    }

    private bool IsScenarioCallStep(string stepText, GherkinDialect dialect)
    {
        return ScenarioCallParsing.IsScenarioCallStep(stepText, dialect);
    }

    private string ExpandScenarioCall(string callStepLine, string currentFeatureName, GherkinDialect dialect)
    {
        var scenarioCall = ScenarioCallParsing.TryParseScenarioCall(callStepLine, dialect);
        if (scenarioCall == null) return null;
        
        var leadingWhitespace = callStepLine.Substring(0, callStepLine.Length - callStepLine.TrimStart().Length);

        try
        {
            var backgroundSteps = scenarioCall.IncludeBackground ? FindBackgroundSteps(scenarioCall.FeatureName) : null;
            var (scenarioSteps, diagnosticMessage) = FindScenarioStepsWithDiagnostics(scenarioCall.ScenarioName, scenarioCall.FeatureName);
            
            // Need at least scenario steps to expand
            if (scenarioSteps != null && scenarioSteps.Any())
            {
                var result = new StringBuilder();
                result.AppendLine($"{leadingWhitespace}# Expanded from scenario call: \"{scenarioCall.ScenarioName}\" from feature \"{scenarioCall.FeatureName}\"");
                result.AppendLine(FormatScenarioCallMarkerStep(leadingWhitespace, scenarioCall.CallKeyword, scenarioCall.ScenarioName, scenarioCall.FeatureName, scenarioCall.IncludeBackground));
                
                // Include Background steps only if requested
                if (scenarioCall.IncludeBackground && backgroundSteps != null && backgroundSteps.Any())
                {
                    result.AppendLine($"{leadingWhitespace}# Including Background steps from feature \"{scenarioCall.FeatureName}\"");
                    foreach (var step in backgroundSteps)
                    {
                        result.AppendLine($"{leadingWhitespace}{step}");
                    }
                }
                    
                // Include Scenario steps
                foreach (var step in scenarioSteps)
                {
                    result.AppendLine($"{leadingWhitespace}{step}");
                }
                    
                return result.ToString();
            }
            else if (!string.IsNullOrEmpty(diagnosticMessage))
            {
                return FormatDiagnosticScenarioCallFailure(leadingWhitespace, dialect, diagnosticMessage);
            }
        }
        catch (Exception ex)
        {
            return FormatDiagnosticScenarioCallFailure(leadingWhitespace, dialect, $"Exception during scenario call expansion - {ex.Message}");
        }

        return null;
    }

    private static string FormatScenarioCallMarkerStep(string leadingWhitespace, string callKeyword, string scenarioName, string featureName, bool includeBackground)
    {
        var suffix = includeBackground ? " with background" : string.Empty;
        return $"{leadingWhitespace}{callKeyword} {ScenarioCallMarkerPrefix} \"{scenarioName}\" from feature \"{featureName}\"{suffix}";
    }

    private static string FormatDiagnosticScenarioCallFailure(string leadingWhitespace, GherkinDialect dialect, string message)
    {
        var stepKeyword = GetDiagnosticStepKeyword(dialect);

        return $"{leadingWhitespace}# ERROR: {message}\n" +
               $"{leadingWhitespace}{stepKeyword}{ScenarioCallExpansionFailedPrefix} {message}\n";
    }

    private static string GetDiagnosticStepKeyword(GherkinDialect dialect)
    {
        return ScenarioCallParsing.GetDiagnosticStepKeyword(dialect);
    }

    private List<string> FindBackgroundSteps(string featureName)
    {
        var (steps, _) = FindBackgroundStepsWithDiagnostics(featureName);
        return steps;
    }

    private (List<string> steps, string diagnosticMessage) FindBackgroundStepsWithDiagnostics(string featureName)
    {
        var featureContent = FindFeatureFileContent(featureName);
        if (featureContent == null) 
        {
            return (null, $"Could not find feature file for \"{featureName}\". Ensure the feature file exists in the project or referenced projects.");
        }

        var dialect = GetDialect(featureContent);
        var lines = featureContent.Split('\n');
        var steps = new List<string>();
        var inBackground = false;
        var foundFeature = false;
        var collectingStepArgument = false;
        var inDocString = false;

        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();

            // Check if we're in the right feature
            if (StartsWithAnyKeyword(trimmedLine, dialect.FeatureKeywords))
            {
                var currentFeatureName = ExtractFeatureNameFromLine(trimmedLine, dialect.FeatureKeywords);
                foundFeature = string.Equals(currentFeatureName, featureName, StringComparison.OrdinalIgnoreCase);
                continue;
            }

            if (!foundFeature) continue;

            // Check for Background section
            if (StartsWithAnyKeyword(trimmedLine, dialect.BackgroundKeywords))
            {
                inBackground = true;
                collectingStepArgument = false;
                inDocString = false;
                continue;
            }

            // Stop if we hit a scenario or another major section
            if (inBackground && (StartsWithAnyKeyword(trimmedLine, dialect.ScenarioKeywords) ||
                                 StartsWithAnyKeyword(trimmedLine, dialect.FeatureKeywords)))
            {
                break;
            }

            if (inBackground)
            {
                // Check for doc string delimiters (""" or ```)
                if (trimmedLine.StartsWith("\"\"\"") || trimmedLine.StartsWith("```"))
                {
                    inDocString = !inDocString;
                    collectingStepArgument = true;
                    steps.Add(trimmedLine);
                    continue;
                }

                // If we're inside a doc string, collect all lines (trimmed)
                if (inDocString)
                {
                    steps.Add(trimmedLine);
                    continue;
                }

                // Check for datatable rows (lines starting with |)
                if (trimmedLine.StartsWith("|"))
                {
                    collectingStepArgument = true;
                    steps.Add("    " + trimmedLine);
                    continue;
                }

                // Check if this is a step line
                if (IsStepLine(trimmedLine, dialect))
                {
                    collectingStepArgument = false;
                    steps.Add(trimmedLine);
                    continue;
                }

                // If we were collecting step arguments and hit a non-table, non-doc-string line
                // that's also not a step, stop collecting arguments
                if (collectingStepArgument && !string.IsNullOrWhiteSpace(trimmedLine))
                {
                    collectingStepArgument = false;
                }
            }
        }

        return (steps.Any() ? steps : null, null);
    }

    private (List<string> steps, string diagnosticMessage) FindScenarioStepsWithDiagnostics(string scenarioName, string featureName)
    {
        var featureContent = FindFeatureFileContent(featureName);
        if (featureContent == null)
        {
            return (null, $"Could not find feature file for \"{featureName}\". Ensure the feature file exists in the project or referenced projects.");
        }

        var dialect = GetDialect(featureContent);
        var lines = featureContent.Split('\n');
        var steps = new List<string>();
        var inTargetScenario = false;
        var foundFeature = false;
        var featureFound = false;
        var collectingStepArgument = false;
        var inDocString = false;

        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();

            if (StartsWithAnyKeyword(trimmedLine, dialect.FeatureKeywords))
            {
                var currentFeatureName = ExtractFeatureNameFromLine(trimmedLine, dialect.FeatureKeywords);
                foundFeature = string.Equals(currentFeatureName, featureName, StringComparison.OrdinalIgnoreCase);
                if (foundFeature)
                {
                    featureFound = true;
                }
                continue;
            }

            if (!foundFeature) continue;

            if (StartsWithAnyKeyword(trimmedLine, dialect.ScenarioKeywords))
            {
                var currentScenarioName = ExtractScenarioNameFromLine(trimmedLine, dialect.ScenarioKeywords);
                
                // If we were in the target scenario and hit a new scenario, stop
                if (inTargetScenario)
                {
                    break;
                }
                
                inTargetScenario = string.Equals(currentScenarioName, scenarioName, StringComparison.OrdinalIgnoreCase);
                collectingStepArgument = false;
                inDocString = false;
                continue;
            }

            if (inTargetScenario && StartsWithAnyKeyword(trimmedLine, dialect.FeatureKeywords))
            {
                break;
            }

            if (inTargetScenario)
            {
                // Check for doc string delimiters (""" or ```)
                if (trimmedLine.StartsWith("\"\"\"") || trimmedLine.StartsWith("```"))
                {
                    inDocString = !inDocString;
                    collectingStepArgument = true;
                    steps.Add(trimmedLine);
                    continue;
                }

                // If we're inside a doc string, collect all lines (trimmed)
                if (inDocString)
                {
                    steps.Add(trimmedLine);
                    continue;
                }

                // Check for datatable rows (lines starting with |)
                // For datatables, we need to add extra indentation (4 spaces) to maintain Gherkin structure
                if (trimmedLine.StartsWith("|"))
                {
                    collectingStepArgument = true;
                    // Add 4 spaces for datatable indentation relative to steps
                    steps.Add("    " + trimmedLine);
                    continue;
                }

                // Check if this is a step line
                if (IsStepLine(trimmedLine, dialect))
                {
                    collectingStepArgument = false;
                    steps.Add(trimmedLine);
                    continue;
                }

                // If we were collecting step arguments and hit a non-table, non-doc-string line
                // that's also not a step, stop collecting arguments
                if (collectingStepArgument && !string.IsNullOrWhiteSpace(trimmedLine))
                {
                    collectingStepArgument = false;
                }
            }
        }

        if (!featureFound)
        {
            return (null, $"Feature \"{featureName}\" was not found in the feature file. Check feature name spelling and case.");
        }

        if (!steps.Any())
        {
            return (null, $"Scenario \"{scenarioName}\" was not found in feature \"{featureName}\". Check scenario name spelling and case.");
        }

        return (steps, null);
    }

    private List<string> FindScenarioSteps(string scenarioName, string featureName)
    {
        // Use the diagnostic method to get error messages if scenario is not found
        var (steps, diagnosticMessage) = FindScenarioStepsWithDiagnostics(scenarioName, featureName);
        
        // For backward compatibility, still return null, but the diagnostic message is now available
        // if needed in the caller
        return steps;
    }

    private string ExtractFeatureNameFromLine(string line, IEnumerable<string> featureKeywords)
    {
        return ScenarioCallParsing.ExtractFeatureNameFromLine(line, featureKeywords);
    }

    private string ExtractScenarioNameFromLine(string line, IEnumerable<string> scenarioKeywords)
    {
        return ScenarioCallParsing.ExtractScenarioNameFromLine(line, scenarioKeywords);
    }

    private bool IsStepLine(string line, GherkinDialect dialect)
    {
        return ScenarioCallParsing.IsStepLine(line, dialect);
    }

    private string FindFeatureFileContent(string featureName)
    {
        if (_featureFileCache.TryGetValue(featureName, out var cachedContent))
        {
            return cachedContent;
        }

        // Try project folder first, fall back to current directory
        var searchDirectories = new List<string>();
        if (_projectSettings?.ProjectFolder != null)
        {
            searchDirectories.Add(_projectSettings.ProjectFolder);
        }
        searchDirectories.Add(Environment.CurrentDirectory);

        foreach (var currentDirectory in searchDirectories.Distinct())
        {
            var featureFiles = GetFeatureFilePaths(currentDirectory);

            foreach (var featureFile in featureFiles)
            {
                try
                {
                    var content = File.ReadAllText(featureFile);
                    // Try with language-aware extraction
                    var dialect = GetDialect(content);
                    var extractedFeatureName = ExtractFeatureNameFromContent(content, dialect);
                        
                    if (extractedFeatureName != null)
                    {
                        _featureFileCache[extractedFeatureName] = content;
                        if (string.Equals(extractedFeatureName, featureName, StringComparison.OrdinalIgnoreCase))
                        {
                            return content;
                        }
                    }
                }
                catch
                {
                    // Continue with next file
                }
            }
        }

        return null;
    }

    private string ExtractFeatureNameFromContent(string content, GherkinDialect dialect)
    {
        var lines = content.Split('\n');
        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();
            if (StartsWithAnyKeyword(trimmedLine, dialect.FeatureKeywords))
            {
                return ExtractFeatureNameFromLine(trimmedLine, dialect.FeatureKeywords);
            }
        }
        return null;
    }

    private IEnumerable<string> GetFeatureFilePaths(string baseDirectory)
    {
        var featureFiles = new List<string>();

        var searchPaths = new[]
        {
            baseDirectory,
            Path.Combine(baseDirectory, "Features"),
            Path.Combine(baseDirectory, "Specs"),
            Path.Combine(baseDirectory, "Tests")
        };

        foreach (var searchPath in searchPaths)
        {
            if (Directory.Exists(searchPath))
            {
                featureFiles.AddRange(Directory.GetFiles(searchPath, "*.feature", SearchOption.AllDirectories));
            }
        }

        // Add feature files from referenced projects
        var referencedProjectPaths = GetReferencedProjectPaths(baseDirectory);
        foreach (var referencedProjectPath in referencedProjectPaths)
        {
            var referencedProjectDir = Path.GetDirectoryName(referencedProjectPath);
            if (!string.IsNullOrEmpty(referencedProjectDir))
            {
                var referencedSearchPaths = new[]
                {
                    referencedProjectDir,
                    Path.Combine(referencedProjectDir, "Features"),
                    Path.Combine(referencedProjectDir, "Specs"),
                    Path.Combine(referencedProjectDir, "Tests")
                };

                foreach (var searchPath in referencedSearchPaths)
                {
                    if (Directory.Exists(searchPath))
                    {
                        featureFiles.AddRange(Directory.GetFiles(searchPath, "*.feature", SearchOption.AllDirectories));
                    }
                }
            }
        }

        return featureFiles.Distinct();
    }

    private IEnumerable<string> GetReferencedProjectPaths(string baseDirectory)
    {
        var referencedProjects = new List<string>();

        try
        {
            var projectFiles = Directory.GetFiles(baseDirectory, "*.csproj", SearchOption.TopDirectoryOnly);
            
            foreach (var projectFile in projectFiles)
            {
                try
                {
                    var doc = XDocument.Load(projectFile);
                    var projectReferences = doc.Descendants("ProjectReference")
                        .Select(pr => pr.Attribute("Include")?.Value)
                        .Where(v => !string.IsNullOrEmpty(v))
                        .ToList();

                    foreach (var relativePath in projectReferences)
                    {
                        // Normalize path separators (handle both \ and /)
                        var normalizedPath = relativePath.Replace('\\', Path.DirectorySeparatorChar);
                        // Convert relative path to absolute path
                        var absolutePath = Path.GetFullPath(Path.Combine(baseDirectory, normalizedPath));
                        if (File.Exists(absolutePath))
                        {
                            referencedProjects.Add(absolutePath);
                        }
                    }
                }
                catch
                {
                    // Continue with next project file if this one fails
                }
            }
        }
        catch
        {
            // Return empty list if we can't read project references
        }

        return referencedProjects;
    }
}
