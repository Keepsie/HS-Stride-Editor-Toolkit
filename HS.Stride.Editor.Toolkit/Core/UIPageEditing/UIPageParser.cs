// HS Stride Editor Toolkit (c) 2025 Happenstance Games LLC - Apache License 2.0

using HS.Stride.Editor.Toolkit.Core.StrideYamlParser;

namespace HS.Stride.Editor.Toolkit.Core.UIPageEditing
{
    /// <summary>
    /// Parses Stride UI page (.sduipage) YAML files
    /// </summary>
    internal static class UIPageParser
    {
        public static UIPageContent Parse(string filePath)
        {
            var lines = File.ReadAllLines(filePath);
            var content = new UIPageContent
            {
                FilePath = filePath,
                RawContent = string.Join("\n", lines)
            };

            // Parse generic properties for Get/Set API compatibility
            try
            {
                var asset = StrideYamlAssetParser.ParseAsset(filePath);
                content.Properties = asset.Properties;
            }
            catch
            {
                // If generic parsing fails, continue with structured parsing only
                content.Properties = new Dictionary<string, object>();
            }

            // Parse header
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];

                // Page ID
                if (line.StartsWith("Id:"))
                {
                    content.Id = line.Substring(3).Trim();
                }

                // Design Resolution
                if (line.Trim() == "Design:")
                {
                    // Next line should be Resolution
                    if (i + 1 < lines.Length && lines[i + 1].Contains("Resolution:"))
                    {
                        var resLine = lines[i + 1];
                        // Parse: "    Resolution: {X: 1280.0, Y: 720.0, Z: 1000.0}"
                        var startIdx = resLine.IndexOf('{');
                        var endIdx = resLine.IndexOf('}');
                        if (startIdx > 0 && endIdx > startIdx)
                        {
                            var resData = resLine.Substring(startIdx + 1, endIdx - startIdx - 1);
                            var parts = resData.Split(',');
                            foreach (var part in parts)
                            {
                                var kv = part.Split(':');
                                if (kv.Length == 2)
                                {
                                    var key = kv[0].Trim();
                                    var value = float.Parse(kv[1].Trim());
                                    content.Resolution[key] = value;
                                }
                            }
                        }
                    }
                }

                // Hierarchy section
                if (line.Trim() == "Hierarchy:")
                {
                    ParseHierarchy(lines, i + 1, content);
                    break;
                }
            }

            // Build parent-child relationships
            BuildHierarchy(content);

