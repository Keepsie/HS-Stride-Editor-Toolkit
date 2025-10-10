// HS Stride Editor Toolkit (c) 2025 Happenstance Games LLC - Apache License 2.0

namespace HS.Stride.Editor.Toolkit.Utilities
{
    /// <summary>
    /// Helper methods for generating and working with GUIDs in Stride format
    /// </summary>
    public static class GuidHelper
    {
        /// <summary>
        /// Generates a new GUID string with dashes
        /// </summary>
        public static string NewGuid()
        {
            return Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Generates a new GUID string without dashes (used for component keys)
        /// </summary>
        public static string NewGuidNoDashes()
        {
            return Guid.NewGuid().ToString().Replace("-", "");
        }

        /// <summary>
        /// Extracts the Id field from YAML content, or generates a new GUID if not found
        /// </summary>
        public static string ExtractGuidFromYaml(string yamlContent)
        {
            var lines = yamlContent.Split('\n');
            var idLine = lines.FirstOrDefault(l => l.Trim().StartsWith("Id: "));
            return idLine?.Substring(idLine.IndexOf("Id: ") + 4).Trim() ?? NewGuid();
        }
    }
}
