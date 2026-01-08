using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Reqnroll.Generator.UnitTestConverter;
using Reqnroll.Parser;
using Reqnroll.Bindings;
using Gherkin;

namespace Reqnroll.ScenarioCall.Generator;

public class ScenarioCallFeatureGenerator : IFeatureGenerator
{
    private readonly IFeatureGenerator _baseGenerator;
    private readonly Dictionary<string, string> _featureFileCache = new();
    private readonly Dictionary<string, GherkinDialect> _dialectCache = new();

    public ScenarioCallFeatureGenerator(IFeatureGenerator baseGenerator, ReqnrollDocument document)
    {
        _baseGenerator = baseGenerator;
    }

    private GherkinDialect GetDialect(string content)
    {
        var language = DetectLanguage(content);
        
        if (!_dialectCache.TryGetValue(language, out var dialect))
        {
            var dialectProvider = new GherkinDialectProvider(language);
            dialect = dialectProvider.DefaultDialect;
            _dialectCache[language] = dialect;
        }
        
        return dialect;
    }

    private static string DetectLanguage(string content)
    {
        // Check for # language: directive in the first few lines
        var lines = content.Split('\n');
        foreach (var line in lines.Take(10))
        {
            var trimmed = line.Trim();
            if (trimmed.StartsWith("#") && trimmed.Contains("language:"))
            {
                var match = Regex.Match(trimmed, @"#\s*language:\s*([a-z]{2}(-[A-Z]{2})?)", RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    return match.Groups[1].Value;
                }
            }
            // Stop at first non-comment, non-blank line
            if (!string.IsNullOrWhiteSpace(trimmed) && !trimmed.StartsWith("#"))
            {
                break;
            }
        }
        
        return "en"; // Default to English
    }

    private static List<(string callPhrase, string fromPhrase)> GetScenarioCallPhrases(string language)
    {
        // Return language-specific phrases for scenario calls
        var phrases = new List<(string, string)>();
        
        switch (language.ToLowerInvariant())
        {
            case "nl": // Dutch
                phrases.Add(("ik roep scenario", "aan uit functionaliteit"));
                phrases.Add(("ik roep scenario", "aan van functionaliteit"));
                break;
            case "de": // German
                phrases.Add(("ich rufe Szenario", "auf aus Funktionalität"));
                phrases.Add(("ich rufe Szenario", "auf von Funktionalität"));
                break;
            case "fr": // French
                phrases.Add(("j'appelle le scénario", "de la fonctionnalité"));
                break;
            case "es": // Spanish
                phrases.Add(("llamo al escenario", "de la característica"));
                break;
        }
        
        // Always include English as fallback
        phrases.Add(("I call scenario", "from feature"));
        
        return phrases;
    }

    private bool StartsWithAnyKeyword(string line, IEnumerable<string> keywords)
    {
        foreach (var keyword in keywords)
        {
            if (line.StartsWith(keyword))
            {
                return true;
            }
        }
        return false;
    }

