// HS Stride Editor Toolkit (c) 2025 Happenstance Games LLC - Apache License 2.0

using System.Text;

namespace HS.Stride.Editor.Toolkit.Core.UIPageEditing
{
    /// <summary>
    /// Writes UIPage objects to Stride .sduipage YAML format
    /// </summary>
    internal static class UIPageWriter
    {
        public static void Write(UIPage page, UIPageContent content)
        {
            // Validate and build the save path
            string filePath = ValidateAndBuildUIPagePath(content);

            var sb = new StringBuilder();

            // Header
            sb.AppendLine("!UIPageAsset");
            sb.AppendLine($"Id: {content.Id}");
            sb.AppendLine("SerializedVersion: {Stride: 2.1.0.1}");
            sb.AppendLine("Tags: []");

            // Design section
            sb.AppendLine("Design:");
            sb.AppendLine($"    Resolution: {{X: {content.Resolution["X"]}, Y: {content.Resolution["Y"]}, Z: {content.Resolution["Z"]}}}");

            // Hierarchy section
            sb.AppendLine("Hierarchy:");

            // RootParts
            sb.AppendLine("    RootParts:");
            foreach (var rootId in content.RootElementIds)
            {
                var rootElement = content.Elements.FirstOrDefault(e => e.Id == rootId);
                if (rootElement != null)
                {
                    sb.AppendLine($"        - !{rootElement.Type} ref!! {rootElement.Id}");
                }
            }

            // Parts
            sb.AppendLine("    Parts:");
            foreach (var element in content.Elements)
            {
                WriteUIElement(sb, element);
            }

            // Ensure directory exists
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            // Write to file
            File.WriteAllText(filePath, sb.ToString());

            // Update content with the final path
            content.FilePath = filePath;
        }

