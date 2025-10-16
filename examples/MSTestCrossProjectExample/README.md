# MSTest Cross-Project Example - Reqnroll Scenario Call Generator

This example demonstrates how to **call scenarios from another project** within the same solution using the Reqnroll.ScenarioCall.Generator plugin.

## 🎯 What This Example Demonstrates

This example shows a real-world pattern where:
- **SharedAuthLibrary** - A shared test library containing reusable authentication scenarios
- **MSTestCrossProjectExample** (this project) - References SharedAuthLibrary and calls its scenarios

This pattern is useful for:
- **Sharing common test scenarios** across multiple test projects
- **Creating reusable test libraries** for authentication, data setup, etc.
- **Maintaining consistency** across different test suites
- **Team collaboration** where different teams own different shared libraries

## 📁 Project Structure

```
MSTestCrossProjectExample/
├── Features/
│   ├── OrderManagement.feature       # Calls scenarios from SharedAuthLibrary
│   └── ProductReviews.feature        # Also calls scenarios from SharedAuthLibrary
├── StepDefinitions/
│   ├── OrderManagementSteps.cs       # Business logic step definitions
│   └── ProductReviewSteps.cs         # Review-specific step definitions
├── GlobalUsings.cs                   # Global using directives
├── reqnroll.json                     # Reqnroll configuration
├── MSTestCrossProjectExample.csproj  # Project file with special configuration
└── README.md                         # This file
```

## 🔧 How Cross-Project Scenario Calling Works

### Step 1: Project Reference

The `.csproj` file includes a project reference to SharedAuthLibrary:

```xml
<ItemGroup>
  <!-- Reference the shared authentication library -->
  <ProjectReference Include="..\SharedAuthLibrary\SharedAuthLibrary.csproj" />
</ItemGroup>
```

**What this does:**
- Makes SharedAuthLibrary's step definitions available to this project
- Allows dependency injection to work across projects
- Enables compilation and build order management
- **Automatically discovers feature files** from the referenced project (no manual copying needed!)

### Step 2: Automatic Feature File Discovery ✨ NEW!

**No additional configuration needed!** The Reqnroll.ScenarioCall.Generator automatically discovers feature files from referenced projects.

The plugin now:
1. **Parses your .csproj file** to find ProjectReference elements
2. **Searches referenced projects** for feature files in common locations (Features/, Specs/, Tests/)
3. **Makes scenarios available** for calling without any manual file copying or complex configuration

**That's it!** Just add the project reference and start calling scenarios.

#### Legacy Approach (No Longer Required)

In previous versions, you needed to manually copy feature files or use MSBuild to link them. This is **no longer necessary** as the plugin handles discovery automatically:

<details>
<summary>Click to see the old manual configuration (for reference only)</summary>

```xml
<!-- ❌ NO LONGER NEEDED - Plugin discovers files automatically -->
<ItemGroup>
  <None Include="..\SharedAuthLibrary\Features\*.feature">
    <Link>Features\SharedAuth\%(Filename)%(Extension)</Link>
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </None>
  <Compile Remove="Features\SharedAuth\*.feature.cs" />
</ItemGroup>
```

The plugin now handles all of this automatically through project reference discovery!

</details>

### Step 3: Call Scenarios from Other Projects

In your feature files, reference scenarios from the shared library:

```gherkin
Feature: Order Management

Scenario: Place Order as Authenticated User
    # This scenario call references the SharedAuthLibrary project
    # The scenario is expanded at build time by the generator plugin
    Given I call scenario "Login with Valid Credentials" from feature "Shared Authentication"
    When I navigate to the products page
    And I add "Widget" to cart
    And I proceed to checkout
    Then I should see order confirmation
```

**How the expansion works:**

1. **Before expansion** (what you write):
   ```gherkin
   Given I call scenario "Login with Valid Credentials" from feature "Shared Authentication"
   ```

2. **After expansion** (what the generator creates):
   ```gherkin
   # Expanded from scenario call: "Login with Valid Credentials" from feature "Shared Authentication"
   Given I am on the login page
   When I enter username "john.doe@example.com"
   And I enter password "SecurePassword123"
   And I click the login button
   Then I should be redirected to the dashboard
   And I should see "Welcome, John Doe" message
   ```

3. **Generated code**: The expanded steps are compiled into the test methods

## 🚀 Getting Started

### Prerequisites

- .NET 8.0 SDK or later
- Visual Studio 2022, VS Code, or JetBrains Rider (optional)

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

### Option 2: Building from Command Line

1. **Restore dependencies** (both projects):
   ```bash
   cd ../SharedAuthLibrary
   dotnet restore
   cd ../MSTestCrossProjectExample
   dotnet restore
   ```

2. **Build the project**:
   ```bash
   dotnet build
   ```
   
   During the build, you'll see:
   - SharedAuthLibrary is built first (dependency order)
   - Feature files are copied to this project
   - The ScenarioCall.Generator expands scenario calls
   - Test code is generated with expanded steps

3. **Run the tests**:
   ```bash
   dotnet test
   ```

## 🔍 Verifying the Expansion

After building, you can verify that scenario calls were expanded:

1. **Check the generated code**:
   ```bash
   # View the generated test file for OrderManagement
   cat Features/OrderManagement.feature.cs
   ```