    public string PreprocessFeatureContent(string originalContent)
    {
        var dialect = GetDialect(originalContent);
        var lines = originalContent.Split('\n');

        // Fast path: if there are no scenario-call steps within Scenario blocks,
        // return the original content unchanged and just add a trailing newline.
        // This preserves the original line endings used in the content.
        var hasScenarioCall = false;
        var scanInScenario = false;
        foreach (var l in lines)
        {
            var t = l.Trim();
            if (StartsWithAnyKeyword(t, dialect.ScenarioKeywords))
            {
                scanInScenario = true;
            }
            else if (scanInScenario && IsScenarioCallStep(t, dialect))
            {
                hasScenarioCall = true;
                break;
            }
        }

        if (!hasScenarioCall)
        {
            return originalContent + Environment.NewLine;
        }

        var result = new StringBuilder();
        var inScenario = false;
        var currentFeatureName = ExtractFeatureNameFromContent(originalContent, dialect);

        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();
                
            if (StartsWithAnyKeyword(trimmedLine, dialect.ScenarioKeywords))
            {
                inScenario = true;
                result.AppendLine(line);
                continue;
            }
                
            if (inScenario && IsScenarioCallStep(trimmedLine, dialect))
            {
                var expandedSteps = ExpandScenarioCall(trimmedLine, currentFeatureName, dialect);
                if (expandedSteps != null)
                {
                    result.Append(expandedSteps);
                    // Don't add the original line if expansion was successful or returned an error message
                    continue; 
                }
                else
                {
                    // This should not happen anymore since ExpandScenarioCall now returns diagnostics
                    // but keep as a fallback for unexpected scenarios
                    var leadingWhitespace = line.Substring(0, line.Length - line.TrimStart().Length);
                    result.AppendLine($"{leadingWhitespace}# ERROR: Could not expand scenario call - unknown reason");
                    // Don't add the original line to avoid undefined step
                    continue;
                }
            }
                
            // Add the original line
            result.AppendLine(line);
        }