        /// <summary>
        /// Validates and builds the save path for a UI page.
        /// Handles empty paths, relative paths, and ensures saves are within the project.
        /// </summary>
        private static string ValidateAndBuildUIPagePath(UIPageContent content)
        {
            var filePath = content.FilePath;

            // Case 1: Empty or whitespace path - save to Assets root with UI page ID or default name
            if (string.IsNullOrWhiteSpace(filePath))
            {
                if (content.ParentProject == null)
                    throw new InvalidOperationException("Cannot save UI page: no file path set and no parent project available. Load or create UI pages via StrideProject.");

                // Use root element name or ID as filename fallback
                var rootElement = content.Elements.FirstOrDefault(e => content.RootElementIds.Contains(e.Id));
                var name = rootElement?.Name ?? (!string.IsNullOrWhiteSpace(content.Id) ? content.Id.Substring(0, 8) : "UIPage");
                return Path.Combine(content.ParentProject.AssetsPath, $"{name}.sduipage");
            }

            // Case 2: Relative path - build from ParentProject.AssetsPath
            if (!Path.IsPathRooted(filePath))
            {
                if (content.ParentProject == null)
                    throw new InvalidOperationException($"Cannot save UI page with relative path '{filePath}': no parent project available. Load or create UI pages via StrideProject.");

                filePath = Path.Combine(content.ParentProject.AssetsPath, filePath);
            }

            // Case 3: Full path - validate it's inside the project assets folder
            if (content.ParentProject != null)
            {
                var assetsPath = Path.GetFullPath(content.ParentProject.AssetsPath);
                var targetPath = Path.GetFullPath(Path.GetDirectoryName(filePath) ?? filePath);

                if (!targetPath.StartsWith(assetsPath, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException($"Cannot save UI page outside project Assets folder. Attempted path: {filePath}");
            }

            // Ensure .sduipage extension
            if (!filePath.EndsWith(".sduipage", StringComparison.OrdinalIgnoreCase))
            {
                filePath += ".sduipage";
            }

            return filePath;
        }

        private static void WriteUIElement(StringBuilder sb, UIElement element)
        {
            sb.AppendLine($"        -   UIElement: !{element.Type}");
            sb.AppendLine($"                Id: {element.Id}");

            // Write DependencyProperties
            if (element.Properties.TryGetValue("DependencyProperties", out var depProps) &&
                depProps is Dictionary<string, object> depDict && depDict.Count > 0)
            {
                sb.AppendLine("                DependencyProperties:");
                foreach (var kvp in depDict)
                {
                    sb.AppendLine($"                    {kvp.Key}: {kvp.Value}");
                }
            }
            else
            {
                sb.AppendLine("                DependencyProperties: {}");
            }

            // Write BackgroundColor
            WriteColor(sb, "BackgroundColor", element);

            // Write layout properties
            WritePropertyIfExists(sb, "ClipToBounds", element);
            WritePropertyIfExists(sb, "DrawLayerNumber", element);
            WritePropertyIfExists(sb, "CanBeHitByUser", element);
            WritePropertyIfExists(sb, "Visibility", element);
            WritePropertyIfExists(sb, "Width", element);
            WritePropertyIfExists(sb, "Height", element);
            WritePropertyIfExists(sb, "HorizontalAlignment", element);
            WritePropertyIfExists(sb, "VerticalAlignment", element);
            WritePropertyIfExists(sb, "DepthAlignment", element);

            // Write Margin
            WriteMargin(sb, element);

            // Write Maximum dimensions
            sb.AppendLine($"                MaximumWidth: {element.Get<object>("MaximumWidth") ?? 3.4028235E+38f}");
            sb.AppendLine($"                MaximumHeight: {element.Get<object>("MaximumHeight") ?? 3.4028235E+38f}");
            sb.AppendLine($"                MaximumDepth: {element.Get<object>("MaximumDepth") ?? 3.4028235E+38f}");

            // Write Name
            if (!string.IsNullOrEmpty(element.Name))
            {
                sb.AppendLine($"                Name: {element.Name}");
            }

            // Write type-specific properties
            WriteTypeSpecificProperties(sb, element);
        }

        private static void WriteTypeSpecificProperties(StringBuilder sb, UIElement element)
        {
            switch (element.Type.ToLower())
            {
                case "textblock":
                    WritePropertyIfExists(sb, "Text", element);
                    WritePropertyIfExists(sb, "Font", element);
                    WritePropertyIfExists(sb, "TextSize", element);
                    WriteColor(sb, "TextColor", element);
                    WriteColor(sb, "OutlineColor", element);
                    WritePropertyIfExists(sb, "OutlineThickness", element);
                    WritePropertyIfExists(sb, "TextAlignment", element);
                    break;

                case "imageelement":
                    WriteSprite(sb, "Source", element);
                    WriteColor(sb, "Color", element);
                    WritePropertyIfExists(sb, "StretchType", element);
                    break;

                case "button":
                    WritePropertyIfExists(sb, "Content", element);
                    WriteSprite(sb, "PressedImage", element);
                    WriteSprite(sb, "NotPressedImage", element);
                    WriteSprite(sb, "MouseOverImage", element);
                    break;

                case "edittext":
                    WritePropertyIfExists(sb, "Padding", element);
                    WritePropertyIfExists(sb, "Font", element);
                    WritePropertyIfExists(sb, "TextSize", element);
                    WriteColor(sb, "TextColor", element);
                    WriteSprite(sb, "ActiveImage", element);
                    WriteSprite(sb, "InactiveImage", element);
                    WriteSprite(sb, "MouseOverImage", element);
                    WriteColor(sb, "CaretColor", element);
                    WritePropertyIfExists(sb, "CaretWidth", element);
                    WriteColor(sb, "SelectionColor", element);
                    WriteColor(sb, "IMESelectionColor", element);
                    WritePropertyIfExists(sb, "CaretFrequency", element);
                    break;

                case "scrollviewer":
                    WritePropertyIfExists(sb, "Padding", element);
                    WriteColor(sb, "ScrollBarColor", element);
                    WritePropertyIfExists(sb, "Content", element);
                    break;

                case "stackpanel":
                case "canvas":
                case "grid":
                    // Write Padding if exists
                    WritePropertyIfExists(sb, "Padding", element);

                    // Write Children
                    WriteChildren(sb, element);

                    // Grid-specific
                    if (element.Type.ToLower() == "grid")
                    {
                        sb.AppendLine("                RowDefinitions: {}");
                        sb.AppendLine("                ColumnDefinitions: {}");
                        sb.AppendLine("                LayerDefinitions: {}");
                    }
                    break;
            }
        }

        private static void WriteChildren(StringBuilder sb, UIElement element)
        {
            if (element.Children.Count == 0)
            {
                sb.AppendLine("                Children: {}");
            }
            else
            {
                sb.AppendLine("                Children:");
                foreach (var kvp in element.Children)
                {
                    var child = kvp.Value;
                    sb.AppendLine($"                    {kvp.Key}: !{child.Type} ref!! {child.Id}");
                }
            }
        }

        private static void WriteMargin(StringBuilder sb, UIElement element)
        {
            if (element.Properties.TryGetValue("Margin", out var marginObj) &&
                marginObj is Dictionary<string, object> margin && margin.Count > 0)
            {
                sb.Append("                Margin: {");
                var parts = new List<string>();
                if (margin.ContainsKey("Left")) parts.Add($"Left: {margin["Left"]}");
                if (margin.ContainsKey("Top")) parts.Add($"Top: {margin["Top"]}");
                if (margin.ContainsKey("Right")) parts.Add($"Right: {margin["Right"]}");
                if (margin.ContainsKey("Bottom")) parts.Add($"Bottom: {margin["Bottom"]}");
                sb.Append(string.Join(", ", parts));
                sb.AppendLine("}");
            }
            else
            {
                sb.AppendLine("                Margin: {}");
            }
        }

        private static void WriteColor(StringBuilder sb, string propertyName, UIElement element)
        {
            if (element.Properties.TryGetValue(propertyName, out var colorObj) &&
                colorObj is Dictionary<string, object> color)
            {
                sb.AppendLine($"                {propertyName}: {{R: {color.GetValueOrDefault("R", 0)}, G: {color.GetValueOrDefault("G", 0)}, B: {color.GetValueOrDefault("B", 0)}, A: {color.GetValueOrDefault("A", 255)}}}");
            }
        }

        private static void WriteSprite(StringBuilder sb, string propertyName, UIElement element)
        {
            if (element.Properties.TryGetValue(propertyName, out var spriteObj) &&
                spriteObj is Dictionary<string, object> sprite)
            {
                // Check for SpriteFromSheet or SpriteFromTexture
                var isSpriteFromSheet = sprite.ContainsKey("!SpriteFromSheet") || sprite.ContainsKey("Sheet");
                var isSpriteFromTexture = sprite.ContainsKey("!SpriteFromTexture") || sprite.ContainsKey("Texture");

                if (isSpriteFromSheet)
                {
                    sb.AppendLine($"                {propertyName}: !SpriteFromSheet");
                    sb.AppendLine($"                    Sheet: {sprite.GetValueOrDefault("Sheet", "null")}");
                    sb.AppendLine($"                    CurrentFrame: {sprite.GetValueOrDefault("CurrentFrame", 0)}");
                }
                else if (isSpriteFromTexture)
                {
                    sb.AppendLine($"                {propertyName}: !SpriteFromTexture");
                    sb.AppendLine($"                    Texture: {sprite.GetValueOrDefault("Texture", "null")}");
                    sb.AppendLine($"                    Center: {{X: {sprite.GetValueOrDefault("CenterX", 0.0f)}, Y: {sprite.GetValueOrDefault("CenterY", 0.0f)}}}");
                }
            }
        }

        private static void WritePropertyIfExists(StringBuilder sb, string propertyName, UIElement element)
        {
            if (element.Properties.TryGetValue(propertyName, out var value))
            {
                // Handle reference format (for Content in Button, etc.)
                if (value is string strValue && strValue.Contains("ref!!"))
                {
                    // Extract type and ID: "!TextBlock ref!! guid"
                    var parts = strValue.Split(new[] { "ref!!" }, StringSplitOptions.None);
                    if (parts.Length == 2)
                    {
                        var typePrefix = parts[0].Trim();
                        var refId = parts[1].Trim();
                        sb.AppendLine($"                {propertyName}: {typePrefix} ref!! {refId}");
                    }
                    else
                    {
                        sb.AppendLine($"                {propertyName}: {EscapeYamlString(strValue)}");
                    }
                }
                else if (value is Dictionary<string, object> dict)
                {
                    // Handle Padding, etc.
                    sb.Append($"                {propertyName}: {{");
                    var parts = new List<string>();
                    foreach (var kvp in dict)
                    {
                        parts.Add($"{kvp.Key}: {kvp.Value}");
                    }
                    sb.Append(string.Join(", ", parts));
                    sb.AppendLine("}");
                }
                else if (value is string str && !string.IsNullOrEmpty(str))
                {
                    sb.AppendLine($"                {propertyName}: {EscapeYamlString(str)}");
                }
                else if (value is float floatValue)
                {
                    // Ensure floats always have decimal point
                    sb.AppendLine($"                {propertyName}: {floatValue.ToString("0.0###############", System.Globalization.CultureInfo.InvariantCulture)}");
                }
                else
                {
                    sb.AppendLine($"                {propertyName}: {value}");
                }
            }
        }

        /// <summary>
        /// Escapes a string value for YAML if it contains special characters
        /// </summary>
        private static string EscapeYamlString(string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            // Check if string needs quoting (contains special YAML characters)
            bool needsQuoting = value.Contains(':') ||
                                value.Contains('{') ||
                                value.Contains('}') ||
                                value.Contains('[') ||
                                value.Contains(']') ||
                                value.Contains(',') ||
                                value.Contains('&') ||
                                value.Contains('*') ||
                                value.Contains('#') ||
                                value.Contains('?') ||
                                value.Contains('|') ||
                                value.Contains('-') && value.StartsWith("-") ||
                                value.Contains('>') ||
                                value.Contains('<') ||
                                value.Contains('=') ||
                                value.Contains('!') ||
                                value.Contains('%') ||
                                value.Contains('@') ||
                                value.Contains('`') ||
                                value.Contains('"') ||
                                value.Contains('\'') ||
                                value.Contains('\\') ||
                                value.Contains('\n') ||
                                value.Contains('\r') ||
                                value.Contains('\t') ||
                                value.StartsWith(" ") ||
                                value.EndsWith(" ");

            if (!needsQuoting)
                return value;

            // Use double quotes and escape internal quotes, backslashes, and special characters
            value = value.Replace("\\", "\\\\")
                         .Replace("\"", "\\\"")
                         .Replace("\n", "\\n")
                         .Replace("\r", "\\r")
                         .Replace("\t", "\\t");
            return $"\"{value}\"";
        }
    }
}
