// HS Stride Editor Toolkit (c) 2025 Happenstance Games LLC - Apache License 2.0

using HS.Stride.Editor.Toolkit.Utilities;

namespace HS.Stride.Editor.Toolkit.Core.UIPageEditing
{
    /// <summary>
    /// Represents a Stride UI page (.sduipage file) that can be loaded, modified, and saved.
    /// Similar to Scene/Prefab but for UI elements.
    /// </summary>
    public class UIPage
    {
        private readonly UIPageContent _content;

        public string Id => _content.Id;
        public string FilePath => _content.FilePath;

        /// <summary>
        /// Design resolution for the UI page
        /// </summary>
        public Dictionary<string, float> Resolution => _content.Resolution;

        /// <summary>
        /// All UI elements in the page
        /// </summary>
        public List<UIElement> AllElements => _content.Elements;

        /// <summary>
        /// Root UI elements (top-level elements in hierarchy)
        /// </summary>
        public List<UIElement> RootElements =>
            _content.Elements.Where(e => _content.RootElementIds.Contains(e.Id)).ToList();

        private UIPage(UIPageContent content)
        {
            _content = content;

            // Set ParentPage reference for all elements
            foreach (var element in _content.Elements)
            {
                element.ParentPage = this;
            }
        }

        /// <summary>
        /// Loads a UI page from a .sduipage file
        /// </summary>
        /// <param name="filePath">Path to the .sduipage file</param>
        /// <returns>Loaded UI page</returns>
        public static UIPage Load(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"UI page file not found: {filePath}");

            var content = UIPageParser.Parse(filePath);
            return new UIPage(content);
        }

