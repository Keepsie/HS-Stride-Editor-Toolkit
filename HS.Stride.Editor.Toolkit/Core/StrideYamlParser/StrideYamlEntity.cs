using System.Text;

namespace HS.Stride.Editor.Toolkit.Core.StrideYamlParser
{
    /// <summary>
    /// Generates YAML text for a single entity including ID, Name, Components, and Base (prefab) info.
    /// Self-contained - returns complete entity text block.
    /// </summary>
    public class StrideYamlEntity : StrideYaml
    {
        /// <summary>
        /// Generates complete entity YAML text at the specified indent level.
        /// </summary>
        public static string GenerateEntityYaml(Entity entity, int baseIndent)
        {
            var sb = new StringBuilder();

            // Entity ID
            sb.AppendLine($"{Indent(baseIndent)}Id: {entity.Id}");

            // Entity Name
            sb.AppendLine($"{Indent(baseIndent)}Name: {entity.Name}");

            // Note: Folder is written OUTSIDE the Entity block by the scene/prefab writer

            // Components section
            sb.AppendLine($"{Indent(baseIndent)}Components:");
            WriteComponents(sb, entity, baseIndent + 1);

            return sb.ToString();
        }

        /// <summary>
        /// Gets the raw component YAML for an entity from the scene's raw content.
        /// Used to preserve unloaded components during serialization.
        /// </summary>
        private static string? GetRawComponentsSection(Entity entity)
        {
            if (entity.ParentScene == null || string.IsNullOrEmpty(entity.ParentScene.RawContent))
                return null;

            var rawContent = entity.ParentScene.RawContent;
            var entityIdMarker = $"Id: {entity.Id}";
            var entityStartIndex = rawContent.IndexOf(entityIdMarker);
            
            if (entityStartIndex == -1)
                return null;

            // Find the Components: line
            var componentsMarker = "Components:";
            var componentsIndex = rawContent.IndexOf(componentsMarker, entityStartIndex);
            
            if (componentsIndex == -1)
                return null;

            // Find where components section ends (next "Base:", "Entity:", "Folder:", or "Parts:")
            var nextSectionIndex = rawContent.Length;
            var baseIndex = rawContent.IndexOf("\n            Base:", componentsIndex);
            var entityIndex = rawContent.IndexOf("\n        -   Entity:", componentsIndex);
            var folderIndex = rawContent.IndexOf("\n        -   Folder:", componentsIndex);

            if (baseIndex != -1 && baseIndex < nextSectionIndex)
                nextSectionIndex = baseIndex;
            if (entityIndex != -1 && entityIndex < nextSectionIndex)
                nextSectionIndex = entityIndex;
            if (folderIndex != -1 && folderIndex < nextSectionIndex)
                nextSectionIndex = folderIndex;

            var componentsSection = rawContent.Substring(componentsIndex + componentsMarker.Length, 
                nextSectionIndex - (componentsIndex + componentsMarker.Length));

            return componentsSection;
        }

        /// <summary>
        /// Extracts raw component entries from raw YAML that haven't been loaded.
        /// Only captures top-level component headers directly under the Components: section,
        /// avoiding nested type tags (e.g., ColliderShapes entries).
        /// Returns a dictionary of component key -> raw YAML text.
        /// </summary>
        private static Dictionary<string, string> GetUnloadedComponentsFromRaw(Entity entity)
        {
            var unloadedComponents = new Dictionary<string, string>();
            var rawComponentsSection = GetRawComponentsSection(entity);
            if (string.IsNullOrEmpty(rawComponentsSection))
                return unloadedComponents;

            var lines = rawComponentsSection.Split('\n');

            // Helper: check if left side looks like a 32-char hex key (component key)
            static bool IsHex32(string s)
            {
                if (s.Length != 32) return false;
                for (int i = 0; i < s.Length; i++)
                {
                    char c = s[i];
                    bool isHex = (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F');
                    if (!isHex) return false;
                }
                return true;
            }

            // First pass: determine the indent level of actual component headers (minimum indent among header lines)
            int headerIndent = int.MaxValue;
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                var trimmed = line.Trim();
                if (!trimmed.Contains(": !")) continue;

                // left side of ":"
                var colon = trimmed.IndexOf(':');
                if (colon <= 0) continue;
                var keyCandidate = trimmed.Substring(0, colon).Trim();

                if (IsHex32(keyCandidate))
                {
                    int indent = line.TakeWhile(c => c == ' ').Count();
                    if (indent < headerIndent) headerIndent = indent;
                }
            }
            if (headerIndent == int.MaxValue)
            {
                // No recognizable headers found
                return unloadedComponents;
            }

            // Second pass: capture only components that start at headerIndent
            string? currentKey = null;
            var currentLines = new List<string>();
            int currentIndent = -1;

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                var trimmed = line.Trim();
                int indent = line.TakeWhile(c => c == ' ').Count();

                // Start of a top-level component header directly under Components:
                if (indent == headerIndent && trimmed.Contains(": !"))
                {
                    var colon = trimmed.IndexOf(':');
                    var keyCandidate = trimmed.Substring(0, colon).Trim();

                    if (IsHex32(keyCandidate))
                    {
                        // Save previous if not loaded
                        if (currentKey != null && !entity.Components.ContainsKey(currentKey))
                        {
                            unloadedComponents[currentKey] = string.Join("\n", currentLines);
                        }

                        currentKey = keyCandidate;
                        currentLines = new List<string> { line };
                        currentIndent = indent;
                        continue;
                    }
                }

                // Continue current component block if we are inside one and the indent is deeper (or list continuation at same indent)
                if (currentKey != null)
                {
                    if (indent > currentIndent || (indent == currentIndent && trimmed.StartsWith("-")))
                    {
                        currentLines.Add(line);
                    }
                }
            }

