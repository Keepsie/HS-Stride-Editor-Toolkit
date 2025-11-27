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
        /// Creates a TextBlock element with common properties.
        /// Default alignment is Left/Top for proper positioning with SetPosition().
        /// For centered text within a button or container, the text will center automatically
        /// when used as Content, or you can use SetAlignment("Center", "Center") after creation.
        /// </summary>
        public static UIElement CreateTextBlock(this UIPage page, string name, string text,
            UIElement? parent = null,
            float fontSize = 20.0f,
            string horizontalAlignment = "Left",
            string verticalAlignment = "Top",
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
            // Button text should be centered within the button
            var textBlock = page.CreateTextBlock($"{name}_text", buttonText, parent: null,
                horizontalAlignment: "Center", verticalAlignment: "Center", autoAttach: false);

            // Link text to button as content
            button.Set("Content", $"!TextBlock ref!! {textBlock.Id}");

            return button;
        }

        /// <summary>
        /// Creates an ImageElement with sprite sheet reference.
        /// Set stretchFill=true for background images that should fill their parent container.
        /// </summary>
        public static UIElement CreateImage(this UIPage page, string name,
            AssetReference? spriteSheet = null,
            int frame = 0,
            UIElement? parent = null,
            float width = 100.0f,
            float height = 100.0f,
            bool stretchFill = false)
        {
            var image = page.CreateElement("ImageElement", name, parent);

            if (stretchFill)
            {
                // Configure for full parent coverage (backgrounds)
                image.SetImageStretchFill();
            }
            else
            {
                image.Set("Width", width);
                image.Set("Height", height);
            }

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

            // Create text content if provided (centered within toggle)
            if (!string.IsNullOrEmpty(text))
            {
                var textBlock = page.CreateTextBlock($"{name}_text", text, parent: null,
                    horizontalAlignment: "Center", verticalAlignment: "Center", autoAttach: false);
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
        /// Creates a ModalElement for dialog overlays
        /// </summary>
        public static UIElement CreateModalElement(this UIPage page, string name,
            UIElement? parent = null,
            float width = 200.0f,
            float height = 100.0f)
        {
            var modal = page.CreateElement("ModalElement", name, parent);
            modal.Set("Width", width);
            modal.Set("Height", height);

            return modal;
        }

        /// <summary>
        /// Creates a Border element with optional border styling
        /// </summary>
        public static UIElement CreateBorder(this UIPage page, string name,
            UIElement? parent = null,
            float width = 200.0f,
            float height = 100.0f)
        {
            var border = page.CreateElement("Border", name, parent);
            border.Set("Width", width);
            border.Set("Height", height);

            return border;
        }

        /// <summary>
        /// Creates a UniformGrid container
        /// </summary>
        public static UIElement CreateUniformGrid(this UIPage page, string name, UIElement? parent = null)
        {
            return page.CreateElement("UniformGrid", name, parent);
        }

        /// <summary>
        /// Creates a ScrollingText element for animated text
        /// </summary>
        public static UIElement CreateScrollingText(this UIPage page, string name, string text,
            UIElement? parent = null,
            float fontSize = 20.0f)
        {
            var scrollingText = page.CreateElement("ScrollingText", name, parent);
            scrollingText.Set("Text", text);
            scrollingText.Set("TextSize", fontSize);

            return scrollingText;
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
        /// Sets the size of a UI element and recalculates margins if position was set.
        /// </summary>
        public static void SetSize(this UIElement element, float width, float height)
        {
            element.Set("Width", width);
            element.Set("Height", height);

            // Recalculate margins if element already has position set
            var margin = element.GetMargin();
            if (margin.Left != 0 || margin.Top != 0 || margin.Right != 0 || margin.Bottom != 0)
            {
                // Get parent dimensions and recalculate Right/Bottom
                var (parentWidth, parentHeight) = element.GetParentDimensions();
                float right = parentWidth - (margin.Left + width);
                float bottom = parentHeight - (margin.Top + height);

                element.SetMargin(left: margin.Left, top: margin.Top, right: right, bottom: bottom);
            }
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
        /// Sets the text color for TextBlock and ScrollingText elements
        /// </summary>
        public static void SetTextColor(this UIElement element, int r, int g, int b, int a = 255)
        {
            if (element.Type != "TextBlock" && element.Type != "ScrollingText")
                throw new InvalidOperationException("SetTextColor can only be used on TextBlock or ScrollingText elements");

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
        /// Configures an element to stretch and fill its parent container.
        /// Best used for background images/overlays that should cover the entire page.
        /// Sets Stretch alignment with zero margins and removes explicit Width/Height.
        /// </summary>
        public static void SetStretchFill(this UIElement element)
        {
            element.Set("HorizontalAlignment", "Stretch");
            element.Set("VerticalAlignment", "Stretch");
            element.SetMargin(left: 0, top: 0, right: 0, bottom: 0);

            // Remove explicit Width/Height so Stretch can work
            element.Properties.Remove("Width");
            element.Properties.Remove("Height");
        }

        /// <summary>
        /// Configures an ImageElement to fill its parent with proper stretch settings.
        /// Sets StretchType to Fill and StretchDirection to Both for full coverage.
        /// </summary>
        public static void SetImageStretchFill(this UIElement element)
        {
            element.SetStretchFill();
            element.Set("StretchType", "Fill");
            element.Set("StretchDirection", "Both");
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

        // ===== ToggleButton Image Methods =====

        /// <summary>
        /// Sets the CheckedImage for ToggleButton elements
        /// </summary>
        public static void SetCheckedImage(this UIElement element, AssetReference spriteSheet, int frame = 0)
        {
            element.Set("CheckedImage", new Dictionary<string, object>
            {
                ["!SpriteFromSheet"] = "",
                ["Sheet"] = spriteSheet.Reference,
                ["CurrentFrame"] = frame
            });
        }

        /// <summary>
        /// Sets the UncheckedImage for ToggleButton elements
        /// </summary>
        public static void SetUncheckedImage(this UIElement element, AssetReference spriteSheet, int frame = 0)
        {
            element.Set("UncheckedImage", new Dictionary<string, object>
            {
                ["!SpriteFromSheet"] = "",
                ["Sheet"] = spriteSheet.Reference,
                ["CurrentFrame"] = frame
            });
        }

        /// <summary>
        /// Sets the IndeterminateImage for ToggleButton elements (for three-state toggles)
        /// </summary>
        public static void SetIndeterminateImage(this UIElement element, AssetReference spriteSheet, int frame = 0)
        {
            element.Set("IndeterminateImage", new Dictionary<string, object>
            {
                ["!SpriteFromSheet"] = "",
                ["Sheet"] = spriteSheet.Reference,
                ["CurrentFrame"] = frame
            });
        }

        /// <summary>
        /// Sets all ToggleButton images at once (checked, unchecked, and optionally indeterminate)
        /// </summary>
        public static void SetToggleButtonImages(this UIElement element, AssetReference spriteSheet,
            int checkedFrame, int uncheckedFrame, int? indeterminateFrame = null)
        {
            element.SetCheckedImage(spriteSheet, checkedFrame);
            element.SetUncheckedImage(spriteSheet, uncheckedFrame);
            if (indeterminateFrame.HasValue)
            {
                element.SetIndeterminateImage(spriteSheet, indeterminateFrame.Value);
                element.Set("IsThreeState", true);
            }
        }

        /// <summary>
        /// Sets the CheckedImage for ToggleButton using a texture directly
        /// </summary>
        public static void SetCheckedTexture(this UIElement element, AssetReference texture)
        {
            element.Set("CheckedImage", new Dictionary<string, object>
            {
                ["!SpriteFromTexture"] = "",
                ["Texture"] = texture.Reference
            });
        }

        /// <summary>
        /// Sets the UncheckedImage for ToggleButton using a texture directly
        /// </summary>
        public static void SetUncheckedTexture(this UIElement element, AssetReference texture)
        {
            element.Set("UncheckedImage", new Dictionary<string, object>
            {
                ["!SpriteFromTexture"] = "",
                ["Texture"] = texture.Reference
            });
        }

        /// <summary>
        /// Sets the IndeterminateImage for ToggleButton using a texture directly
        /// </summary>
        public static void SetIndeterminateTexture(this UIElement element, AssetReference texture)
        {
            element.Set("IndeterminateImage", new Dictionary<string, object>
            {
                ["!SpriteFromTexture"] = "",
                ["Texture"] = texture.Reference
            });
        }

        /// <summary>
        /// Sets all ToggleButton images at once using textures
        /// </summary>
        public static void SetToggleButtonTextures(this UIElement element,
            AssetReference checkedTexture, AssetReference uncheckedTexture, AssetReference? indeterminateTexture = null)
        {
            element.SetCheckedTexture(checkedTexture);
            element.SetUncheckedTexture(uncheckedTexture);
            if (indeterminateTexture != null)
            {
                element.SetIndeterminateTexture(indeterminateTexture);
                element.Set("IsThreeState", true);
            }
        }

        // ===== Border Methods =====

        /// <summary>
        /// Sets the border color for Border elements
        /// </summary>
        public static void SetBorderColor(this UIElement element, int r, int g, int b, int a = 255)
        {
            element.Set("BorderColor", new Dictionary<string, object>
            {
                ["R"] = r,
                ["G"] = g,
                ["B"] = b,
                ["A"] = a
            });
        }

        /// <summary>
        /// Sets the border thickness for Border elements
        /// </summary>
        public static void SetBorderThickness(this UIElement element,
            float? left = null,
            float? top = null,
            float? right = null,
            float? bottom = null)
        {
            var thickness = new Dictionary<string, object>();

            if (left.HasValue) thickness["Left"] = left.Value;
            if (top.HasValue) thickness["Top"] = top.Value;
            if (right.HasValue) thickness["Right"] = right.Value;
            if (bottom.HasValue) thickness["Bottom"] = bottom.Value;

            element.Set("BorderThickness", thickness);
        }

        /// <summary>
        /// Sets uniform border thickness on all sides
        /// </summary>
        public static void SetBorderThicknessUniform(this UIElement element, float thickness)
        {
            element.SetBorderThickness(thickness, thickness, thickness, thickness);
        }

        // ===== ModalElement Methods =====

        /// <summary>
        /// Sets the overlay color for ModalElement elements
        /// </summary>
        public static void SetOverlayColor(this UIElement element, int r, int g, int b, int a = 153)
        {
            element.Set("OverlayColor", new Dictionary<string, object>
            {
                ["R"] = r,
                ["G"] = g,
                ["B"] = b,
                ["A"] = a
            });
        }

        // ===== Slider Image Methods =====

        /// <summary>
        /// Sets the track background image for Slider elements
        /// </summary>
        public static void SetTrackBackgroundImage(this UIElement element, AssetReference spriteSheet, int frame = 0)
        {
            element.Set("TrackBackgroundImage", new Dictionary<string, object>
            {
                ["!SpriteFromSheet"] = "",
                ["Sheet"] = spriteSheet.Reference,
                ["CurrentFrame"] = frame
            });
        }

        /// <summary>
        /// Sets the track foreground image for Slider elements
        /// </summary>
        public static void SetTrackForegroundImage(this UIElement element, AssetReference spriteSheet, int frame = 0)
        {
            element.Set("TrackForegroundImage", new Dictionary<string, object>
            {
                ["!SpriteFromSheet"] = "",
                ["Sheet"] = spriteSheet.Reference,
                ["CurrentFrame"] = frame
            });
        }

        /// <summary>
        /// Sets the thumb image for Slider elements
        /// </summary>
        public static void SetThumbImage(this UIElement element, AssetReference spriteSheet, int frame = 0)
        {
            element.Set("ThumbImage", new Dictionary<string, object>
            {
                ["!SpriteFromSheet"] = "",
                ["Sheet"] = spriteSheet.Reference,
                ["CurrentFrame"] = frame
            });
        }

        /// <summary>
        /// Sets the mouse over thumb image for Slider elements
        /// </summary>
        public static void SetMouseOverThumbImage(this UIElement element, AssetReference spriteSheet, int frame = 0)
        {
            element.Set("MouseOverThumbImage", new Dictionary<string, object>
            {
                ["!SpriteFromSheet"] = "",
                ["Sheet"] = spriteSheet.Reference,
                ["CurrentFrame"] = frame
            });
        }

        /// <summary>
        /// Sets the track background image for Slider using a texture directly
        /// </summary>
        public static void SetTrackBackgroundTexture(this UIElement element, AssetReference texture)
        {
            element.Set("TrackBackgroundImage", new Dictionary<string, object>
            {
                ["!SpriteFromTexture"] = "",
                ["Texture"] = texture.Reference
            });
        }

        /// <summary>
        /// Sets the track foreground image for Slider using a texture directly
        /// </summary>
        public static void SetTrackForegroundTexture(this UIElement element, AssetReference texture)
        {
            element.Set("TrackForegroundImage", new Dictionary<string, object>
            {
                ["!SpriteFromTexture"] = "",
                ["Texture"] = texture.Reference
            });
        }

        /// <summary>
        /// Sets the thumb image for Slider using a texture directly
        /// </summary>
        public static void SetThumbTexture(this UIElement element, AssetReference texture)
        {
            element.Set("ThumbImage", new Dictionary<string, object>
            {
                ["!SpriteFromTexture"] = "",
                ["Texture"] = texture.Reference
            });
        }

        /// <summary>
        /// Sets the mouse over thumb image for Slider using a texture directly
        /// </summary>
        public static void SetMouseOverThumbTexture(this UIElement element, AssetReference texture)
        {
            element.Set("MouseOverThumbImage", new Dictionary<string, object>
            {
                ["!SpriteFromTexture"] = "",
                ["Texture"] = texture.Reference
            });
        }

        // ===== EditText Image Methods =====

        /// <summary>
        /// Sets the active (focused) image for EditText elements
        /// </summary>
        public static void SetActiveImage(this UIElement element, AssetReference spriteSheet, int frame = 0)
        {
            element.Set("ActiveImage", new Dictionary<string, object>
            {
                ["!SpriteFromSheet"] = "",
                ["Sheet"] = spriteSheet.Reference,
                ["CurrentFrame"] = frame
            });
        }

        /// <summary>
        /// Sets the inactive (unfocused) image for EditText elements
        /// </summary>
        public static void SetInactiveImage(this UIElement element, AssetReference spriteSheet, int frame = 0)
        {
            element.Set("InactiveImage", new Dictionary<string, object>
            {
                ["!SpriteFromSheet"] = "",
                ["Sheet"] = spriteSheet.Reference,
                ["CurrentFrame"] = frame
            });
        }

        /// <summary>
        /// Sets the mouse over image for EditText elements
        /// </summary>
        public static void SetEditTextMouseOverImage(this UIElement element, AssetReference spriteSheet, int frame = 0)
        {
            element.Set("MouseOverImage", new Dictionary<string, object>
            {
                ["!SpriteFromSheet"] = "",
                ["Sheet"] = spriteSheet.Reference,
                ["CurrentFrame"] = frame
            });
        }

        /// <summary>
        /// Sets all three EditText state images at once
        /// </summary>
        public static void SetEditTextImages(this UIElement element, AssetReference spriteSheet,
            int activeFrame, int inactiveFrame, int mouseOverFrame)
        {
            element.SetActiveImage(spriteSheet, activeFrame);
            element.SetInactiveImage(spriteSheet, inactiveFrame);
            element.SetEditTextMouseOverImage(spriteSheet, mouseOverFrame);
        }

        /// <summary>
        /// Sets the active (focused) image for EditText using a texture directly
        /// </summary>
        public static void SetActiveTexture(this UIElement element, AssetReference texture)
        {
            element.Set("ActiveImage", new Dictionary<string, object>
            {
                ["!SpriteFromTexture"] = "",
                ["Texture"] = texture.Reference
            });
        }

        /// <summary>
        /// Sets the inactive (unfocused) image for EditText using a texture directly
        /// </summary>
        public static void SetInactiveTexture(this UIElement element, AssetReference texture)
        {
            element.Set("InactiveImage", new Dictionary<string, object>
            {
                ["!SpriteFromTexture"] = "",
                ["Texture"] = texture.Reference
            });
        }

        /// <summary>
        /// Sets the mouse over image for EditText using a texture directly
        /// </summary>
        public static void SetEditTextMouseOverTexture(this UIElement element, AssetReference texture)
        {
            element.Set("MouseOverImage", new Dictionary<string, object>
            {
                ["!SpriteFromTexture"] = "",
                ["Texture"] = texture.Reference
            });
        }

        /// <summary>
        /// Sets all three EditText state images at once using textures
        /// </summary>
        public static void SetEditTextTextures(this UIElement element,
            AssetReference activeTexture, AssetReference inactiveTexture, AssetReference mouseOverTexture)
        {
            element.SetActiveTexture(activeTexture);
            element.SetInactiveTexture(inactiveTexture);
            element.SetEditTextMouseOverTexture(mouseOverTexture);
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

        // ===== Sprite/Texture Detection Methods =====

        /// <summary>
        /// Checks if a sprite property is using SpriteFromSheet format.
        /// </summary>
        public static bool IsSpriteFromSheet(this UIElement element, string propertyName = "Source")
        {
            if (!element.Properties.TryGetValue(propertyName, out var spriteObj))
                return false;

            if (spriteObj is not Dictionary<string, object> sprite)
                return false;

            // Check for type tag added by parser
            if (sprite.TryGetValue("!TypeTag", out var typeTag) && typeTag?.ToString() == "!SpriteFromSheet")
                return true;

            return sprite.ContainsKey("!SpriteFromSheet") ||
                   (sprite.ContainsKey("Sheet") && !sprite.ContainsKey("Texture"));
        }

        /// <summary>
        /// Checks if a sprite property is using SpriteFromTexture format.
        /// </summary>
        public static bool IsSpriteFromTexture(this UIElement element, string propertyName = "Source")
        {
            if (!element.Properties.TryGetValue(propertyName, out var spriteObj))
                return false;

            if (spriteObj is not Dictionary<string, object> sprite)
                return false;

            // Check for type tag added by parser
            if (sprite.TryGetValue("!TypeTag", out var typeTag) && typeTag?.ToString() == "!SpriteFromTexture")
                return true;

            return sprite.ContainsKey("!SpriteFromTexture") || sprite.ContainsKey("Texture");
        }

        /// <summary>
        /// Checks if a sprite property has any valid source set (not null/empty).
        /// </summary>
        public static bool HasSpriteSource(this UIElement element, string propertyName = "Source")
        {
            if (!element.Properties.TryGetValue(propertyName, out var spriteObj))
                return false;

            if (spriteObj is not Dictionary<string, object> sprite)
                return false;

            // Check for valid Sheet reference
            if (sprite.TryGetValue("Sheet", out var sheet))
            {
                if (sheet != null && sheet.ToString() != "null" && !string.IsNullOrWhiteSpace(sheet.ToString()))
                    return true;
            }

            // Check for valid Texture reference
            if (sprite.TryGetValue("Texture", out var texture))
            {
                if (texture != null && texture.ToString() != "null" && !string.IsNullOrWhiteSpace(texture.ToString()))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Gets the sprite source type for a property.
        /// Returns "SpriteFromSheet", "SpriteFromTexture", or null if not set.
        /// </summary>
        public static string? GetSpriteSourceType(this UIElement element, string propertyName = "Source")
        {
            if (element.IsSpriteFromTexture(propertyName))
                return "SpriteFromTexture";
            if (element.IsSpriteFromSheet(propertyName))
                return "SpriteFromSheet";
            return null;
        }

        /// <summary>
        /// Gets the sprite sheet asset reference and frame for a SpriteFromSheet property.
        /// Returns null if not set or if using SpriteFromTexture.
        /// </summary>
        public static (string? AssetReference, int Frame)? GetSpriteSheet(this UIElement element, string propertyName = "Source")
        {
            if (!element.Properties.TryGetValue(propertyName, out var spriteObj))
                return null;

            if (spriteObj is not Dictionary<string, object> sprite)
                return null;

            // Check type tag or presence of Sheet key
            var hasTypeTag = sprite.TryGetValue("!TypeTag", out var typeTag) && typeTag?.ToString() == "!SpriteFromSheet";
            if (!hasTypeTag && !sprite.ContainsKey("!SpriteFromSheet") && !sprite.ContainsKey("Sheet"))
                return null;

            string? assetRef = null;
            int frame = 0;

            if (sprite.TryGetValue("Sheet", out var sheet) && sheet != null && sheet.ToString() != "null")
                assetRef = sheet.ToString();

            if (sprite.TryGetValue("CurrentFrame", out var frameObj))
                frame = Convert.ToInt32(frameObj);

            return (assetRef, frame);
        }

        /// <summary>
        /// Gets the texture asset reference for a SpriteFromTexture property.
        /// Returns null if not set or if using SpriteFromSheet.
        /// </summary>
        public static string? GetTextureSource(this UIElement element, string propertyName = "Source")
        {
            if (!element.Properties.TryGetValue(propertyName, out var spriteObj))
                return null;

            if (spriteObj is not Dictionary<string, object> sprite)
                return null;

            // Check type tag or presence of Texture key
            var hasTypeTag = sprite.TryGetValue("!TypeTag", out var typeTag) && typeTag?.ToString() == "!SpriteFromTexture";
            if (!hasTypeTag && !sprite.ContainsKey("!SpriteFromTexture") && !sprite.ContainsKey("Texture"))
                return null;

            if (sprite.TryGetValue("Texture", out var texture) && texture != null && texture.ToString() != "null")
                return texture.ToString();

            return null;
        }

        /// <summary>
        /// Clears a sprite property (removes the source).
        /// </summary>
        public static void ClearSpriteSource(this UIElement element, string propertyName = "Source")
        {
            element.Properties.Remove(propertyName);
        }

        // ===== ImageElement Sprite Getters =====

        /// <summary>
        /// Gets the Source sprite info for ImageElement.
        /// Returns sprite sheet reference and frame, or null.
        /// </summary>
        public static (string? AssetReference, int Frame)? GetImageSprite(this UIElement element)
        {
            return element.GetSpriteSheet("Source");
        }

        /// <summary>
        /// Gets the Source texture for ImageElement.
        /// Returns texture reference, or null.
        /// </summary>
        public static string? GetImageTexture(this UIElement element)
        {
            return element.GetTextureSource("Source");
        }

        /// <summary>
        /// Checks if ImageElement is using a sprite sheet.
        /// </summary>
        public static bool IsImageSpriteSheet(this UIElement element)
        {
            return element.IsSpriteFromSheet("Source");
        }

        /// <summary>
        /// Checks if ImageElement is using a texture.
        /// </summary>
        public static bool IsImageTexture(this UIElement element)
        {
            return element.IsSpriteFromTexture("Source");
        }

        // ===== Button Sprite Getters =====

        /// <summary>
        /// Gets the PressedImage sprite info for Button.
        /// </summary>
        public static (string? AssetReference, int Frame)? GetPressedImageSprite(this UIElement element)
        {
            return element.GetSpriteSheet("PressedImage");
        }

        /// <summary>
        /// Gets the PressedImage texture for Button.
        /// </summary>
        public static string? GetPressedImageTexture(this UIElement element)
        {
            return element.GetTextureSource("PressedImage");
        }

        /// <summary>
        /// Gets the NotPressedImage sprite info for Button.
        /// </summary>
        public static (string? AssetReference, int Frame)? GetNotPressedImageSprite(this UIElement element)
        {
            return element.GetSpriteSheet("NotPressedImage");
        }

        /// <summary>
        /// Gets the NotPressedImage texture for Button.
        /// </summary>
        public static string? GetNotPressedImageTexture(this UIElement element)
        {
            return element.GetTextureSource("NotPressedImage");
        }

        /// <summary>
        /// Gets the MouseOverImage sprite info for Button.
        /// </summary>
        public static (string? AssetReference, int Frame)? GetMouseOverImageSprite(this UIElement element)
        {
            return element.GetSpriteSheet("MouseOverImage");
        }

        /// <summary>
        /// Gets the MouseOverImage texture for Button.
        /// </summary>
        public static string? GetMouseOverImageTexture(this UIElement element)
        {
            return element.GetTextureSource("MouseOverImage");
        }

        /// <summary>
        /// Checks if Button images are using sprite sheets (checks PressedImage).
        /// </summary>
        public static bool IsButtonUsingSpriteSheet(this UIElement element)
        {
            return element.IsSpriteFromSheet("PressedImage") ||
                   element.IsSpriteFromSheet("NotPressedImage") ||
                   element.IsSpriteFromSheet("MouseOverImage");
        }

        /// <summary>
        /// Checks if Button images are using textures (checks PressedImage).
        /// </summary>
        public static bool IsButtonUsingTexture(this UIElement element)
        {
            return element.IsSpriteFromTexture("PressedImage") ||
                   element.IsSpriteFromTexture("NotPressedImage") ||
                   element.IsSpriteFromTexture("MouseOverImage");
        }

        // ===== ToggleButton Sprite Getters =====

        /// <summary>
        /// Gets the CheckedImage sprite info for ToggleButton.
        /// </summary>
        public static (string? AssetReference, int Frame)? GetCheckedImageSprite(this UIElement element)
        {
            return element.GetSpriteSheet("CheckedImage");
        }

        /// <summary>
        /// Gets the UncheckedImage sprite info for ToggleButton.
        /// </summary>
        public static (string? AssetReference, int Frame)? GetUncheckedImageSprite(this UIElement element)
        {
            return element.GetSpriteSheet("UncheckedImage");
        }

        /// <summary>
        /// Gets the IndeterminateImage sprite info for ToggleButton.
        /// </summary>
        public static (string? AssetReference, int Frame)? GetIndeterminateImageSprite(this UIElement element)
        {
            return element.GetSpriteSheet("IndeterminateImage");
        }

        // ===== Slider Sprite Getters =====

        /// <summary>
        /// Gets the TrackBackgroundImage sprite info for Slider.
        /// </summary>
        public static (string? AssetReference, int Frame)? GetTrackBackgroundImageSprite(this UIElement element)
        {
            return element.GetSpriteSheet("TrackBackgroundImage");
        }

        /// <summary>
        /// Gets the ThumbImage sprite info for Slider.
        /// </summary>
        public static (string? AssetReference, int Frame)? GetThumbImageSprite(this UIElement element)
        {
            return element.GetSpriteSheet("ThumbImage");
        }

        // ===== EditText Sprite Getters =====

        /// <summary>
        /// Gets the ActiveImage sprite info for EditText.
        /// </summary>
        public static (string? AssetReference, int Frame)? GetActiveImageSprite(this UIElement element)
        {
            return element.GetSpriteSheet("ActiveImage");
        }

        /// <summary>
        /// Gets the InactiveImage sprite info for EditText.
        /// </summary>
        public static (string? AssetReference, int Frame)? GetInactiveImageSprite(this UIElement element)
        {
            return element.GetSpriteSheet("InactiveImage");
        }

        // ===== Content Methods (Button, ToggleButton, ScrollViewer) =====

        /// <summary>
        /// Sets the content element for Button or ToggleButton.
        /// The content element must already exist in the page.
        /// </summary>
        public static void SetContent(this UIElement element, UIElement content)
        {
            if (element.Type != "Button" && element.Type != "ToggleButton" && element.Type != "ScrollViewer")
                throw new InvalidOperationException($"SetContent can only be used on Button, ToggleButton, or ScrollViewer elements, not {element.Type}");

            element.Set("Content", $"!{content.Type} ref!! {content.Id}");
        }

        /// <summary>
        /// Gets the content element reference for Button, ToggleButton, or ScrollViewer.
        /// Returns the referenced UIElement if found, null otherwise.
        /// </summary>
        public static UIElement? GetContent(this UIElement element)
        {
            if (element.ParentPage == null)
                return null;

            var contentValue = element.Get<string>("Content");
            if (string.IsNullOrEmpty(contentValue))
                return null;

            // Parse reference format: "!TextBlock ref!! guid"
            if (contentValue.Contains("ref!!"))
            {
                var parts = contentValue.Split(new[] { "ref!!" }, StringSplitOptions.None);
                if (parts.Length == 2)
                {
                    var refId = parts[1].Trim();
                    return element.ParentPage.FindElementById(refId);
                }
            }

            return null;
        }

        // ===== Position/Margin Getter Methods =====

        /// <summary>
        /// Gets the position (X, Y) from Margin.Left and Margin.Top.
        /// Returns (0, 0) if Margin is not set.
        /// </summary>
        public static (float X, float Y) GetPosition(this UIElement element)
        {
            var margin = element.GetMargin();
            return (margin.Left, margin.Top);
        }

        /// <summary>
        /// Sets the position using all 4 margin values for proper Stride UI positioning.
        /// Calculates Right/Bottom margins based on element size and parent container size.
        /// Formula: Right = ParentWidth - X - Width, Bottom = ParentHeight - Y - Height
        /// </summary>
        public static void SetPosition(this UIElement element, float x, float y)
        {
            // Get element size (default to 0 if not set)
            var width = element.GetWidth() ?? 0f;
            var height = element.GetHeight() ?? 0f;

            // Get parent container dimensions
            var (parentWidth, parentHeight) = element.GetParentDimensions();

            // Calculate all 4 margins using Stride's formula
            float left = x;
            float top = y;
            float right = parentWidth - (x + width);
            float bottom = parentHeight - (y + height);

            // Ensure alignment is Left/Top for absolute positioning
            element.Set("HorizontalAlignment", "Left");
            element.Set("VerticalAlignment", "Top");

            element.SetMargin(left: left, top: top, right: right, bottom: bottom);
        }

        /// <summary>
        /// Gets the parent container dimensions.
        /// Returns design resolution if parent is root, otherwise returns parent's Width/Height.
        /// </summary>
        public static (float Width, float Height) GetParentDimensions(this UIElement element)
        {
            // If element has a parent UIElement with explicit size, use that
            if (element.Parent != null)
            {
                var parentWidth = element.Parent.GetWidth();
                var parentHeight = element.Parent.GetHeight();

                if (parentWidth.HasValue && parentHeight.HasValue)
                {
                    return (parentWidth.Value, parentHeight.Value);
                }
            }

            // Otherwise, use design resolution from the page
            if (element.ParentPage != null)
            {
                var resolution = element.ParentPage.Resolution;
                if (resolution.TryGetValue("X", out var resX) && resolution.TryGetValue("Y", out var resY))
                {
                    return (resX, resY);
                }
            }

            // Fallback to common 1920x1080 if nothing else available
            return (1920f, 1080f);
        }

        /// <summary>
        /// Gets the full Margin values (Left, Top, Right, Bottom).
        /// Returns (0, 0, 0, 0) for any values not set.
        /// </summary>
        public static (float Left, float Top, float Right, float Bottom) GetMargin(this UIElement element)
        {
            if (!element.Properties.TryGetValue("Margin", out var marginObj))
                return (0, 0, 0, 0);

            if (marginObj is not Dictionary<string, object> margin)
                return (0, 0, 0, 0);

            float left = 0, top = 0, right = 0, bottom = 0;

            if (margin.TryGetValue("Left", out var l))
                left = Convert.ToSingle(l);
            if (margin.TryGetValue("Top", out var t))
                top = Convert.ToSingle(t);
            if (margin.TryGetValue("Right", out var r))
                right = Convert.ToSingle(r);
            if (margin.TryGetValue("Bottom", out var b))
                bottom = Convert.ToSingle(b);

            return (left, top, right, bottom);
        }

        /// <summary>
        /// Gets the Width of an element. Returns null if not explicitly set.
        /// </summary>
        public static float? GetWidth(this UIElement element)
        {
            var value = element.Get<object>("Width");
            if (value == null) return null;
            return Convert.ToSingle(value);
        }

        /// <summary>
        /// Gets the Height of an element. Returns null if not explicitly set.
        /// </summary>
        public static float? GetHeight(this UIElement element)
        {
            var value = element.Get<object>("Height");
            if (value == null) return null;
            return Convert.ToSingle(value);
        }

        /// <summary>
        /// Gets the size (Width, Height) of an element.
        /// Returns null for dimensions not explicitly set.
        /// </summary>
        public static (float? Width, float? Height) GetSize(this UIElement element)
        {
            return (element.GetWidth(), element.GetHeight());
        }

        // ===== Color Getter Methods =====

        /// <summary>
        /// Gets the BackgroundColor as RGBA values.
        /// Returns (0, 0, 0, 0) if not set.
        /// </summary>
        public static (int R, int G, int B, int A) GetBackgroundColor(this UIElement element)
        {
            return GetColorProperty(element, "BackgroundColor");
        }

        /// <summary>
        /// Gets the TextColor for TextBlock elements.
        /// Returns (240, 240, 240, 255) default if not set.
        /// </summary>
        public static (int R, int G, int B, int A) GetTextColor(this UIElement element)
        {
            return GetColorProperty(element, "TextColor", 240, 240, 240, 255);
        }

        /// <summary>
        /// Gets the Color (tint) for ImageElement elements.
        /// Returns (255, 255, 255, 255) default if not set.
        /// </summary>
        public static (int R, int G, int B, int A) GetColor(this UIElement element)
        {
            return GetColorProperty(element, "Color", 255, 255, 255, 255);
        }

        private static (int R, int G, int B, int A) GetColorProperty(UIElement element, string propertyName,
            int defaultR = 0, int defaultG = 0, int defaultB = 0, int defaultA = 0)
        {
            if (!element.Properties.TryGetValue(propertyName, out var colorObj))
                return (defaultR, defaultG, defaultB, defaultA);

            if (colorObj is not Dictionary<string, object> color)
                return (defaultR, defaultG, defaultB, defaultA);

            int r = defaultR, g = defaultG, b = defaultB, a = defaultA;

            if (color.TryGetValue("R", out var rv)) r = Convert.ToInt32(rv);
            if (color.TryGetValue("G", out var gv)) g = Convert.ToInt32(gv);
            if (color.TryGetValue("B", out var bv)) b = Convert.ToInt32(bv);
            if (color.TryGetValue("A", out var av)) a = Convert.ToInt32(av);

            return (r, g, b, a);
        }

        // ===== Text Getter Methods =====

        /// <summary>
        /// Gets the Text content for TextBlock or EditText elements.
        /// Returns empty string if not set.
        /// </summary>
        public static string GetText(this UIElement element)
        {
            return element.Get<string>("Text") ?? "";
        }

        /// <summary>
        /// Gets the TextSize (font size) for TextBlock or EditText elements.
        /// Returns 20.0 default if not set.
        /// </summary>
        public static float GetFontSize(this UIElement element)
        {
            var value = element.Get<object>("TextSize");
            if (value == null) return 20.0f;
            return Convert.ToSingle(value);
        }

        /// <summary>
        /// Gets the Font asset reference string for TextBlock or EditText elements.
        /// Returns the default Stride font reference if not set.
        /// </summary>
        public static string GetFont(this UIElement element)
        {
            return element.Get<string>("Font") ?? "c90f3988-0544-4cbe-993f-13af7d9c23c6:StrideDefaultFont";
        }

        // ===== Alignment Getter Methods =====

        /// <summary>
        /// Gets the HorizontalAlignment. Returns "Stretch" if not set.
        /// </summary>
        public static string GetHorizontalAlignment(this UIElement element)
        {
            return element.Get<string>("HorizontalAlignment") ?? "Stretch";
        }

        /// <summary>
        /// Gets the VerticalAlignment. Returns "Stretch" if not set.
        /// </summary>
        public static string GetVerticalAlignment(this UIElement element)
        {
            return element.Get<string>("VerticalAlignment") ?? "Stretch";
        }

        /// <summary>
        /// Gets the alignment as a tuple.
        /// </summary>
        public static (string Horizontal, string Vertical) GetAlignment(this UIElement element)
        {
            return (element.GetHorizontalAlignment(), element.GetVerticalAlignment());
        }

        // ===== Slider Getter Methods =====

        /// <summary>
        /// Gets the current Value of a Slider. Returns 50 if not set.
        /// </summary>
        public static float GetSliderValue(this UIElement slider)
        {
            var value = slider.Get<object>("Value");
            if (value == null) return 50f;
            return Convert.ToSingle(value);
        }

        /// <summary>
        /// Gets the Minimum value of a Slider. Returns 0 if not set.
        /// </summary>
        public static float GetSliderMinimum(this UIElement slider)
        {
            var value = slider.Get<object>("Minimum");
            if (value == null) return 0f;
            return Convert.ToSingle(value);
        }

        /// <summary>
        /// Gets the Maximum value of a Slider. Returns 100 if not set.
        /// </summary>
        public static float GetSliderMaximum(this UIElement slider)
        {
            var value = slider.Get<object>("Maximum");
            if (value == null) return 100f;
            return Convert.ToSingle(value);
        }

        /// <summary>
        /// Gets the full range of a Slider as (Min, Max, Value).
        /// </summary>
        public static (float Min, float Max, float Value) GetSliderRange(this UIElement slider)
        {
            return (slider.GetSliderMinimum(), slider.GetSliderMaximum(), slider.GetSliderValue());
        }

        // ===== Visibility/Interaction Getter Methods =====

        /// <summary>
        /// Gets whether the element is visible. Returns true if Visibility is not "Hidden".
        /// </summary>
        public static bool IsVisible(this UIElement element)
        {
            var visibility = element.Get<string>("Visibility");
            return visibility != "Hidden";
        }

        /// <summary>
        /// Gets the DrawLayerNumber (z-order). Returns 0 if not set.
        /// </summary>
        public static int GetDrawLayer(this UIElement element)
        {
            var value = element.Get<object>("DrawLayerNumber");
            if (value == null) return 0;
            return Convert.ToInt32(value);
        }

        /// <summary>
        /// Gets whether the element can be hit by user input. Returns true if not explicitly set.
        /// </summary>
        public static bool GetCanBeHitByUser(this UIElement element)
        {
            var value = element.Get<object>("CanBeHitByUser");
            if (value == null) return true;
            return Convert.ToBoolean(value);
        }

        /// <summary>
        /// Gets the Opacity of an element. Returns 1.0 if not set.
        /// </summary>
        public static float GetOpacity(this UIElement element)
        {
            var value = element.Get<object>("Opacity");
            if (value == null) return 1.0f;
            return Convert.ToSingle(value);
        }

        // ===== StackPanel Getter Methods =====

        /// <summary>
        /// Gets the Orientation of a StackPanel. Returns "Vertical" if not set.
        /// </summary>
        public static string GetOrientation(this UIElement stackPanel)
        {
            return stackPanel.Get<string>("Orientation") ?? "Vertical";
        }

        /// <summary>
        /// Returns true if StackPanel orientation is Vertical.
        /// </summary>
        public static bool IsVerticalOrientation(this UIElement stackPanel)
        {
            return stackPanel.GetOrientation() == "Vertical";
        }

        /// <summary>
        /// Returns true if StackPanel orientation is Horizontal.
        /// </summary>
        public static bool IsHorizontalOrientation(this UIElement stackPanel)
        {
            return stackPanel.GetOrientation() == "Horizontal";
        }

        // ===== Common Behavior Properties (All Elements) =====

        /// <summary>
        /// Gets whether the element is enabled. Returns true if not set.
        /// </summary>
        public static bool GetIsEnabled(this UIElement element)
        {
            var value = element.Get<object>("IsEnabled");
            if (value == null) return true;
            return Convert.ToBoolean(value);
        }

        /// <summary>
        /// Sets whether the element is enabled.
        /// </summary>
        public static void SetIsEnabled(this UIElement element, bool isEnabled)
        {
            element.Set("IsEnabled", isEnabled);
        }

        /// <summary>
        /// Gets the Grid.Z Index or Canvas.Z Index for z-ordering within parent.
        /// </summary>
        public static int GetZIndex(this UIElement element)
        {
            // Try Canvas.ZIndex first, then Grid.ZIndex
            var value = element.Get<object>("Canvas.ZIndex") ?? element.Get<object>("Grid.ZIndex");
            if (value == null) return 0;
            return Convert.ToInt32(value);
        }

        /// <summary>
        /// Sets the Z Index for z-ordering within parent (works for both Canvas and Grid).
        /// </summary>
        public static void SetZIndex(this UIElement element, int zIndex)
        {
            element.Set("Canvas.ZIndex", zIndex);
        }

        // ===== ClickMode Property (Button, ToggleButton, ModalElement) =====

        /// <summary>
        /// Gets the ClickMode for Button/ToggleButton/ModalElement.
        /// Returns "Release" if not set. Options: Release, Press, Hover
        /// </summary>
        public static string GetClickMode(this UIElement element)
        {
            return element.Get<string>("ClickMode") ?? "Release";
        }

        /// <summary>
        /// Sets the ClickMode for Button/ToggleButton/ModalElement.
        /// Options: Release (default), Press, Hover
        /// </summary>
        public static void SetClickMode(this UIElement element, string clickMode)
        {
            element.Set("ClickMode", clickMode);
        }

        // ===== TextBlock Behavior Properties =====

        /// <summary>
        /// Gets whether text wrapping is enabled. Returns false if not set.
        /// </summary>
        public static bool GetWrapText(this UIElement element)
        {
            var value = element.Get<object>("WrapText");
            if (value == null) return false;
            return Convert.ToBoolean(value);
        }

        /// <summary>
        /// Sets whether text should wrap to multiple lines.
        /// </summary>
        public static void SetWrapText(this UIElement element, bool wrapText)
        {
            element.Set("WrapText", wrapText);
        }

        /// <summary>
        /// Gets whether text snapping is disabled. Returns false if not set.
        /// </summary>
        public static bool GetDoNotSnapText(this UIElement element)
        {
            var value = element.Get<object>("DoNotSnapText");
            if (value == null) return false;
            return Convert.ToBoolean(value);
        }

        /// <summary>
        /// Sets whether to disable text snapping to pixel boundaries.
        /// </summary>
        public static void SetDoNotSnapText(this UIElement element, bool doNotSnap)
        {
            element.Set("DoNotSnapText", doNotSnap);
        }

        /// <summary>
        /// Gets whether synchronous character generation is enabled. Returns false if not set.
        /// </summary>
        public static bool GetSynchronousCharacterGeneration(this UIElement element)
        {
            var value = element.Get<object>("SynchronousCharacterGeneration");
            if (value == null) return false;
            return Convert.ToBoolean(value);
        }

        /// <summary>
        /// Sets whether character generation should be synchronous.
        /// </summary>
        public static void SetSynchronousCharacterGeneration(this UIElement element, bool synchronous)
        {
            element.Set("SynchronousCharacterGeneration", synchronous);
        }

        // ===== ImageElement Properties =====

        /// <summary>
        /// Gets the StretchType for ImageElement. Returns "FillOnStretch" if not set.
        /// Options: FillOnStretch, Fill, Uniform, UniformToFill
        /// </summary>
        public static string GetStretchType(this UIElement element)
        {
            return element.Get<string>("StretchType") ?? "FillOnStretch";
        }

        /// <summary>
        /// Sets the StretchType for ImageElement.
        /// </summary>
        public static void SetStretchType(this UIElement element, string stretchType)
        {
            element.Set("StretchType", stretchType);
        }

        /// <summary>
        /// Gets the StretchDirection for ImageElement. Returns "Both" if not set.
        /// Options: Both, UpOnly, DownOnly
        /// </summary>
        public static string GetStretchDirection(this UIElement element)
        {
            return element.Get<string>("StretchDirection") ?? "Both";
        }

        /// <summary>
        /// Sets the StretchDirection for ImageElement.
        /// </summary>
        public static void SetStretchDirection(this UIElement element, string stretchDirection)
        {
            element.Set("StretchDirection", stretchDirection);
        }

        // ===== EditText Additional Properties =====

        /// <summary>
        /// Gets the minimum number of lines for EditText. Returns 1 if not set.
        /// </summary>
        public static int GetMinLines(this UIElement element)
        {
            var value = element.Get<object>("MinLines");
            if (value == null) return 1;
            return Convert.ToInt32(value);
        }

        /// <summary>
        /// Sets the minimum number of lines for EditText.
        /// </summary>
        public static void SetMinLines(this UIElement element, int minLines)
        {
            element.Set("MinLines", minLines);
        }

        /// <summary>
        /// Gets the maximum number of lines for EditText. Returns int.MaxValue if not set.
        /// </summary>
        public static int GetMaxLines(this UIElement element)
        {
            var value = element.Get<object>("MaxLines");
            if (value == null) return int.MaxValue;
            return Convert.ToInt32(value);
        }

        /// <summary>
        /// Sets the maximum number of lines for EditText.
        /// </summary>
        public static void SetMaxLines(this UIElement element, int maxLines)
        {
            element.Set("MaxLines", maxLines);
        }

        /// <summary>
        /// Gets the InputType for EditText (for on-screen keyboard).
        /// </summary>
        public static string? GetInputType(this UIElement element)
        {
            return element.Get<string>("InputType");
        }

        /// <summary>
        /// Sets the InputType for EditText (for on-screen keyboard).
        /// </summary>
        public static void SetInputType(this UIElement element, string inputType)
        {
            element.Set("InputType", inputType);
        }

        /// <summary>
        /// Gets the caret blink frequency for EditText. Returns 1.0 if not set.
        /// </summary>
        public static float GetCaretFrequency(this UIElement element)
        {
            var value = element.Get<object>("CaretFrequency");
            if (value == null) return 1.0f;
            return Convert.ToSingle(value);
        }

        /// <summary>
        /// Sets the caret blink frequency for EditText.
        /// </summary>
        public static void SetCaretFrequency(this UIElement element, float frequency)
        {
            element.Set("CaretFrequency", frequency);
        }

        /// <summary>
        /// Gets the MaxLength for EditText. Returns int.MaxValue if not set.
        /// </summary>
        public static int GetMaxLength(this UIElement element)
        {
            var value = element.Get<object>("MaxLength");
            if (value == null) return int.MaxValue;
            return Convert.ToInt32(value);
        }

        /// <summary>
        /// Gets whether EditText is read-only. Returns false if not set.
        /// </summary>
        public static bool GetIsReadOnly(this UIElement element)
        {
            var value = element.Get<object>("IsReadOnly");
            if (value == null) return false;
            return Convert.ToBoolean(value);
        }

        /// <summary>
        /// Sets whether EditText is read-only.
        /// </summary>
        public static void SetIsReadOnly(this UIElement element, bool isReadOnly)
        {
            element.Set("IsReadOnly", isReadOnly);
        }

        // ===== Slider Additional Properties =====

        /// <summary>
        /// Sets the tick image for Slider.
        /// </summary>
        public static void SetTickImage(this UIElement element, AssetReference spriteSheet, int frame = 0)
        {
            element.Set("TickImage", new Dictionary<string, object>
            {
                ["!SpriteFromSheet"] = "",
                ["Sheet"] = spriteSheet.Reference,
                ["CurrentFrame"] = frame
            });
        }

        /// <summary>
        /// Gets the tick offset for Slider. Returns 0 if not set.
        /// </summary>
        public static float GetTickOffset(this UIElement element)
        {
            var value = element.Get<object>("TickOffset");
            if (value == null) return 0f;
            return Convert.ToSingle(value);
        }

        /// <summary>
        /// Sets the tick offset for Slider.
        /// </summary>
        public static void SetTickOffset(this UIElement element, float offset)
        {
            element.Set("TickOffset", offset);
        }

        /// <summary>
        /// Gets whether ticks are displayed on Slider. Returns false if not set.
        /// </summary>
        public static bool GetAreTicksDisplayed(this UIElement element)
        {
            var value = element.Get<object>("AreTicksDisplayed");
            if (value == null) return false;
            return Convert.ToBoolean(value);
        }

        /// <summary>
        /// Sets whether ticks are displayed on Slider.
        /// </summary>
        public static void SetAreTicksDisplayed(this UIElement element, bool display)
        {
            element.Set("AreTicksDisplayed", display);
        }

        /// <summary>
        /// Gets whether Slider should snap to tick values. Returns false if not set.
        /// </summary>
        public static bool GetShouldSnapToTicks(this UIElement element)
        {
            var value = element.Get<object>("ShouldSnapToTicks");
            if (value == null) return false;
            return Convert.ToBoolean(value);
        }

        /// <summary>
        /// Sets whether Slider should snap to tick values.
        /// </summary>
        public static void SetShouldSnapToTicks(this UIElement element, bool snap)
        {
            element.Set("ShouldSnapToTicks", snap);
        }

        /// <summary>
        /// Gets whether Slider direction is reversed. Returns false if not set.
        /// </summary>
        public static bool GetIsDirectionReversed(this UIElement element)
        {
            var value = element.Get<object>("IsDirectionReversed");
            if (value == null) return false;
            return Convert.ToBoolean(value);
        }

        /// <summary>
        /// Sets whether Slider direction is reversed.
        /// </summary>
        public static void SetIsDirectionReversed(this UIElement element, bool reversed)
        {
            element.Set("IsDirectionReversed", reversed);
        }

        /// <summary>
        /// Gets the track starting offsets for Slider (X, Y).
        /// </summary>
        public static (float X, float Y) GetTrackStartingOffsets(this UIElement element)
        {
            if (!element.Properties.TryGetValue("TrackStartingOffsets", out var offsetObj))
                return (0, 0);

            if (offsetObj is not Dictionary<string, object> offsets)
                return (0, 0);

            float x = 0, y = 0;
            if (offsets.TryGetValue("X", out var xv)) x = Convert.ToSingle(xv);
            if (offsets.TryGetValue("Y", out var yv)) y = Convert.ToSingle(yv);

            return (x, y);
        }

        /// <summary>
        /// Sets the track starting offsets for Slider.
        /// </summary>
        public static void SetTrackStartingOffsets(this UIElement element, float x, float y)
        {
            element.Set("TrackStartingOffsets", new Dictionary<string, object>
            {
                ["X"] = x,
                ["Y"] = y
            });
        }

        // ===== ScrollViewer Additional Properties =====

        /// <summary>
        /// Gets the scroll bar thickness for ScrollViewer. Returns 6 if not set.
        /// </summary>
        public static float GetScrollBarThickness(this UIElement element)
        {
            var value = element.Get<object>("ScrollBarThickness");
            if (value == null) return 6f;
            return Convert.ToSingle(value);
        }

        /// <summary>
        /// Sets the scroll bar thickness for ScrollViewer.
        /// </summary>
        public static void SetScrollBarThickness(this UIElement element, float thickness)
        {
            element.Set("ScrollBarThickness", thickness);
        }

        /// <summary>
        /// Gets the scroll mode for ScrollViewer. Returns "Horizontal" if not set.
        /// Options: Horizontal, Vertical, Both
        /// </summary>
        public static string GetScrollMode(this UIElement element)
        {
            return element.Get<string>("ScrollMode") ?? "Horizontal";
        }

        /// <summary>
        /// Sets the scroll mode for ScrollViewer.
        /// </summary>
        public static void SetScrollMode(this UIElement element, string scrollMode)
        {
            element.Set("ScrollMode", scrollMode);
        }

        /// <summary>
        /// Gets the scroll start threshold for ScrollViewer. Returns 10 if not set.
        /// </summary>
        public static float GetScrollStartThreshold(this UIElement element)
        {
            var value = element.Get<object>("ScrollStartThreshold");
            if (value == null) return 10f;
            return Convert.ToSingle(value);
        }

        /// <summary>
        /// Sets the scroll start threshold for ScrollViewer.
        /// </summary>
        public static void SetScrollStartThreshold(this UIElement element, float threshold)
        {
            element.Set("ScrollStartThreshold", threshold);
        }

        /// <summary>
        /// Gets the deceleration for ScrollViewer. Returns 1500 if not set.
        /// </summary>
        public static float GetDeceleration(this UIElement element)
        {
            var value = element.Get<object>("Deceleration");
            if (value == null) return 1500f;
            return Convert.ToSingle(value);
        }

        /// <summary>
        /// Sets the deceleration for ScrollViewer.
        /// </summary>
        public static void SetDeceleration(this UIElement element, float deceleration)
        {
            element.Set("Deceleration", deceleration);
        }

        /// <summary>
        /// Gets whether touch scrolling is enabled for ScrollViewer. Returns true if not set.
        /// </summary>
        public static bool GetTouchScrollingEnabled(this UIElement element)
        {
            var value = element.Get<object>("TouchScrollingEnabled");
            if (value == null) return true;
            return Convert.ToBoolean(value);
        }

        /// <summary>
        /// Sets whether touch scrolling is enabled for ScrollViewer.
        /// </summary>
        public static void SetTouchScrollingEnabled(this UIElement element, bool enabled)
        {
            element.Set("TouchScrollingEnabled", enabled);
        }

        /// <summary>
        /// Gets whether snap to anchors is enabled for ScrollViewer. Returns false if not set.
        /// </summary>
        public static bool GetSnapToAnchors(this UIElement element)
        {
            var value = element.Get<object>("SnapToAnchors");
            if (value == null) return false;
            return Convert.ToBoolean(value);
        }

        /// <summary>
        /// Sets whether snap to anchors is enabled for ScrollViewer.
        /// </summary>
        public static void SetSnapToAnchors(this UIElement element, bool snap)
        {
            element.Set("SnapToAnchors", snap);
        }

        // ===== ScrollingText Properties =====

        /// <summary>
        /// Gets the scrolling speed for ScrollingText. Returns 40 if not set.
        /// </summary>
        public static float GetScrollingSpeed(this UIElement element)
        {
            var value = element.Get<object>("ScrollingSpeed");
            if (value == null) return 40f;
            return Convert.ToSingle(value);
        }

        /// <summary>
        /// Sets the scrolling speed for ScrollingText.
        /// </summary>
        public static void SetScrollingSpeed(this UIElement element, float speed)
        {
            element.Set("ScrollingSpeed", speed);
        }

        /// <summary>
        /// Gets the desired character number for ScrollingText. Returns 10 if not set.
        /// </summary>
        public static int GetDesiredCharacterNumber(this UIElement element)
        {
            var value = element.Get<object>("DesiredCharacterNumber");
            if (value == null) return 10;
            return Convert.ToInt32(value);
        }

        /// <summary>
        /// Sets the desired character number for ScrollingText.
        /// </summary>
        public static void SetDesiredCharacterNumber(this UIElement element, int charCount)
        {
            element.Set("DesiredCharacterNumber", charCount);
        }

        /// <summary>
        /// Gets whether text repeats for ScrollingText. Returns true if not set.
        /// </summary>
        public static bool GetRepeatText(this UIElement element)
        {
            var value = element.Get<object>("RepeatText");
            if (value == null) return true;
            return Convert.ToBoolean(value);
        }

        /// <summary>
        /// Sets whether text repeats for ScrollingText.
        /// </summary>
        public static void SetRepeatText(this UIElement element, bool repeat)
        {
            element.Set("RepeatText", repeat);
        }

        // ===== ModalElement Properties =====

        /// <summary>
        /// Gets whether ModalElement blocks all input. Returns true if not set.
        /// </summary>
        public static bool GetIsModal(this UIElement element)
        {
            var value = element.Get<object>("IsModal");
            if (value == null) return true;
            return Convert.ToBoolean(value);
        }

        /// <summary>
        /// Sets whether ModalElement blocks all input.
        /// </summary>
        public static void SetIsModal(this UIElement element, bool isModal)
        {
            element.Set("IsModal", isModal);
        }

        // ===== ContentDecorator Properties =====

        /// <summary>
        /// Sets the background image for ContentDecorator.
        /// </summary>
        public static void SetBackgroundImage(this UIElement element, AssetReference spriteSheet, int frame = 0)
        {
            element.Set("BackgroundImage", new Dictionary<string, object>
            {
                ["!SpriteFromSheet"] = "",
                ["Sheet"] = spriteSheet.Reference,
                ["CurrentFrame"] = frame
            });
        }

        /// <summary>
        /// Sets the background image for ContentDecorator using a texture.
        /// </summary>
        public static void SetBackgroundTexture(this UIElement element, AssetReference texture)
        {
            element.Set("BackgroundImage", new Dictionary<string, object>
            {
                ["!SpriteFromTexture"] = "",
                ["Texture"] = texture.Reference
            });
        }

        /// <summary>
        /// Gets the background image sprite info for ContentDecorator.
        /// </summary>
        public static (string? AssetReference, int Frame)? GetBackgroundImageSprite(this UIElement element)
        {
            return element.GetSpriteSheet("BackgroundImage");
        }

        /// <summary>
        /// Gets the background texture for ContentDecorator.
        /// </summary>
        public static string? GetBackgroundImageTexture(this UIElement element)
        {
            return element.GetTextureSource("BackgroundImage");
        }
    }
}
