// HS Stride Editor Toolkit (c) 2025 Happenstance Games LLC - Apache License 2.0

namespace HS.Stride.Editor.Toolkit.Core.ScriptEditing
{
    /// <summary>
    /// Contains metadata about a C# script component extracted from source code
    /// </summary>
    public class ScriptInfo
    {
        /// <summary>
        /// The namespace the script is in (e.g., "TopDownRPG")
        /// </summary>
        public string Namespace { get; set; } = string.Empty;

        /// <summary>
        /// The class name (e.g., "Tester")
        /// </summary>
        public string ClassName { get; set; } = string.Empty;

        /// <summary>
        /// The assembly name (e.g., "TopDownRPG.Game")
        /// </summary>
        public string AssemblyName { get; set; } = string.Empty;

        /// <summary>
        /// Full path to the script file
        /// </summary>
        public string FilePath { get; set; } = string.Empty;

        /// <summary>
        /// Public properties/fields that Stride would serialize
        /// Key: property name, Value: type name
        /// </summary>
        public Dictionary<string, string> PublicMembers { get; set; } = new();

        /// <summary>
        /// Gets the full type tag for YAML (e.g., "TopDownRPG.Tester,TopDownRPG.Game" or ".NoNameSpace,TopDownRPG.Game")
        /// </summary>
        public string GetFullTypeName()
        {
            if (string.IsNullOrEmpty(Namespace))
                return $".{ClassName},{AssemblyName}";  // Leading dot when no namespace

            return $"{Namespace}.{ClassName},{AssemblyName}";
        }
    }
}
