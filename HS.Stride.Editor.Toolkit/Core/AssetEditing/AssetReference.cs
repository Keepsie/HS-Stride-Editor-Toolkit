// HS Stride Editor Toolkit (c) 2025 Happenstance Games LLC - Apache License 2.0

namespace HS.Stride.Editor.Toolkit.Core.AssetEditing
{
    /// <summary>
    /// Represents a reference to a Stride asset (prefab, model, material, texture, etc)
    /// </summary>
    public class AssetReference
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public AssetType Type { get; set; }

        /// <summary>
        /// Returns the reference string used in scenes: "id:path"
        /// </summary>
        public string Reference => $"{Id}:{Path}";
    }

    
}
