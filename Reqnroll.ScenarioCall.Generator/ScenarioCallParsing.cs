using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Gherkin;

namespace Reqnroll.ScenarioCall.Generator;

internal static class ScenarioCallParsing
{
    public static string DetectLanguage(string content)
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

    public static bool HasExplicitLanguageDirective(string content)
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
                    return true;
                }
            }

            if (!string.IsNullOrWhiteSpace(trimmed) && !trimmed.StartsWith("#"))
            {
                break;
            }
        }

        return false;
    }

    public static bool StartsWithAnyKeyword(string line, IEnumerable<string> keywords)
    {
        return TryGetMatchingKeyword(line, keywords) != null;
    }

    public static string TryGetMatchingKeyword(string line, IEnumerable<string> keywords)
    {
        foreach (var keyword in keywords.OrderByDescending(k => k.Length))
        {
            if (StartsWithKeyword(line, keyword))
            {
                return keyword;
            }
        }

        return null;
    }

    private static bool StartsWithKeyword(string line, string keyword)
    {
        if (string.IsNullOrEmpty(keyword) || !line.StartsWith(keyword))
        {
            return false;
        }

        if (char.IsWhiteSpace(keyword[keyword.Length - 1]) || keyword.EndsWith(":"))
        {
            return true;
        }

        return line.Length > keyword.Length && line[keyword.Length] == ':';
    }

    public static bool IsStepLine(string line, GherkinDialect dialect)
    {
        return StartsWithAnyKeyword(line, dialect.GivenStepKeywords) ||
               StartsWithAnyKeyword(line, dialect.WhenStepKeywords) ||
               StartsWithAnyKeyword(line, dialect.ThenStepKeywords) ||
               StartsWithAnyKeyword(line, dialect.AndStepKeywords) ||
               StartsWithAnyKeyword(line, dialect.ButStepKeywords);
    }

    public static string ExtractFeatureNameFromLine(string line, IEnumerable<string> featureKeywords)
    {
        return ExtractNameFromLine(line, featureKeywords);
    }

    public static string ExtractScenarioNameFromLine(string line, IEnumerable<string> scenarioKeywords)
    {
        return ExtractNameFromLine(line, scenarioKeywords);
    }

    private static string ExtractNameFromLine(string line, IEnumerable<string> keywords)
    {
        var keyword = TryGetMatchingKeyword(line, keywords);
        if (keyword != null)
        {
            return line.Substring(keyword.Length).Trim().TrimStart(':').Trim();
        }

        return null;
    }

    public static bool IsScenarioOutlineLine(string line)
    {
        return Regex.IsMatch(line, @"^\s*(Scenario Outline|Scenario Template|Voorbeeld|Abstract Scenario|Esquema del escenario|Szenariogrundriss|Plan du scénario|Plan du Scénario)\s*:", RegexOptions.IgnoreCase);
    }

    public static bool IsExamplesLine(string line)
    {
        return Regex.IsMatch(line, @"^\s*(Examples|Voorbeelden|Beispiele|Exemples|Ejemplos)\s*:", RegexOptions.IgnoreCase);
    }

    public static bool IsScenarioCallStep(string stepText, GherkinDialect dialect)
    {
        return TryParseScenarioCall(stepText, dialect) != null;
    }

    public static ScenarioCallMatch TryParseScenarioCall(string callStepLine, GherkinDialect dialect)
    {
        var keywordPattern = BuildStepKeywordPattern(dialect);
        var scenarioCallPhrases = GetScenarioCallPhrases(dialect.Language);

        foreach (var phrase in scenarioCallPhrases)
        {
            var patternWithBackground = BuildScenarioCallPattern(keywordPattern, phrase, includeBackground: true);
            var matchWithBackground = Regex.Match(callStepLine, patternWithBackground, RegexOptions.IgnoreCase);

            if (matchWithBackground.Success)
            {
                return new ScenarioCallMatch(
                    matchWithBackground.Groups[1].Value,
                    matchWithBackground.Groups[2].Value,
                    matchWithBackground.Groups[3].Value,
                    includeBackground: true);
            }

            var pattern = BuildScenarioCallPattern(keywordPattern, phrase, includeBackground: false);
            var match = Regex.Match(callStepLine, pattern, RegexOptions.IgnoreCase);

            if (match.Success)
            {
                return new ScenarioCallMatch(
                    match.Groups[1].Value,
                    match.Groups[2].Value,
                    match.Groups[3].Value,
                    includeBackground: false);
            }
        }

        return null;
    }

    public static string GetDiagnosticStepKeyword(GherkinDialect dialect)
    {
        var keyword = dialect.GivenStepKeywords
            .Where(k => k != "* ")
            .Select(k => k.Trim())
            .FirstOrDefault(k => !string.IsNullOrEmpty(k));

        return $"{keyword ?? "Given"} ";
    }

    private static string BuildStepKeywordPattern(GherkinDialect dialect)
    {
        var allStepKeywords = dialect.GivenStepKeywords
            .Concat(dialect.WhenStepKeywords)
            .Concat(dialect.ThenStepKeywords)
            .Concat(dialect.AndStepKeywords)
            .Concat(dialect.ButStepKeywords)
            .Where(k => k != "* ")
            .Select(k => k.Trim())
            .Distinct();

        return string.Join("|", allStepKeywords.Select(Regex.Escape));
    }

    private static string BuildScenarioCallPattern(string keywordPattern, ScenarioCallPhrase phrase, bool includeBackground)
    {
        var suffix = includeBackground ? $@"\s+{Regex.Escape(phrase.WithBackgroundPhrase)}" : string.Empty;
        return $@"^\s*({keywordPattern})\s+{Regex.Escape(phrase.CallPhrase)}\s+""([^""]+)""\s+{Regex.Escape(phrase.FromPhrase)}\s+""([^""]+)""{suffix}\s*$";
    }

    private static List<ScenarioCallPhrase> GetScenarioCallPhrases(string language)
    {
        var phrases = new List<ScenarioCallPhrase>();

        switch (language.ToLowerInvariant())
        {
            case "nl":
                phrases.Add(new ScenarioCallPhrase("ik roep scenario", "aan uit functionaliteit", "met achtergrond"));
                phrases.Add(new ScenarioCallPhrase("ik roep scenario", "aan van functionaliteit", "met achtergrond"));
                break;
            case "de":
                phrases.Add(new ScenarioCallPhrase("ich rufe Szenario", "auf aus Funktionalität", "mit Hintergrund"));
                phrases.Add(new ScenarioCallPhrase("ich rufe Szenario", "auf von Funktionalität", "mit Hintergrund"));
                break;
            case "fr":
                phrases.Add(new ScenarioCallPhrase("j'appelle le scénario", "de la fonctionnalité", "avec contexte"));
                break;
            case "es":
                phrases.Add(new ScenarioCallPhrase("llamo al escenario", "de la característica", "con antecedentes"));
                break;
        }

        phrases.Add(new ScenarioCallPhrase("I call scenario", "from feature", "with background"));

        return phrases;
    }

    private sealed class ScenarioCallPhrase
    {
        public ScenarioCallPhrase(string callPhrase, string fromPhrase, string withBackgroundPhrase)
        {
            CallPhrase = callPhrase;
            FromPhrase = fromPhrase;
            WithBackgroundPhrase = withBackgroundPhrase;
        }

        public string CallPhrase { get; }
        public string FromPhrase { get; }
        public string WithBackgroundPhrase { get; }
    }
}

internal sealed class ScenarioCallMatch
{
    public ScenarioCallMatch(string callKeyword, string scenarioName, string featureName, bool includeBackground)
    {
        CallKeyword = callKeyword;
        ScenarioName = scenarioName;
        FeatureName = featureName;
        IncludeBackground = includeBackground;
    }

    public string CallKeyword { get; }
    public string ScenarioName { get; }
    public string FeatureName { get; }
    public bool IncludeBackground { get; }
}
