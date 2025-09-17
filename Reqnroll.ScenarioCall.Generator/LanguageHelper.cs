using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Gherkin;

namespace Reqnroll.ScenarioCall.Generator
{
    /// <summary>
    /// Helper class for language-aware step keyword detection and scenario call pattern matching
    /// </summary>
    public class LanguageHelper
    {
        private readonly IGherkinDialectProvider _dialectProvider;
        private readonly Dictionary<string, GherkinDialect> _dialectCache = new();

        public LanguageHelper() : this(new GherkinDialectProvider("en"))
        {
        }

        public LanguageHelper(IGherkinDialectProvider dialectProvider)
        {
            _dialectProvider = dialectProvider ?? throw new ArgumentNullException(nameof(dialectProvider));
        }

        /// <summary>
        /// Extracts the language code from feature file content
        /// </summary>
        /// <param name="featureContent">The complete feature file content</param>
        /// <returns>Language code if found, otherwise "en" (English)</returns>
        public string ExtractLanguage(string featureContent)
        {
            if (string.IsNullOrEmpty(featureContent))
                return "en";

            var lines = featureContent.Split('\n');
            foreach (var line in lines.Take(10)) // Check first 10 lines for language directive
            {
                var trimmedLine = line.Trim();
                // Handle both "#language:" and "# language:" formats
                if (trimmedLine.StartsWith("#", StringComparison.OrdinalIgnoreCase))
                {
                    var withoutHash = trimmedLine.Substring(1).Trim();
                    if (withoutHash.StartsWith("language:", StringComparison.OrdinalIgnoreCase))
                    {
                        var language = withoutHash.Substring("language:".Length).Trim();
                        return string.IsNullOrEmpty(language) ? "en" : language;
                    }
                }

                // Stop looking if we hit a non-comment, non-empty line
                if (!string.IsNullOrWhiteSpace(trimmedLine) && !trimmedLine.StartsWith("#"))
                    break;
            }

            return "en"; // Default to English
        }

        /// <summary>
        /// Gets the Gherkin dialect for the specified language
        /// </summary>
        /// <param name="language">Language code (e.g., "en", "de", "fr")</param>
        /// <returns>GherkinDialect for the language</returns>
        public GherkinDialect GetDialect(string language)
        {
            if (_dialectCache.TryGetValue(language, out var cachedDialect))
                return cachedDialect;

            try
            {
                var dialect = _dialectProvider.GetDialect(language, null);
                _dialectCache[language] = dialect;
                return dialect;
            }
            catch
            {
                // Fallback to English if language not supported
                var fallbackDialect = _dialectProvider.GetDialect("en", null);
                _dialectCache[language] = fallbackDialect;
                return fallbackDialect;
            }
        }

        /// <summary>
        /// Checks if a line is a step line in the given language
        /// </summary>
        /// <param name="line">The line to check</param>
        /// <param name="language">Language code</param>
        /// <returns>True if the line is a step line</returns>
        public bool IsStepLine(string line, string language = "en")
        {
            if (string.IsNullOrWhiteSpace(line))
                return false;

            var trimmedLine = line.Trim();
            var dialect = GetDialect(language);

            return IsStepKeyword(trimmedLine, dialect.GivenStepKeywords) ||
                   IsStepKeyword(trimmedLine, dialect.WhenStepKeywords) ||
                   IsStepKeyword(trimmedLine, dialect.ThenStepKeywords) ||
                   IsStepKeyword(trimmedLine, dialect.AndStepKeywords) ||
                   IsStepKeyword(trimmedLine, dialect.ButStepKeywords);
        }

