// HS Stride Editor Toolkit (c) 2025 Happenstance Games LLC - Apache License 2.0

namespace HS.Stride.Editor.Toolkit.Utilities
{
    /// <summary>
    /// Represents a namespace found in Stride asset files with references to where it was found
    /// </summary>
    public class NamespaceReference
    {
        public string Namespace { get; set; } = string.Empty;
        public List<string> FoundInFiles { get; set; } = new();
    }

}
