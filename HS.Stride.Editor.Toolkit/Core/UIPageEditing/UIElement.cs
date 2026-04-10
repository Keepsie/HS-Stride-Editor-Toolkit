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
            if (child == null)
                throw new ArgumentNullException(nameof(child));

            // Reparent safely: detach from previous parent first
            if (child.Parent != null && !ReferenceEquals(child.Parent, this))
            {
                child.Parent.RemoveChild(child);
            }

            // Prevent duplicate references in this parent (same object or same ID)
            var existingKeys = Children
                .Where(kvp => ReferenceEquals(kvp.Value, child) ||
                             (!string.IsNullOrEmpty(child.Id) && kvp.Value.Id == child.Id))
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in existingKeys)
            {
                Children.Remove(key);
            }

            var hash = Guid.NewGuid().ToString("N");
            Children[hash] = child;
            child.Parent = this;

            // Keep page references in sync when attaching through hierarchy operations
            if (ParentPage != null)
            {
                child.ParentPage = ParentPage;
            }

            RecalculateDirectChildZIndices();
        }

        /// <summary>
        /// Removes a child UIElement from this container
        /// </summary>
        public bool RemoveChild(UIElement child)
        {
            if (child == null)
                return false;

            var entry = Children.FirstOrDefault(kvp =>
                ReferenceEquals(kvp.Value, child) ||
                (!string.IsNullOrEmpty(child.Id) && kvp.Value.Id == child.Id));

            if (!string.IsNullOrEmpty(entry.Key))
            {
                Children.Remove(entry.Key);

                // Clear parent reference on the removed child instance
                if (ReferenceEquals(entry.Value.Parent, this))
                    entry.Value.Parent = null;

                // Also clear caller instance if it was pointing at this parent
                if (ReferenceEquals(child.Parent, this))
                    child.Parent = null;

                RecalculateDirectChildZIndices();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Recalculates direct child ZIndex values based on current child order.
        /// Child at index 0 gets ZIndex 0, index 1 gets ZIndex 1, etc.
        /// </summary>
        private void RecalculateDirectChildZIndices()
        {
            int index = 0;
            foreach (var child in Children.Values)
            {
                child.SetZIndex(index);
                index++;
            }
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

        #region ZIndex (Panel.ZIndexPropertyKey) Helpers

        /// <summary>
        /// Returns true if a Panel.ZIndexPropertyKey entry exists in DependencyProperties.
        /// Note: ZIndex 0 is the default and is stored implicitly (key absent), so this
        /// returns false for elements that have never had SetZIndex called or whose ZIndex is 0.
        /// Use in combination with GetZIndex() when you need to distinguish "explicitly set to 0" from "never set".
        /// </summary>
        public bool HasExplicitZIndexKey()
        {
            var depProps = Get<Dictionary<string, object>>("DependencyProperties");
            if (depProps == null) return false;
            return depProps.Keys.Any(k => k.EndsWith("~Panel.ZIndexPropertyKey"));
        }

        /// <summary>
        /// Gets the ZIndex (Panel.ZIndex) value from DependencyProperties.
        /// ZIndex controls sibling draw order within a panel - higher values are drawn on top.
        /// </summary>
        /// <returns>The ZIndex value, or 0 if not set</returns>
        public int GetZIndex()
        {
            var depProps = Get<Dictionary<string, object>>("DependencyProperties");
            if (depProps == null) return 0;

            foreach (var kvp in depProps)
            {
                if (kvp.Key.EndsWith("~Panel.ZIndexPropertyKey"))
                {
                    if (kvp.Value is int intVal)
                        return intVal;
                    if (int.TryParse(kvp.Value?.ToString(), out int parsedVal))
                        return parsedVal;
                }
            }
            return 0;
        }

        /// <summary>
        /// Sets the ZIndex (Panel.ZIndex) value in DependencyProperties.
        /// ZIndex controls sibling draw order within a panel - higher values are drawn on top.
        /// </summary>
        /// <param name="zIndex">The ZIndex value to set</param>
        public void SetZIndex(int zIndex)
        {
            var depProps = Get<Dictionary<string, object>>("DependencyProperties")
                ?? new Dictionary<string, object>();

            // Remove any existing ZIndex entry
            var existingKey = depProps.Keys.FirstOrDefault(k => k.EndsWith("~Panel.ZIndexPropertyKey"));
            if (existingKey != null)
            {
                depProps.Remove(existingKey);
            }

            // Add new ZIndex entry (format: "<guid>~Panel.ZIndexPropertyKey": value)
            // Only add if non-zero (0 is the default in Stride serialization)
            if (zIndex != 0)
            {
                var zIndexKey = $"{Guid.NewGuid():N}~Panel.ZIndexPropertyKey";
                depProps[zIndexKey] = zIndex;
            }

            // Clear legacy Canvas.ZIndex / Grid.ZIndex aliases to keep a single source of truth
            Properties.Remove("Canvas.ZIndex");
            Properties.Remove("Grid.ZIndex");

            // Update or remove DependencyProperties based on whether it has entries
            if (depProps.Count > 0)
            {
                Set("DependencyProperties", depProps);
            }
            else
            {
                Properties.Remove("DependencyProperties");
            }
        }

        #endregion
    }
}
