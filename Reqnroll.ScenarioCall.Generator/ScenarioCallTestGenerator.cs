using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
        
        // Debug: Log that the generator is being used
        try
        {
            File.AppendAllText(Path.Combine(Path.GetTempPath(), "reqnroll_debug.log"),
                $"[{DateTime.Now:HH:mm:ss}] ScenarioCallTestGenerator created\n");
            File.AppendAllText(Path.Combine(Path.GetTempPath(), "reqnroll_debug.log"),
                $"[{DateTime.Now:HH:mm:ss}]   - Project folder: {projectSettings?.ProjectFolder}\n");
        }
        catch { }
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

    private string PreprocessFeatureContent(string originalContent)
    {
        // Debug: Log preprocessing
        try
        {
            File.AppendAllText(Path.Combine(Path.GetTempPath(), "reqnroll_debug.log"),
                $"[{DateTime.Now:HH:mm:ss}] PreprocessFeatureContent called\n");
        }
        catch { }
        
        var dialect = GetDialect(originalContent);
        var lines = originalContent.Split('\n');
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
                
            if (StartsWithAnyKeyword(trimmedLine, dialect.FeatureKeywords) || 
                StartsWithAnyKeyword(trimmedLine, dialect.BackgroundKeywords))
            {
                inScenario = false;
                result.AppendLine(line);
                continue;
            }
                
            if (inScenario && IsScenarioCallStep(trimmedLine, dialect))
            {
                // Debug: Log scenario call detection
                try
                {
                    File.AppendAllText(Path.Combine(Path.GetTempPath(), "reqnroll_debug.log"),
                        $"[{DateTime.Now:HH:mm:ss}]   - Found scenario call: {trimmedLine}\n");
                }
                catch { }
                
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

        var dialect = GetDialect(featureContent);
        var lines = featureContent.Split('\n');
        var steps = new List<string>();
        var inTargetScenario = false;
        var foundFeature = false;

        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();

            if (StartsWithAnyKeyword(trimmedLine, dialect.FeatureKeywords))
            {
                var currentFeatureName = ExtractFeatureNameFromLine(trimmedLine, dialect.FeatureKeywords);
                foundFeature = string.Equals(currentFeatureName, featureName, StringComparison.OrdinalIgnoreCase);
                continue;
            }

            if (!foundFeature) continue;

            if (StartsWithAnyKeyword(trimmedLine, dialect.ScenarioKeywords))
            {
                var currentScenarioName = ExtractScenarioNameFromLine(trimmedLine, dialect.ScenarioKeywords);
                inTargetScenario = string.Equals(currentScenarioName, scenarioName, StringComparison.OrdinalIgnoreCase);
                continue;
            }

            if (inTargetScenario && (StartsWithAnyKeyword(trimmedLine, dialect.ScenarioKeywords) || 
                                     StartsWithAnyKeyword(trimmedLine, dialect.FeatureKeywords)))
            {
                break;
            }

            if (inTargetScenario && IsStepLine(trimmedLine, dialect))
            {
                steps.Add(trimmedLine);
            }
        }

        return steps.Any() ? steps : null;
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
        // Debug: Write search attempt
        try
        {
            File.AppendAllText(Path.Combine(Path.GetTempPath(), "reqnroll_debug.log"),
                $"[{DateTime.Now:HH:mm:ss}] FindFeatureFileContent: '{featureName}'\n");
        }
        catch { }
        
        if (_featureFileCache.TryGetValue(featureName, out var cachedContent))
        {
            try
            {
                File.AppendAllText(Path.Combine(Path.GetTempPath(), "reqnroll_debug.log"),
                    $"[{DateTime.Now:HH:mm:ss}]   - Found in cache\n");
            }
            catch { }
            return cachedContent;
        }

        // Try project folder first, fall back to current directory
        var searchDirectories = new List<string>();
        if (_projectSettings?.ProjectFolder != null)
        {
            searchDirectories.Add(_projectSettings.ProjectFolder);
        }
        searchDirectories.Add(Environment.CurrentDirectory);
        
        // Debug: Write search directories
        try
        {
            File.AppendAllText(Path.Combine(Path.GetTempPath(), "reqnroll_debug.log"),
                $"[{DateTime.Now:HH:mm:ss}]   - Project folder: {_projectSettings?.ProjectFolder ?? "null"}\n");
            File.AppendAllText(Path.Combine(Path.GetTempPath(), "reqnroll_debug.log"),
                $"[{DateTime.Now:HH:mm:ss}]   - Current dir: {Environment.CurrentDirectory}\n");
        }
        catch { }

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
                            try
                            {
                                File.AppendAllText(Path.Combine(Path.GetTempPath(), "reqnroll_debug.log"),
                                    $"[{DateTime.Now:HH:mm:ss}]   - Found '{featureName}' in {featureFile}\n");
                            }
                            catch { }
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

        // Debug: Not found
        try
        {
            File.AppendAllText(Path.Combine(Path.GetTempPath(), "reqnroll_debug.log"),
                $"[{DateTime.Now:HH:mm:ss}]   - Feature '{featureName}' NOT FOUND\n");
        }
        catch { }

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

        // Debug: Write search info
        try
        {
            File.AppendAllText(Path.Combine(Path.GetTempPath(), "reqnroll_debug.log"),
                $"[{DateTime.Now:HH:mm:ss}] GetFeatureFilePaths base: {baseDirectory}\n");
        }
        catch { }

        foreach (var searchPath in searchPaths)
        {
            if (Directory.Exists(searchPath))
            {
                featureFiles.AddRange(Directory.GetFiles(searchPath, "*.feature", SearchOption.AllDirectories));
            }
        }

        // Add feature files from referenced projects
        var referencedProjectPaths = GetReferencedProjectPaths(baseDirectory);
        
        // Debug: Write referenced projects
        try
        {
            File.AppendAllText(Path.Combine(Path.GetTempPath(), "reqnroll_debug.log"),
                $"[{DateTime.Now:HH:mm:ss}] Found {referencedProjectPaths.Count()} project references\n");
            foreach (var proj in referencedProjectPaths)
            {
                File.AppendAllText(Path.Combine(Path.GetTempPath(), "reqnroll_debug.log"),
                    $"[{DateTime.Now:HH:mm:ss}]   - {proj}\n");
            }
        }
        catch { }
        
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

        // Debug: Write feature files found
        try
        {
            File.AppendAllText(Path.Combine(Path.GetTempPath(), "reqnroll_debug.log"),
                $"[{DateTime.Now:HH:mm:ss}] Found {featureFiles.Count} feature files total\n");
        }
        catch { }

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