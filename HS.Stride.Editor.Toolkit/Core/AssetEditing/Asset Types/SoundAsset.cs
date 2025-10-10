// HS Stride Editor Toolkit (c) 2025 Happenstance Games LLC - Apache License 2.0

using HS.Stride.Editor.Toolkit.Core.StrideYamlParser;
using HS.Stride.Editor.Toolkit.Utilities;
using System.Collections.Generic;

namespace HS.Stride.Editor.Toolkit.Core
{
    /// <summary>
    /// Represents an editable Stride Sound asset (.sdsnd).
    /// </summary>
    public class SoundAsset : IStrideAsset
    {
        private readonly Asset _sound;

        private SoundAsset(Asset sound)
        {
            _sound = sound;
        }

        /// <summary>
        /// Loads a sound asset from the specified file path.
        /// </summary>
        public static SoundAsset Load(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException(nameof(filePath));

            var soundData = StrideYamlAssetParser.ParseAsset(filePath);
            return new SoundAsset(soundData);
        }

        public string Id => _sound.Id;
        public string FilePath => _sound.FilePath;

        public object? Get(string propertyName)
        {
            return _sound.Properties.TryGetValue(propertyName, out var value) ? value : null;
        }

        public void Set(string propertyName, object value)
        {
            _sound.Properties[propertyName] = value;
        }

        public Dictionary<string, object> GetAllProperties()
        {
            return _sound.Properties;
        }

        /// <summary>
        /// Saves the sound's current state back to its original file.
        /// </summary>
        public void Save()
        {
            var yaml = StrideYamlAsset.GenerateAssetYaml(_sound);
            FileHelper.SaveFile(yaml, _sound.FilePath);
        }

        /// <summary>
        /// Saves the sound's current state to a new file.
        /// </summary>
        public void SaveAs(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException(nameof(filePath));

            var yaml = StrideYamlAsset.GenerateAssetYaml(_sound);
            FileHelper.SaveFile(yaml, filePath);
        }
    }
}
