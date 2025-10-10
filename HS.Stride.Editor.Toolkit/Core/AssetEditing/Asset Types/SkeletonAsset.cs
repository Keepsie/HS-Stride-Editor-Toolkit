// HS Stride Editor Toolkit (c) 2025 Happenstance Games LLC - Apache License 2.0

using HS.Stride.Editor.Toolkit.Core.StrideYamlParser;
using HS.Stride.Editor.Toolkit.Utilities;
using System.Collections.Generic;

namespace HS.Stride.Editor.Toolkit.Core
{
    /// <summary>
    /// Represents an editable Stride Skeleton asset (.sdskel).
    /// </summary>
    public class SkeletonAsset : IStrideAsset
    {
        private readonly Asset _skeleton;

        private SkeletonAsset(Asset skeleton)
        {
            _skeleton = skeleton;
        }

        /// <summary>
        /// Loads a skeleton asset from the specified file path.
        /// </summary>
        public static SkeletonAsset Load(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException(nameof(filePath));

            var skeletonData = StrideYamlAssetParser.ParseAsset(filePath);
            return new SkeletonAsset(skeletonData);
        }

        public string Id => _skeleton.Id;
        public string FilePath => _skeleton.FilePath;

        public string? GetSource()
        {
            if (_skeleton.Properties.TryGetValue("Source", out var source))
                return source.ToString().Replace("!file ", "");
            return null;
        }

        public void SetSource(string sourcePath)
        {
            _skeleton.Properties["Source"] = $"!file {sourcePath}";
        }

        public object? Get(string propertyName)
        {
            return _skeleton.Properties.TryGetValue(propertyName, out var value) ? value : null;
        }

        public void Set(string propertyName, object value)
        {
            _skeleton.Properties[propertyName] = value;
        }

        public Dictionary<string, object> GetAllProperties()
        {
            return _skeleton.Properties;
        }

        /// <summary>
        /// Saves the skeleton's current state back to its original file.
        /// </summary>
        public void Save()
        {
            var yaml = StrideYamlAsset.GenerateAssetYaml(_skeleton);
            FileHelper.SaveFile(yaml, _skeleton.FilePath);
        }

        /// <summary>
        /// Saves the skeleton's current state to a new file.
        /// </summary>
        public void SaveAs(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException(nameof(filePath));

            var yaml = StrideYamlAsset.GenerateAssetYaml(_skeleton);
            FileHelper.SaveFile(yaml, filePath);
        }
    }
}
