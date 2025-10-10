// HS Stride Editor Toolkit (c) 2025 Happenstance Games LLC - Apache License 2.0

namespace HS.Stride.Editor.Toolkit.Core
{
    /// <summary>
    /// Defines a common interface for reloadable, editable Stride assets like scenes and prefabs.
    /// </summary>
    public interface IStrideAsset
    {
        /// <summary>
        /// Gets the unique ID of the asset.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Gets the full file path of the asset on disk.
        /// </summary>
        string FilePath { get; }

        /// <summary>
        /// Saves the asset's current state back to its original file.
        /// </summary>
        void Save();

        /// <summary>
        /// Saves the asset's current state to a new file.
        /// </summary>
        /// <param name="filePath">The target file path to save to.</param>
        void SaveAs(string filePath);
    }
}
