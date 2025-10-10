// HS Stride Editor Toolkit (c) 2025 Happenstance Games LLC - Apache License 2.0

using HS.Stride.Editor.Toolkit.Core.AssetEditing;

namespace HS.Stride.Editor.Toolkit.Core.UIPageEditing
{
    /// <summary>
    /// Extension methods and helpers for creating common UI elements programmatically
    /// </summary>
    public static class UIPageManager
    {
        /// <summary>
        /// Creates a TextBlock element with common properties
        /// </summary>
        public static UIElement CreateTextBlock(this UIPage page, string name, string text,
            UIElement? parent = null,
            float fontSize = 20.0f,
            string horizontalAlignment = "Center",
            string verticalAlignment = "Center")
        {
            var textBlock = page.CreateElement("TextBlock", name, parent);
            textBlock.Set("Text", text);
            textBlock.Set("TextSize", fontSize);
            textBlock.Set("HorizontalAlignment", horizontalAlignment);
            textBlock.Set("VerticalAlignment", verticalAlignment);

            return textBlock;
        }

        /// <summary>
        /// Creates a Button element with text content
        /// </summary>
        public static UIElement CreateButton(this UIPage page, string name, string buttonText,
            UIElement? parent = null,
            float width = 200.0f,
            float height = 50.0f)
        {
            // Create button
            var button = page.CreateElement("Button", name, parent);
            button.Set("Width", width);
            button.Set("Height", height);

            // Create text content for button
            var textBlock = page.CreateTextBlock($"{name}_text", buttonText, null);

            // Link text to button as content
            button.Set("Content", $"!TextBlock ref!! {textBlock.Id}");

            return button;
        }

        /// <summary>
        /// Creates an ImageElement with sprite sheet reference
        /// </summary>
        public static UIElement CreateImage(this UIPage page, string name,
            AssetReference? spriteSheet = null,
            int frame = 0,
            UIElement? parent = null,
            float width = 100.0f,
            float height = 100.0f)
        {
            var image = page.CreateElement("ImageElement", name, parent);
            image.Set("Width", width);
            image.Set("Height", height);

            if (spriteSheet != null)
            {
                image.Set("Source", new Dictionary<string, object>
                {
                    ["!SpriteFromSheet"] = "",
                    ["Sheet"] = spriteSheet.Reference,
                    ["CurrentFrame"] = frame
                });
            }

            return image;
        }

        /// <summary>
        /// Creates a Canvas container for absolute positioning
        /// </summary>
        public static UIElement CreateCanvas(this UIPage page, string name,
            UIElement? parent = null,
            float? width = null,
            float? height = null)
        {
            var canvas = page.CreateElement("Canvas", name, parent);

            if (width.HasValue)
                canvas.Set("Width", width.Value);
            if (height.HasValue)
                canvas.Set("Height", height.Value);

            return canvas;
        }

        /// <summary>
        /// Creates a Grid container
        /// </summary>
        public static UIElement CreateGrid(this UIPage page, string name, UIElement? parent = null)
        {
            return page.CreateElement("Grid", name, parent);
        }

        /// <summary>
        /// Creates a StackPanel container
        /// </summary>
        public static UIElement CreateStackPanel(this UIPage page, string name, UIElement? parent = null)
        {
            return page.CreateElement("StackPanel", name, parent);
        }

        /// <summary>
        /// Creates a ScrollViewer with content
        /// </summary>
        public static UIElement CreateScrollViewer(this UIPage page, string name,
            UIElement contentElement,
            UIElement? parent = null)
        {
            var scrollViewer = page.CreateElement("ScrollViewer", name, parent);

            // Link content
            scrollViewer.Set("Content", $"!{contentElement.Type} ref!! {contentElement.Id}");

            return scrollViewer;
        }

        /// <summary>
        /// Sets the margin for absolute positioning
        /// </summary>
        public static void SetMargin(this UIElement element,
            float? left = null,
            float? top = null,
            float? right = null,
            float? bottom = null)
        {
            var margin = new Dictionary<string, object>();

            if (left.HasValue) margin["Left"] = left.Value;
            if (top.HasValue) margin["Top"] = top.Value;
            if (right.HasValue) margin["Right"] = right.Value;
            if (bottom.HasValue) margin["Bottom"] = bottom.Value;

            element.Set("Margin", margin);
        }

        /// <summary>
        /// Sets the size of a UI element
        /// </summary>
        public static void SetSize(this UIElement element, float width, float height)
        {
            element.Set("Width", width);
            element.Set("Height", height);
        }

        /// <summary>
        /// Sets the background color of a UI element
        /// </summary>
        public static void SetBackgroundColor(this UIElement element, int r, int g, int b, int a = 255)
        {
            element.Set("BackgroundColor", new Dictionary<string, object>
            {
                ["R"] = r,
                ["G"] = g,
                ["B"] = b,
                ["A"] = a
            });
        }

        /// <summary>
        /// Sets the text color for TextBlock elements
        /// </summary>
        public static void SetTextColor(this UIElement element, int r, int g, int b, int a = 255)
        {
            if (element.Type != "TextBlock")
                throw new InvalidOperationException("SetTextColor can only be used on TextBlock elements");

            element.Set("TextColor", new Dictionary<string, object>
            {
                ["R"] = r,
                ["G"] = g,
                ["B"] = b,
                ["A"] = a
            });
        }

        /// <summary>
        /// Sets sprite sheet for ImageElement or Button images
        /// </summary>
        public static void SetSpriteSheet(this UIElement element, string propertyName,
            AssetReference spriteSheet, int frame = 0)
        {
            element.Set(propertyName, new Dictionary<string, object>
            {
                ["!SpriteFromSheet"] = "",
                ["Sheet"] = spriteSheet.Reference,
                ["CurrentFrame"] = frame
            });
        }

        /// <summary>
        /// Sets alignment for UI element
        /// </summary>
        public static void SetAlignment(this UIElement element,
            string? horizontal = null,
            string? vertical = null)
        {
            if (!string.IsNullOrEmpty(horizontal))
                element.Set("HorizontalAlignment", horizontal);

            if (!string.IsNullOrEmpty(vertical))
                element.Set("VerticalAlignment", vertical);
        }

        /// <summary>
        /// Sets visibility of UI element
        /// </summary>
        public static void SetVisibility(this UIElement element, bool visible)
        {
            if (!visible)
                element.Set("Visibility", "Hidden");
            else
                element.Properties.Remove("Visibility"); // Default is visible
        }
    }
}