        return result.ToString();
    }

    private bool IsScenarioCallStep(string stepText, GherkinDialect dialect)
    {
        // Build a pattern that matches any step keyword in the current dialect
        var allStepKeywords = dialect.GivenStepKeywords
            .Concat(dialect.WhenStepKeywords)
            .Concat(dialect.ThenStepKeywords)
            .Concat(dialect.AndStepKeywords)
            .Concat(dialect.ButStepKeywords)
            .Where(k => k != "* ")
            .Select(k => k.Trim())
            .Distinct();
        
        var keywordPattern = string.Join("|", allStepKeywords.Select(Regex.Escape));
        
        // Get language-specific phrases for scenario calls
        var scenarioCallPhrases = GetScenarioCallPhrases(dialect.Language);
        var patterns = new List<string>();
        
        foreach (var (callPhrase, fromPhrase) in scenarioCallPhrases)
        {
            var pattern = $@"({keywordPattern})\s+{Regex.Escape(callPhrase)}\s+""([^""]+)""\s+{Regex.Escape(fromPhrase)}\s+""([^""]+)""";
            patterns.Add(pattern);
        }
        
        foreach (var pattern in patterns)
        {
            if (Regex.IsMatch(stepText, pattern, RegexOptions.IgnoreCase))
            {
                return true;
            }
        }
        
        return false;
    }

    private string ExpandScenarioCall(string callStepLine, string currentFeatureName, GherkinDialect dialect)
    {
        // Build a pattern that matches any step keyword in the current dialect
        var allStepKeywords = dialect.GivenStepKeywords
            .Concat(dialect.WhenStepKeywords)
            .Concat(dialect.ThenStepKeywords)
            .Concat(dialect.AndStepKeywords)
            .Concat(dialect.ButStepKeywords)
            .Where(k => k != "* ")
            .Select(k => k.Trim())
            .Distinct();
        
        var keywordPattern = string.Join("|", allStepKeywords.Select(Regex.Escape));
        
        // Try all language-specific patterns
        var scenarioCallPhrases = GetScenarioCallPhrases(dialect.Language);
        string scenarioName = null;
        string featureName = null;
        
        foreach (var (callPhrase, fromPhrase) in scenarioCallPhrases)
        {
            var pattern = $@"({keywordPattern})\s+{Regex.Escape(callPhrase)}\s+""([^""]+)""\s+{Regex.Escape(fromPhrase)}\s+""([^""]+)""";
            var match = Regex.Match(callStepLine, pattern, RegexOptions.IgnoreCase);
            
            if (match.Success)
            {
                scenarioName = match.Groups[2].Value;
                featureName = match.Groups[3].Value;
                break;
            }
        }
        
        if (scenarioName == null || featureName == null) return null;
        
        var leadingWhitespace = callStepLine.Substring(0, callStepLine.Length - callStepLine.TrimStart().Length);

        try
        {
            var (scenarioSteps, diagnosticMessage) = FindScenarioStepsWithDiagnostics(scenarioName, featureName);
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
            else if (!string.IsNullOrEmpty(diagnosticMessage))
            {
                // Return diagnostic message instead of null to provide clear feedback
                return $"{leadingWhitespace}# ERROR: {diagnosticMessage}\n";
            }
        }
        catch (Exception ex)
        {
            return $"{leadingWhitespace}# ERROR: Exception during scenario call expansion - {ex.Message}\n";
        }

        return null;
    }

    private List<string> FindScenarioSteps(string scenarioName, string featureName)
    {
        var featureContent = FindFeatureFileContent(featureName);
        if (featureContent == null) return null;

        var dialect = GetDialect(featureContent);
        var lines = featureContent.Split('\n');
        var steps = new List<string>();
        var inTargetScenario = false;
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

            // Check for target scenario
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

            // Stop if we hit a feature keyword while in target scenario
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

        return steps.Any() ? steps : null;
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
        var featureFound = false;
        var collectingStepArgument = false;
        var inDocString = false;

        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();

            // Check if we're in the right feature
            if (StartsWithAnyKeyword(trimmedLine, dialect.FeatureKeywords))
            {
                var currentFeatureName = ExtractFeatureNameFromLine(trimmedLine, dialect.FeatureKeywords);
                featureFound = string.Equals(currentFeatureName, featureName, StringComparison.OrdinalIgnoreCase);
                continue;
            }

            if (!featureFound) continue;

            // Check for target scenario
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

            // Stop if we hit a feature keyword while in target scenario
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

    private string ExtractFeatureNameFromLine(string line, IEnumerable<string> featureKeywords)
    {
        foreach (var keyword in featureKeywords)
        {
            if (line.StartsWith(keyword))
            {
                return line.Substring(keyword.Length).Trim().TrimStart(':').Trim();
            }
        }
        return null;
    }

    private string ExtractScenarioNameFromLine(string line, IEnumerable<string> scenarioKeywords)
    {
        foreach (var keyword in scenarioKeywords)
        {
            if (line.StartsWith(keyword))
            {
                return line.Substring(keyword.Length).Trim().TrimStart(':').Trim();
            }
        }
        return null;
    }

    private bool IsStepLine(string line, GherkinDialect dialect)
    {
        return StartsWithAnyKeyword(line, dialect.GivenStepKeywords) ||
               StartsWithAnyKeyword(line, dialect.WhenStepKeywords) ||
               StartsWithAnyKeyword(line, dialect.ThenStepKeywords) ||
               StartsWithAnyKeyword(line, dialect.AndStepKeywords) ||
               StartsWithAnyKeyword(line, dialect.ButStepKeywords);
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

    // Backward-compatible wrapper methods for testing (default to English dialect)
    private static bool IsStepLine(string line)
    {
        var dialectProvider = new GherkinDialectProvider("en");
        var dialect = dialectProvider.DefaultDialect;
        
        return line.StartsWith("Given ") || 
               line.StartsWith("When ") || 
               line.StartsWith("Then ") || 
               line.StartsWith("And ") || 
               line.StartsWith("But ");
    }

    private bool IsScenarioCallStep(string stepText)
    {
        var dialect = new GherkinDialectProvider("en").DefaultDialect;
        return IsScenarioCallStep(stepText, dialect);
    }

    private string ExpandScenarioCall(string callStepLine, string currentFeatureName)
    {
        var dialect = new GherkinDialectProvider("en").DefaultDialect;
        return ExpandScenarioCall(callStepLine, currentFeatureName, dialect);
    }

    public UnitTestFeatureGenerationResult GenerateUnitTestFixture(ReqnrollDocument document, string testClassName,
        string targetNamespace)
    {
        return _baseGenerator.GenerateUnitTestFixture(document, testClassName, targetNamespace);
    }
}