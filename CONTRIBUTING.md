# Contributing to Reqnroll.ScenarioCall.Generator

Thank you for your interest in contributing to the Reqnroll.ScenarioCall.Generator project! We welcome contributions from the community and appreciate your help in making this plugin better.

## Table of Contents

- [Code of Conduct](#code-of-conduct)
- [Getting Started](#getting-started)
- [Development Setup](#development-setup)
- [How to Contribute](#how-to-contribute)
- [Submitting Changes](#submitting-changes)
- [Code Style Guidelines](#code-style-guidelines)
- [Testing](#testing)
- [Documentation](#documentation)
- [Issue Reporting](#issue-reporting)

## Code of Conduct

This project adheres to a Code of Conduct to ensure a welcoming environment for all contributors. Please be respectful and professional in all interactions.

## Getting Started

1. Fork the repository on GitHub
2. Clone your fork locally
3. Set up the development environment (see [Development Setup](#development-setup))
4. Create a feature branch for your changes
5. Make your changes and test them
6. Submit a pull request

## Development Setup

### Prerequisites

- .NET 8.0 SDK or higher
- Git
- A code editor (Visual Studio, VS Code, JetBrains Rider, etc.)

### Setup Steps

1. **Clone the repository:**
   ```bash
   git clone https://github.com/[your-username]/Reqnroll.ScenarioCall.Generator.git
   cd Reqnroll.ScenarioCall.Generator
   ```

2. **Restore dependencies:**
   ```bash
   dotnet restore
   ```

3. **Build the project:**
   ```bash
   dotnet build
   ```

4. **Run tests:**
   ```bash
   dotnet test
   ```

5. **Verify everything works:**
   ```bash
   dotnet test --verbosity normal
   ```

## How to Contribute

### Types of Contributions

We welcome various types of contributions:

- **Bug fixes**: Fix issues reported by users or found during development
- **Feature enhancements**: Add new functionality or improve existing features
- **Documentation**: Improve README, code comments, or create new documentation
- **Testing**: Add or improve unit tests, integration tests, or test coverage
- **Performance**: Optimize code for better performance
- **Code quality**: Refactoring, cleanup, or modernization

### Finding Work

- Check the [Issues](https://github.com/jgerits/Reqnroll.ScenarioCall.Generator/issues) page for open bugs and feature requests
- Look for issues labeled `good first issue` for beginner-friendly tasks
- Issues labeled `help wanted` are specifically looking for community contributions
- Feel free to propose new features by opening an issue first

## Submitting Changes

### Pull Request Process

1. **Create a feature branch:**
   ```bash
   git checkout -b feature/your-feature-name
   # or
   git checkout -b fix/issue-description
   ```

2. **Make your changes:**
   - Write clear, concise commit messages
   - Follow the code style guidelines
   - Add or update tests as needed
   - Update documentation if necessary

3. **Test your changes:**
   ```bash
   dotnet build
   dotnet test
   ```

4. **Commit your changes:**
   ```bash
   git add .
   git commit -m "feat: add new scenario call validation feature"
   ```

5. **Push to your fork:**
   ```bash
   git push origin feature/your-feature-name
   ```

6. **Create a Pull Request:**
   - Go to the GitHub repository
   - Click "New Pull Request"
   - Select your branch
   - Fill out the PR template with a clear description

### Pull Request Guidelines

- **Title**: Use a clear, descriptive title
- **Description**: Explain what your changes do and why
- **Testing**: Describe how you tested your changes
- **Documentation**: Update relevant documentation
- **Breaking Changes**: Clearly mark any breaking changes

### Commit Message Convention

We use conventional commits for consistency:

- `feat:` - New feature
- `fix:` - Bug fix
- `docs:` - Documentation changes
- `test:` - Adding or updating tests
- `refactor:` - Code refactoring
- `perf:` - Performance improvements
- `chore:` - Maintenance tasks

Example: `feat: add support for scenario outline calls`

## Code Style Guidelines

### C# Code Style

- Follow [Microsoft C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/inside-a-program/coding-conventions)
- Use 4 spaces for indentation (no tabs)
- Place opening braces on new lines
- Use meaningful variable and method names
- Add XML documentation comments for public APIs

### Code Organization

- Keep classes focused and follow Single Responsibility Principle
- Use proper namespacing
- Organize using statements
- Remove unused imports

### Example Code Style

```csharp
namespace Reqnroll.ScenarioCall.Generator
{
    /// <summary>
    /// Handles scenario call expansion and validation.
    /// </summary>
    public class ScenarioCallHandler
    {
        private readonly IFeatureFileService _featureFileService;

        public ScenarioCallHandler(IFeatureFileService featureFileService)
        {
            _featureFileService = featureFileService ?? throw new ArgumentNullException(nameof(featureFileService));
        }

        /// <summary>
        /// Expands a scenario call into its constituent steps.
        /// </summary>
        /// <param name="scenarioCall">The scenario call to expand.</param>
        /// <returns>The expanded steps or null if not found.</returns>
        public string ExpandScenarioCall(string scenarioCall)
        {
            // Implementation here
        }
    }
}
```

## Testing

### Test Requirements

- All new features must include unit tests
- Bug fixes should include regression tests
- Aim for high test coverage on new code
- Tests should be clear, focused, and maintainable

### Test Structure

```csharp
[Fact]
public void MethodName_Condition_ExpectedResult()
{
    // Arrange
    var service = new TestService();
    var input = "test input";

    // Act
    var result = service.ProcessInput(input);

    // Assert
    Assert.Equal("expected output", result);
}
```

### Running Tests

```bash
# Run all tests
dotnet test

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test class
dotnet test --filter "ClassName"

# Run tests with verbose output
dotnet test --verbosity normal
```

## Documentation

### Documentation Standards

- Keep README.md up to date with new features
- Add XML documentation for all public APIs
- Include code examples for complex features
- Update CHANGELOG.md for notable changes

### Writing Guidelines

- Use clear, concise language
- Include practical examples
- Keep documentation current with code changes
- Consider the target audience (developers using the plugin)

## Issue Reporting

### Before Creating an Issue

1. Search existing issues to avoid duplicates
2. Check if the issue is already fixed in the latest version
3. Gather relevant information about your environment

### Issue Information

When reporting bugs, please include:

- **Description**: Clear description of the issue
- **Steps to Reproduce**: Detailed steps to reproduce the problem
- **Expected Behavior**: What you expected to happen
- **Actual Behavior**: What actually happened
- **Environment**: 
  - .NET version
  - Reqnroll version
  - Operating system
  - IDE/Editor
- **Sample Code**: Minimal example that demonstrates the issue

### Feature Requests

For feature requests, include:

- **Use Case**: Why is this feature needed?
- **Proposed Solution**: How should it work?
- **Alternatives**: What alternatives have you considered?
- **Examples**: Sample code or scenarios

## Getting Help

If you need help with contributing:

- Open a [Discussion](https://github.com/jgerits/Reqnroll.ScenarioCall.Generator/discussions)
- Comment on relevant issues
- Reach out to maintainers

## Recognition

Contributors will be recognized in:

- CHANGELOG.md for significant contributions
- GitHub contributors page
- Release notes for major features

Thank you for contributing to Reqnroll.ScenarioCall.Generator! 🎉