# Reqnroll.ScenarioCall.Generator

[![Build Status](https://img.shields.io/badge/build-passing-brightgreen)](https://github.com/zelda1link3/Reqnroll.ScenarioCall.Generator)
[![License](https://img.shields.io/badge/license-BSD%203--Clause-blue.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-Standard%202.0-purple)](https://dotnet.microsoft.com/download/dotnet-standard)

A powerful Reqnroll generator plugin that enables calling and embedding scenarios from other feature files directly within your test scenarios. This plugin promotes test reusability, modularity, and better organization of your Gherkin specifications.

## Features

- 🔄 **Scenario Reusability**: Call existing scenarios from any feature file
- 🎯 **Inline Expansion**: Automatically expands scenario calls during test generation
- 🗂️ **Cross-Feature Support**: Reference scenarios across different feature files
- 🏗️ **Build-Time Processing**: No runtime overhead - scenarios are expanded at build time
- 🛡️ **Error Handling**: Graceful handling of missing scenarios with clear warnings
- 📁 **Flexible File Discovery**: Automatically searches common feature file locations

## Quick Start

### Syntax
Use the following syntax to call scenarios from other features:

```gherkin
Given I call scenario "ScenarioName" from feature "FeatureName"
When I call scenario "ScenarioName" from feature "FeatureName"  
Then I call scenario "ScenarioName" from feature "FeatureName"
And I call scenario "ScenarioName" from feature "FeatureName"
But I call scenario "ScenarioName" from feature "FeatureName"
```

### Basic Example

See the [examples/BasicUsage](examples/BasicUsage/) folder for a complete working example.

**Authentication.feature**
```gherkin
Feature: Authentication
    As a user
    I want to be able to login and logout
    So that I can access the application securely

Scenario: Login
    Given I am on the login page
    When I enter valid credentials
    Then I should be logged in successfully

Scenario: Logout
    Given I am logged in
    When I click the logout button
    Then I should be logged out
```

**UserManagement.feature**
```gherkin
Feature: User Management
    As an administrator
    I want to manage user accounts
    So that I can control access to the system

Scenario: Create New User Account
    Given I call scenario "Login" from feature "Authentication"
    When I navigate to user management
    And I create a new user account
    Then the user should be created successfully
    And I call scenario "Logout" from feature "Authentication"
```

### Generated Output
The scenario call is automatically expanded during build time:

```gherkin
Scenario: Create New User Account
    # Expanded from scenario call: "Login" from feature "Authentication"
    Given I am on the login page
    When I enter valid credentials
    Then I should be logged in successfully
    When I navigate to user management
    And I create a new user account
    Then the user should be created successfully
    # Expanded from scenario call: "Logout" from feature "Authentication"
    Given I am logged in
    When I click the logout button
    Then I should be logged out
```

## Installation

### NuGet Package
Install the plugin via NuGet Package Manager:

```bash
dotnet add package Reqnroll.ScenarioCall.Generator
```

Or via Package Manager Console in Visual Studio:
```powershell
Install-Package Reqnroll.ScenarioCall.Generator
```

### Manual Installation
1. Download the latest release from the [releases page](https://github.com/zelda1link3/Reqnroll.ScenarioCall.Generator/releases)
2. Add the assembly reference to your test project
3. Ensure the plugin is registered (see Configuration section)

## Configuration

### Automatic Registration
The plugin automatically registers itself with Reqnroll when referenced in your project. No additional configuration is required for basic usage.

### Feature File Discovery
The plugin automatically searches for feature files in the following locations relative to your project:
- Current directory
- `Features/` folder
- `Specs/` folder  
- `Tests/` folder

All searches include subdirectories recursively.

### Project Structure Example
```
MyProject/
├── Features/
│   ├── Authentication.feature
│   ├── UserManagement.feature
│   └── Shopping/
│       ├── Cart.feature
│       └── Checkout.feature
├── Specs/
│   └── Integration/
│       └── EndToEnd.feature
└── Tests/
    └── Smoke/
        └── SmokeTests.feature
```

## Advanced Usage

### Error Handling
When a scenario call cannot be resolved, the plugin adds a warning comment instead of failing the build:

```gherkin
Scenario: Test with Missing Reference
    Given I call scenario "NonExistent" from feature "Missing"
    # Warning: Could not expand scenario call
```

### Nested Scenario Calls
Scenarios can contain calls to other scenarios, enabling complex composition:

**BaseOperations.feature**
```gherkin
Feature: Base Operations

Scenario: Setup Test Environment
    Given the application is started
    And the database is clean

Scenario: Complete Login Flow
    Given I call scenario "Setup Test Environment" from feature "Base Operations" 
    When I call scenario "Login" from feature "Authentication"
    Then I should see the dashboard
```

### Case Sensitivity
- Feature names are case-insensitive
- Scenario names are case-insensitive
- File discovery is case-insensitive

## Requirements

- .NET Standard 2.0 or higher
- Reqnroll (compatible with SpecFlow migration)
- MSBuild or .NET CLI for build-time processing

## Troubleshooting

### Common Issues

**Issue**: Scenario not found
```
# Warning: Could not expand scenario call
```
**Solution**: 
- Verify the feature name matches the `Feature:` declaration exactly
- Ensure the scenario name exists in the target feature file
- Check that the feature file is in a discoverable location

**Issue**: Infinite recursion
**Solution**: Avoid circular references between scenario calls. The plugin does not currently detect circular dependencies.

**Issue**: Feature file not found
**Solution**: 
- Ensure feature files are included in the project build
- Verify the feature file path is in one of the searched directories
- Check file naming conventions (`.feature` extension required)

### Debug Tips
1. Check the generated test files to see expanded scenario content
2. Enable verbose MSBuild logging to see plugin activity
3. Verify feature file discovery paths match your project structure

## Limitations

- Circular scenario call references are not detected
- Scenario Outline templates cannot be called directly
- Parameters cannot be passed between called scenarios
- Background steps are not included in scenario calls

## Contributing

We welcome contributions! Please see our [Contributing Guidelines](CONTRIBUTING.md) for details.

### Development Setup
1. Clone the repository
2. Install .NET 8.0 SDK or higher
3. Restore packages: `dotnet restore`
4. Build: `dotnet build`
5. Run tests: `dotnet test`

### Running Tests
```bash
# Run all tests
dotnet test

# Run tests with verbose output
dotnet test --verbosity normal

# Run specific test
dotnet test --filter "TestMethodName"
```

## Changelog

### Version 1.0.0
- Initial release
- Basic scenario calling functionality
- Automatic feature file discovery
- Error handling for missing scenarios
- Support for all Gherkin step keywords

## License

This project is licensed under the BSD 3-Clause License - see the [LICENSE](LICENSE) file for details.

## Support

- 📫 **Issues**: [GitHub Issues](https://github.com/zelda1link3/Reqnroll.ScenarioCall.Generator/issues)
- 💬 **Discussions**: [GitHub Discussions](https://github.com/zelda1link3/Reqnroll.ScenarioCall.Generator/discussions)
- 📚 **Documentation**: This README and inline code documentation

## Related Projects

- [Reqnroll](https://github.com/reqnroll-net/Reqnroll) - The .NET BDD framework this plugin extends
- [SpecFlow](https://github.com/SpecFlowOSS/SpecFlow) - The original framework (Reqnroll is the successor)

---

**Made with ❤️ for the BDD community**
