# SharedAuthLibrary - Reusable Authentication Scenarios

This project demonstrates how to create a shared library of reusable authentication scenarios that can be referenced and called from other projects within the same solution.

## Purpose

The SharedAuthLibrary serves as a **shared test library** containing:
- Common authentication scenarios (login, logout, password reset)
- Reusable step definitions for authentication workflows
- Feature files that can be referenced by other projects

## Project Structure

```
SharedAuthLibrary/
├── Features/
│   └── SharedAuthentication.feature    # Reusable authentication scenarios
├── StepDefinitions/
│   └── SharedAuthenticationSteps.cs    # Step definitions for authentication
├── GlobalUsings.cs                     # Global using directives
├── reqnroll.json                       # Reqnroll configuration
├── SharedAuthLibrary.csproj            # Project file
└── README.md                           # This file
```

## Key Concepts

### Shared Feature Files

The `SharedAuthentication.feature` file contains scenarios that are designed to be called from other projects:

```gherkin
Scenario: Login with Valid Credentials
    Given I am on the login page
    When I enter username "john.doe@example.com"
    And I enter password "SecurePassword123"
    And I click the login button
    Then I should be redirected to the dashboard
    And I should see "Welcome, John Doe" message
```

### Shared Step Definitions

The step definitions in this project are marked with the `[Binding]` attribute and will be available to any project that references this library through dependency injection.

## How Other Projects Use This Library

### 1. Add Project Reference

Other projects reference this library in their `.csproj` file:

```xml
<ItemGroup>
  <ProjectReference Include="..\SharedAuthLibrary\SharedAuthLibrary.csproj" />
</ItemGroup>
```

### 2. Copy Feature Files to Referencing Project

To allow the scenario call generator to find and expand scenarios, the feature files must be copied to the referencing project during build:

```xml
<ItemGroup>
  <!-- Copy SharedAuthLibrary feature files to this project's Features directory -->
  <None Include="..\SharedAuthLibrary\Features\*.feature">
    <Link>Features\SharedAuth\%(Filename)%(Extension)</Link>
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </None>
</ItemGroup>
```

**Why is this needed?**
- The Reqnroll.ScenarioCall.Generator searches for feature files in the current project's directory structure
- By copying the feature files, we make them discoverable by the generator
- The `<Link>` element creates a logical folder structure in the project without physically moving files

### 3. Call Scenarios from Other Projects

Once the project reference and feature file copying are configured, scenarios can be called:

```gherkin
Feature: Order Management
    
Scenario: Place Order as Authenticated User
    # Call a scenario from SharedAuthLibrary
    Given I call scenario "Login with Valid Credentials" from feature "Shared Authentication"
    When I navigate to the products page
    And I add "Widget" to cart
    And I proceed to checkout
    Then I should see order confirmation
```

### 4. Step Definitions Automatically Available

The step definitions from SharedAuthLibrary are automatically available to the referencing project through Reqnroll's binding discovery mechanism. No additional configuration is needed.

## Benefits of This Approach

1. **Code Reuse**: Authentication logic is maintained in one place
2. **Consistency**: All projects use the same authentication scenarios
3. **Maintainability**: Updates to authentication only need to be made once
4. **Separation of Concerns**: Authentication logic is isolated from business logic
5. **Team Collaboration**: Different teams can own different shared libraries

## Testing This Library

You can build and test this library independently:

```bash
dotnet build
dotnet test
```

## See Also

- [MSTestCrossProjectExample](../MSTestCrossProjectExample/README.md) - Example project that uses this library
- [Examples README](../README.md) - Overview of all examples
