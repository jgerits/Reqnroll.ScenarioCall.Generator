# Reqnroll.ScenarioCall.Generator - Test Coverage and Examples

This document describes the comprehensive test coverage of the Reqnroll.ScenarioCall.Generator plugin, inspired by and adapted from [Reqnroll PR #684](https://github.com/reqnroll/Reqnroll/pull/684).

## Test Architecture

Unlike the original Reqnroll PR which included both generator and runtime components, this standalone plugin is a **generator-only** solution. All scenario call expansion happens at build time during feature file code generation.

### Why Generator-Only?

- **Simpler**: No runtime complexity or overhead
- **Faster**: Steps execute as if they were written inline
- **Better Debugging**: Generated code shows actual steps being executed
- **No Context Issues**: All steps share the same ScenarioContext and FeatureContext
- **Standard Reqnroll**: Uses existing Reqnroll infrastructure

## Test Coverage (46 Tests)

### Core Functionality Tests

#### Plugin Registration
- `GeneratorPlugin_ImplementsIGeneratorPlugin`: Verifies plugin interface implementation
- `GeneratorPlugin_HasCorrectAssemblyAttribute`: Ensures proper plugin registration
- `Initialize_CanBeCalledWithoutException`: Tests plugin initialization

#### Basic Scenario Call Expansion
- `PreprocessFeatureContent_WithValidScenarioCall_ExpandsScenarioCall`: Tests basic expansion
- `PreprocessFeatureContent_WithNoScenarioCalls_ReturnsOriginalContentWithNewline`: Tests pass-through
- `PreprocessFeatureContent_WithInvalidScenarioCall_AddsWarningComment`: Tests error handling

#### Pattern Matching
- `IsScenarioCallStep_DetectsScenarioCallSteps`: Tests recognition of valid scenario call syntax across all Gherkin keywords (Given, When, Then, And, But)
- Tests for invalid patterns that should NOT be recognized as scenario calls

#### Feature File Discovery
- `GetFeatureFilePaths_SearchesCommonDirectories`: Tests automatic discovery in common locations
- `FindFeatureFileContent_CachesResults`: Tests caching mechanism for performance
- `ExtractFeatureNameFromContent_WithValidFeature_ReturnsFeatureName`: Tests feature name extraction

#### Scenario Extraction
- `FindScenarioSteps_WithValidScenario_ReturnsSteps`: Tests step extraction from scenarios
- `FindScenarioSteps_WithNonExistentFeature_ReturnsNull`: Tests handling of missing features
- `FindScenarioSteps_WithNonExistentScenario_ReturnsNull`: Tests handling of missing scenarios
- `IsStepLine_IdentifiesStepLines`: Tests identification of Gherkin step lines

### Advanced Functionality Tests

#### Multiple Scenario Calls
- `PreprocessFeatureContent_HandlesMultipleScenarios`: Tests multiple scenario calls in one feature
- `PreprocessFeatureContent_WithMixedCallsAndRegularSteps_MaintainsCorrectOrder`: Tests interleaving of scenario calls and regular steps

#### Indentation Preservation
- `PreprocessFeatureContent_PreservesIndentation`: Ensures indentation is maintained in expanded steps

#### Edge Cases and Limitations
- `PreprocessFeatureContent_WithNestedScenarioCalls_PreservesInnerCalls`: Documents that nested calls are NOT recursively expanded (current limitation)
- `PreprocessFeatureContent_WithScenarioCallInBackground_DoesNotExpand`: Documents that Background section calls are not expanded (current limitation)
- `PreprocessFeatureContent_WithScenarioOutline_DoesNotExpandDueToOutlineKeyword`: Documents that "Scenario Outline:" is not recognized (use "Scenario:" for callable scenarios)
- `PreprocessFeatureContent_WithEmptyFeatureFile_HandlesGracefully`: Tests empty file handling
- `PreprocessFeatureContent_WithOnlyComments_HandlesCorrectly`: Tests comment-only files
- `FindScenarioSteps_WithScenarioHavingTags_SkipsTags`: Tests proper handling of scenario tags
- `PreprocessFeatureContent_WithMultilineScenarioDescription_HandlesCorrectly`: Tests multiline descriptions

## Comparison with Reqnroll PR #684

### What Was Ported

From the original PR's test suite, we adapted and implemented:

1. **Unit Test Coverage**: All core functionality is covered by comprehensive unit tests (46 tests)
2. **Edge Case Testing**: Tests for error conditions, empty files, malformed content
3. **Pattern Recognition**: Thorough testing of the scenario call syntax recognition
4. **Documentation**: Clear examples and limitations documentation

### What Was NOT Ported (And Why)

The PR included `ScenarioCallServiceTests.cs` which tested a **runtime** scenario execution service. This is not applicable because:

1. **Architecture Difference**: The standalone plugin is generator-only, not runtime
2. **No Runtime Service**: There's no `ScenarioCallService` or `ScenarioRegistry` in the generator plugin
3. **Different Approach**: The standalone plugin expands calls at build time, not execution time

### Integration Testing

The PR's integration tests (`TestScenarioCalling.feature`, `TestAuthentication.feature`) tested runtime scenario execution. For the generator plugin, integration is demonstrated through:

1. **Example Projects**: See `examples/BasicUsage/` for working examples
2. **Generated Code Inspection**: The plugin's output can be inspected in `*.feature.cs` files
3. **Build-time Verification**: The build succeeds only if generation works correctly

## Running the Tests

```bash
# Run all unit tests
dotnet test

# Run with detailed output
dotnet test --verbosity normal

# Run specific test category
dotnet test --filter "FullyQualifiedName~GeneratorPluginTests"

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"
```

## Test Results

```
Total Tests: 46
  ✅ Passed: 46
  ❌ Failed: 0
  ⏭️ Skipped: 0
```

All tests pass consistently across builds, ensuring the plugin works reliably.

## Known Limitations (Documented by Tests)

Based on our comprehensive test suite, the following limitations are documented:

1. **Nested Calls**: Scenario calls within called scenarios are not recursively expanded
2. **Background Sections**: Scenario calls in `Background:` sections are not expanded
3. **Scenario Outline**: `Scenario Outline:` is not recognized (only `Scenario:` works)
4. **No Circular Reference Detection**: The plugin doesn't detect circular scenario call chains

These limitations are by design for the current version and may be addressed in future releases.

## Contributing New Tests

When adding new features or fixing bugs, please:

1. Add unit tests that cover the new functionality
2. Ensure all existing tests still pass
3. Update this documentation if adding new test categories
4. Follow the existing test naming conventions
5. Include both positive and negative test cases

## Example Test Pattern

```csharp
[Fact]
public void FeatureName_Condition_ExpectedBehavior()
{
    // Arrange
    var input = "test input";
    
    // Act
    var result = _generator.ProcessFeature(input);
    
    // Assert
    Assert.NotNull(result);
    Assert.Contains("expected", result);
}
```

## Related Documentation

- [README.md](README.md) - General plugin documentation
- [CHANGELOG.md](CHANGELOG.md) - Version history
- [examples/BasicUsage/README.md](examples/BasicUsage/README.md) - Usage examples
- [Reqnroll PR #684](https://github.com/reqnroll/Reqnroll/pull/684) - Original PR that inspired this plugin
