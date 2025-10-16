# Upgrade Guide: Version 3.1.4

## New Feature: Zero-Configuration Cross-Project Scenario References

Version 3.1.4 introduces **automatic convention-based exclusion** of reference-only feature files, eliminating the need for ANY configuration in most cases!

### Before (Version 3.1.3 and earlier)

Previously, when you wanted to reference feature files from another project (e.g., for scenario call expansion), you had to manually configure:

```xml
<ItemGroup>
  <!-- Mark as None to prevent Reqnroll from treating them as test features -->
  <None Include="Features\SharedAuth\*.feature">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    <Visible>true</Visible>
  </None>
  
  <!-- Exclude generated .cs files from compilation -->
  <Compile Remove="Features\SharedAuth\*.feature.cs" />
</ItemGroup>
```

This was verbose and error-prone.

### After (Version 3.1.4+)

**Option 1: Convention-Based (Recommended - Zero Configuration!)**

Simply place your reference-only feature files in folders matching these patterns:
- `Features/Shared*/` - e.g., `Features/SharedAuth/`
- `Features/Reference*/` - e.g., `Features/ReferenceScenarios/`
- `Features/*Lib/` - e.g., `Features/AuthLib/`

**No .csproj configuration needed!** The plugin automatically excludes these files from code generation.

**Option 2: Explicit Control**

If you need custom folder names, use the `ReqnrollFeatureReference` item type:

```xml
<ItemGroup>
  <ReqnrollFeatureReference Include="Features\MyCustomFolder\*.feature">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </ReqnrollFeatureReference>
</ItemGroup>
```

### What Changed?

The automatic convention-based exclusion:
1. **Detects** reference-only feature files based on folder naming patterns
2. **Excludes** them from Reqnroll code generation (no test duplication)
3. **Ensures** the files are copied to the output directory
4. **Makes** the files available for the ScenarioCall.Generator to read during expansion

No more manual configuration for the common case!

### Migration Steps

If you're already using the old approach:

**For Convention-Based Approach:**

1. **Rename** your folder to match a convention pattern (if not already):
   ```bash
   # If your folder doesn't contain "Shared", "Reference", or end with "Lib"
   mv Features/Auth Features/SharedAuth
   ```

2. **Remove** the old configuration from your .csproj:
   ```xml
   <!-- DELETE THIS -->
   <None Include="Features\SharedAuth\*.feature">
     <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
     <Visible>true</Visible>
   </None>
   <Compile Remove="Features\SharedAuth\*.feature.cs" />
   ```

3. **Delete** any generated `.feature.cs` files in the reference directory:
   ```bash
   rm Features/SharedAuth/*.feature.cs
   ```

4. **Rebuild** your project

**For Explicit Control:**

1. **Replace** your existing configuration:
   ```xml
   <!-- OLD - Remove this -->
   <None Include="Features\SharedAuth\*.feature">
     <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
     <Visible>true</Visible>
   </None>
   <Compile Remove="Features\SharedAuth\*.feature.cs" />
   ```

2. **With** the new simplified configuration:
   ```xml
   <!-- NEW - Use this instead -->
   <ReqnrollFeatureReference Include="Features\SharedAuth\*.feature">
     <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
   </ReqnrollFeatureReference>
   ```

3. **Delete** generated files and rebuild as above

### Compatibility

- The old approach still works in version 3.1.4
- You can migrate at your own pace
- No breaking changes
- Convention-based exclusion is enabled by default but can be disabled:
  ```xml
  <PropertyGroup>
    <ReqnrollAutoExcludeReferenceFeatures>false</ReqnrollAutoExcludeReferenceFeatures>
  </PropertyGroup>
  ```

### Example

See the updated [MSTestCrossProjectExample](examples/MSTestCrossProjectExample/) which now requires **zero configuration** in the .csproj file!

### Questions?

If you encounter any issues, please [open an issue](https://github.com/jgerits/Reqnroll.ScenarioCall.Generator/issues) on GitHub.
