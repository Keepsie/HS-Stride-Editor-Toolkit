// HS Stride Editor Toolkit (c) 2025 Happenstance Games LLC - Apache License 2.0

namespace HS.Stride.Editor.Toolkit.Utilities
{
    /// <summary>
    /// Represents the detected structure of a Stride project (Fresh vs Template layout)
    /// </summary>
    public class TargetProjectStructure
    {
        public ProjectStructureType Type { get; set; }
        public string AssetsPath { get; set; } = string.Empty;
        public string ResourcesPath { get; set; } = string.Empty;
        public string CodePath { get; set; } = string.Empty;
    }

    /// <summary>
    /// Types of Stride project structures
    /// </summary>
    public enum ProjectStructureType
    {
        Unknown,
        Fresh,
        Template
    }

    /// <summary>
    /// Detects the structure type of a Stride project
    /// </summary>
    public static class ProjectStructureDetector
    {
        /// <summary>
        /// Detects the project structure and returns path information
        /// </summary>
        public static TargetProjectStructure DetectTargetProjectStructure(string projectPath)
        {
            var projectName = Path.GetFileName(projectPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));

            var nestedAssetsPath = Path.Combine(projectPath, projectName, "Assets");
            if (Directory.Exists(nestedAssetsPath))
            {
                return new TargetProjectStructure
                {
                    Type = ProjectStructureType.Fresh,
                    AssetsPath = Path.Combine(projectName, "Assets"),
                    ResourcesPath = Path.Combine(projectName, "Resources"),
                    CodePath = projectName
                };
            }

            var rootAssetsPath = Path.Combine(projectPath, "Assets");
            if (Directory.Exists(rootAssetsPath))
            {
                return new TargetProjectStructure
                {
                    Type = ProjectStructureType.Template,
                    AssetsPath = "Assets",
                    ResourcesPath = "Resources",
                    CodePath = DetermineTemplateCodePath(projectPath)
                };
            }

            return new TargetProjectStructure
            {
                Type = ProjectStructureType.Template,
                AssetsPath = "Assets",
                ResourcesPath = "Resources",
                CodePath = DetermineTemplateCodePath(projectPath)
            };
        }

        private static string DetermineTemplateCodePath(string projectPath)
        {
            var gameFolder = Directory.GetDirectories(projectPath, "*.Game", SearchOption.TopDirectoryOnly)
                                    .FirstOrDefault();

            if (gameFolder != null)
            {
                return Path.GetFileName(gameFolder);
            }

            return "";
        }
    }
}
