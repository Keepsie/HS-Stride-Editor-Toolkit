using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HS.Stride.Editor.Toolkit.Core;

namespace HS.Stride.Editor.Toolkit.Core.StrideYamlParser
{
    /// <summary>
    /// Generates YAML for generic Stride assets (materials, textures, animations, etc.).
    /// Handles header (type tag, ID, version, tags) and properties.
    /// </summary>
    public class StrideYamlAsset : StrideYaml
    {
        /// <summary>
        /// Generates complete asset YAML from Asset object.
        /// </summary>
        public static string GenerateAssetYaml(Asset asset)
        {
            var sb = new StringBuilder();

            // Write asset header
            WriteAssetHeader(sb, asset);

            // Write all properties
            WriteProperties(sb, asset);

            return sb.ToString();
        }

        /// <summary>
        /// Writes the asset header: type tag (!MaterialAsset, !Texture, etc.), ID, version, tags.
        /// </summary>
        private static void WriteAssetHeader(StringBuilder sb, Asset asset)
        {
            // Asset type header (e.g., "!MaterialAsset", "!Texture")
            sb.AppendLine(asset.AssetTypeHeader);

            // ID
            sb.AppendLine($"Id: {asset.Id}");

            // Serialized version
            if (!string.IsNullOrEmpty(asset.SerializedVersion))
            {
                sb.AppendLine($"SerializedVersion: {asset.SerializedVersion}");
            }

            // Tags
            if (asset.Tags.Count > 0)
            {
                sb.AppendLine($"Tags: [{string.Join(", ", asset.Tags)}]");
            }
            else
            {
                sb.AppendLine("Tags: []");
            }
        }

        /// <summary>
        /// Writes all asset properties.
        /// </summary>
        private static void WriteProperties(StringBuilder sb, Asset asset)
        {
            foreach (var prop in asset.Properties)
            {
                WriteProperty(sb, prop.Key, prop.Value, 0);
            }
        }

        /// <summary>
        /// Writes a single property with proper formatting based on its type.
        /// Handles primitives, dictionaries, lists, and nested structures.
        /// </summary>
        private static void WriteProperty(StringBuilder sb, string key, object? value, int indent)
        {
            if (value == null)
            {
                sb.AppendLine($"{Indent(indent)}{key}: null");
                return;
            }

            // Handle dictionaries
            if (value is Dictionary<string, object> dict)
            {
                WriteDictionary(sb, key, dict, indent);
                return;
            }

            // Handle lists
            if (value is System.Collections.IList list)
            {
                WriteList(sb, key, list, indent);
                return;
            }

            // Handle primitives and strings
            sb.AppendLine($"{Indent(indent)}{key}: {FormatValue(value)}");
        }

        /// <summary>
        /// Writes a dictionary property.
        /// Handles inline format {X: 0.0, Y: 0.0} and nested format with type tags.
        /// </summary>
        private static void WriteDictionary(StringBuilder sb, string key, Dictionary<string, object> dict, int indent)
        {
            // Empty dictionary
            if (dict.Count == 0)
            {
                sb.AppendLine($"{Indent(indent)}{key}: {{}}");
                return;
            }

            // Check for type tag (key starting with "!")
            var typeTag = dict.Keys.FirstOrDefault(k => k.StartsWith("!"));

            if (typeTag != null)
            {
                // Format with type tag: "DiffuseMap: !ComputeTextureColor"
                sb.AppendLine($"{Indent(indent)}{key}: {typeTag}");

                // Write remaining properties (excluding the type tag itself)
                foreach (var kvp in dict.Where(kvp => kvp.Key != typeTag))
                {
                    WriteProperty(sb, kvp.Key, kvp.Value, indent + 1);
                }
            }
            else if (IsSimpleValueDict(dict))
            {
                // Inline format for simple dicts: {X: 0.0, Y: 0.0, Z: 0.0}
                var items = dict.Select(kvp => $"{kvp.Key}: {FormatValue(kvp.Value)}");
                sb.AppendLine($"{Indent(indent)}{key}: {{{string.Join(", ", items)}}}");
            }
            else
            {
                // Nested dictionary format
                sb.AppendLine($"{Indent(indent)}{key}:");
                foreach (var kvp in dict)
                {
                    WriteProperty(sb, kvp.Key, kvp.Value, indent + 1);
                }
            }
        }

        /// <summary>
        /// Writes a list/array property.
        /// </summary>
        private static void WriteList(StringBuilder sb, string key, System.Collections.IList list, int indent)
        {
            // Empty list
            if (list.Count == 0)
            {
                sb.AppendLine($"{Indent(indent)}{key}: []");
                return;
            }

            sb.AppendLine($"{Indent(indent)}{key}:");

            foreach (var item in list)
            {
                if (item is Dictionary<string, object> dictItem)
                {
                    // Complex list item
                    WriteDictionaryListItem(sb, dictItem, indent + 1);
                }
                else
                {
                    // Simple list item
                    sb.AppendLine($"{Indent(indent + 1)}- {FormatValue(item)}");
                }
            }
        }

        /// <summary>
        /// Writes a dictionary that appears as a list item.
        /// </summary>
        private static void WriteDictionaryListItem(StringBuilder sb, Dictionary<string, object> dict, int indent)
        {
            bool first = true;

            foreach (var kvp in dict)
            {
                if (first)
                {
                    sb.Append($"{Indent(indent)}- ");
                    first = false;
                }
                else
                {
                    sb.Append($"{Indent(indent + 1)}");
                }

                if (kvp.Value is Dictionary<string, object> nestedDict)
                {
                    sb.Append($"{kvp.Key}:");
                    if (IsSimpleValueDict(nestedDict))
                    {
                        var items = nestedDict.Select(nkvp => $"{nkvp.Key}: {FormatValue(nkvp.Value)}");
                        sb.AppendLine($" {{{string.Join(", ", items)}}}");
                    }
                    else
                    {
                        sb.AppendLine();
                        foreach (var nkvp in nestedDict)
                        {
                            WriteProperty(sb, nkvp.Key, nkvp.Value, indent + 2);
                        }
                    }
                }
                else
                {
                    sb.AppendLine($"{kvp.Key}: {FormatValue(kvp.Value)}");
                }
            }
        }
    }
}
