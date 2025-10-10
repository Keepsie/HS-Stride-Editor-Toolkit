// HS Stride Editor Toolkit (c) 2025 Happenstance Games LLC - Apache License 2.0

using HS.Stride.Editor.Toolkit.Core.AssetEditing;

namespace HS.Stride.Editor.Toolkit.Core.DataTypes
{
    /// <summary>
    /// Represents an Asset reference in Stride YAML format (guid:path)
    /// Used for Prefab, Model, Material, Texture, RawAsset (UrlReference), etc.
    /// </summary>
    public class AssetRefData
    {
        public string Guid { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;

        /// <summary>
        /// Gets the YAML reference format: "guid:path"
        /// </summary>
        public string Reference => $"{Guid}:{Path}";

        /// <summary>
        /// Parses a YAML asset reference string into an AssetRef object
        /// </summary>
        public static AssetRefData? Parse(string? value)
        {
            if (string.IsNullOrEmpty(value) || value == "null")
                return null;

            var parts = value.Split(':');
            if (parts.Length != 2)
                return null;

            return new AssetRefData { Guid = parts[0], Path = parts[1] };
        }

        /// <summary>
        /// Resolves this reference to the actual AssetReference in the project
        /// </summary>
        public AssetReference? Resolve(StrideProject project) => project.FindAssetByGuid(Guid);
    }
}
