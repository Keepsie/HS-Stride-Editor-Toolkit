// HS Stride Editor Toolkit (c) 2025 Happenstance Games LLC - Apache License 2.0

using HS.Stride.Editor.Toolkit.Utilities;

namespace HS.Stride.Editor.Toolkit.Core.UIPageEditing
{
    /// <summary>
    /// Represents a Stride UI page (.sduipage file) that can be loaded, modified, and saved.
    /// Similar to Scene/Prefab but for UI elements.
    /// </summary>
    public class UIPage : IStrideAsset
    {
        private readonly UIPageContent _content;

        public string Id => _content.Id;
        public string FilePath => _content.FilePath;

        /// <summary>
        /// Design resolution for the UI page
        /// </summary>
        public Dictionary<string, float> Resolution => _content.Resolution;

        /// <summary>
        /// Gets the design resolution as a tuple (X, Y, Z).
        /// </summary>
        /// <returns>Design resolution tuple or null if not set</returns>
        public (float X, float Y, float Z)? GetDesignResolution()
        {
            if (_content.Resolution.ContainsKey("X") &&
                _content.Resolution.ContainsKey("Y") &&
                _content.Resolution.ContainsKey("Z"))
            {
                return (_content.Resolution["X"], _content.Resolution["Y"], _content.Resolution["Z"]);
            }
            return null;
        }

        /// <summary>
        /// Sets the design resolution.
        /// </summary>
        /// <param name="x">Width resolution</param>
        /// <param name="y">Height resolution</param>
        /// <param name="z">Depth resolution</param>
        public void SetDesignResolution(float x, float y, float z)
        {
            _content.Resolution["X"] = x;
            _content.Resolution["Y"] = y;
            _content.Resolution["Z"] = z;
        }

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
        /// Creates a new empty UI page with a root Grid element.
        /// INTERNAL: Use StrideProject.CreateUIPage() instead.
        /// </summary>
        /// <param name="name">Name of the UI page</param>
        /// <param name="filePath">File path for the UI page</param>
        /// <returns>A new UIPage with a root Grid container</returns>
        internal static UIPage Create(string name, string? filePath = null)
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
        /// <param name="autoAttach">If true and parent is null, auto-attaches to root Grid. Set to false for button content.</param>
        /// <returns>The newly created UI element</returns>
        public UIElement CreateElement(string type, string name, UIElement? parent = null, bool autoAttach = true)
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

            // Add to parent (or root Grid if parent is null and autoAttach is true)
            if (parent != null)
            {
                parent.AddChild(element);
            }
            else if (autoAttach)
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
            UIPageWriter.Write(this, _content);
        }

        /// <summary>
        /// Saves the UI page to a new file path.
        /// If ParentProject exists, takes a relative path from Assets folder.
        /// If no ParentProject, takes a full file path.
        /// If path ends with / or \, uses first root element name as filename.
        /// </summary>
        /// <param name="path">Relative path from Assets folder or full file path</param>
        public void SaveAs(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentNullException(nameof(path));

            if (_content.ParentProject != null)
            {
                // If path ends with separator, append first root element name
                if (path.EndsWith("/") || path.EndsWith("\\"))
                {
                    var fileName = "UIPage";
                    if (_content.RootElementIds.Count > 0)
                    {
                        var rootElement = _content.Elements.FirstOrDefault(e => e.Id == _content.RootElementIds[0]);
                        fileName = rootElement?.Name ?? "UIPage";
                    }
                    path = path + fileName;
                }
            }

            _content.FilePath = path;
            UIPageWriter.Write(this, _content);
        }

        /// <summary>
        /// Gets the internal content object (for writer access)
        /// </summary>
        internal UIPageContent GetContent() => _content;

        /// <summary>
        /// Sets the parent project for this UI page (enables path resolution and validation)
        /// </summary>
        internal void SetParentProject(StrideProject project)
        {
            _content.ParentProject = project;
        }

        /// <summary>
        /// Gets a property value by name. Supports nested paths with dot notation.
        /// e.g. "Design.Resolution", "Hierarchy.RootParts"
        /// NOTE: This is a compatibility method for migration from UIPageAsset.
        /// Prefer using structured properties like Resolution, GetDesignResolution(), etc.
        /// </summary>
        /// <param name="propertyName">The property name or path</param>
        /// <returns>The property value or null if not found</returns>
        public object? Get(string propertyName)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
                throw new ArgumentNullException(nameof(propertyName));

            return GetNestedProperty(_content.Properties, propertyName);
        }

        /// <summary>
        /// Sets a property value by name. Supports nested paths with dot notation.
        /// NOTE: This is a compatibility method for migration from UIPageAsset.
        /// Prefer using structured properties like SetDesignResolution(), etc.
        /// Only properties that Stride serializes will persist when saved.
        /// </summary>
        /// <param name="propertyName">The property name or path</param>
        /// <param name="value">The value to set</param>
        public void Set(string propertyName, object value)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
                throw new ArgumentNullException(nameof(propertyName));

            SetNestedProperty(_content.Properties, propertyName, value);
        }

        /// <summary>
        /// Gets all properties as a dictionary.
        /// NOTE: This is a compatibility method for migration from UIPageAsset.
        /// </summary>
        /// <returns>Dictionary of all properties</returns>
        public Dictionary<string, object> GetAllProperties()
        {
            return _content.Properties;
        }

        private object? GetNestedProperty(Dictionary<string, object> dict, string path)
        {
            var parts = path.Split('.');
            object? current = dict;

            foreach (var part in parts)
            {
                if (current is Dictionary<string, object> currentDict)
                {
                    if (currentDict.ContainsKey(part))
                    {
                        current = currentDict[part];
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }

            return current;
        }

        private void SetNestedProperty(Dictionary<string, object> dict, string path, object value)
        {
            var parts = path.Split('.');
            var current = dict;

            for (int i = 0; i < parts.Length - 1; i++)
            {
                var part = parts[i];
                if (!current.ContainsKey(part))
                {
                    current[part] = new Dictionary<string, object>();
                }

                if (current[part] is Dictionary<string, object> nextDict)
                {
                    current = nextDict;
                }
                else
                {
                    throw new InvalidOperationException($"Cannot navigate through non-dictionary property '{part}' in path '{path}'");
                }
            }

            current[parts[^1]] = value;
        }
    }
}
