// HS Stride Editor Toolkit (c) 2025 Happenstance Games LLC - Apache License 2.0

namespace HS.Stride.Editor.Toolkit.Utilities
{
    /// <summary>
    /// Helper methods for validating and working with Stride project paths
    /// </summary>
    public static class PathHelper
    {
        /// <summary>
        /// Checks if the specified directory is a valid Stride project
        /// </summary>
        public static bool IsStrideProject(string directoryPath)
        {
            var validation = ValidateStrideProject(directoryPath);
            return validation.IsValid;
        }

        /// <summary>
        /// Validates a Stride project directory and returns detailed validation results
        /// </summary>
        public static ProjectValidationResult ValidateStrideProject(string directoryPath)
        {
            var result = new ProjectValidationResult();

            try
            {
                if (string.IsNullOrEmpty(directoryPath) || !Directory.Exists(directoryPath))
                {
                    result.IsValid = false;
                    result.ErrorMessage = "Directory does not exist";
                    return result;
                }

                var slnFiles = Directory.GetFiles(directoryPath, "*.sln", SearchOption.TopDirectoryOnly);
                result.HasSolutionFile = slnFiles.Any();

                var sdpkgFiles = Directory.GetFiles(directoryPath, "*.sdpkg", SearchOption.AllDirectories);
                result.HasStridePackages = sdpkgFiles.Any();

                if (result.HasSolutionFile && result.HasStridePackages)
                {
                    result.IsValid = true;
                    result.SuccessMessage = "✓ Valid Stride project";
                }
                else if (!result.HasSolutionFile && !result.HasStridePackages)
                {
                    result.IsValid = false;
                    result.ErrorMessage = "❌ Not a Stride project root";
                }
                else if (!result.HasSolutionFile)
                {
                    result.IsValid = false;
                    result.ErrorMessage = "❌ Found Stride packages but no .sln file";
                }
                else
                {
                    result.IsValid = false;
                    result.ErrorMessage = "❌ Found .sln but no Stride packages";
                }
            }
            catch (Exception ex)
            {
                result.IsValid = false;
                result.ErrorMessage = $"Error validating project: {ex.Message}";
            }

            return result;
        }

        /// <summary>
        /// Checks if the file path points to a Stride scene file (.sdscene)
        /// </summary>
        public static bool IsSceneFile(string filePath)
        {
            return Path.GetExtension(filePath).ToLower() == ".sdscene";
        }

        /// <summary>
        /// Checks if the file path points to a Stride prefab file (.sdprefab)
        /// </summary>
        public static bool IsPrefabFile(string filePath)
        {
            return Path.GetExtension(filePath).ToLower() == ".sdprefab";
        }
    }

    /// <summary>
    /// Result of Stride project validation containing detailed information about the validation
    /// </summary>
    public class ProjectValidationResult
    {
        public bool IsValid { get; set; }
        public bool HasSolutionFile { get; set; }
        public bool HasStridePackages { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public string SuccessMessage { get; set; } = string.Empty;
    }
}
