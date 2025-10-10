// HS Stride Editor Toolkit (c) 2025 Happenstance Games LLC - Apache License 2.0

using HS.Stride.Editor.Toolkit.Core.StrideYamlParser;
using HS.Stride.Editor.Toolkit.Utilities;
using System.Collections.Generic;
using System.Linq;

namespace HS.Stride.Editor.Toolkit.Core
{
    /// <summary>
    /// Represents an editable Stride Material asset (.sdmat).
    /// </summary>
    public class MaterialAsset : IStrideAsset
    {
        private readonly Asset _material;

        private MaterialAsset(Asset material)
        {
            _material = material;
        }

        /// <summary>
        /// Loads a material asset from the specified file path.
        /// </summary>
        public static MaterialAsset Load(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException(nameof(filePath));

            var materialData = StrideYamlAssetParser.ParseAsset(filePath);
            return new MaterialAsset(materialData);
        }

        public string Id => _material.Id;
        public string FilePath => _material.FilePath;

        public string? GetDiffuseTexture()
        {
            var texture = Get("Attributes.Diffuse.DiffuseMap.Texture");
            return texture?.ToString();
        }

        public void SetDiffuseTexture(string textureReference)
        {
            Set("Attributes.Diffuse.DiffuseMap.Texture", textureReference);
        }

        public (float X, float Y)? GetUVScale()
        {
            if (Get("Attributes.Overrides.UVScale") is Dictionary<string, object> uvScale)
            {
                var x = uvScale.ContainsKey("X") ? Convert.ToSingle(uvScale["X"]) : 1.0f;
                var y = uvScale.ContainsKey("Y") ? Convert.ToSingle(uvScale["Y"]) : 1.0f;
                return (x, y);
            }
            return null;
        }

        public void SetUVScale(float x, float y)
        {
            var uvScale = new Dictionary<string, object>
            {
                ["X"] = x,
                ["Y"] = y
            };
            Set("Attributes.Overrides.UVScale", uvScale);
        }

        /// <summary>
        /// Gets a property value by name. Supports nested paths with dot notation.
        /// NOTE: Only properties saved in the .sdmat file (visible in Stride's Property Grid) can be accessed.
        /// e.g. "Attributes.Diffuse", "Attributes.DiffuseMap.Texture"
        /// </summary>
        public object? Get(string propertyName)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
                throw new ArgumentNullException(nameof(propertyName));

            return GetNestedProperty(_material.Properties, propertyName);
        }

        /// <summary>
        /// Sets a property value by name. Supports nested paths with dot notation.
        /// NOTE: Only properties that Stride serializes will persist when saved.
        /// e.g. Set("Attributes.Diffuse", new { R = 1.0, G = 0.5, B = 0.2, A = 1.0 })
        /// </summary>
        public void Set(string propertyName, object value)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
                throw new ArgumentNullException(nameof(propertyName));

            SetNestedProperty(_material.Properties, propertyName, value);
        }

        /// <summary>
        /// Gets all properties as a dictionary.
        /// </summary>
        public Dictionary<string, object> GetAllProperties()
        {
            return _material.Properties;
        }

        /// <summary>
        /// Saves the material's current state back to its original file.
        /// </summary>
        public void Save()
        {
            var yaml = StrideYamlAsset.GenerateAssetYaml(_material);
            FileHelper.SaveFile(yaml, _material.FilePath);
        }

        /// <summary>
        /// Saves the material's current state to a new file.
        /// </summary>
        public void SaveAs(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException(nameof(filePath));

            var yaml = StrideYamlAsset.GenerateAssetYaml(_material);
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
                        
                        // If we navigated into a dictionary with a type tag, unwrap it
                        // Type tags have keys starting with ! and empty values
                        if (current is Dictionary<string, object> nestedDict)
                        {
                            var typeTagKey = nestedDict.Keys.FirstOrDefault(k => k.StartsWith("!"));
                            if (typeTagKey != null)
                            {
                                // Continue navigation from the same dictionary (skip type tag)
                                current = nestedDict;
                            }
                        }
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