            // Save last pending component block if it wasn't loaded already
            if (currentKey != null && !entity.Components.ContainsKey(currentKey))
            {
                unloadedComponents[currentKey] = string.Join("\n", currentLines);
            }

            return unloadedComponents;
        }

        /// <summary>
        /// Writes all components for an entity.
        /// Includes both loaded components from entity.Components and unloaded components from raw YAML.
        /// </summary>
        private static void WriteComponents(StringBuilder sb, Entity entity, int indent)
        {
            // Write loaded components
            foreach (var componentEntry in entity.Components)
            {
                var component = componentEntry.Value;

                // Component key (guid without hyphens) with type tag
                // Format: "6d7abb0cc49bcc40bb47fb4c3e7e6097: !TransformComponent"
                sb.AppendLine($"{Indent(indent)}{componentEntry.Key}: !{component.Type}");

                // Write all component properties
                WriteComponentProperties(sb, component, indent + 1);
            }

            // Write unloaded components (preserve from raw YAML)
            var unloadedComponents = GetUnloadedComponentsFromRaw(entity);
            foreach (var unloadedComponent in unloadedComponents)
            {
                // Write raw component YAML directly
                sb.AppendLine(unloadedComponent.Value);
            }
        }

        /// <summary>
        /// Writes all properties of a component.
        /// Ensures the component Id is emitted first, then other properties (excluding any "Id" in Properties).
        /// </summary>
        private static void WriteComponentProperties(StringBuilder sb, Component component, int indent)
        {
            // Always write component Id first if available
            if (!string.IsNullOrEmpty(component.Id))
            {
                sb.AppendLine($"{Indent(indent)}Id: {component.Id}");
            }

            foreach (var prop in component.Properties)
            {
                // Skip any "Id" property duplicates; Id is handled explicitly above
                if (string.Equals(prop.Key, "Id", System.StringComparison.OrdinalIgnoreCase))
                    continue;

                WriteProperty(sb, prop.Key, prop.Value, indent);
            }
        }

        /// <summary>
        /// Writes a single property with proper formatting based on its type.
        /// Handles primitives, dictionaries, lists, and special Stride types.
        /// </summary>
        private static void WriteProperty(StringBuilder sb, string key, object? value, int indent)
        {
            if (value == null)
            {
                sb.AppendLine($"{Indent(indent)}{key}: null");
                return;
            }

            // Handle dictionaries
            if (value is Dictionary<string, object> dict)
            {
                WriteDictionary(sb, key, dict, indent);
                return;
            }

            // Handle lists
            if (value is System.Collections.IList list)
            {
                WriteList(sb, key, list, indent);
                return;
            }

            // Handle primitives and strings
            sb.AppendLine($"{Indent(indent)}{key}: {FormatValue(value)}");
        }

