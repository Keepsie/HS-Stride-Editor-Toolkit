// HS Stride Editor Toolkit (c) 2025 Happenstance Games LLC - Apache License 2.0

namespace HS.Stride.Editor.Toolkit.Core
{
    /// <summary>
    /// Generic data model for Stride assets (materials, textures, animations, etc.)
    /// </summary>
    public class Asset
    {
        /// <summary>
        /// The unique ID of the asset.
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// The file path on disk.
        /// </summary>
        public string FilePath { get; set; } = string.Empty;

        /// <summary>
        /// The raw YAML content of the asset file.
        /// </summary>
        public string RawContent { get; set; } = string.Empty;

        /// <summary>
        /// The asset type header (e.g., "!MaterialAsset", "!Texture").
        /// </summary>
        public string AssetTypeHeader { get; set; } = string.Empty;

        /// <summary>
        /// The serialized version of the asset.
        /// </summary>
        public string SerializedVersion { get; set; } = string.Empty;

        /// <summary>
        /// Tags associated with the asset.
        /// </summary>
        public List<string> Tags { get; set; } = new();

        /// <summary>
        /// All properties of the asset stored as key-value pairs.
        /// Values can be primitives, strings, or nested Dictionary&lt;string, object&gt;.
        /// </summary>
        public Dictionary<string, object> Properties { get; set; } = new();
    }
}