            return content;
        }

        private static void ParseHierarchy(string[] lines, int startIndex, UIPageContent content)
        {
            int i = startIndex;

            // State-based parsing: find RootParts, then Parts
            bool foundRootParts = false;
            bool foundParts = false;

            while (i < lines.Length && !foundParts)
            {
                var line = lines[i].Trim();

                // Parse RootParts section
                if (line == "RootParts:")
                {
                    foundRootParts = true;
                    i++;
                    // Parse root element references
                    while (i < lines.Length && lines[i].StartsWith("        -"))
                    {
                        var rootLine = lines[i].Trim();
                        // Format: "- !Grid ref!! guid"
                        var parts = rootLine.Split(new[] { "ref!!" }, StringSplitOptions.None);
                        if (parts.Length == 2)
                        {
                            var rootId = parts[1].Trim();
                            content.RootElementIds.Add(rootId);
                        }
                        i++;
                    }
                    // Continue to find Parts section (don't increment i here, already advanced)
                    continue;
                }

                // Parse Parts section
                if (line == "Parts:")
                {
                    foundParts = true;
                    i++;
                    // Parse all UI elements
                    while (i < lines.Length)
                    {
                        if (lines[i].Trim().StartsWith("-   UIElement:"))
                        {
                            i = ParseUIElement(lines, i, content);
                        }
                        else
                        {
                            i++;
                        }
                    }
                    break;
                }

                i++;
            }
        }

        private static int ParseUIElement(string[] lines, int startIndex, UIPageContent content)
        {
            int i = startIndex;
            var headerLine = lines[i].Trim();

            // Extract type: "-   UIElement: !Grid" or "-   UIElement: !TextBlock"
            var typeParts = headerLine.Split('!');
            if (typeParts.Length < 2)
                return i + 1;

            var type = typeParts[1].Trim();
            var element = new UIElement { Type = type };

            // Capture raw YAML for lazy loading
            var elementRawYaml = new List<string>();
            i++;

            // Find the first content line to determine base indent
            int? baseIndent = null;
            while (i < lines.Length && !baseIndent.HasValue)
            {
                var line = lines[i];
                if (!string.IsNullOrWhiteSpace(line) && !line.Trim().StartsWith("-   UIElement:"))
                {
                    baseIndent = GetIndent(line);
                    break;
                }
                i++;
            }

            if (!baseIndent.HasValue)
                return i;

            // Parse element properties
            while (i < lines.Length)
            {
                var line = lines[i];
                var indent = GetIndent(line);
                var trimmed = line.Trim();

                // Check if we've moved to next element (at same level as "-   UIElement:")
                if (trimmed.StartsWith("-   UIElement:"))
                    break;

                // Stop if indent goes back to Parts level or less (but allow empty lines)
                if (!string.IsNullOrWhiteSpace(line) && indent < baseIndent.Value)
                    break;

                elementRawYaml.Add(line);

                // Parse basic properties
                if (trimmed.StartsWith("Id:"))
                {
                    element.Id = trimmed.Substring(3).Trim();
                }
                else if (trimmed.StartsWith("Name:"))
                {
                    element.Name = trimmed.Substring(5).Trim();
                }
                else if (trimmed.Contains(":") && !trimmed.StartsWith("-"))
                {
                    // Store raw property (will be parsed on-demand)
                    var colonIdx = trimmed.IndexOf(':');
                    var key = trimmed.Substring(0, colonIdx).Trim();
                    var value = trimmed.Substring(colonIdx + 1).Trim();

                    // Parse inline object format: {Key: Value, Key2: Value2}
                    if (!string.IsNullOrEmpty(value) && value.StartsWith("{") && value.EndsWith("}"))
                    {
                        var parsed = ParseInlineObject(value);
                        if (parsed != null)
                        {
                            element.Properties[key] = parsed;
                        }
                    }
                    // Parse reference format: !Type ref!! guid
                    else if (!string.IsNullOrEmpty(value) && value.Contains("ref!!"))
                    {
                        element.Properties[key] = value;
                    }
                    // Handle YAML type tags that start multiline blocks (like !SpriteFromSheet)
                    else if (!string.IsNullOrEmpty(value) && value.StartsWith("!"))
                    {
                        // Parse multiline block property
                        var blockData = ParseMultilineBlock(lines, ref i, baseIndent.Value);
                        if (blockData != null)
                        {
                            blockData["!TypeTag"] = value; // Store the type tag (e.g., !SpriteFromSheet)
                            element.Properties[key] = blockData;
                        }
                        continue; // ParseMultilineBlock already advanced i
                    }
                    // Handle DependencyProperties multiline block (empty value means multiline follows)
                    else if (key == "DependencyProperties" && string.IsNullOrEmpty(value))
                    {
                        var depProps = ParseDependencyProperties(lines, ref i, baseIndent.Value);
                        if (depProps != null && depProps.Count > 0)
                        {
                            element.Properties[key] = depProps;
                        }
                        continue; // ParseDependencyProperties already advanced i
                    }
                    // Basic property storage
                    else if (!string.IsNullOrEmpty(value))
                    {
                        element.Properties[key] = UnquoteYamlString(value);
                    }
                }

                i++;
            }

            element.RawYaml = elementRawYaml;
            content.Elements.Add(element);

            return i;
        }

        private static void BuildHierarchy(UIPageContent content)
        {
            // Build parent-child relationships by parsing Children properties in raw YAML
            foreach (var element in content.Elements)
            {
                var childrenStarted = false;
                var childrenDict = new Dictionary<string, string>(); // hash -> element ID

                foreach (var line in element.RawYaml)
                {
                    var trimmed = line.Trim();

                    if (trimmed == "Children:")
                    {
                        childrenStarted = true;
                        continue;
                    }

                    if (childrenStarted && trimmed.Contains("ref!!"))
                    {
                        // Format: "hash: !Type ref!! elementId"
                        var parts = trimmed.Split(new[] { "ref!!" }, StringSplitOptions.None);
                        if (parts.Length == 2)
                        {
                            var hash = parts[0].Split(':')[0].Trim();
                            var childId = parts[1].Trim();
                            childrenDict[hash] = childId;
                        }
                    }

                    // Stop when we hit next section
                    if (childrenStarted && (trimmed == "RowDefinitions:" || trimmed == "ColumnDefinitions:" ||
                                           trimmed == "LayerDefinitions:" || trimmed.StartsWith("Name:")))
                    {
                        break;
                    }
                }

                // Link children
                foreach (var kvp in childrenDict)
                {
                    var child = content.Elements.FirstOrDefault(e => e.Id == kvp.Value);
                    if (child != null)
                    {
                        element.Children[kvp.Key] = child;
                        child.Parent = element;
                    }
                }
            }
        }

        private static int GetIndent(string line)
        {
            int count = 0;
            foreach (char c in line)
            {
                if (c == ' ')
                    count++;
                else if (c == '\t')
                    count += 4;
                else
                    break;
            }
            return count;
        }

        /// <summary>
        /// Parses inline YAML object format: {Key: Value, Key2: Value2}
        /// Handles Margin, Color, Resolution, etc.
        /// </summary>
        private static Dictionary<string, object>? ParseInlineObject(string value)
        {
            if (string.IsNullOrEmpty(value) || !value.StartsWith("{") || !value.EndsWith("}"))
                return null;

            var result = new Dictionary<string, object>();

            // Remove braces
            var inner = value.Substring(1, value.Length - 2).Trim();

            if (string.IsNullOrEmpty(inner))
                return result; // Empty object {}

            // Split by comma, but handle nested values
            var parts = SplitByComma(inner);

            foreach (var part in parts)
            {
                var colonIdx = part.IndexOf(':');
                if (colonIdx > 0)
                {
                    var key = part.Substring(0, colonIdx).Trim();
                    var val = part.Substring(colonIdx + 1).Trim();

                    // Try to parse as number
                    if (float.TryParse(val, System.Globalization.NumberStyles.Float,
                        System.Globalization.CultureInfo.InvariantCulture, out var floatVal))
                    {
                        // Check if it's actually an integer (no decimal part)
                        if (floatVal == Math.Floor(floatVal) && floatVal >= int.MinValue && floatVal <= int.MaxValue)
                        {
                            result[key] = (int)floatVal;
                        }
                        else
                        {
                            result[key] = floatVal;
                        }
                    }
                    else if (bool.TryParse(val, out var boolVal))
                    {
                        result[key] = boolVal;
                    }
                    else
                    {
                        result[key] = UnquoteYamlString(val);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Splits a string by comma, respecting nested braces
        /// </summary>
        private static List<string> SplitByComma(string input)
        {
            var result = new List<string>();
            var current = new System.Text.StringBuilder();
            var depth = 0;

            foreach (var c in input)
            {
                if (c == '{')
                {
                    depth++;
                    current.Append(c);
                }
                else if (c == '}')
                {
                    depth--;
                    current.Append(c);
                }
                else if (c == ',' && depth == 0)
                {
                    result.Add(current.ToString().Trim());
                    current.Clear();
                }
                else
                {
                    current.Append(c);
                }
            }

            if (current.Length > 0)
            {
                result.Add(current.ToString().Trim());
            }

            return result;
        }

        /// <summary>
        /// Removes enclosing quotes from a YAML string value.
        /// YAML uses quotes to wrap strings containing special characters like colons.
        /// When parsing, we need to strip these quotes to get the actual value.
        /// Also handles escaped quotes within the string.
        /// </summary>
        private static string UnquoteYamlString(string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            // Check for double-quoted string
            if (value.StartsWith("\"") && value.EndsWith("\"") && value.Length >= 2)
            {
                var inner = value.Substring(1, value.Length - 2);
                // Unescape common escape sequences
                return inner.Replace("\\\"", "\"")
                            .Replace("\\\\", "\\")
                            .Replace("\\n", "\n")
                            .Replace("\\r", "\r")
                            .Replace("\\t", "\t");
            }

            // Check for single-quoted string
            if (value.StartsWith("'") && value.EndsWith("'") && value.Length >= 2)
            {
                var inner = value.Substring(1, value.Length - 2);
                // Single quotes escape by doubling
                return inner.Replace("''", "'");
            }

            return value;
        }

        /// <summary>
        /// Parses a multiline block property (like SpriteFromSheet, Content, etc.)
        /// Expects the current line to be the property header (e.g., "PressedImage: !SpriteFromSheet")
        /// and parses the indented properties that follow.
        /// </summary>
        private static Dictionary<string, object>? ParseMultilineBlock(string[] lines, ref int i, int baseIndent)
        {
            var result = new Dictionary<string, object>();
            var currentLineIndent = GetIndent(lines[i]);

            i++; // Move past the header line

            while (i < lines.Length)
            {
                var line = lines[i];

                // Skip empty lines
                if (string.IsNullOrWhiteSpace(line))
                {
                    i++;
                    continue;
                }

                var indent = GetIndent(line);
                var trimmed = line.Trim();

                // Stop if we've returned to base indent or less (new property at element level)
                if (indent <= currentLineIndent)
                {
                    i--; // Back up so the caller can re-process this line
                    break;
                }

                // Stop if we hit the next UIElement
                if (trimmed.StartsWith("-   UIElement:"))
                {
                    i--; // Back up so the caller can re-process this line
                    break;
                }

                // Parse child property
                if (trimmed.Contains(":"))
                {
                    var colonIdx = trimmed.IndexOf(':');
                    var key = trimmed.Substring(0, colonIdx).Trim();
                    var value = trimmed.Substring(colonIdx + 1).Trim();

                    // Parse value based on type
                    if (float.TryParse(value, System.Globalization.NumberStyles.Float,
                        System.Globalization.CultureInfo.InvariantCulture, out var floatVal))
                    {
                        if (floatVal == Math.Floor(floatVal) && floatVal >= int.MinValue && floatVal <= int.MaxValue)
                        {
                            result[key] = (int)floatVal;
                        }
                        else
                        {
                            result[key] = floatVal;
                        }
                    }
                    else if (bool.TryParse(value, out var boolVal))
                    {
                        result[key] = boolVal;
                    }
                    else
                    {
                        result[key] = UnquoteYamlString(value);
                    }
                }

                i++;
            }

            return result.Count > 0 ? result : null;
        }

        /// <summary>
        /// Parses DependencyProperties multiline block.
        /// Format:
        /// DependencyProperties:
        ///     guid~Panel.ZIndexPropertyKey: 10
        ///     guid~OtherProperty: value
        /// </summary>
        private static Dictionary<string, object>? ParseDependencyProperties(string[] lines, ref int i, int baseIndent)
        {
            var result = new Dictionary<string, object>();
            var currentLineIndent = GetIndent(lines[i]);

            i++; // Move past the "DependencyProperties:" line

            while (i < lines.Length)
            {
                var line = lines[i];

                // Skip empty lines
                if (string.IsNullOrWhiteSpace(line))
                {
                    i++;
                    continue;
                }

                var indent = GetIndent(line);
                var trimmed = line.Trim();

                // Stop if we've returned to base indent or less (new property at element level)
                if (indent <= currentLineIndent)
                {
                    i--; // Back up so the caller can re-process this line
                    break;
                }

                // Stop if we hit the next UIElement
                if (trimmed.StartsWith("-   UIElement:"))
                {
                    i--; // Back up so the caller can re-process this line
                    break;
                }

                // Parse dependency property entry (format: "guid~PropertyKey: value")
                if (trimmed.Contains(":"))
                {
                    var colonIdx = trimmed.IndexOf(':');
                    var key = trimmed.Substring(0, colonIdx).Trim();
                    var value = trimmed.Substring(colonIdx + 1).Trim();

                    // Parse value based on type
                    if (float.TryParse(value, System.Globalization.NumberStyles.Float,
                        System.Globalization.CultureInfo.InvariantCulture, out var floatVal))
                    {
                        // Check if it's actually an integer
                        if (floatVal == Math.Floor(floatVal) && floatVal >= int.MinValue && floatVal <= int.MaxValue)
                        {
                            result[key] = (int)floatVal;
                        }
                        else
                        {
                            result[key] = floatVal;
                        }
                    }
                    else if (bool.TryParse(value, out var boolVal))
                    {
                        result[key] = boolVal;
                    }
                    else
                    {
                        result[key] = UnquoteYamlString(value);
                    }
                }

                i++;
            }

            return result.Count > 0 ? result : null;
        }
    }
}
