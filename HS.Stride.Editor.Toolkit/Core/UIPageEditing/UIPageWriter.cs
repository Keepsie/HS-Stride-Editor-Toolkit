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

            // Write to file
            File.WriteAllText(content.FilePath, sb.ToString());
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
                        sb.AppendLine($"                {propertyName}: {value}");
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
                    sb.AppendLine($"                {propertyName}: {str}");
                }
                else
                {
                    sb.AppendLine($"                {propertyName}: {value}");
                }
            }
        }
    }
}
