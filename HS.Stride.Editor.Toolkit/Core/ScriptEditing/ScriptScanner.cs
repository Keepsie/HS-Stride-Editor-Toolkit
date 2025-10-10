// HS Stride Editor Toolkit (c) 2025 Happenstance Games LLC - Apache License 2.0

using System.Text.RegularExpressions;
using HS.Stride.Editor.Toolkit.Core.AssetEditing;

namespace HS.Stride.Editor.Toolkit.Core.ScriptEditing
{
    /// <summary>
    /// Scans C# script files in a Stride project to extract metadata for component creation
    /// </summary>
    public class ScriptScanner
    {
        /// <summary>
        /// Finds a script by name in the Stride project and extracts its metadata
        /// </summary>
        public static ScriptInfo? FindScript(StrideProject project, string scriptName)
        {
            if (project == null || string.IsNullOrEmpty(scriptName))
                return null;
            
            var scriptAsset = project.FindAsset(scriptName, AssetType.Script);
            if (scriptAsset == null)
                return null;
            
            var content = File.ReadAllText(scriptAsset.FilePath);
            return ParseScriptFile(scriptAsset.FilePath, content, scriptName);
        }

        private static ScriptInfo ParseScriptFile(string filePath, string content, string className)
        {
            var scriptInfo = new ScriptInfo
            {
                FilePath = filePath,
                ClassName = className
            };

            // Extract namespace
            var namespaceMatch = Regex.Match(content, @"namespace\s+([\w\.]+)");
            if (namespaceMatch.Success)
            {
                scriptInfo.Namespace = namespaceMatch.Groups[1].Value;
            }

            // Extract assembly name from directory structure
            // Typically: ProjectName/ProjectName.Game/ScriptName.cs
            // Assembly is ProjectName.Game
            var fileDir = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(fileDir))
            {
                // Check parent directories for .csproj files
                var currentDir = fileDir;
                while (!string.IsNullOrEmpty(currentDir))
                {
                    var csprojFiles = Directory.GetFiles(currentDir, "*.csproj");
                    if (csprojFiles.Length > 0)
                    {
                        var csprojName = Path.GetFileNameWithoutExtension(csprojFiles[0]);
                        scriptInfo.AssemblyName = csprojName;
                        break;
                    }
                    currentDir = Path.GetDirectoryName(currentDir);
                }
            }

            // Extract public fields and properties
            scriptInfo.PublicMembers = ExtractPublicMembers(content);

            return scriptInfo;
        }

        private static Dictionary<string, string> ExtractPublicMembers(string content)
        {
            var members = new Dictionary<string, string>();

            // Match public fields: public int health; or public int[] healthArray;
            // Also handles generics like List<int> and Dictionary<string, int>
            var fieldPattern = @"public\s+(\w+(?:<[\w\s,]+>)?(?:\[\])?)\s+(\w+)\s*(?:=|;)";
            var fieldMatches = Regex.Matches(content, fieldPattern);

            foreach (Match match in fieldMatches)
            {
                var typeName = match.Groups[1].Value;
                var fieldName = match.Groups[2].Value;

                // Skip properties, methods, classes
                if (!typeName.Equals("class", StringComparison.OrdinalIgnoreCase) &&
                    !typeName.Equals("void", StringComparison.OrdinalIgnoreCase))
                {
                    members[fieldName] = typeName;
                }
            }

            // Match public properties with { get; set; }: public int Health { get; set; }
            // Also handles arrays and generics
            var propertyPattern = @"public\s+(\w+(?:<[\w\s,]+>)?(?:\[\])?)\s+(\w+)\s*\{\s*get;";
            var propertyMatches = Regex.Matches(content, propertyPattern);

            foreach (Match match in propertyMatches)
            {
                var typeName = match.Groups[1].Value;
                var propertyName = match.Groups[2].Value;
                members[propertyName] = typeName;
            }

            return members;
        }
    }
}