        /// <summary>
        /// Writes a dictionary property.
        /// Handles inline format {X: 0.0, Y: 0.0} and nested format with type tags.
        /// </summary>
        private static void WriteDictionary(StringBuilder sb, string key, Dictionary<string, object> dict, int indent)
        {
            // Empty dictionary
            if (dict.Count == 0)
            {
                sb.AppendLine($"{Indent(indent)}{key}: {{}}");
                return;
            }

            // Check for type tag (key starting with "!")
            var typeTag = dict.Keys.FirstOrDefault(k => k.StartsWith("!"));

            if (typeTag != null)
            {
                // Format with type tag: "Type: !LightDirectional"
                sb.AppendLine($"{Indent(indent)}{key}: {typeTag}");

                // Write remaining properties (excluding the type tag itself)
                foreach (var kvp in dict.Where(kvp => kvp.Key != typeTag))
                {
                    WriteProperty(sb, kvp.Key, kvp.Value, indent + 1);
                }
            }
            else if (IsSimpleValueDict(dict) && !dict.Values.Any(v => v is string sv && (sv.TrimStart().StartsWith("!") || sv.Contains("ref!!"))))
            {
                // Inline format for simple dicts: {X: 0.0, Y: 0.0, Z: 0.0}
                var items = dict.Select(kvp => $"{kvp.Key}: {FormatValue(kvp.Value)}");
                sb.AppendLine($"{Indent(indent)}{key}: {{{string.Join(", ", items)}}}");
            }
            else
            {
                // Nested dictionary format
                sb.AppendLine($"{Indent(indent)}{key}:");
                foreach (var kvp in dict)
                {
                    // Check if the value is a dictionary with a type tag (like ColliderShapes entries)
                    if (kvp.Value is Dictionary<string, object> nestedDict)
                    {
                        var nestedTypeTag = nestedDict.Keys.FirstOrDefault(k => k.StartsWith("!"));
                        
                        if (nestedTypeTag != null)
                        {
                            // Write the key with type tag: "guid: !BoxColliderShapeDesc"
                            sb.AppendLine($"{Indent(indent + 1)}{kvp.Key}: {nestedTypeTag}");
                            
                            // Write the remaining properties of the nested dictionary
                            foreach (var nestedKvp in nestedDict.Where(nkvp => nkvp.Key != nestedTypeTag))
                            {
                                WriteProperty(sb, nestedKvp.Key, nestedKvp.Value, indent + 2);
                            }
                        }
                        else
                        {
                            // Regular nested dictionary
                            WriteProperty(sb, kvp.Key, kvp.Value, indent + 1);
                        }
                    }
                    else
                    {
                        WriteProperty(sb, kvp.Key, kvp.Value, indent + 1);
                    }
                }
            }
        }

        /// <summary>
        /// Writes a list/array property.
        /// Handles empty lists, reference lists, and complex nested lists.
        /// </summary>
        private static void WriteList(StringBuilder sb, string key, System.Collections.IList list, int indent)
        {
            // Empty list
            if (list.Count == 0)
            {
                sb.AppendLine($"{Indent(indent)}{key}: []");
                return;
            }

            sb.AppendLine($"{Indent(indent)}{key}:");

            foreach (var item in list)
            {
                if (item is string str && str.StartsWith("ref!! "))
                {
                    // Entity reference: "- ref!! guid"
                    sb.AppendLine($"{Indent(indent + 1)}- {str}");
                }
                else if (item is Dictionary<string, object> dictItem)
                {
                    // Complex list item (like ColliderShapes)
                    WriteDictionaryListItem(sb, dictItem, indent + 1);
                }
                else
                {
                    // Simple list item
                    sb.AppendLine($"{Indent(indent + 1)}- {FormatValue(item)}");
                }
            }
        }

        /// <summary>
        /// Writes a dictionary that appears as a list item.
        /// Used for ColliderShapes and similar nested structures.
        /// Format:
        ///     - guid: !TypeTag
        ///       Property: value
        /// </summary>
        private static void WriteDictionaryListItem(StringBuilder sb, Dictionary<string, object> dict, int indent)
        {
            bool first = true;

            foreach (var kvp in dict)
            {
                if (first)
                {
                    // First property gets the list marker "-"
                    sb.Append($"{Indent(indent)}- ");
                    first = false;
                }
                else
                {
                    sb.Append($"{Indent(indent + 1)}");
                }

                // Check if this is a type tag
                if (kvp.Key.StartsWith("!"))
                {
                    // Type tag with empty value
                    sb.AppendLine($"{kvp.Key}: {FormatValue(kvp.Value)}");
                }
                else if (kvp.Value is Dictionary<string, object> nestedDict)
                {
                    // Nested dictionary
                    sb.Append($"{kvp.Key}:");
                    if (IsSimpleValueDict(nestedDict) && !nestedDict.Values.Any(v => v is string sv && (sv.TrimStart().StartsWith("!") || sv.Contains("ref!!"))))
                    {
                        // Inline format (only when there are no YAML type tags or ref!! tokens in values)
                        var items = nestedDict.Select(nkvp => $"{nkvp.Key}: {FormatValue(nkvp.Value)}");
                        sb.AppendLine($" {{{string.Join(", ", items)}}}");
                    }
                    else
                    {
                        // Nested format
                        sb.AppendLine();
                        foreach (var nkvp in nestedDict)
                        {
                            WriteProperty(sb, nkvp.Key, nkvp.Value, indent + 2);
                        }
                    }
                }
                else
                {
                    // Simple property
                    sb.AppendLine($"{kvp.Key}: {FormatValue(kvp.Value)}");
                }
            }
        }
    }
}
