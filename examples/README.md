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

## Requirements

- .NET 8.0 SDK or later
- Visual Studio 2022, VS Code, or JetBrains Rider (optional, for IDE support)

## Getting Started

1. **Clone or download** this repository
2. **Open** `Examples.sln` in your preferred IDE
3. **Explore** the MSTestExample project to see the plugin in action
4. **Build** the solution to see code generation happen
5. **Run** the tests to verify everything works

## Note on Building

The MSTestExample is configured to use the published NuGet package version 3.0.7. Once this version is published to NuGet.org, the solution will build successfully out of the box. Until then, the example serves as a reference for project structure and configuration.

## Learn More

- [Main Repository README](../README.md)
- [MSTest Example Documentation](MSTestExample/README.md)
- [Reqnroll Documentation](https://docs.reqnroll.net/)
