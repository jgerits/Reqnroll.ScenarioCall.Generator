# Basic Usage Example

This example demonstrates the core functionality of the Reqnroll.ScenarioCall.Generator plugin.

## Files

### English Examples
- **Authentication.feature**: Contains reusable authentication scenarios
- **UserManagement.feature**: Demonstrates calling authentication scenarios from user management tests

### Dutch Examples (Nederlands)
- **AuthenticatieNL.feature**: Bevat herbruikbare authenticatie scenario's
- **GebruikersbeheerNL.feature**: Demonstreert het aanroepen van authenticatie scenario's vanuit gebruikersbeheer tests

## Key Concepts Shown

### 1. Reusable Scenarios
The `Authentication.feature` file contains scenarios that can be reused across multiple feature files:
- Login with Valid Credentials
- Logout
- Login with Invalid Credentials

### 2. Scenario Calls
The `UserManagement.feature` file shows how to call scenarios from other features:

```gherkin
Given I call scenario "Login with Valid Credentials" from feature "Authentication"
```

### 3. Test Composition
Each user management test follows a pattern:
1. Login (using scenario call)
2. Perform specific user management action
3. Logout (using scenario call)

This ensures consistent setup and teardown across all tests.

## Dutch Language Examples

The same examples are also provided in Dutch (Nederlands) to demonstrate the multi-language support of the plugin.

### Key Differences in Dutch

The Dutch examples use the `# language: nl` directive and Dutch Gherkin keywords:

```gherkin
# language: nl
Functionaliteit: Authenticatie

Scenario: Inloggen met geldige inloggegevens
    Gegeven ik ben op de inlogpagina
    Als ik gebruikersnaam "john.doe@example.com" invoer
    En ik wachtwoord "SecurePassword123" invoer
    En ik op de inlogknop klik
    Dan zou ik naar het dashboard geleid moeten worden
    En zou ik "Welkom, John Doe" bericht moeten zien
```

### Dutch Scenario Call Syntax

When calling scenarios in Dutch, use the translated phrase:

```gherkin
Gegeven ik roep scenario "Inloggen met geldige inloggegevens" aan uit functionaliteit "Authenticatie"
```

This is equivalent to the English version:

```gherkin
Given I call scenario "Login with Valid Credentials" from feature "Authentication"
```

## Generated Output

When processed by the plugin, the scenario calls are expanded inline. For example, the "Create New User Account" scenario becomes:

```gherkin
Scenario: Create New User Account
    # Expanded from scenario call: "Login with Valid Credentials" from feature "Authentication"
    Given I am on the login page
    When I enter username "john.doe@example.com"
    And I enter password "SecurePassword123"
    And I click the login button
    Then I should be redirected to the dashboard
    And I should see "Welcome, John Doe" message
    When I navigate to the user management section
    And I click "Add New User" button
    And I fill in the new user form:
        | Field    | Value                |
        | Username | jane.smith           |
        | Email    | jane.smith@test.com  |
        | Role     | Standard User        |
    And I click "Create User" button
    Then I should see "User created successfully" message
    And I should see "jane.smith" in the user list
    # Expanded from scenario call: "Logout" from feature "Authentication"
    Given I am logged into the system
    When I click the logout button
    Then I should be redirected to the login page
    And I should see "You have been logged out" message
```

## Benefits

1. **Reduced Duplication**: Login/logout logic is defined once and reused
2. **Consistency**: All tests use the same authentication flow
3. **Maintainability**: Changes to authentication only need to be made in one place
4. **Readability**: High-level test scenarios are easier to understand

## Running This Example

To use this example in your own project:

1. Install the Reqnroll.ScenarioCall.Generator plugin
2. Copy these feature files to your project's Features folder
3. Build your project - the scenario calls will be automatically expanded
4. Implement the corresponding step definitions for the expanded steps