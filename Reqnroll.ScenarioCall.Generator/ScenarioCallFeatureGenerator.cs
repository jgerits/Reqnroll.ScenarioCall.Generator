using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Reqnroll.Generator.UnitTestConverter;
using Reqnroll.Parser;
using Reqnroll.Bindings;
using Gherkin;

namespace Reqnroll.ScenarioCall.Generator;

public class ScenarioCallFeatureGenerator : IFeatureGenerator
{
    private const string CircularReferenceErrorFormat = "# Error: Circular reference detected - scenario \"{0}\" from feature \"{1}\" is already in the call chain";
    private const string ScenarioCallMarkerPrefix = "scenario call:";
    private const string ScenarioCallExpansionFailedPrefix = "scenario call expansion failed:";
    
    private readonly IFeatureGenerator _baseGenerator;
    private readonly Dictionary<string, string> _featureFileCache = new();
    private readonly Dictionary<string, GherkinDialect> _dialectCache = new();
    
    // Common Gherkin language codes to try when language directive is missing
    private static readonly string[] CommonLanguages = { "en", "nl", "de", "fr", "es" };

    public ScenarioCallFeatureGenerator(IFeatureGenerator baseGenerator, ReqnrollDocument document)
    {
        _baseGenerator = baseGenerator;
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

    private string ValidateLanguageDirectives(string callingLanguage, string calledFeatureContent, string calledFeatureName)
    {
        var calledLanguage = ScenarioCallParsing.DetectLanguage(calledFeatureContent);
        var calledHasDirective = ScenarioCallParsing.HasExplicitLanguageDirective(calledFeatureContent);
        
        // If calling feature is non-English but called feature has no directive
        if (callingLanguage != "en" && !calledHasDirective)
        {
            return $"Language directive missing in called feature '{calledFeatureName}'. Add '# language: {callingLanguage}' at the top of the feature file.";
        }
        
        // If languages are different (and both are explicit or calling is non-English)
        if (callingLanguage != calledLanguage && callingLanguage != "en")
        {
            return $"Language mismatch: calling feature uses '{callingLanguage}' but called feature '{calledFeatureName}' uses '{calledLanguage}'. Both feature files should use the same language directive.";
        }
        
        return null;
    }

    private bool StartsWithAnyKeyword(string line, IEnumerable<string> keywords)
    {
        return ScenarioCallParsing.StartsWithAnyKeyword(line, keywords);
    }

    public string PreprocessFeatureContent(string originalContent)
    {
        var dialect = GetDialect(originalContent);
        var lines = originalContent.Split('\n');

        // Fast path: if there are no scenario call steps within Scenario or Background blocks,
        // return the original content unchanged and just add a trailing newline.
        // This preserves the original line endings used in the content.
        var hasScenarioCall = false;
        var scanInScenario = false;
        var scanInBackground = false;
        foreach (var l in lines)
        {
            var t = l.Trim();
            if (ScenarioCallParsing.IsScenarioOutlineLine(t) || ScenarioCallParsing.IsExamplesLine(t))
            {
                scanInScenario = false;
                scanInBackground = false;
            }
            else if (StartsWithAnyKeyword(t, dialect.BackgroundKeywords))
            {
                scanInScenario = false;
                scanInBackground = true;
            }
            else if (StartsWithAnyKeyword(t, dialect.ScenarioKeywords))
            {
                scanInScenario = true;
                scanInBackground = false;
            }
            else if ((scanInScenario || scanInBackground) && IsScenarioCallStep(t, dialect))
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
        var inBackground = false;
        var currentScenarioName = "";
        var currentFeatureName = ExtractFeatureNameFromContent(originalContent, dialect);
        var callStack = new HashSet<string>();

        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();

            if (ScenarioCallParsing.IsScenarioOutlineLine(trimmedLine) || ScenarioCallParsing.IsExamplesLine(trimmedLine))
            {
                inScenario = false;
                inBackground = false;
                result.AppendLine(line);
                continue;
            }
                
            if (StartsWithAnyKeyword(trimmedLine, dialect.ScenarioKeywords))
            {
                inScenario = true;
                inBackground = false;
                currentScenarioName = ExtractScenarioNameFromLine(trimmedLine, dialect.ScenarioKeywords);
                // Clear call stack for each new scenario - each scenario is processed independently
                // to prevent recursion only within its own expansion, not across unrelated scenarios
                callStack.Clear();
                if (!string.IsNullOrEmpty(currentScenarioName) && !string.IsNullOrEmpty(currentFeatureName))
                {
                    callStack.Add(CreateCallStackKey(currentFeatureName, currentScenarioName));
                }
                result.AppendLine(line);
                continue;
            }
            
            if (StartsWithAnyKeyword(trimmedLine, dialect.BackgroundKeywords))
            {
                inBackground = true;
                inScenario = false;
                result.AppendLine(line);
                continue;
            }
                
            if ((inScenario || inBackground) && IsScenarioCallStep(trimmedLine, dialect))
            {
                var expandedSteps = ExpandScenarioCall(trimmedLine, currentFeatureName, dialect, originalContent, callStack);
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
        return ScenarioCallParsing.IsScenarioCallStep(stepText, dialect);
    }

    private string ExpandScenarioCall(string callStepLine, string currentFeatureName, GherkinDialect dialect, string currentFeatureContent, HashSet<string> callStack)
    {
        var scenarioCall = ScenarioCallParsing.TryParseScenarioCall(callStepLine, dialect);
        if (scenarioCall == null) return null;
        
        var leadingWhitespace = callStepLine.Substring(0, callStepLine.Length - callStepLine.TrimStart().Length);

        // Check for recursion - prevents direct self-reference
        // Note: Since nested scenario calls are not recursively expanded (documented limitation),
        // we don't need to add the called scenario to the stack or check for indirect circular references
        var callKey = CreateCallStackKey(scenarioCall.FeatureName, scenarioCall.ScenarioName);
        if (callStack.Contains(callKey))
        {
            return $"{leadingWhitespace}{string.Format(CircularReferenceErrorFormat, scenarioCall.ScenarioName, scenarioCall.FeatureName)}\n";
        }

        try
        {
            // Get feature content for validation
            string featureContent;
            if (string.Equals(scenarioCall.FeatureName, currentFeatureName, StringComparison.OrdinalIgnoreCase))
            {
                // Use the current feature content for same-feature calls
                featureContent = currentFeatureContent;
            }
            else
            {
                // Look up the feature file content for cross-feature calls
                featureContent = FindFeatureFileContent(scenarioCall.FeatureName);
            }
            
            // Validate language directives before expanding
            string languageValidationWarning = null;
            if (featureContent != null)
            {
                languageValidationWarning = ValidateLanguageDirectives(dialect.Language, featureContent, scenarioCall.FeatureName);
            }
            
            var backgroundSteps = scenarioCall.IncludeBackground ? FindBackgroundSteps(scenarioCall.FeatureName, currentFeatureName, currentFeatureContent) : null;
            var (scenarioSteps, diagnosticMessage) = FindScenarioStepsWithDiagnostics(scenarioCall.ScenarioName, scenarioCall.FeatureName, currentFeatureName, currentFeatureContent);
            
            if (scenarioSteps != null && scenarioSteps.Any())
            {
                var result = new StringBuilder();
                
                // Add language validation warning if present
                if (!string.IsNullOrEmpty(languageValidationWarning))
                {
                    result.AppendLine($"{leadingWhitespace}# WARNING: {languageValidationWarning}");
                }
                
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
            else if (!string.IsNullOrEmpty(languageValidationWarning))
            {
                // If scenario steps couldn't be found but there's a language validation warning,
                // it's likely due to the language mismatch. Return the warning.
                return $"{leadingWhitespace}# WARNING: {languageValidationWarning}\n" +
                       $"{leadingWhitespace}# Warning: Could not expand scenario call (likely due to language directive issue)\n" +
                       FormatDiagnosticScenarioCallFailure(leadingWhitespace, dialect, $"Could not expand scenario call (likely due to language directive issue). {languageValidationWarning}", includeComment: false);
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

    private static string FormatDiagnosticScenarioCallFailure(string leadingWhitespace, GherkinDialect dialect, string message, bool includeComment = true)
    {
        var stepKeyword = GetDiagnosticStepKeyword(dialect);

        var result = new StringBuilder();
        if (includeComment)
        {
            result.AppendLine($"{leadingWhitespace}# ERROR: {message}");
        }

        result.AppendLine($"{leadingWhitespace}{stepKeyword}{ScenarioCallExpansionFailedPrefix} {message}");
        return result.ToString();
    }

    private static string GetDiagnosticStepKeyword(GherkinDialect dialect)
    {
        return ScenarioCallParsing.GetDiagnosticStepKeyword(dialect);
    }

    private List<string> FindBackgroundSteps(string featureName, string currentFeatureName, string currentFeatureContent)
    {
        // Check if we're calling a scenario from the same feature
        string featureContent;
        if (string.Equals(featureName, currentFeatureName, StringComparison.OrdinalIgnoreCase))
        {
            // Use the current feature content for same-feature calls
            featureContent = currentFeatureContent;
        }
        else
        {
            // Look up the feature file content for cross-feature calls
            featureContent = FindFeatureFileContent(featureName);
        }
        
        if (featureContent == null) return null;

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
                var currentFeatureNameInFile = ExtractFeatureNameFromLine(trimmedLine, dialect.FeatureKeywords);
                foundFeature = string.Equals(currentFeatureNameInFile, featureName, StringComparison.OrdinalIgnoreCase);
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

        return steps.Any() ? steps : null;
    }

    private (List<string> steps, string diagnosticMessage) FindScenarioStepsWithDiagnostics(string scenarioName, string featureName, string currentFeatureName, string currentFeatureContent)
    {
        // Check if we're calling a scenario from the same feature
        string featureContent;
        if (string.Equals(featureName, currentFeatureName, StringComparison.OrdinalIgnoreCase))
        {
            // Use the current feature content for same-feature calls
            featureContent = currentFeatureContent;
        }
        else
        {
            // Look up the feature file content for cross-feature calls
            featureContent = FindFeatureFileContent(featureName);
        }
        
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

            // Check if we're in the right feature
            if (StartsWithAnyKeyword(trimmedLine, dialect.FeatureKeywords))
            {
                var currentFeatureNameInFile = ExtractFeatureNameFromLine(trimmedLine, dialect.FeatureKeywords);
                foundFeature = string.Equals(currentFeatureNameInFile, featureName, StringComparison.OrdinalIgnoreCase);
                if (foundFeature)
                {
                    featureFound = true;
                }
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
                
                // If feature name extraction failed with detected dialect, try common languages
                if (extractedFeatureName == null)
                {
                    // Try common language dialects when language directive is missing
                    foreach (var lang in CommonLanguages)
                    {
                        var dialectProvider = new GherkinDialectProvider(lang);
                        var testDialect = dialectProvider.DefaultDialect;
                        extractedFeatureName = ExtractFeatureNameFromContent(content, testDialect);
                        if (extractedFeatureName != null)
                        {
                            break;
                        }
                    }
                }
                    
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

    private static string CreateCallStackKey(string featureName, string scenarioName)
    {
        return $"{featureName}:{scenarioName}";
    }

    // Backward-compatible wrapper methods for testing (default to English dialect)
    // Note: These are private methods used only by unit tests and should not be used in production
    private static string DetectLanguage(string content)
    {
        return ScenarioCallParsing.DetectLanguage(content);
    }

    private static bool HasExplicitLanguageDirective(string content)
    {
        return ScenarioCallParsing.HasExplicitLanguageDirective(content);
    }

    private static bool IsStepLine(string line)
    {
        var dialectProvider = new GherkinDialectProvider("en");
        var dialect = dialectProvider.DefaultDialect;

        return ScenarioCallParsing.IsStepLine(line, dialect);
    }

    private bool IsScenarioCallStep(string stepText)
    {
        var dialect = new GherkinDialectProvider("en").DefaultDialect;
        return IsScenarioCallStep(stepText, dialect);
    }

    private string ExpandScenarioCall(string callStepLine, string currentFeatureName)
    {
        var dialect = new GherkinDialectProvider("en").DefaultDialect;
        // Create a fresh call stack for each test invocation
        // Note: Same-feature calls are not supported via this wrapper (empty current feature content),
        // so the call stack provides limited recursion protection. This is acceptable for unit tests
        // since they test individual method behavior in isolation.
        var callStack = new HashSet<string>();
        // For backward compatibility, pass empty string as current feature content
        // This will force lookup from file system
        return ExpandScenarioCall(callStepLine, currentFeatureName, dialect, "", callStack);
    }

    private List<string> FindScenarioSteps(string scenarioName, string featureName, string currentFeatureName, string currentFeatureContent)
    {
        return FindScenarioStepsWithDiagnostics(scenarioName, featureName, currentFeatureName, currentFeatureContent).steps;
    }

    private List<string> FindScenarioSteps(string scenarioName, string featureName)
    {
        // For backward compatibility, pass null for current feature name and empty for content
        // This will force lookup from file system
        return FindScenarioStepsWithDiagnostics(scenarioName, featureName, null, "").steps;
    }

    public UnitTestFeatureGenerationResult GenerateUnitTestFixture(ReqnrollDocument document, string testClassName,
        string targetNamespace)
    {
        return _baseGenerator.GenerateUnitTestFixture(document, testClassName, targetNamespace);
    }
}
