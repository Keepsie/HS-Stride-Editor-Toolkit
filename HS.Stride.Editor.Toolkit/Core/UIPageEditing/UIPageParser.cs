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

                    // Basic property storage (lazy loading for complex nested properties)
                    if (!string.IsNullOrEmpty(value) && !value.StartsWith("{") && !value.StartsWith("!"))
                    {
                        element.Properties[key] = value;
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
    }
}
