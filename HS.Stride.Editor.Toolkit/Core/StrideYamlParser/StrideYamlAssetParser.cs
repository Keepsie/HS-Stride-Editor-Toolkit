// HS Stride Editor Toolkit (c) 2025 Happenstance Games LLC - Apache License 2.0

namespace HS.Stride.Editor.Toolkit.Core.StrideYamlParser
{
    /// <summary>
    /// YAML parser for Stride assets (materials, textures, animations, etc.)
    /// Consolidated into StrideYamlParser namespace for better organization.
    /// </summary>
    public static class StrideYamlAssetParser
    {
        /// <summary>
        /// Parses a Stride asset file into an AssetData object.
        /// </summary>
        public static Asset ParseAsset(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Asset file not found: {filePath}");

            var content = File.ReadAllText(filePath);
            return ParseAssetContent(content, filePath);
        }

        /// <summary>
        /// Parses asset content from a string.
        /// </summary>
        public static Asset ParseAssetContent(string content, string filePath)
        {
            var asset = new Asset
            {
                FilePath = filePath,
                RawContent = content
            };

            var lines = content.Split('\n');

            // Parse header and basic properties
            for (int i = 0; i < lines.Length && i < 10; i++)
            {
                var line = lines[i].Trim();

                // Asset type header (e.g., "!MaterialAsset", "!Texture")
                if (line.StartsWith("!"))
                {
                    asset.AssetTypeHeader = line;
                }
                // ID
                else if (line.StartsWith("Id: "))
                {
                    asset.Id = line.Substring(4).Trim();
                }
                // Serialized version
                else if (line.StartsWith("SerializedVersion: "))
                {
                    asset.SerializedVersion = line.Substring(19).Trim();
                }
                // Tags (usually empty array)
                else if (line.StartsWith("Tags: "))
                {
                    var tagsContent = line.Substring(6).Trim();
                    if (tagsContent != "[]")
                    {
                        // Parse tags if not empty
                        asset.Tags = tagsContent.Trim('[', ']').Split(',')
                            .Select(t => t.Trim())
                            .Where(t => !string.IsNullOrEmpty(t))
                            .ToList();
                    }
                }
            }

            // Parse all properties
            asset.Properties = ParseProperties(lines);

            return asset;
        }

        /// <summary>
        /// Parses all properties from the asset file.
        /// </summary>
        private static Dictionary<string, object> ParseProperties(string[] lines)
        {
            var properties = new Dictionary<string, object>();
            var contextStack = new Stack<(Dictionary<string, object> dict, int indent)>();
            contextStack.Push((properties, -1)); // Root context

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                var indent = GetIndentLevel(line);
                var trimmed = line.Trim();

                // Skip header/metadata lines
                if (trimmed.StartsWith("!") && !trimmed.Contains(": ")) continue;
                if (trimmed.StartsWith("Id: ")) continue;
                if (trimmed.StartsWith("SerializedVersion: ")) continue;
                if (trimmed.StartsWith("Tags: ")) continue;
                if (trimmed.StartsWith("~")) continue;

                // Adjust context based on indentation
                while (indent <= contextStack.Peek().indent)
                {
                    contextStack.Pop();
                }

                var parentDict = contextStack.Peek().dict;

                if (trimmed.Contains(":"))
                {
                    var colonIndex = trimmed.IndexOf(":");
                    var key = trimmed.Substring(0, colonIndex).Trim();
                    var value = colonIndex + 1 < trimmed.Length ? trimmed.Substring(colonIndex + 1).Trim() : string.Empty;

                    if (string.IsNullOrEmpty(value))
                    {
                        // This is a new nested dictionary
                        var newDict = new Dictionary<string, object>();
                        parentDict[key] = newDict;
                        contextStack.Push((newDict, indent));
                    }
                    else if (value.StartsWith("!") && !value.Contains(" "))
                    {
                        // Type tag (e.g., "DiffuseMap: !ComputeTextureColor")
                        // Create nested dictionary with type tag as a key
                        var newDict = new Dictionary<string, object>();
                        newDict[value] = string.Empty; // Type tag with empty value
                        parentDict[key] = newDict;
                        contextStack.Push((newDict, indent));
                    }
                    else
                    {
                        parentDict[key] = ParsePropertyValue(value);
                    }
                }
            }

            return properties;
        }

        /// <summary>
        /// Gets the indentation level of a line.
        /// </summary>
        private static int GetIndentLevel(string line)
        {
            int spaces = 0;
            foreach (char c in line)
            {
                if (c == ' ') spaces++;
                else if (c == '\t') spaces += 4;
                else break;
            }
            return spaces;
        }

        /// <summary>
        /// Parses a property value (handles primitives, vectors, references, etc.)
        /// </summary>
        private static object ParsePropertyValue(string value)
        {
            value = value.Trim();

            // Empty or null
            if (string.IsNullOrEmpty(value) || value == "null")
                return value;

            // Dictionary/Object (Vector3, Quaternion, Color, etc.)
            if (value.StartsWith("{") && value.EndsWith("}"))
            {
                var props = new Dictionary<string, object>();
                var content = value.Trim('{', '}').Trim();

                if (string.IsNullOrEmpty(content))
                    return props;

                var parts = content.Split(',');

                foreach (var part in parts)
                {
                    var colonIndex = part.IndexOf(':');
                    if (colonIndex > 0)
                    {
                        var key = part.Substring(0, colonIndex).Trim();
                        var val = part.Substring(colonIndex + 1).Trim();

                        // Try parse as number
                        if (float.TryParse(val, System.Globalization.NumberStyles.Float,
                            System.Globalization.CultureInfo.InvariantCulture, out float floatVal))
                        {
                            props[key] = floatVal;
                        }
                        else if (int.TryParse(val, out int intVal))
                        {
                            props[key] = intVal;
                        }
                        else if (bool.TryParse(val, out bool boolVal))
                        {
                            props[key] = boolVal;
                        }
                        else
                        {
                            props[key] = val;
                        }
                    }
                }
                return props;
            }

            // Asset reference (guid:path)
            if (value.Contains(":") && !value.StartsWith("!"))
            {
                var colonIndex = value.IndexOf(':');
                var beforeColon = value.Substring(0, colonIndex);
                if (Guid.TryParse(beforeColon, out _))
                {
                    return value; // Keep as string
                }
            }

            // File reference
            if (value.StartsWith("!file "))
            {
                return value;
            }

            // Type descriptors (e.g., "!MaterialDiffuseMapFeature")
            if (value.StartsWith("!"))
            {
                return value;
            }

            // Boolean
            if (bool.TryParse(value, out bool bVal))
                return bVal;

            // Float/Double (including scientific notation)
            if (float.TryParse(value, System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out float fVal))
                return fVal;

            // Int
            if (int.TryParse(value, out int iVal))
                return iVal;

            // TimeSpan format (e.g., "0:00:00:00.0000000")
            if (value.Contains(":") && value.Split(':').Length >= 3)
            {
                return value; // Keep as string
            }

            // String/default
            return value;
        }
    }
}
