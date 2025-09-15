# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- **Automatic Release System**: Every commit to main now automatically creates a new release
  - Patch version is automatically incremented (e.g., 3.0.0 → 3.0.1)
  - GitHub releases are created automatically with NuGet packages attached
  - Maintains the same MAJOR.MINOR versioning strategy aligned with Reqnroll
- Comprehensive documentation improvements
- Contributing guidelines
- Enhanced project metadata for NuGet package
- Detailed README with usage examples and troubleshooting

### Changed
- Fixed typo in original README ("Scenerio's" → "Scenarios")
- Updated CI/CD pipeline to support automatic releases on main branch commits
- **BREAKING**: Changed package ID from `Reqnroll.ScenarioCall.Generator` to `JGerits.Reqnroll.ScenarioCall.Generator` to avoid conflicts with reserved "reqnroll" prefix on nuget.org

## [3.0.0] - 2025-01-XX

### Added
- Initial release of Reqnroll.ScenarioCall.Generator
- Core scenario calling functionality with syntax: `Given I call scenario "ScenarioName" from feature "FeatureName"`
- Support for all Gherkin step keywords (Given, When, Then, And, But)
- Automatic feature file discovery in common locations:
  - Current directory
  - Features/ folder
  - Specs/ folder
  - Tests/ folder
- Build-time scenario expansion with no runtime overhead
- Error handling for missing scenarios with warning comments
- Case-insensitive scenario and feature name matching
- Recursive subdirectory search for feature files
- Caching mechanism for feature file content
- Comprehensive unit test suite (38 tests)

### Technical Details
- Built on .NET Standard 2.0 for broad compatibility
- Implements IFeatureGenerator interface from Reqnroll
- Uses regular expressions for scenario call pattern matching
- Integrates with Reqnroll's plugin architecture
- Preserves original indentation in expanded scenarios

### Dependencies
- Reqnroll.CustomPlugin 3.0.1

---

## Version Numbering Strategy

This project follows a modified semantic versioning approach aligned with Reqnroll:

- **MAJOR.MINOR**: Matches the Reqnroll version (e.g., 3.0 for Reqnroll 3.0.x)
- **PATCH**: Independently managed for plugin-specific updates and fixes

This ensures compatibility with the corresponding Reqnroll version while allowing independent plugin updates.

---

## Release Notes Format

For each release, we document:

### Added
- New features and capabilities

### Changed
- Changes to existing functionality
- Breaking changes (clearly marked)

### Deprecated
- Features that will be removed in future versions

### Removed
- Features that have been removed

### Fixed
- Bug fixes and corrections

### Security
- Security-related improvements and fixes

---

## Version Numbering

This project follows a modified semantic versioning approach aligned with Reqnroll:

- **MAJOR.MINOR**: Matches the Reqnroll version (e.g., 3.0 for Reqnroll 3.0.x)  
- **PATCH**: Independently managed for plugin-specific updates and fixes

This ensures compatibility with the corresponding Reqnroll version while allowing independent plugin updates.

## Links

- [Unreleased]: https://github.com/jgerits/Reqnroll.ScenarioCall.Generator/compare/v3.0.0...HEAD
- [3.0.0]: https://github.com/jgerits/Reqnroll.ScenarioCall.Generator/releases/tag/v3.0.0