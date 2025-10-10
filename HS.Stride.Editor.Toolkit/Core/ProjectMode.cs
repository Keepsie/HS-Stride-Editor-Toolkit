// HS Stride Editor Toolkit (c) 2025 Happenstance Games LLC - Apache License 2.0

namespace HS.Stride.Editor.Toolkit.Core
{
    /// <summary>
    /// Defines how the toolkit handles property validation
    /// </summary>
    public enum ProjectMode
    {
        /// <summary>
        /// Strict mode (default) - throws exceptions when setting properties that don't exist in the script.
        /// Helps catch typos and ensures all properties are valid.
        /// </summary>
        Strict = 0,

        /// <summary>
        /// Loose mode - allows setting any property on components.
        /// Properties that don't exist in the script may not be recognized by Stride.
        /// Use when you need maximum flexibility or dynamic properties.
        /// </summary>
        Loose = 1
    }
}