        /// <summary>
        /// Creates a new empty UI page with a root Grid element
        /// </summary>
        /// <param name="name">Name of the UI page</param>
        /// <param name="filePath">Optional file path (can be set later via SaveAs)</param>
        /// <returns>A new UIPage with a root Grid container</returns>
        public static UIPage Create(string name, string? filePath = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));

            var content = new UIPageContent
            {
                Id = GuidHelper.NewGuid(),
                FilePath = filePath ?? string.Empty,
                Elements = new List<UIElement>(),
                RootElementIds = new List<string>(),
                RawContent = string.Empty
            };

            var page = new UIPage(content);

            // Create root Grid element (standard Stride UI page pattern)
            var rootGrid = page.CreateElement("Grid", name);
            page._content.RootElementIds.Add(rootGrid.Id);

            return page;
        }

        /// <summary>
        /// Creates a new UI element and adds it to the page
        /// </summary>
        /// <param name="type">The UI element type (e.g., "Grid", "TextBlock", "Button", "ImageElement")</param>
        /// <param name="name">The name of the element</param>
        /// <param name="parent">Optional parent element to nest this element under</param>
        /// <returns>The newly created UI element</returns>
        public UIElement CreateElement(string type, string name, UIElement? parent = null)
        {
            if (string.IsNullOrWhiteSpace(type))
                throw new ArgumentNullException(nameof(type));
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));

            var element = new UIElement
            {
                Id = GuidHelper.NewGuid(),
                Name = name,
                Type = type,
                ParentPage = this
            };

            // Add default properties based on type
            element.Properties = new Dictionary<string, object>
            {
                ["DependencyProperties"] = new Dictionary<string, object>(),
                ["BackgroundColor"] = new Dictionary<string, object>
                {
                    ["R"] = 0,
                    ["G"] = 0,
                    ["B"] = 0,
                    ["A"] = 0
                },
                ["Margin"] = new Dictionary<string, object>(),
                ["MaximumWidth"] = 3.4028235E+38f,
                ["MaximumHeight"] = 3.4028235E+38f,
                ["MaximumDepth"] = 3.4028235E+38f
            };

            // Add type-specific defaults
            switch (type.ToLower())
            {
                case "grid":
                case "canvas":
                case "stackpanel":
                    element.Properties["Children"] = new Dictionary<string, object>();
                    if (type.ToLower() == "grid")
                    {
                        element.Properties["RowDefinitions"] = new Dictionary<string, object>();
                        element.Properties["ColumnDefinitions"] = new Dictionary<string, object>();
                        element.Properties["LayerDefinitions"] = new Dictionary<string, object>();
                    }
                    break;

                case "textblock":
                    element.Properties["Text"] = "";
                    element.Properties["Font"] = "c90f3988-0544-4cbe-993f-13af7d9c23c6:StrideDefaultFont";
                    element.Properties["TextSize"] = 20.0f;
                    element.Properties["TextColor"] = new Dictionary<string, object>
                    {
                        ["R"] = 240,
                        ["G"] = 240,
                        ["B"] = 240,
                        ["A"] = 255
                    };
                    element.Properties["OutlineColor"] = new Dictionary<string, object>
                    {
                        ["R"] = 0,
                        ["G"] = 0,
                        ["B"] = 0,
                        ["A"] = 255
                    };
                    element.Properties["OutlineThickness"] = 0.0f;
                    element.Properties["HorizontalAlignment"] = "Center";
                    element.Properties["VerticalAlignment"] = "Center";
                    break;

                case "imageelement":
                    element.Properties["Source"] = new Dictionary<string, object>
                    {
                        ["!SpriteFromSheet"] = "",
                        ["Sheet"] = "null",
                        ["CurrentFrame"] = 0
                    };
                    element.Properties["Color"] = new Dictionary<string, object>
                    {
                        ["R"] = 255,
                        ["G"] = 255,
                        ["B"] = 255,
                        ["A"] = 255
                    };
                    element.Properties["StretchType"] = "FillOnStretch";
                    break;

                case "button":
                    element.Properties["DrawLayerNumber"] = 2;
                    element.Properties["PressedImage"] = new Dictionary<string, object>
                    {
                        ["!SpriteFromSheet"] = "",
                        ["Sheet"] = "null",
                        ["CurrentFrame"] = 0
                    };
                    element.Properties["NotPressedImage"] = new Dictionary<string, object>
                    {
                        ["!SpriteFromSheet"] = "",
                        ["Sheet"] = "null",
                        ["CurrentFrame"] = 0
                    };
                    element.Properties["MouseOverImage"] = new Dictionary<string, object>
                    {
                        ["!SpriteFromSheet"] = "",
                        ["Sheet"] = "null",
                        ["CurrentFrame"] = 0
                    };
                    break;
            }

            // Add to page
            _content.Elements.Add(element);

            // Add to parent (or root Grid if parent is null)
            if (parent != null)
            {
                parent.AddChild(element);
            }
            else
            {
                // Auto-attach to root Grid when no parent is specified
                var rootGrid = RootElements.FirstOrDefault();
                if (rootGrid != null && element != rootGrid)  // Don't add root to itself
                {
                    rootGrid.AddChild(element);
                }
            }

            return element;
        }

        /// <summary>
        /// Removes a UI element from the page
        /// </summary>
        /// <param name="element">The element to remove</param>
        /// <returns>True if the element was removed, false otherwise</returns>
        public bool RemoveElement(UIElement element)
        {
            if (element == null)
                return false;

            // Remove from parent's children
            element.Parent?.RemoveChild(element);

            // Remove from root elements if present
            _content.RootElementIds.Remove(element.Id);

            // Remove from all elements
            return _content.Elements.Remove(element);
        }

        /// <summary>
        /// Finds a UI element by ID
        /// </summary>
        /// <param name="id">The element ID</param>
        /// <returns>The UI element if found, null otherwise</returns>
        public UIElement? FindElementById(string id)
        {
            return _content.Elements.FirstOrDefault(e => e.Id == id);
        }

        /// <summary>
        /// Finds a UI element by name (exact match)
        /// </summary>
        /// <param name="name">The element name</param>
        /// <returns>The UI element if found, null otherwise</returns>
        public UIElement? FindElementByName(string name)
        {
            return _content.Elements.FirstOrDefault(e => e.Name == name);
        }

        /// <summary>
        /// Finds all UI elements with a specific name pattern (supports * and ?)
        /// </summary>
        /// <param name="pattern">Name pattern with wildcards (* for any characters, ? for single character)</param>
        /// <returns>Collection of matching UI elements</returns>
        public IEnumerable<UIElement> FindElementsByName(string pattern)
        {
            var regexPattern = "^" + System.Text.RegularExpressions.Regex.Escape(pattern)
                .Replace("\\*", ".*")
                .Replace("\\?", ".") + "$";

            return _content.Elements.Where(e => System.Text.RegularExpressions.Regex.IsMatch(e.Name, regexPattern));
        }

        /// <summary>
        /// Finds all UI elements of a specific type
        /// </summary>
        /// <param name="type">The UI element type (e.g., "TextBlock", "Button")</param>
        /// <returns>Collection of matching UI elements</returns>
        public IEnumerable<UIElement> FindElementsByType(string type)
        {
            return _content.Elements.Where(e => e.Type.Equals(type, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Finds UI elements using a custom predicate
        /// </summary>
        /// <param name="predicate">Custom filter function</param>
        /// <returns>Collection of matching UI elements</returns>
        public IEnumerable<UIElement> FindElements(Func<UIElement, bool> predicate)
        {
            return _content.Elements.Where(predicate);
        }

        /// <summary>
        /// Saves the UI page to its current file path
        /// </summary>
        public void Save()
        {
            if (string.IsNullOrEmpty(_content.FilePath))
                throw new InvalidOperationException("FilePath is not set. Use SaveAs instead.");

            UIPageWriter.Write(this, _content);
        }

        /// <summary>
        /// Saves the UI page to a new file path
        /// </summary>
        /// <param name="filePath">The file path to save to</param>
        public void SaveAs(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException(nameof(filePath));

            _content.FilePath = filePath;
            UIPageWriter.Write(this, _content);
        }

        /// <summary>
        /// Gets the internal content object (for writer access)
        /// </summary>
        internal UIPageContent GetContent() => _content;
    }
}
