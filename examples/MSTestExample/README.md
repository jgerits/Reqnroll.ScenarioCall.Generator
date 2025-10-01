# MSTest Example - Reqnroll Scenario Call Generator

This example demonstrates how to use the Reqnroll.ScenarioCall.Generator plugin with MSTest as the test framework.

## Overview

This is a complete, working example that shows:
- How to install and configure the plugin using NuGet (version 3.0.7 or later)
- How to create reusable scenarios in feature files
- How to call scenarios from other feature files using the scenario call syntax
- How to implement step definitions for MSTest
- Best practices for organizing test code

**Note**: This example uses the published NuGet package `JGerits.Reqnroll.ScenarioCall.Generator` version 3.0.7. Once version 3.0.8+ is published with the fixed targets file, the example will work out of the box.

## Project Structure

```
MSTestExample/
├── Features/
│   ├── Authentication.feature       # Reusable authentication scenarios
│   └── UserManagement.feature       # Feature that calls authentication scenarios
├── StepDefinitions/
│   ├── AuthenticationSteps.cs       # Step definitions for authentication
│   └── UserManagementSteps.cs       # Step definitions for user management
├── GlobalUsings.cs                  # Global using directives
├── reqnroll.json                    # Reqnroll configuration
├── MSTestExample.csproj             # Project file with NuGet dependencies
└── README.md                        # This file
```

## Key Features Demonstrated

### 1. NuGet Package Usage

This example uses the published NuGet package `JGerits.Reqnroll.ScenarioCall.Generator` version 3.0.7:

```xml
<PackageReference Include="JGerits.Reqnroll.ScenarioCall.Generator" Version="3.0.7" />
```

The plugin automatically registers itself via the included targets file and processes scenario calls at build time. The targets file has been fixed in version 3.0.8+ to properly reference the plugin DLL from the NuGet package.

### 2. Reusable Scenarios

The `Authentication.feature` file contains reusable scenarios:

```gherkin
Scenario: Login with Valid Credentials
    Given I am on the login page
    When I enter username "john.doe@example.com"
    And I enter password "SecurePassword123"
    And I click the login button
    Then I should be redirected to the dashboard
    And I should see "Welcome, John Doe" message

Scenario: Logout
    Given I am logged into the system
    When I click the logout button
    Then I should be redirected to the login page
    And I should see "You have been logged out" message
```

### 3. Scenario Calls

The `UserManagement.feature` file uses scenario calls to reuse authentication logic:

```gherkin
Scenario: Create New User Account
    Given I call scenario "Login with Valid Credentials" from feature "Authentication"
    When I navigate to the user management section
    And I click "Add New User" button
    ...
    And I call scenario "Logout" from feature "Authentication"
```

### 4. Automatic Expansion

When you build the project, the plugin automatically expands scenario calls inline. The generated code in `UserManagement.feature.cs` will contain all the steps from the called scenarios, allowing them to execute as if they were written directly in the test.

## Requirements

- .NET 8.0 SDK or later
- MSTest test framework
- Reqnroll 3.0.7 or later

## Getting Started

### Option 1: Using the Examples Solution

Open the `Examples.sln` file in the parent examples folder:

```bash
cd ..
start Examples.sln  # Windows
# or
open Examples.sln   # macOS
# or
code Examples.sln   # VS Code
```

This solution includes the MSTestExample project and makes it easy to explore and run the example.

### Option 2: Using the Project Directly

### 1. Install Dependencies

The project file already contains all required dependencies. Simply restore packages:

```bash
dotnet restore
```

### 2. Build the Project

When you build the project, the Reqnroll.ScenarioCall.Generator plugin will process the feature files and expand scenario calls:

```bash
dotnet build
```

During the build, you'll see the plugin working. Look for the generated `*.feature.cs` files in the Features directory to see the expanded scenarios.

### 3. Run the Tests

Execute the tests using dotnet test:

```bash
dotnet test
```

Or using MSTest test explorer in Visual Studio.

## What Happens During Build

1. **Feature File Discovery**: The plugin automatically discovers all `.feature` files in your project
2. **Scenario Call Detection**: The plugin identifies lines with the pattern `I call scenario "X" from feature "Y"`
3. **Scenario Expansion**: Called scenarios are expanded inline, replacing the call with the actual steps
4. **Code Generation**: Reqnroll generates C# code from the expanded feature files
5. **Compilation**: The project builds normally with the generated code

## How It Works

### The Scenario Call Syntax

```gherkin
Given I call scenario "Scenario Name" from feature "Feature Name"
```

- `Scenario Name`: The exact name of the scenario to call (case-sensitive)
- `Feature Name`: The name of the feature file without the `.feature` extension
- The plugin searches for feature files in standard locations (Features/, Specs/, Tests/ folders)

### Generated Code

After building, check the `Authentication.feature.cs` and `UserManagement.feature.cs` files to see the generated test methods. The scenario calls will have been replaced with the actual steps from the referenced scenarios.

## Best Practices

1. **Organize Reusable Scenarios**: Keep commonly used scenarios (like login/logout) in dedicated feature files
2. **Clear Naming**: Use descriptive scenario names that make their purpose clear
3. **Avoid Deep Nesting**: Don't call scenarios that themselves call other scenarios (not supported)
4. **Document Intent**: Add comments to explain why you're reusing a particular scenario
5. **Test Isolation**: Ensure called scenarios don't have side effects that could impact other tests

## Troubleshooting

### Scenario Not Found

If you see a warning like "Scenario 'X' not found in feature 'Y'":
- Verify the scenario name matches exactly (case-sensitive)
- Ensure the feature name is correct (without .feature extension)
- Check that the feature file is in a discoverable location

### Plugin Not Working

If scenario calls aren't being expanded:
- Ensure the NuGet package is installed: `dotnet list package`
- Clean and rebuild: `dotnet clean && dotnet build`
- Check the build output for any plugin-related messages

### Generated Code Missing

If the `.feature.cs` files aren't generated:
- Ensure Reqnroll is properly installed
- Verify the reqnroll.json configuration is valid
- Check that your feature files have the correct build action (usually "None" or "Content")

## Learn More

- [Main Repository](https://github.com/jgerits/Reqnroll.ScenarioCall.Generator)
- [Reqnroll Documentation](https://docs.reqnroll.net/)
- [MSTest Documentation](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-with-mstest)

## Support

For issues, questions, or contributions, please visit the [GitHub repository](https://github.com/jgerits/Reqnroll.ScenarioCall.Generator).
