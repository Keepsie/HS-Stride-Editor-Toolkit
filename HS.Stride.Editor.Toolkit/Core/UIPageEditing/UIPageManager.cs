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
            string verticalAlignment = "Center",
            bool autoAttach = true)
        {
            var textBlock = page.CreateElement("TextBlock", name, parent, autoAttach);
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

            // Create text content for button (unattached - only referenced by Content property)
            var textBlock = page.CreateTextBlock($"{name}_text", buttonText, parent: null, autoAttach: false);

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
                image.SetSprite(spriteSheet, frame);
            }

            return image;
        }

        /// <summary>
        /// Creates an ImageElement with a texture reference (no sprite sheet)
        /// </summary>
        public static UIElement CreateImageFromTexture(this UIPage page, string name,
            AssetReference? texture = null,
            UIElement? parent = null,
            float width = 100.0f,
            float height = 100.0f)
        {
            var image = page.CreateElement("ImageElement", name, parent);
            image.Set("Width", width);
            image.Set("Height", height);

            if (texture != null)
            {
                image.SetTexture(texture);
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
        /// Creates an EditText element (text input field)
        /// </summary>
        public static UIElement CreateEditText(this UIPage page, string name,
            string placeholder = "",
            UIElement? parent = null,
            float width = 200.0f,
            float height = 35.0f)
        {
            var editText = page.CreateElement("EditText", name, parent);
            editText.Set("Width", width);
            editText.Set("Height", height);
            editText.Set("Text", placeholder);
            editText.Set("TextSize", 16.0f);

            return editText;
        }

        /// <summary>
        /// Creates a Slider element (value slider)
        /// </summary>
        public static UIElement CreateSlider(this UIPage page, string name,
            float min = 0f,
            float max = 100f,
            float value = 50f,
            UIElement? parent = null,
            float width = 200.0f)
        {
            var slider = page.CreateElement("Slider", name, parent);
            slider.Set("Width", width);
            slider.Set("Minimum", min);
            slider.Set("Maximum", max);
            slider.Set("Value", value);

            return slider;
        }

        /// <summary>
        /// Creates a ToggleButton element (checkbox/toggle)
        /// </summary>
        public static UIElement CreateToggleButton(this UIPage page, string name,
            string text = "",
            bool isChecked = false,
            UIElement? parent = null,
            float width = 150.0f,
            float height = 35.0f)
        {
            var toggle = page.CreateElement("ToggleButton", name, parent);
            toggle.Set("Width", width);
            toggle.Set("Height", height);
            toggle.Set("IsThreeState", false);
            toggle.Set("State", isChecked ? "Checked" : "Unchecked");

            // Create text content if provided
            if (!string.IsNullOrEmpty(text))
            {
                var textBlock = page.CreateTextBlock($"{name}_text", text, parent: null, autoAttach: false);
                toggle.Set("Content", $"!TextBlock ref!! {textBlock.Id}");
            }

            return toggle;
        }

        /// <summary>
        /// Creates a ScrollBar element (standalone scrollbar)
        /// </summary>
        public static UIElement CreateScrollBar(this UIPage page, string name,
            bool isVertical = true,
            UIElement? parent = null,
            float width = 20.0f,
            float height = 200.0f)
        {
            var scrollBar = page.CreateElement("ScrollBar", name, parent);

            if (isVertical)
            {
                scrollBar.Set("Width", width);
                scrollBar.Set("Height", height);
            }
            else
            {
                scrollBar.Set("Width", height);
                scrollBar.Set("Height", width);
            }

            scrollBar.Set("Minimum", 0f);
            scrollBar.Set("Maximum", 100f);
            scrollBar.Set("Value", 0f);

            return scrollBar;
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

        // ===== Font Methods (AssetReference pattern) =====

        /// <summary>
        /// Sets the font for TextBlock or text-based elements using an AssetReference
        /// </summary>
        public static void SetFont(this UIElement element, AssetReference font)
        {
            element.Set("Font", font.Reference);
        }

        /// <summary>
        /// Sets the font size
        /// </summary>
        public static void SetFontSize(this UIElement element, float size)
        {
            element.Set("TextSize", size);
        }

        /// <summary>
        /// Sets the text content for TextBlock elements
        /// </summary>
        public static void SetText(this UIElement element, string text)
        {
            element.Set("Text", text);
        }

        // ===== Sprite/Image Methods (AssetReference pattern) =====

        /// <summary>
        /// Sets the sprite sheet and frame for ImageElement using SpriteFromSheet
        /// </summary>
        public static void SetSprite(this UIElement element, AssetReference spriteSheet, int frame = 0)
        {
            element.Set("Source", new Dictionary<string, object>
            {
                ["!SpriteFromSheet"] = "",
                ["Sheet"] = spriteSheet.Reference,
                ["CurrentFrame"] = frame
            });
        }

        /// <summary>
        /// Sets a texture directly for ImageElement using SpriteFromTexture (no sprite sheet needed)
        /// </summary>
        public static void SetTexture(this UIElement element, AssetReference texture)
        {
            element.Set("Source", new Dictionary<string, object>
            {
                ["!SpriteFromTexture"] = "",
                ["Texture"] = texture.Reference
            });
        }

        /// <summary>
        /// Sets the PressedImage for Button elements
        /// </summary>
        public static void SetPressedImage(this UIElement element, AssetReference spriteSheet, int frame = 0)
        {
            element.Set("PressedImage", new Dictionary<string, object>
            {
                ["!SpriteFromSheet"] = "",
                ["Sheet"] = spriteSheet.Reference,
                ["CurrentFrame"] = frame
            });
        }

        /// <summary>
        /// Sets the NotPressedImage for Button elements
        /// </summary>
        public static void SetNotPressedImage(this UIElement element, AssetReference spriteSheet, int frame = 0)
        {
            element.Set("NotPressedImage", new Dictionary<string, object>
            {
                ["!SpriteFromSheet"] = "",
                ["Sheet"] = spriteSheet.Reference,
                ["CurrentFrame"] = frame
            });
        }

        /// <summary>
        /// Sets the MouseOverImage for Button elements
        /// </summary>
        public static void SetMouseOverImage(this UIElement element, AssetReference spriteSheet, int frame = 0)
        {
            element.Set("MouseOverImage", new Dictionary<string, object>
            {
                ["!SpriteFromSheet"] = "",
                ["Sheet"] = spriteSheet.Reference,
                ["CurrentFrame"] = frame
            });
        }

        /// <summary>
        /// Sets all three button images at once (pressed, not pressed, mouse over) using sprite sheet
        /// </summary>
        public static void SetButtonImages(this UIElement element, AssetReference spriteSheet,
            int pressedFrame, int notPressedFrame, int mouseOverFrame)
        {
            element.SetPressedImage(spriteSheet, pressedFrame);
            element.SetNotPressedImage(spriteSheet, notPressedFrame);
            element.SetMouseOverImage(spriteSheet, mouseOverFrame);
        }

        // ===== Button Texture Methods (SpriteFromTexture pattern) =====

        /// <summary>
        /// Sets the PressedImage for Button elements using a texture directly
        /// </summary>
        public static void SetPressedTexture(this UIElement element, AssetReference texture)
        {
            element.Set("PressedImage", new Dictionary<string, object>
            {
                ["!SpriteFromTexture"] = "",
                ["Texture"] = texture.Reference
            });
        }

        /// <summary>
        /// Sets the NotPressedImage for Button elements using a texture directly
        /// </summary>
        public static void SetNotPressedTexture(this UIElement element, AssetReference texture)
        {
            element.Set("NotPressedImage", new Dictionary<string, object>
            {
                ["!SpriteFromTexture"] = "",
                ["Texture"] = texture.Reference
            });
        }

        /// <summary>
        /// Sets the MouseOverImage for Button elements using a texture directly
        /// </summary>
        public static void SetMouseOverTexture(this UIElement element, AssetReference texture)
        {
            element.Set("MouseOverImage", new Dictionary<string, object>
            {
                ["!SpriteFromTexture"] = "",
                ["Texture"] = texture.Reference
            });
        }

        /// <summary>
        /// Sets all three button images at once using textures directly (no sprite sheet)
        /// </summary>
        public static void SetButtonTextures(this UIElement element,
            AssetReference pressedTexture,
            AssetReference notPressedTexture,
            AssetReference mouseOverTexture)
        {
            element.SetPressedTexture(pressedTexture);
            element.SetNotPressedTexture(notPressedTexture);
            element.SetMouseOverTexture(mouseOverTexture);
        }

        // ===== Size and Dimension Methods =====

        /// <summary>
        /// Sets the width of a UI element
        /// </summary>
        public static void SetWidth(this UIElement element, float width)
        {
            element.Set("Width", width);
        }

        /// <summary>
        /// Sets the height of a UI element
        /// </summary>
        public static void SetHeight(this UIElement element, float height)
        {
            element.Set("Height", height);
        }

        /// <summary>
        /// Sets padding for UI elements
        /// </summary>
        public static void SetPadding(this UIElement element,
            float? left = null,
            float? top = null,
            float? right = null,
            float? bottom = null)
        {
            var padding = new Dictionary<string, object>();

            if (left.HasValue) padding["Left"] = left.Value;
            if (top.HasValue) padding["Top"] = top.Value;
            if (right.HasValue) padding["Right"] = right.Value;
            if (bottom.HasValue) padding["Bottom"] = bottom.Value;

            element.Set("Padding", padding);
        }

        // ===== Horizontal/Vertical Alignment =====

        /// <summary>
        /// Sets horizontal alignment (Left, Center, Right, Stretch)
        /// </summary>
        public static void SetHorizontalAlignment(this UIElement element, string alignment)
        {
            element.Set("HorizontalAlignment", alignment);
        }

        /// <summary>
        /// Sets vertical alignment (Top, Center, Bottom, Stretch)
        /// </summary>
        public static void SetVerticalAlignment(this UIElement element, string alignment)
        {
            element.Set("VerticalAlignment", alignment);
        }

        // ===== Color Methods (Extended) =====

        /// <summary>
        /// Sets the color for ImageElement elements
        /// </summary>
        public static void SetColor(this UIElement element, int r, int g, int b, int a = 255)
        {
            element.Set("Color", new Dictionary<string, object>
            {
                ["R"] = r,
                ["G"] = g,
                ["B"] = b,
                ["A"] = a
            });
        }

        /// <summary>
        /// Sets the outline color for TextBlock elements
        /// </summary>
        public static void SetOutlineColor(this UIElement element, int r, int g, int b, int a = 255)
        {
            element.Set("OutlineColor", new Dictionary<string, object>
            {
                ["R"] = r,
                ["G"] = g,
                ["B"] = b,
                ["A"] = a
            });
        }

        /// <summary>
        /// Sets the outline thickness for TextBlock elements
        /// </summary>
        public static void SetOutlineThickness(this UIElement element, float thickness)
        {
            element.Set("OutlineThickness", thickness);
        }

        // ===== Interaction Properties =====

        /// <summary>
        /// Sets whether the element can be clicked/interacted with
        /// </summary>
        public static void SetCanBeHitByUser(this UIElement element, bool canHit)
        {
            element.Set("CanBeHitByUser", canHit);
        }

        /// <summary>
        /// Sets the draw layer number (higher = drawn on top)
        /// </summary>
        public static void SetDrawLayer(this UIElement element, int layer)
        {
            element.Set("DrawLayerNumber", layer);
        }

        /// <summary>
        /// Sets whether content is clipped to bounds
        /// </summary>
        public static void SetClipToBounds(this UIElement element, bool clip)
        {
            element.Set("ClipToBounds", clip);
        }

        // ===== Opacity =====

        /// <summary>
        /// Sets the opacity of the element (0.0 = transparent, 1.0 = opaque)
        /// </summary>
        public static void SetOpacity(this UIElement element, float opacity)
        {
            element.Set("Opacity", Math.Clamp(opacity, 0f, 1f));
        }

        // ===== Slider-Specific Methods =====

        /// <summary>
        /// Sets the range for a Slider
        /// </summary>
        public static void SetRange(this UIElement slider, float min, float max)
        {
            slider.Set("Minimum", min);
            slider.Set("Maximum", max);
        }

        /// <summary>
        /// Sets the current value for a Slider
        /// </summary>
        public static void SetValue(this UIElement slider, float value)
        {
            slider.Set("Value", value);
        }

        /// <summary>
        /// Sets the step/tick frequency for a Slider
        /// </summary>
        public static void SetStep(this UIElement slider, float step)
        {
            slider.Set("TickFrequency", step);
        }

        // ===== ToggleButton-Specific Methods =====

        /// <summary>
        /// Sets the checked state of a ToggleButton
        /// </summary>
        public static void SetChecked(this UIElement toggle, bool isChecked)
        {
            toggle.Set("State", isChecked ? "Checked" : "Unchecked");
        }

        /// <summary>
        /// Gets the checked state of a ToggleButton
        /// </summary>
        public static bool IsChecked(this UIElement toggle)
        {
            var state = toggle.Get<string>("State");
            return state == "Checked";
        }

        // ===== EditText-Specific Methods =====

        /// <summary>
        /// Sets the max length for EditText input
        /// </summary>
        public static void SetMaxLength(this UIElement editText, int maxLength)
        {
            editText.Set("MaxLength", maxLength);
        }

        /// <summary>
        /// Sets whether EditText is read-only
        /// </summary>
        public static void SetReadOnly(this UIElement editText, bool readOnly)
        {
            editText.Set("IsReadOnly", readOnly);
        }

        /// <summary>
        /// Sets the selection color for EditText
        /// </summary>
        public static void SetSelectionColor(this UIElement editText, int r, int g, int b, int a = 255)
        {
            editText.Set("SelectionColor", new Dictionary<string, object>
            {
                ["R"] = r,
                ["G"] = g,
                ["B"] = b,
                ["A"] = a
            });
        }

        /// <summary>
        /// Sets the caret (cursor) color for EditText
        /// </summary>
        public static void SetCaretColor(this UIElement editText, int r, int g, int b, int a = 255)
        {
            editText.Set("CaretColor", new Dictionary<string, object>
            {
                ["R"] = r,
                ["G"] = g,
                ["B"] = b,
                ["A"] = a
            });
        }

        // ===== ScrollViewer-Specific Methods =====

        /// <summary>
        /// Sets the scroll bar color for ScrollViewer
        /// </summary>
        public static void SetScrollBarColor(this UIElement scrollViewer, int r, int g, int b, int a = 255)
        {
            scrollViewer.Set("ScrollBarColor", new Dictionary<string, object>
            {
                ["R"] = r,
                ["G"] = g,
                ["B"] = b,
                ["A"] = a
            });
        }

        /// <summary>
        /// Sets the content reference for ScrollViewer
        /// </summary>
        public static void SetScrollContent(this UIElement scrollViewer, UIElement content)
        {
            scrollViewer.Set("Content", $"!{content.Type} ref!! {content.Id}");
        }

        // ===== StackPanel-Specific Methods =====

        /// <summary>
        /// Sets the orientation for StackPanel (Vertical or Horizontal)
        /// </summary>
        public static void SetOrientation(this UIElement stackPanel, string orientation)
        {
            stackPanel.Set("Orientation", orientation);
        }

        /// <summary>
        /// Sets vertical orientation for StackPanel
        /// </summary>
        public static void SetVerticalOrientation(this UIElement stackPanel)
        {
            stackPanel.Set("Orientation", "Vertical");
        }

        /// <summary>
        /// Sets horizontal orientation for StackPanel
        /// </summary>
        public static void SetHorizontalOrientation(this UIElement stackPanel)
        {
            stackPanel.Set("Orientation", "Horizontal");
        }
    }
}
