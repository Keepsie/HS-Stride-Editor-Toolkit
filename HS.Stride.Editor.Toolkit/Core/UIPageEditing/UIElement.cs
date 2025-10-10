// HS Stride Editor Toolkit (c) 2025 Happenstance Games LLC - Apache License 2.0

namespace HS.Stride.Editor.Toolkit.Core.UIPageEditing
{
    /// <summary>
    /// Represents a UI element in a Stride UI page (Canvas, Grid, Button, TextBlock, ImageElement, etc.).
    /// Similar to Entity in scenes, but for UI elements.
    /// </summary>
    public class UIElement
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // Grid, Canvas, Button, TextBlock, ImageElement, etc.

        /// <summary>
        /// Properties of this UI element (Width, Height, Margin, Color, etc.)
        /// </summary>
        public Dictionary<string, object> Properties { get; set; } = new();

        /// <summary>
        /// Raw YAML lines for lazy loading (unparsed data)
        /// </summary>
        internal List<string> RawYaml { get; set; } = new();

        /// <summary>
        /// Child elements (for container types like Canvas, Grid, StackPanel)
        /// Key = child ID hash, Value = child UIElement reference
        /// </summary>
        public Dictionary<string, UIElement> Children { get; set; } = new();

        /// <summary>
        /// Parent UI element (null if root)
        /// </summary>
        public UIElement? Parent { get; set; }

        /// <summary>
        /// Reference to the parent UIPage
        /// </summary>
        public UIPage? ParentPage { get; set; }

        public UIElement() { }

        public UIElement(string id, string name, string type)
        {
            Id = id;
            Name = name;
            Type = type;
        }

        /// <summary>
        /// Gets a property value by key
        /// </summary>
        public T? Get<T>(string key)
        {
            if (Properties.TryGetValue(key, out var value))
            {
                if (value is T typed)
                    return typed;

                // Attempt conversion
                try
                {
                    return (T)Convert.ChangeType(value, typeof(T));
                }
                catch
                {
                    return default;
                }
            }
            return default;
        }

        /// <summary>
        /// Sets a property value
        /// </summary>
        public void Set(string key, object value)
        {
            Properties[key] = value;
        }

        /// <summary>
        /// Checks if a property exists
        /// </summary>
        public bool HasProperty(string key)
        {
            return Properties.ContainsKey(key);
        }

        /// <summary>
        /// Gets all children UIElements (for containers)
        /// </summary>
        public List<UIElement> GetChildren()
        {
            return Children.Values.ToList();
        }

        /// <summary>
        /// Adds a child UIElement to this container
        /// </summary>
        public void AddChild(UIElement child)
        {
            var hash = Guid.NewGuid().ToString("N");
            Children[hash] = child;
            child.Parent = this;
        }

        /// <summary>
        /// Removes a child UIElement from this container
        /// </summary>
        public bool RemoveChild(UIElement child)
        {
            var entry = Children.FirstOrDefault(kvp => kvp.Value.Id == child.Id);
            if (!string.IsNullOrEmpty(entry.Key))
            {
                Children.Remove(entry.Key);
                child.Parent = null;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Finds a child by name
        /// </summary>
        public UIElement? FindChildByName(string name)
        {
            return Children.Values.FirstOrDefault(c => c.Name == name);
        }

        /// <summary>
        /// Gets all descendants recursively (children, grandchildren, etc.)
        /// </summary>
        public List<UIElement> GetDescendants()
        {
            var descendants = new List<UIElement>();

            foreach (var child in Children.Values)
            {
                descendants.Add(child);
                descendants.AddRange(child.GetDescendants());
            }

            return descendants;
        }
    }
}
