# Upgrade Guide: Version 3.1.4

## New Feature: Simplified Cross-Project Scenario References

Version 3.1.4 introduces the `ReqnrollFeatureReference` item type to simplify the configuration needed when referencing feature files from other projects.

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

Now you can simply use the `ReqnrollFeatureReference` item type:

```xml
<ItemGroup>
  <ReqnrollFeatureReference Include="Features\SharedAuth\*.feature">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </ReqnrollFeatureReference>
</ItemGroup>
```

### What Changed?

The `ReqnrollFeatureReference` item type automatically:
1. Excludes the feature files from Reqnroll code generation (no test duplication)
2. Ensures the files are copied to the output directory
3. Makes the files available for the ScenarioCall.Generator to read during expansion

No more manual `<Compile Remove>` needed!

### Migration Steps

If you're already using the old approach, you can simply:

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

3. **Delete** any generated `.feature.cs` files in the reference directory:
   ```bash
   rm Features/SharedAuth/*.feature.cs
   ```

4. **Rebuild** your project

### Compatibility

- The old approach still works in version 3.1.4
- You can migrate at your own pace
- No breaking changes

### Example

See the updated [MSTestCrossProjectExample](examples/MSTestCrossProjectExample/) for a complete working example.

### Questions?

If you encounter any issues, please [open an issue](https://github.com/jgerits/Reqnroll.ScenarioCall.Generator/issues) on GitHub.
