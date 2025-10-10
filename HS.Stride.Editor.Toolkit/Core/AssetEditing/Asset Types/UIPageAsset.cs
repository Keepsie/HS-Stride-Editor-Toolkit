// HS Stride Editor Toolkit (c) 2025 Happenstance Games LLC - Apache License 2.0

using HS.Stride.Editor.Toolkit.Core.StrideYamlParser;
using HS.Stride.Editor.Toolkit.Utilities;

namespace HS.Stride.Editor.Toolkit.Core
{
    /// <summary>
    /// Represents an editable Stride UI Page asset (.sduipage).
    /// </summary>
    public class UIPageAsset : IStrideAsset
    {
        private readonly Asset _page;

        private UIPageAsset(Asset page)
        {
            _page = page;
        }

        /// <summary>
        /// Loads a UI page asset from the specified file path.
        /// </summary>
        public static UIPageAsset Load(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException(nameof(filePath));

            var pageData = StrideYamlAssetParser.ParseAsset(filePath);
            return new UIPageAsset(pageData);
        }

        public string Id => _page.Id;
        public string FilePath => _page.FilePath;

        /// <summary>
        /// Gets the design resolution.
        /// </summary>
        public (float X, float Y, float Z)? GetDesignResolution()
        {
            if (_page.Properties.ContainsKey("Design") &&
                _page.Properties["Design"] is Dictionary<string, object> design &&
                design.ContainsKey("Resolution") &&
                design["Resolution"] is Dictionary<string, object> res)
            {
                var x = res.ContainsKey("X") ? Convert.ToSingle(res["X"]) : 1280.0f;
                var y = res.ContainsKey("Y") ? Convert.ToSingle(res["Y"]) : 720.0f;
                var z = res.ContainsKey("Z") ? Convert.ToSingle(res["Z"]) : 1000.0f;
                return (x, y, z);
            }
            return null;
        }

        /// <summary>
        /// Sets the design resolution.
        /// </summary>
        public void SetDesignResolution(float x, float y, float z)
        {
            if (!_page.Properties.ContainsKey("Design"))
            {
                _page.Properties["Design"] = new Dictionary<string, object>();
            }

            if (_page.Properties["Design"] is Dictionary<string, object> design)
            {
                design["Resolution"] = new Dictionary<string, object>
                {
                    ["X"] = x,
                    ["Y"] = y,
                    ["Z"] = z
                };
            }
        }

        /// <summary>
        /// Gets a property value by name. Supports nested paths with dot notation.
        /// NOTE: Only properties saved in the .sduipage file (visible in Stride's Property Grid) can be accessed.
        /// e.g. "Design.Resolution", "Hierarchy.RootParts"
        /// </summary>
        public object? Get(string propertyName)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
                throw new ArgumentNullException(nameof(propertyName));

            return GetNestedProperty(_page.Properties, propertyName);
        }

        /// <summary>
        /// Sets a property value by name. Supports nested paths with dot notation.
        /// NOTE: Only properties that Stride serializes will persist when saved.
        /// </summary>
        public void Set(string propertyName, object value)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
                throw new ArgumentNullException(nameof(propertyName));

            SetNestedProperty(_page.Properties, propertyName, value);
        }

        /// <summary>
        /// Gets all properties as a dictionary.
        /// </summary>
        public Dictionary<string, object> GetAllProperties()
        {
            return _page.Properties;
        }

        /// <summary>
        /// Saves the UI page's current state back to its original file.
        /// </summary>
        public void Save()
        {
            var yaml = StrideYamlAsset.GenerateAssetYaml(_page);
            FileHelper.SaveFile(yaml, _page.FilePath);
        }

        /// <summary>
        /// Saves the UI page's current state to a new file.
        /// </summary>
        public void SaveAs(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException(nameof(filePath));

            var yaml = StrideYamlAsset.GenerateAssetYaml(_page);
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