        /// <summary>
        /// Checks if a step is a scenario call step in the given language
        /// </summary>
        /// <param name="stepText">The step text to check</param>
        /// <param name="language">Language code</param>
        /// <returns>True if this is a scenario call step</returns>
        public bool IsScenarioCallStep(string stepText, string language = "en")
        {
            if (string.IsNullOrWhiteSpace(stepText))
                return false;

            var dialect = GetDialect(language);
            var allStepKeywords = new List<string>();
            allStepKeywords.AddRange(dialect.GivenStepKeywords);
            allStepKeywords.AddRange(dialect.WhenStepKeywords);
            allStepKeywords.AddRange(dialect.ThenStepKeywords);
            allStepKeywords.AddRange(dialect.AndStepKeywords);
            allStepKeywords.AddRange(dialect.ButStepKeywords);

            // Remove the generic "*" keyword to avoid overly broad matching
            allStepKeywords = allStepKeywords.Where(k => k != "*").Distinct().ToList();

            foreach (var keyword in allStepKeywords)
            {
                // Create pattern for each keyword: (keyword)\s+I call scenario "..." from feature "..."
                var escapedKeyword = Regex.Escape(keyword.Trim());
                var pattern = $@"^{escapedKeyword}\s+I call scenario\s+""([^""]+)""\s+from feature\s+""([^""]+)""";
                
                if (Regex.IsMatch(stepText.Trim(), pattern, RegexOptions.IgnoreCase))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Extracts scenario call information from a step
        /// </summary>
        /// <param name="stepText">The step text</param>
        /// <param name="language">Language code</param>
        /// <returns>Tuple with (scenarioName, featureName) or null if not a scenario call</returns>
        public (string scenarioName, string featureName)? ExtractScenarioCall(string stepText, string language = "en")
        {
            if (string.IsNullOrWhiteSpace(stepText) || !IsScenarioCallStep(stepText, language))
                return null;

            var dialect = GetDialect(language);
            var allStepKeywords = new List<string>();
            allStepKeywords.AddRange(dialect.GivenStepKeywords);
            allStepKeywords.AddRange(dialect.WhenStepKeywords);
            allStepKeywords.AddRange(dialect.ThenStepKeywords);
            allStepKeywords.AddRange(dialect.AndStepKeywords);
            allStepKeywords.AddRange(dialect.ButStepKeywords);

            // Remove the generic "*" keyword
            allStepKeywords = allStepKeywords.Where(k => k != "*").Distinct().ToList();

            foreach (var keyword in allStepKeywords)
            {
                var escapedKeyword = Regex.Escape(keyword.Trim());
                var pattern = $@"^{escapedKeyword}\s+I call scenario\s+""([^""]+)""\s+from feature\s+""([^""]+)""";
                var match = Regex.Match(stepText.Trim(), pattern, RegexOptions.IgnoreCase);
                
                if (match.Success)
                {
                    var scenarioName = match.Groups[1].Value;
                    var featureName = match.Groups[2].Value;
                    return (scenarioName, featureName);
                }
            }

            return null;
        }

        /// <summary>
        /// Gets all supported languages
        /// </summary>
        /// <returns>List of supported language codes</returns>
        public IEnumerable<string> GetSupportedLanguages()
        {
            // Return a subset of commonly used languages that Gherkin supports
            return new[]
            {
                "en", "en-US", "en-GB",       // English
                "de", "de-DE", "de-AT",       // German
                "fr", "fr-FR", "fr-CA",       // French
                "es", "es-ES", "es-MX",       // Spanish
                "it", "it-IT",                // Italian
                "pt", "pt-BR", "pt-PT",       // Portuguese
                "nl", "nl-NL", "nl-BE",       // Dutch
                "da", "da-DK",                // Danish
                "sv", "sv-SE",                // Swedish
                "no", "nb-NO",                // Norwegian
                "pl", "pl-PL",                // Polish
                "ru", "ru-RU",                // Russian
                "ja", "ja-JP",                // Japanese
                "zh", "zh-CN", "zh-TW",       // Chinese
                "ko", "ko-KR"                 // Korean
            };
        }

        private bool IsStepKeyword(string line, IEnumerable<string> keywords)
        {
            return keywords.Any(keyword => 
            {
                var trimmedKeyword = keyword.Trim();
                return line.StartsWith(trimmedKeyword + " ", StringComparison.OrdinalIgnoreCase) ||
                       line.Equals(trimmedKeyword, StringComparison.OrdinalIgnoreCase);
            });
        }
    }
}