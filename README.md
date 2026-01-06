# Reqnroll.ScenarioCall.Generator

[![Build Status](https://github.com/jgerits/Reqnroll.ScenarioCall.Generator/actions/workflows/ci-cd.yml/badge.svg)](https://github.com/jgerits/Reqnroll.ScenarioCall.Generator/actions/workflows/ci-cd.yml)
[![NuGet](https://img.shields.io/nuget/v/JGerits.Reqnroll.ScenarioCall.Generator.svg)](https://www.nuget.org/packages/JGerits.Reqnroll.ScenarioCall.Generator/)
[![License](https://img.shields.io/badge/license-BSD%203--Clause-blue.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-Standard%202.0-purple)](https://dotnet.microsoft.com/download/dotnet-standard)

A powerful Reqnroll generator plugin that enables calling and embedding scenarios from other feature files directly within your test scenarios. This plugin promotes test reusability, modularity, and better organization of your Gherkin specifications.

## Features

- 🔄 **Scenario Reusability**: Call existing scenarios from any feature file
- 🎯 **Inline Expansion**: Automatically expands scenario calls during test generation
- 🗂️ **Cross-Feature Support**: Reference scenarios across different feature files
- 📄 **Same-Feature Calls**: Call scenarios within the same feature file ✨ NEW!
- 🚀 **Cross-Project Support**: Automatically discovers and calls scenarios from referenced projects
- 🏗️ **Build-Time Processing**: No runtime overhead - scenarios are expanded at build time
- 🛡️ **Error Handling**: Graceful handling of missing scenarios with clear warnings
- 🔒 **Recursion Detection**: Automatically prevents circular scenario references ✨ NEW!
- 📁 **Automatic Discovery**: Automatically finds feature files in referenced projects - no manual copying needed
- 🌍 **Multi-Language Support**: Supports all Gherkin languages (English, German, French, Spanish, Dutch, and more)

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

See the examples folder for complete working examples:
- [examples/BasicUsage](examples/BasicUsage/) - Feature file examples showing the syntax
- [examples/MSTestExample](examples/MSTestExample/) - Complete MSTest project example with step definitions

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
dotnet add package JGerits.Reqnroll.ScenarioCall.Generator
```

Or via Package Manager Console in Visual Studio:
```powershell
Install-Package JGerits.Reqnroll.ScenarioCall.Generator
```

### Manual Installation
1. Download the latest release from the [releases page](https://github.com/jgerits/Reqnroll.ScenarioCall.Generator/releases)
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

### Same-Feature Scenario Calls ✨ NEW!

Call scenarios within the same feature file - perfect for sharing common setup or reusable steps without creating separate files!

**Example:**
```gherkin
Feature: User Management

Scenario: Common Setup
    Given the system is initialized
    And the database is clean
    And test data is loaded

Scenario: Create User With Setup
    Given I call scenario "Common Setup" from feature "User Management"
    When I create a new user
    Then the user should be created successfully
```

**How it works:**
- Reference the current feature by name to call scenarios within the same file
- The plugin automatically detects the current feature context
- Recursion detection prevents infinite loops from self-referencing scenarios
- Works seamlessly with cross-feature calls in the same test

**Benefits:**
- Reduce duplication within a feature file
- Keep related scenarios organized together
- Avoid creating separate feature files for simple reusable steps

**Recursion Protection:**
The plugin automatically detects and prevents circular references:
```gherkin
Scenario: Self Referencing (Not Allowed)
    Given I call scenario "Self Referencing" from feature "User Management"
    # Error: Circular reference detected - scenario "Self Referencing" from feature "User Management" is already in the call chain
```

**Example:** See [examples/BasicUsage/SameFeatureCalls.feature](examples/BasicUsage/SameFeatureCalls.feature) for a complete example.

### Cross-Project Scenario Calls ✨

Call scenarios from other projects in your solution - perfect for sharing common test scenarios across multiple test projects!

**Setup:**
1. Add a project reference to the shared library:
```xml
<ItemGroup>
  <ProjectReference Include="..\SharedAuthLibrary\SharedAuthLibrary.csproj" />
</ItemGroup>
```

2. Call scenarios from the referenced project:
```gherkin
Feature: Order Management

Scenario: Place Order as Authenticated User
    Given I call scenario "Login with Valid Credentials" from feature "Shared Authentication"
    When I navigate to the products page
    And I proceed to checkout
    Then I should see order confirmation
```

**How it works:**
- The plugin automatically discovers feature files from referenced projects
- No manual file copying or complex MSBuild configuration needed
- Scenarios are expanded at build time just like same-project calls
- Step definitions from referenced projects are available through dependency injection

**Example:** See [examples/MSTestCrossProjectExample](examples/MSTestCrossProjectExample/) for a complete working example.

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

### Multi-Language Support

The plugin supports all Gherkin languages that Reqnroll supports. Specify the language using the `# language:` directive at the top of your feature file.

**Dutch Example (AuthenticatieNL.feature)**
```gherkin
# language: nl
Functionaliteit: Authenticatie

Scenario: Inloggen met geldige inloggegevens
    Gegeven ik ben op de inlogpagina
    Als ik gebruikersnaam "john.doe@example.com" invoer
    En ik wachtwoord "SecurePassword123" invoer
    Dan zou ik ingelogd moeten zijn
```

**Using Dutch scenarios in your feature files:**
```gherkin
# language: nl
Functionaliteit: Gebruikersbeheer

Scenario: Nieuw gebruikersaccount aanmaken
    Gegeven I call scenario "Inloggen met geldige inloggegevens" from feature "Authenticatie"
    Als ik naar het gebruikersbeheersectie navigeer
    Dan zou ik de gebruikerslijst moeten zien
```

**German Example (AuthentifizierungDE.feature)**
```gherkin
# language: de
Funktionalität: Authentifizierung

Szenario: Login mit gültigen Anmeldedaten
    Angenommen ich bin auf der Login-Seite
    Wenn ich Benutzername "john.doe@example.com" eingebe
    Und ich Passwort "SecurePassword123" eingebe
    Dann sollte ich eingeloggt sein
```

**Using German scenarios in your feature files:**
```gherkin
# language: de
Funktionalität: Benutzerverwaltung

Szenario: Neues Benutzerkonto erstellen
    Angenommen I call scenario "Login mit gültigen Anmeldedaten" from feature "Authentifizierung"
    Wenn ich zum Benutzerverwaltungsbereich navigiere
    Dann sollte ich die Benutzerliste sehen
```

**Supported Languages**: English (en), Dutch (nl), German (de), French (fr), Spanish (es), and many more. See [Gherkin language reference](https://cucumber.io/docs/gherkin/languages/) for a complete list.

**Mixed Language Support**: You can call scenarios from feature files written in different languages. The plugin automatically detects the language of each feature file.

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
**Solution**: The plugin now automatically detects and prevents circular references. If you see an error message like "Circular reference detected", check your scenario calls to ensure they don't create a loop.

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

- ~~Circular scenario call references are not detected~~ ✅ **FIXED**: Circular references are now automatically detected and prevented
- ~~Calling scenarios within the same feature file is not supported~~ ✅ **FIXED**: Same-feature scenario calls are now fully supported
- Scenario Outline templates cannot be called directly (use regular Scenario: instead)
- Scenario calls in `Background:` sections are not expanded (only in `Scenario:` sections)
- Nested scenario calls are not recursively expanded (scenario calls within called scenarios remain as-is)
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

## CI/CD Pipeline

This project uses GitHub Actions for automated building, testing, and publishing to NuGet.org.

### Automated Workflows

#### CI/CD Pipeline (`.github/workflows/ci-cd.yml`)

The pipeline automatically:

1. **On every push to main/master and pull requests**:
   - Sets up .NET 8.0 environment
   - Restores NuGet dependencies
   - Builds the solution in Release configuration
   - Runs all tests
   - Creates NuGet packages
   - Uploads packages as build artifacts

2. **On every push to main/master (automatic releases)**:
   - Automatically increments the patch version (e.g., 3.0.0 → 3.0.1)
   - Updates version numbers in the project file
   - Builds and tests with the new version
   - Creates a GitHub release with the new tag
   - Attaches the NuGet package to the release

3. **On GitHub releases**:
   - Downloads the build artifacts
   - Publishes the NuGet package to nuget.org

### Automatic Versioning

**New in this version**: Every commit to the main branch now automatically creates a new release!

The versioning follows the pattern described in the [CHANGELOG.md](CHANGELOG.md):
- **MAJOR.MINOR**: Matches the Reqnroll version (e.g., 3.0 for Reqnroll 3.0.x)
- **PATCH**: Automatically incremented on each commit to main (3.0.0 → 3.0.1 → 3.0.2, etc.)

### Publishing Releases

**Automatic Process** (recommended):
1. Simply push changes to the main branch
2. The CI/CD pipeline will automatically:
   - Increment the patch version
   - Create a GitHub release
   - Publish to NuGet.org

**Manual Process** (for major/minor version changes):
1. Update the version in `Reqnroll.ScenarioCall.Generator.csproj`:
   ```xml
   <Version>4.0.0</Version>
   <AssemblyVersion>4.0.0</AssemblyVersion>
   <FileVersion>4.0.0</FileVersion>
   ```
2. Commit and push to main
3. The pipeline will use your specified version instead of auto-incrementing

### Setup Requirements

To enable automatic publishing, the repository requires a `NUGET_API_KEY` secret containing a valid NuGet.org API key with push permissions.

## License

This project is licensed under the BSD 3-Clause License - see the [LICENSE](LICENSE) file for details.

## Support

- 📫 **Issues**: [GitHub Issues](https://github.com/jgerits/Reqnroll.ScenarioCall.Generator/issues)
- 💬 **Discussions**: [GitHub Discussions](https://github.com/jgerits/Reqnroll.ScenarioCall.Generator/discussions)
- 📚 **Documentation**: This README and inline code documentation

## Related Projects

- [Reqnroll](https://github.com/reqnroll-net/Reqnroll) - The .NET BDD framework this plugin extends
- [SpecFlow](https://github.com/SpecFlowOSS/SpecFlow) - The original framework (Reqnroll is the successor)

---

**Made with ❤️ for the BDD community**
