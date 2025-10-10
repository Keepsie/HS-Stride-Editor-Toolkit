// HS Stride Editor Toolkit (c) 2025 Happenstance Games LLC - Apache License 2.0

using HS.Stride.Editor.Toolkit.Core.StrideYamlParser;
using HS.Stride.Editor.Toolkit.Utilities;

namespace HS.Stride.Editor.Toolkit.Core
{
    /// <summary>
    /// Represents an editable Stride Texture asset (.sdtex).
    /// </summary>
    public class TextureAsset : IStrideAsset
    {
        private readonly Asset _texture;

        private TextureAsset(Asset texture)
        {
            _texture = texture;
        }

        /// <summary>
        /// Loads a texture asset from the specified file path.
        /// </summary>
        public static TextureAsset Load(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException(nameof(filePath));

            var textureData = StrideYamlAssetParser.ParseAsset(filePath);
            return new TextureAsset(textureData);
        }

        public string Id => _texture.Id;
        public string FilePath => _texture.FilePath;

        /// <summary>
        /// Gets the source file path for the texture.
        /// </summary>
        public string? GetSource()
        {
            var source = _texture.Properties.ContainsKey("Source")
                ? _texture.Properties["Source"].ToString()
                : null;

            // Remove "!file " prefix if present
            if (source?.StartsWith("!file ") == true)
            {
                return source.Substring(6);
            }
            return source;
        }

        /// <summary>
        /// Sets the source file path for the texture.
        /// </summary>
        public void SetSource(string sourcePath)
        {
            if (string.IsNullOrWhiteSpace(sourcePath))
                throw new ArgumentNullException(nameof(sourcePath));

            _texture.Properties["Source"] = $"!file {sourcePath}";
        }

        /// <summary>
        /// Gets whether the texture is streamable.
        /// </summary>
        public bool IsStreamable
        {
            get => _texture.Properties.ContainsKey("IsStreamable")
                ? Convert.ToBoolean(_texture.Properties["IsStreamable"])
                : false;
            set => _texture.Properties["IsStreamable"] = value;
        }

        /// <summary>
        /// Gets or sets whether to premultiply alpha (if Type is ColorTextureType).
        /// </summary>
        public bool? PremultiplyAlpha
        {
            get
            {
                // PremultiplyAlpha is nested under Type: !ColorTextureType
                var premultiply = GetNestedProperty(_texture.Properties, "Type.PremultiplyAlpha");
                if (premultiply != null)
                {
                    return Convert.ToBoolean(premultiply);
                }
                return null;
            }
            set
            {
                if (value.HasValue)
                {
                    SetNestedProperty(_texture.Properties, "Type.PremultiplyAlpha", value.Value);
                }
            }
        }

        /// <summary>
        /// Gets a property value by name. Supports nested paths with dot notation.
        /// NOTE: Only properties saved in the .sdtex file (visible in Stride's Property Grid) can be accessed.
        /// </summary>
        public object? Get(string propertyName)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
                throw new ArgumentNullException(nameof(propertyName));

            return GetNestedProperty(_texture.Properties, propertyName);
        }

        /// <summary>
        /// Sets a property value by name. Supports nested paths with dot notation.
        /// NOTE: Only properties that Stride serializes will persist when saved.
        /// </summary>
        public void Set(string propertyName, object value)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
                throw new ArgumentNullException(nameof(propertyName));

            SetNestedProperty(_texture.Properties, propertyName, value);
        }

        /// <summary>
        /// Gets all properties as a dictionary.
        /// </summary>
        public Dictionary<string, object> GetAllProperties()
        {
            return _texture.Properties;
        }

        /// <summary>
        /// Saves the texture's current state back to its original file.
        /// </summary>
        public void Save()
        {
            var yaml = StrideYamlAsset.GenerateAssetYaml(_texture);
            FileHelper.SaveFile(yaml, _texture.FilePath);
        }

        /// <summary>
        /// Saves the texture's current state to a new file.
        /// </summary>
        public void SaveAs(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException(nameof(filePath));

            var yaml = StrideYamlAsset.GenerateAssetYaml(_texture);
            FileHelper.SaveFile(yaml, filePath);
        }

        private object? GetNestedProperty(Dictionary<string, object> dict, string path)
        {
            var parts = path.Split('.');
            object? current = dict;

            foreach (var part in parts)
            {
                if (current is Dictionary<string, object> currentDict)
                {
                    if (currentDict.ContainsKey(part))
                    {
                        current = currentDict[part];
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }

            return current;
        }

        private void SetNestedProperty(Dictionary<string, object> dict, string path, object value)
        {
            var parts = path.Split('.');
            var current = dict;

            for (int i = 0; i < parts.Length - 1; i++)
            {
                var part = parts[i];
                if (!current.ContainsKey(part))
                {
                    current[part] = new Dictionary<string, object>();
                }

                if (current[part] is Dictionary<string, object> nextDict)
                {
                    current = nextDict;
                }
                else
                {
                    throw new InvalidOperationException($"Cannot navigate through non-dictionary property '{part}' in path '{path}'");
                }
            }

            current[parts[^1]] = value;
        }
    }
}