2. **Look for expanded steps**: You should see the authentication steps inline in the generated test methods

3. **Check build output**: Look for messages from the Reqnroll generator during build

## 💡 Key Features Demonstrated

### 1. Cross-Project Scenario Reuse

```gherkin
# Call a scenario from SharedAuthLibrary
Given I call scenario "Login with Valid Credentials" from feature "Shared Authentication"
```

### 2. Step Definition Sharing

Step definitions from SharedAuthLibrary are automatically available:
- `SharedAuthenticationSteps` are discovered through project reference
- No need to duplicate authentication step definitions
- Dependency injection works seamlessly across projects

### 3. Scenario Context Sharing

The `ScenarioContext` is shared across projects:
```csharp
// In SharedAuthLibrary
_scenarioContext["LoggedIn"] = true;

// In MSTestCrossProjectExample (can access the same context)
if (!_scenarioContext.ContainsKey("LoggedIn"))
{
    throw new Exception("User must be logged in");
}
```

### 4. Multiple Feature Files

The project demonstrates calling scenarios from shared libraries in multiple feature files:
- `OrderManagement.feature` - Order processing with authentication
- `ProductReviews.feature` - Product reviews with authentication

## 📝 Best Practices

### 1. **Organize Shared Libraries**

Create dedicated projects for commonly reused scenarios:
- `SharedAuthLibrary` - Authentication and authorization
- `SharedDataSetupLibrary` - Test data creation
- `SharedNavigationLibrary` - Common navigation scenarios

### 2. **Clear Feature Naming**

Use descriptive feature names that won't conflict:
- ✅ "Shared Authentication" (clear origin)
- ✅ "Common Data Setup" (clear purpose)
- ❌ "Authentication" (could be ambiguous)
- ❌ "Login" (too generic)

### 3. **Use Project References**

Simply add project references to shared libraries - the plugin handles the rest:
```xml
<ItemGroup>
  <ProjectReference Include="..\SharedAuthLibrary\SharedAuthLibrary.csproj" />
</ItemGroup>
```

No need for manual file copying or MSBuild link configurations!

### 4. **Version Control**

When using shared libraries:
- Keep shared libraries in the same solution
- Use project references (not assembly references)
- Version shared libraries alongside consuming projects

### 5. **Test Isolation**

Ensure scenarios in shared libraries:
- Don't have side effects that impact other tests
- Reset state appropriately
- Use ScenarioContext for test-specific state

### 6. **Avoid Deep Nesting**

Don't create chains of scenario calls:
- ❌ Scenario A calls Scenario B, which calls Scenario C
- ✅ Scenario A calls Scenario B directly
- The plugin doesn't recursively expand nested scenario calls

## 🐛 Troubleshooting

### Issue: "Could not expand scenario call"

**Cause**: The feature file from SharedAuthLibrary can't be found or the project reference is missing.

**Solution**: 
1. Verify the `<ProjectReference>` in `.csproj` is correct
2. Ensure SharedAuthLibrary builds successfully
3. Clean and rebuild: `dotnet clean && dotnet build`
4. Check that feature files exist in SharedAuthLibrary's Features/ directory

### Issue: "Step definition not found"

**Cause**: The project reference to SharedAuthLibrary is missing or incorrect.

**Solution**:
1. Verify the `<ProjectReference>` in `.csproj` is correct
2. Check that SharedAuthLibrary builds successfully
3. Ensure both projects target the same framework (.NET 8.0)

### Issue: "Feature not found"

**Cause**: The feature name in the scenario call doesn't match the SharedAuthLibrary feature name.

**Solution**:
- Feature name must match the `Feature:` line exactly (case-insensitive)
- Check SharedAuthLibrary/Features/SharedAuthentication.feature
- Verify: `Feature: Shared Authentication` matches `from feature "Shared Authentication"`

### Issue: Build succeeds but tests fail

**Cause**: ScenarioContext state expectations mismatch.

**Solution**:
- Review which context keys SharedAuthLibrary sets
- Ensure your step definitions check for required context keys
- Add defensive checks for context values

## 📚 Learn More

### Related Documentation

- [SharedAuthLibrary README](../SharedAuthLibrary/README.md) - Details about the shared library
- [Examples README](../README.md) - Overview of all examples
- [Main README](../../README.md) - Plugin documentation

### Example Code

- **Feature Files**: See `Features/` directory for scenario call examples
- **Step Definitions**: See `StepDefinitions/` directory for implementation
- **Project Configuration**: See `MSTestCrossProjectExample.csproj` for setup

### Key Concepts

- **Scenario Calls**: Reuse scenarios across features and projects
- **Project References**: Share code and bindings between projects
- **Feature File Copying**: Make scenarios discoverable for expansion
- **Build-Time Processing**: Scenarios expanded during compilation

## 🤝 Support

- 📫 **Issues**: [GitHub Issues](https://github.com/jgerits/Reqnroll.ScenarioCall.Generator/issues)
- 💬 **Discussions**: [GitHub Discussions](https://github.com/jgerits/Reqnroll.ScenarioCall.Generator/discussions)
- 📚 **Documentation**: [Main README](../../README.md)

---

**This example demonstrates the power of cross-project scenario reuse with Reqnroll.ScenarioCall.Generator!** 🚀
