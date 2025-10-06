# Examples

This folder contains working examples demonstrating how to use the Reqnroll.ScenarioCall.Generator plugin.

## Opening the Examples

### Using the Examples Solution

Open the `Examples.sln` solution file in Visual Studio, Visual Studio Code, or JetBrains Rider to explore and run the examples:

```bash
# Open in Visual Studio (Windows)
start Examples.sln

# Open in Visual Studio Code
code Examples.sln

# Or build from command line
dotnet build Examples.sln
```

### Individual Examples

You can also explore each example individually:

## Available Examples

### 1. BasicUsage

Location: `BasicUsage/`

A simple demonstration showing the feature file syntax for scenario calling. This folder contains only `.feature` files that demonstrate:
- Reusable authentication scenarios
- User management scenarios that call authentication scenarios
- The scenario call syntax

**Note**: This is a documentation example without a project file. See the MSTestExample for a complete, buildable project.

### 2. MSTestExample

Location: `MSTestExample/`

A complete, production-ready MSTest project demonstrating the plugin in action:
- Full project configuration using NuGet package
- Feature files with scenario calls
- Complete step definition implementations
- MSTest integration
- Comprehensive documentation

See [MSTestExample/README.md](MSTestExample/README.md) for detailed setup and usage instructions.

### 3. SharedAuthLibrary

Location: `SharedAuthLibrary/`

A shared test library containing reusable authentication scenarios that can be referenced by other projects:
- Reusable authentication scenarios (login, logout, password reset)
- Step definitions for authentication workflows
- Demonstrates creating a shared test library
- Can be referenced by multiple test projects

See [SharedAuthLibrary/README.md](SharedAuthLibrary/README.md) for documentation.

### 4. MSTestCrossProjectExample

Location: `MSTestCrossProjectExample/`

**⭐ NEW: Cross-Project Scenario Calling Example**

A complete example demonstrating how to call scenarios from another project within the same solution:
- References and uses SharedAuthLibrary
- Feature files that call scenarios from SharedAuthLibrary
- Shows how to configure project references and feature file copying
- Demonstrates cross-project step definition sharing
- Includes comprehensive documentation with detailed comments

**This is the recommended example for learning how to share scenarios across multiple projects in a solution.**

See [MSTestCrossProjectExample/README.md](MSTestCrossProjectExample/README.md) for detailed setup and usage instructions.

## Requirements

- .NET 8.0 SDK or later
- Visual Studio 2022, VS Code, or JetBrains Rider (optional, for IDE support)

## Getting Started

1. **Clone or download** this repository
2. **Open** `Examples.sln` in your preferred IDE
3. **Explore** the examples:
   - Start with **MSTestExample** for basic scenario calling within a single project
   - Then explore **MSTestCrossProjectExample** and **SharedAuthLibrary** to see cross-project scenario reuse
4. **Build** the solution to see code generation happen
5. **Run** the tests to verify everything works

### Note on Development Setup

The example projects in this repository use a **project reference** to the local build of the Reqnroll.ScenarioCall.Generator plugin instead of the NuGet package. This allows the examples to always use the latest code during development.

In your own projects, you should use the NuGet package instead:
```xml
<PackageReference Include="JGerits.Reqnroll.ScenarioCall.Generator" Version="3.0.8" />
```



## Learn More

- [Main Repository README](../README.md)
- [MSTest Example Documentation](MSTestExample/README.md)
- [Cross-Project Example Documentation](MSTestCrossProjectExample/README.md)
- [Shared Library Documentation](SharedAuthLibrary/README.md)
- [Reqnroll Documentation](https://docs.reqnroll.net/)
