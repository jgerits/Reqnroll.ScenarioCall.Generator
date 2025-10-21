using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Gherkin;
using Reqnroll;

namespace Reqnroll.ScenarioCall.Generator;

/// <summary>
/// Step definitions for scenario call syntax.
/// These validate that referenced scenarios and features exist, providing proper IDE feedback.
/// The actual scenario expansion happens at build time via the generator plugin.
/// </summary>
[Binding]
public class ScenarioCallStepDefinitions
{
    private readonly ScenarioCallValidator _validator = new ScenarioCallValidator();

    /// <summary>
    /// English: Given/When/Then/And/But I call scenario "ScenarioName" from feature "FeatureName"
    /// </summary>
    [Given(@"I call scenario ""([^""]+)"" from feature ""([^""]+)""")]
    [When(@"I call scenario ""([^""]+)"" from feature ""([^""]+)""")]
    [Then(@"I call scenario ""([^""]+)"" from feature ""([^""]+)""")]
    public void CallScenarioFromFeature(string scenarioName, string featureName)
    {
        _validator.ValidateScenarioCall(scenarioName, featureName);
    }

    /// <summary>
    /// Dutch: ik roep scenario "..." aan uit functionaliteit "..."
    /// </summary>
    [Given(@"ik roep scenario ""([^""]+)"" aan uit functionaliteit ""([^""]+)""")]
    [When(@"ik roep scenario ""([^""]+)"" aan uit functionaliteit ""([^""]+)""")]
    [Then(@"ik roep scenario ""([^""]+)"" aan uit functionaliteit ""([^""]+)""")]
    public void RoepScenarioAanUitFunctionaliteit(string scenarioNaam, string functionaliteitNaam)
    {
        _validator.ValidateScenarioCall(scenarioNaam, functionaliteitNaam);
    }

    /// <summary>
    /// Dutch: ik roep scenario "..." aan van functionaliteit "..."
    /// </summary>
    [Given(@"ik roep scenario ""([^""]+)"" aan van functionaliteit ""([^""]+)""")]
    [When(@"ik roep scenario ""([^""]+)"" aan van functionaliteit ""([^""]+)""")]
    [Then(@"ik roep scenario ""([^""]+)"" aan van functionaliteit ""([^""]+)""")]
    public void RoepScenarioAanVanFunctionaliteit(string scenarioNaam, string functionaliteitNaam)
    {
        _validator.ValidateScenarioCall(scenarioNaam, functionaliteitNaam);
    }

    /// <summary>
    /// German: ich rufe Szenario "..." auf aus Funktionalität "..."
    /// </summary>
    [Given(@"ich rufe Szenario ""([^""]+)"" auf aus Funktionalität ""([^""]+)""")]
    [When(@"ich rufe Szenario ""([^""]+)"" auf aus Funktionalität ""([^""]+)""")]
    [Then(@"ich rufe Szenario ""([^""]+)"" auf aus Funktionalität ""([^""]+)""")]
    public void RufeSzenarioAufAusFunktionalitaet(string szenarioName, string funktionalitaetName)
    {
        _validator.ValidateScenarioCall(szenarioName, funktionalitaetName);
    }

    /// <summary>
    /// German: ich rufe Szenario "..." auf von Funktionalität "..."
    /// </summary>
    [Given(@"ich rufe Szenario ""([^""]+)"" auf von Funktionalität ""([^""]+)""")]
    [When(@"ich rufe Szenario ""([^""]+)"" auf von Funktionalität ""([^""]+)""")]
    [Then(@"ich rufe Szenario ""([^""]+)"" auf von Funktionalität ""([^""]+)""")]
    public void RufeSzenarioAufVonFunktionalitaet(string szenarioName, string funktionalitaetName)
    {
        _validator.ValidateScenarioCall(szenarioName, funktionalitaetName);
    }

    /// <summary>
    /// French: j'appelle le scénario "..." de la fonctionnalité "..."
    /// </summary>
    [Given(@"j'appelle le scénario ""([^""]+)"" de la fonctionnalité ""([^""]+)""")]
    [When(@"j'appelle le scénario ""([^""]+)"" de la fonctionnalité ""([^""]+)""")]
    [Then(@"j'appelle le scénario ""([^""]+)"" de la fonctionnalité ""([^""]+)""")]
    public void AppelleScenarioDeLaFonctionnalite(string nomScenario, string nomFonctionnalite)
    {
        _validator.ValidateScenarioCall(nomScenario, nomFonctionnalite);
    }

    /// <summary>
    /// Spanish: llamo al escenario "..." de la característica "..."
    /// </summary>
    [Given(@"llamo al escenario ""([^""]+)"" de la característica ""([^""]+)""")]
    [When(@"llamo al escenario ""([^""]+)"" de la característica ""([^""]+)""")]
    [Then(@"llamo al escenario ""([^""]+)"" de la característica ""([^""]+)""")]
    public void LlamoAlEscenarioDeLaCaracteristica(string nombreEscenario, string nombreCaracteristica)
    {
        _validator.ValidateScenarioCall(nombreEscenario, nombreCaracteristica);
    }
}

/// <summary>
/// Helper class to validate that scenario calls reference existing scenarios and features.
/// </summary>
internal class ScenarioCallValidator
{
    private readonly Dictionary<string, string> _featureFileCache = new Dictionary<string, string>();
    private readonly Dictionary<string, GherkinDialect> _dialectCache = new Dictionary<string, GherkinDialect>();
    
    public void ValidateScenarioCall(string scenarioName, string featureName)
    {
        // Find the feature file
        var featureContent = FindFeatureFileContent(featureName);
        if (featureContent == null)
        {
            throw new InvalidOperationException($"Feature '{featureName}' not found. Please verify the feature name and ensure the feature file exists in your project.");
        }

        // Validate the scenario exists in the feature
        if (!ScenarioExistsInFeature(scenarioName, featureContent))
        {
            throw new InvalidOperationException($"Scenario '{scenarioName}' not found in feature '{featureName}'. Please verify the scenario name exists in the referenced feature file.");
        }
        
        // If we reach here, the scenario call is valid.
        // Note: This step binding should never actually execute at runtime for valid scenario calls
        // because they are expanded at build time by the generator plugin.
        // This binding exists primarily for IDE step definition discovery.
    }

    private bool ScenarioExistsInFeature(string scenarioName, string featureContent)
    {
        var dialect = GetDialect(featureContent);
        var lines = featureContent.Split('\n');

        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();
            if (StartsWithAnyKeyword(trimmedLine, dialect.ScenarioKeywords))
            {
                var currentScenarioName = ExtractScenarioNameFromLine(trimmedLine, dialect.ScenarioKeywords);
                if (string.Equals(currentScenarioName, scenarioName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
        }

        return false;
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
            if (!string.IsNullOrWhiteSpace(trimmed) && !trimmed.StartsWith("#"))
            {
                break;
            }
        }
        
        return "en";
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
                        var normalizedPath = relativePath.Replace('\\', Path.DirectorySeparatorChar);
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
