using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Gherkin;
using Reqnroll.ScenarioCall.Generator;

// Create a simple test
var dialectProvider = new GherkinDialectProvider();
var helper = new LanguageHelper(dialectProvider);

Console.WriteLine("Testing German keywords with English phrases...");

var stepText1 = "Gegeben sei I call scenario \"Setup\" from feature \"Common\"";
var stepText2 = "Und I call scenario \"Login\" from feature \"Auth\"";

Console.WriteLine($"\nStep 1: {stepText1}");
Console.WriteLine($"IsScenarioCallStep(de-DE): {helper.IsScenarioCallStep(stepText1, "de-DE")}");
var result1 = helper.ExtractScenarioCall(stepText1, "de-DE");
Console.WriteLine($"ExtractScenarioCall(de-DE): {result1}");

Console.WriteLine($"\nStep 2: {stepText2}");
Console.WriteLine($"IsScenarioCallStep(de-DE): {helper.IsScenarioCallStep(stepText2, "de-DE")}");
var result2 = helper.ExtractScenarioCall(stepText2, "de-DE");
Console.WriteLine($"ExtractScenarioCall(de-DE): {result2}");

// Let's check the German dialect specifically
var germanDialect = dialectProvider.GetDialect("de", null);
Console.WriteLine($"\nGerman Given keywords: {string.Join(", ", germanDialect.GivenStepKeywords)}");
Console.WriteLine($"German And keywords: {string.Join(", ", germanDialect.AndStepKeywords)}");

// Test a basic regex pattern that should match
var testKeyword = "Gegeben sei";
var pattern = $@"^{Regex.Escape(testKeyword)}\s+I call scenario\s+""([^""]+)""\s+from feature\s+""([^""]+)""";
Console.WriteLine($"\nTesting pattern: {pattern}");
Console.WriteLine($"Against text: {stepText1}");
var match = Regex.Match(stepText1.Trim(), pattern, RegexOptions.IgnoreCase);
Console.WriteLine($"Match success: {match.Success}");
if (match.Success)
{
    Console.WriteLine($"Groups: {match.Groups[1].Value}, {match.Groups[2].Value}");
}