// HS Stride Editor Toolkit (c) 2025 Happenstance Games LLC - Apache License 2.0

using HS.Stride.Editor.Toolkit.Core.StrideYamlParser;
using HS.Stride.Editor.Toolkit.Utilities;

namespace HS.Stride.Editor.Toolkit.Core
{
    /// <summary>
    /// Represents an editable Stride SpriteSheet asset (.sdsheet).
    /// </summary>
    public class SpriteSheetAsset : IStrideAsset
    {
        private readonly Asset _spriteSheet;

        private SpriteSheetAsset(Asset spriteSheet)
        {
            _spriteSheet = spriteSheet;
        }

        /// <summary>
        /// Loads a sprite sheet asset from the specified file path.
        /// </summary>
        public static SpriteSheetAsset Load(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException(nameof(filePath));

            var spriteSheetData = StrideYamlAssetParser.ParseAsset(filePath);
            return new SpriteSheetAsset(spriteSheetData);
        }

        public string Id => _spriteSheet.Id;
        public string FilePath => _spriteSheet.FilePath;

        /// <summary>
        /// Gets a property value by name. Supports nested paths with dot notation.
        /// NOTE: Only properties saved in the .sdsheet file (visible in Stride's Property Grid) can be accessed.
        /// e.g. "Sprites", "Packing.Enabled"
        /// </summary>
        public object? Get(string propertyName)
        {
            return _spriteSheet.Properties.ContainsKey(propertyName)
                ? _spriteSheet.Properties[propertyName]
                : null;
        }

        /// <summary>
        /// Sets a property value by name. Supports nested paths with dot notation.
        /// NOTE: Only properties that Stride serializes will persist when saved.
        /// e.g. Set("Packing.Enabled", true), Set("Sprites", spriteList)
        /// </summary>
        public void Set(string propertyName, object value)
        {
            _spriteSheet.Properties[propertyName] = value;
        }

        /// <summary>
        /// Gets all properties as a dictionary.
        /// </summary>
        public Dictionary<string, object> GetAllProperties()
        {
            return _spriteSheet.Properties;
        }

        /// <summary>
        /// Saves the sprite sheet's current state back to its original file.
        /// </summary>
        public void Save()
        {
            var yaml = StrideYamlAsset.GenerateAssetYaml(_spriteSheet);
            FileHelper.SaveFile(yaml, _spriteSheet.FilePath);
        }

        /// <summary>
        /// Saves the sprite sheet's current state to a new file.
        /// </summary>
        public void SaveAs(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException(nameof(filePath));

            var yaml = StrideYamlAsset.GenerateAssetYaml(_spriteSheet);
            FileHelper.SaveFile(yaml, filePath);
        }
    }
}
