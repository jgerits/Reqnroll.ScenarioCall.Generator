using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Reqnroll.Generator.UnitTestConverter;
using Reqnroll.Parser;
using Reqnroll.Bindings;

namespace Reqnroll.ScenarioCall.Generator;

public class ScenarioCallFeatureGenerator : IFeatureGenerator
{
    private readonly IFeatureGenerator _baseGenerator;
    private readonly Dictionary<string, string> _featureFileCache = new();
    private readonly LanguageHelper _languageHelper = new();

    public ScenarioCallFeatureGenerator(IFeatureGenerator baseGenerator, ReqnrollDocument document)
    {
        _baseGenerator = baseGenerator;
    }

    public string PreprocessFeatureContent(string originalContent)
    {
        var lines = originalContent.Split('\n');
        var result = new StringBuilder();
        var inScenario = false;
        var currentFeatureName = (from line in lines select line.Trim() into trimmedLine where trimmedLine.StartsWith("Feature:") select trimmedLine.Substring("Feature:".Length).Trim()).FirstOrDefault();
        
        // Extract language from feature content
        var language = _languageHelper.ExtractLanguage(originalContent);

        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();
                
            if (trimmedLine.StartsWith("Scenario:"))
            {
                inScenario = true;
                result.AppendLine(line);
                continue;
            }
                
            if (inScenario && _languageHelper.IsScenarioCallStep(trimmedLine, language))
            {
                var expandedSteps = ExpandScenarioCall(trimmedLine, currentFeatureName, language);
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
                
            // Add the original line
            result.AppendLine(line);
        }

        return result.ToString();
    }

    private bool IsScenarioCallStep(string stepText)
    {
        // Deprecated: Use LanguageHelper.IsScenarioCallStep instead
        return _languageHelper.IsScenarioCallStep(stepText, "en");
    }

    private string ExpandScenarioCall(string callStepLine, string currentFeatureName)
    {
        // Backward compatibility overload - defaults to English
        return ExpandScenarioCall(callStepLine, currentFeatureName, "en");
    }

    private string ExpandScenarioCall(string callStepLine, string currentFeatureName, string language = "en")
    {
        var scenarioCallInfo = _languageHelper.ExtractScenarioCall(callStepLine, language);
        if (!scenarioCallInfo.HasValue) 
            return null;

        var (scenarioName, featureName) = scenarioCallInfo.Value;
        var leadingWhitespace = callStepLine.Substring(0, callStepLine.Length - callStepLine.TrimStart().Length);

        try
        {
            var scenarioSteps = FindScenarioSteps(scenarioName, featureName);
            if (scenarioSteps != null && scenarioSteps.Any())
            {
                var result = new StringBuilder();
                result.AppendLine($"{leadingWhitespace}# Expanded from scenario call: \"{scenarioName}\" from feature \"{featureName}\"");
                    
                foreach (var step in scenarioSteps)
                {
                    result.AppendLine($"{leadingWhitespace}{step}");
                }
                    
                return result.ToString();
            }
        }
        catch (Exception ex)
        {
            return $"{leadingWhitespace}# Error expanding scenario call: {ex.Message}\n";
        }

        return null;
    }

    private List<string> FindScenarioSteps(string scenarioName, string featureName)
    {
        var featureContent = FindFeatureFileContent(featureName);
        if (featureContent == null) return null;

        var lines = featureContent.Split('\n');
        var steps = new List<string>();
        var inTargetScenario = false;
        var foundFeature = false;
        
        // Extract language from the feature file we're reading from
        var featureLanguage = _languageHelper.ExtractLanguage(featureContent);

        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();

            // Check if we're in the right feature
            if (trimmedLine.StartsWith("Feature:"))
            {
                var currentFeatureName = trimmedLine.Substring("Feature:".Length).Trim();
                foundFeature = string.Equals(currentFeatureName, featureName, StringComparison.OrdinalIgnoreCase);
                continue;
            }

            if (!foundFeature) continue;

            // Check for target scenario
            if (trimmedLine.StartsWith("Scenario:"))
            {
                var currentScenarioName = trimmedLine.Substring("Scenario:".Length).Trim();
                inTargetScenario = string.Equals(currentScenarioName, scenarioName, StringComparison.OrdinalIgnoreCase);
                continue;
            }

            // Stop if we hit another scenario or feature
            if (inTargetScenario && (trimmedLine.StartsWith("Scenario:") || trimmedLine.StartsWith("Feature:")))
            {
                break;
            }

            // Collect steps from target scenario using language-aware detection
            if (inTargetScenario && _languageHelper.IsStepLine(trimmedLine, featureLanguage))
            {
                steps.Add(trimmedLine);
            }
        }

        return steps.Any() ? steps : null;
    }

    private static bool IsStepLine(string line)
    {
        // Deprecated: Use LanguageHelper.IsStepLine instead
        // Kept for backward compatibility but updated to use constants
        return line.StartsWith($"{StepDefinitionKeyword.Given} ") || 
               line.StartsWith($"{StepDefinitionKeyword.When} ") || 
               line.StartsWith($"{StepDefinitionKeyword.Then} ") || 
               line.StartsWith($"{StepDefinitionKeyword.And} ") || 
               line.StartsWith($"{StepDefinitionKeyword.But} ");
    }

    private string FindFeatureFileContent(string featureName)
    {
        if (_featureFileCache.TryGetValue(featureName, out var cachedContent))
        {
            return cachedContent;
        }

        var currentDirectory = Environment.CurrentDirectory;
        var featureFiles = GetFeatureFilePaths(currentDirectory);

        foreach (var featureFile in featureFiles)
        {
            try
            {
                var content = File.ReadAllText(featureFile);
                var extractedFeatureName = ExtractFeatureNameFromContent(content);
                    
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

        return null;
    }

    private static string ExtractFeatureNameFromContent(string content)
    {
        var lines = content.Split('\n');
        return (from line in lines select line.Trim() into trimmedLine where trimmedLine.StartsWith("Feature:") 
            select trimmedLine.Substring("Feature:".Length).Trim()).FirstOrDefault();
    }

    private IEnumerable<string> GetFeatureFilePaths(string baseDirectory)
    {
        var featureFiles = new List<string>();

        // Common feature file locations
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

        return featureFiles.Distinct();
    }

    public UnitTestFeatureGenerationResult GenerateUnitTestFixture(ReqnrollDocument document, string testClassName,
        string targetNamespace)
    {
        return _baseGenerator.GenerateUnitTestFixture(document, testClassName, targetNamespace);
    }
}