// HS Stride Editor Toolkit (c) 2025 Happenstance Games LLC - Apache License 2.0

using HS.Stride.Editor.Toolkit.Core.StrideYamlParser;
using HS.Stride.Editor.Toolkit.Utilities;
using System.Collections.Generic;

namespace HS.Stride.Editor.Toolkit.Core
{
    /// <summary>
    /// Represents an editable Stride Effect asset (.sdeffect).
    /// </summary>
    public class EffectAsset : IStrideAsset
    {
        private readonly Asset _effect;

        private EffectAsset(Asset effect)
        {
            _effect = effect;
        }

        /// <summary>
        /// Loads an effect asset from the specified file path.
        /// </summary>
        public static EffectAsset Load(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException(nameof(filePath));

            var effectData = StrideYamlAssetParser.ParseAsset(filePath);
            return new EffectAsset(effectData);
        }

        public string Id => _effect.Id;
        public string FilePath => _effect.FilePath;

        public object? Get(string propertyName)
        {
            return _effect.Properties.TryGetValue(propertyName, out var value) ? value : null;
        }

        public void Set(string propertyName, object value)
        {
            _effect.Properties[propertyName] = value;
        }

        public Dictionary<string, object> GetAllProperties()
        {
            return _effect.Properties;
        }

        /// <summary>
        /// Saves the effect's current state back to its original file.
        /// </summary>
        public void Save()
        {
            var yaml = StrideYamlAsset.GenerateAssetYaml(_effect);
            FileHelper.SaveFile(yaml, _effect.FilePath);
        }

        /// <summary>
        /// Saves the effect's current state to a new file.
        /// </summary>
        public void SaveAs(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException(nameof(filePath));

            var yaml = StrideYamlAsset.GenerateAssetYaml(_effect);
            FileHelper.SaveFile(yaml, filePath);
        }
    }
}
