using System;
using System.IO;

namespace Reqnroll.ScenarioCall.Generator.Tests
{
    /// <summary>
    /// Base class for tests that need to manage temporary directories and working directory isolation
    /// </summary>
    public abstract class TestWithTempDirectory : IDisposable
    {
        private readonly string _originalDirectory;
        private string? _tempDir;

        protected TestWithTempDirectory()
        {
            _originalDirectory = Environment.CurrentDirectory;
        }

        protected void SetupFeatureFileContent(string featureName, string content)
        {
            // Create a temporary feature file for testing in a safe location
            // Use the same temp directory for all calls during a test
            if (_tempDir == null)
            {
                _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
                Directory.CreateDirectory(_tempDir);
                var featuresDir = Path.Combine(_tempDir, "Features");
                Directory.CreateDirectory(featuresDir);
                
                // Set the current directory to the temp directory so the generator can find the files
                Environment.CurrentDirectory = _tempDir;
            }
            
            var featuresDirectory = Path.Combine(_tempDir, "Features");
            var featureFile = Path.Combine(featuresDirectory, $"{featureName}.feature");
            File.WriteAllText(featureFile, content);
        }

        public void Dispose()
        {
            // Clean up temp directory first
            if (_tempDir != null && Directory.Exists(_tempDir))
            {
                try
                {
                    Directory.Delete(_tempDir, true);
                }
                catch
                {
                    // Ignore cleanup errors
                }
            }
            
            // Restore original directory - but make sure it still exists
            try
            {
                if (Directory.Exists(_originalDirectory))
                {
                    Environment.CurrentDirectory = _originalDirectory;
                }
            }
            catch
            {
                // If we can't restore it, try to set to a safe default
                try
                {
                    Environment.CurrentDirectory = Path.GetTempPath();
                }
                catch
                {
                    // Ignore if all fails
                }
            }
        }
    }
}