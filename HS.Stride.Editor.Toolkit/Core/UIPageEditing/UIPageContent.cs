// HS Stride Editor Toolkit (c) 2025 Happenstance Games LLC - Apache License 2.0

namespace HS.Stride.Editor.Toolkit.Core.UIPageEditing
{
    /// <summary>
    /// Internal representation of a UI page's content after parsing
    /// </summary>
    internal class UIPageContent
    {
        public string Id { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;

        /// <summary>
        /// Design resolution (X, Y, Z)
        /// </summary>
        public Dictionary<string, float> Resolution { get; set; } = new()
        {
            { "X", 1280.0f },
            { "Y", 720.0f },
            { "Z", 1000.0f }
        };

        /// <summary>
        /// All UI elements in the page
        /// </summary>
        public List<UIElement> Elements { get; set; } = new();

        /// <summary>
        /// Root UI element IDs (elements at the top level of the hierarchy)
        /// </summary>
        public List<string> RootElementIds { get; set; } = new();

        /// <summary>
        /// Raw YAML content (for preservation of unmodified sections)
        /// </summary>
        public string RawContent { get; set; } = string.Empty;

        /// <summary>
        /// All properties of the UI page stored as key-value pairs for generic access.
        /// This enables compatibility with UIPageAsset's Get/Set API.
        /// </summary>
        public Dictionary<string, object> Properties { get; set; } = new();

        /// <summary>
        /// Parent StrideProject (enables path resolution and validation)
        /// </summary>
        public StrideProject? ParentProject { get; set; }
    }
}
